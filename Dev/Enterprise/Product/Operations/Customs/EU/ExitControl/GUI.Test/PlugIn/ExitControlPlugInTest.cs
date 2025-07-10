using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.GUI.PlugIn.Testing
{
	sealed class ExitControlPlugInTest : ZPlugInGenericTest
	{
		public void TestOnlyExitControlHeadersInThisPlugin()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = Factory.New<CusExitHeader>();
			header.CXH_ParentID = declaration.PK;
			header.CXH_ParentTableCode = declaration.TablePrefix;
			header.CXH_ApplicationCode = "DAC";
			plugIn = GetExitControlPlugIn(declaration);
			AssertNull(plugIn.ExitHeader);
			header.CXH_ApplicationCode = "XIT";
			AssertNotNull(plugIn.ExitHeader);
		}

		public void TestPreventDuplicateExitDeclarationCreation()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (ExitControlCustomsDataRegistry.Instance.EnableExitControlPlugin.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				Freight.Business.ChildEditableService.SetState(Factory, Freight.Integration.ChildEditableServiceStates.Shipment);
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_UniqueConsignRef = "SHP - PreSave";
				shipment.JS_RL_NKOrigin = "DEWIB";
				shipment.JS_RL_NKDestination = "AUSYD";
				Factory.Save();
				using (var form = new ShipmentForm(shipment))
				{
					form.Show();
					var plugIn = (ExitControlPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.EU.ExitSummaryController);
					ZFormModaliser.ShowDialogsInTest = true;
					Globals.SetIsUnitTestingProductionFunctionality(true);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
					CusExitHeader exitHeaderCreatedOutsideFromPlugIn = null;
					ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs(x =>
					{
						exitHeaderCreatedOutsideFromPlugIn = Factory.NewWithValidTestData<CusExitHeader>();
						exitHeaderCreatedOutsideFromPlugIn.Parent = shipment;
						Factory.Save();
					});
					((ISupportSwitchTabPage)form).SwitchTabPage("EUExitControlTabPage");
					Globals.SetIsUnitTestingProductionFunctionality(false);
					CombineAssertions(() =>
					{
						var exitHeaderLinkedToShipment = Factory.Load<CusExitHeader>(new ZQuery(CusExitHeaderSchema.CXH_ParentID, SQLComparisonOperator.Equal, shipment.PK)).SingleOrDefault();
						AssertEquals("PK", exitHeaderCreatedOutsideFromPlugIn.PK, exitHeaderLinkedToShipment.PK);
						AssertEquals("In the meantime an Exit Declaration has been created and linked to this Shipment. Please reload to access it.", plugIn.PlugInNotDisplayedMessage);
					});
				}
			}
		}

		public void TestDefaultDataFromParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var plugIn = new ExitControlPlugInForTestingDefaultData(declaration))
			{
				UnitTestUserNotification.Instance.AddYesAnswer();
				plugIn.CreateExitHeader();
				var exitHeader = (CusExitHeaderForTestingDefaultData)plugIn.ExitHeader;
				CombineAssertions("Invoked with true includeHeaderData", () =>
				{
					AssertEquals(1, exitHeader.DefaultingFromDeclarationLogs.Count);
					AssertEquals(true, exitHeader.DefaultingFromDeclarationLogs.Last());
				});

				plugIn.OnUserControlShown();
				AssertEquals("Not invoked immediately after CreateExitHeader", 1, exitHeader.DefaultingFromDeclarationLogs.Count);

				CombineAssertions("Invoked from the 2nd time with false includeHeaderData", () =>
				{
					plugIn.OnUserControlShown();
					AssertEquals(1, exitHeader.DefaultingFromDeclarationLogs.Count);

					plugIn.OnUserControlShown();
					AssertEquals(1, exitHeader.DefaultingFromDeclarationLogs.Count);
				});
			}
		}

		public void TestDefaultDataFromParent_Shipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var plugIn = new ExitControlPlugInForTestingDefaultData(shipment))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				plugIn.CreateExitHeader();
				var exitHeader = (CusExitHeaderForTestingDefaultData)plugIn.ExitHeader;
				CombineAssertions("Invoked with true includeHeaderData", () =>
				{
					AssertEquals(1, exitHeader.DefaultingFromShipmentLogs.Count);
					AssertEquals(true, exitHeader.DefaultingFromShipmentLogs.Last());
				});

				plugIn.OnUserControlShown();
				AssertEquals("Not invoked immediately after CreateExitHeader", 1, exitHeader.DefaultingFromShipmentLogs.Count);

				CombineAssertions("Invoked from the 2nd time with false includeHeaderData", () =>
				{
					plugIn.OnUserControlShown();
					AssertEquals(1, exitHeader.DefaultingFromShipmentLogs.Count);

					plugIn.OnUserControlShown();
					AssertEquals(1, exitHeader.DefaultingFromShipmentLogs.Count);
				});
			}
		}

		public void TestPluginEnabledOnlyForExport()
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Setup(d => d.ExitControlTabVisible).Returns(true);
			var declaration = declarationMock.Object;
			plugIn = GetExitControlPlugIn(declaration);

			CombineAssertions(() =>
			{
				using (ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>().EnableExitControlPlugin.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("Plugin enabled", true, plugInWrapper.EnabledExposed);

					declarationMock.Setup(d => d.ExitControlTabVisible).Returns(false);
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("Plugin disabled", false, plugInWrapper.EnabledExposed);
				}
			});
		}

		public void TestPluginEnabled_Shipment()
		{
			var localPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZQuery notLocal = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, localPort.Substring(0, 2));
			var foreignPort = (Factory.LoadTop1<RefUNLOCO>(notLocal)).RL_Code;
			ZQuery notLocal2 = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, localPort.Substring(0, 2));
			notLocal2.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, foreignPort.Substring(0, 2));
			var foreignPort2 = (Factory.LoadTop1<RefUNLOCO>(notLocal2)).RL_Code;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			plugIn = GetExitControlPlugIn(shipment);
			var registry = ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>();

			using (registry.EnableExitControlPlugin.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions(() =>
				{
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("Neither CrossTrade nor Export, PlugIn disabled", false, plugInWrapper.EnabledExposed);

					shipment.JS_RL_NKOrigin = foreignPort;
					shipment.JS_RL_NKDestination = foreignPort2;
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("PreReq: IsCrossTrade", true, shipment.IsCrossTrade());
					AssertEquals("CrossTrade, PlugIn enabled", true, plugInWrapper.EnabledExposed);

					shipment.JS_RL_NKOrigin = localPort;
					shipment.JS_RL_NKDestination = foreignPort;
					AssertEquals("Export", true, shipment.IsExport());
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("Export, PlugIn enabled", true, plugInWrapper.EnabledExposed);
				});
			}
		}

		public void TestPluginEnabled_Consol()
		{
			var localPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZQuery notLocal = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, localPort.Substring(0, 2));
			var foreignPort = (Factory.LoadTop1<RefUNLOCO>(notLocal)).RL_Code;
			ZQuery notLocal2 = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, localPort.Substring(0, 2));
			notLocal2.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, foreignPort.Substring(0, 2));
			var foreignPort2 = (Factory.LoadTop1<RefUNLOCO>(notLocal2)).RL_Code;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			plugIn = GetExitControlPlugIn(consol);
			var registry = ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>();

			using (registry.EnableExitControlPlugin.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions(() =>
				{
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("Neither CrossTrade nor Export, PlugIn disabled", false, plugInWrapper.EnabledExposed);

					consol.JK_RL_NKLoadPort = foreignPort;
					consol.JK_RL_NKDischargePort = foreignPort2;
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("PreReq: IsCrossTrade", true, consol.IsCrossTrade());
					AssertEquals("CrossTrade, PlugIn enabled", true, plugInWrapper.EnabledExposed);

					consol.JK_RL_NKLoadPort = localPort;
					consol.JK_RL_NKDischargePort = foreignPort;
					AssertEquals("Export", true, consol.IsExport());
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("Export, PlugIn enabled", true, plugInWrapper.EnabledExposed);
				});
			}
		}

		public void TestRegistryEnablesPlugin()
		{
			var declaration = Factory.New<JobDeclaration>();
			plugIn = GetExitControlPlugIn(declaration);
			var registry = ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				using (registry.EnableExitControlPlugin.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("Plugin enabled", true, plugInWrapper.EnabledExposed);
				}

				using (registry.EnableExitControlPlugin.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("Plugin disabled", false, plugInWrapper.EnabledExposed);
				}
			});
		}

		public void TestName()
		{
			var declaration = Factory.New<JobDeclaration>();
			plugIn = GetExitControlPlugIn(declaration);
			AssertEquals("Name", "EU Exit Control", plugIn.Name);
		}

		public void TestUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			plugIn = GetExitControlPlugIn(declaration);
			AssertNotNull("User Control", plugIn.UserControl);
			AssertEquals("Type", typeof(ExitControlUserControl), plugIn.UserControl.GetType());
		}

		public void TestOnSelectWhenUsersDontWantToCreateExitSummaryNow()
		{
			var declaration = Factory.New<JobDeclaration>();
			plugIn = GetExitControlPlugIn(declaration);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			plugInWrapper.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

			CombineAssertions(() =>
			{
				AssertNull("User don't want to create", plugIn.ExitHeader);
				AssertEquals("The Exit Declaration has not been created.", plugIn.PlugInNotDisplayedMessage);
			});
		}

		public void TestExitControlMenu()
		{
			var declaration = Factory.New<JobDeclaration>();
			plugIn = GetExitControlPlugIn(declaration);
			AssertType<ExitControlMenuItem>("Exit control Menu", plugIn.TopLevelMenu);

			plugIn.Enabled = false;
			plugIn.Enabled = true;
			AssertNull("Precondition: ExitHeader not created", plugIn.ExitHeader);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			plugIn.TopLevelMenu.PerformClick();

			AssertEquals("Are you sure that you want to create the Exit Declaration?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull("User don't want to create", plugIn.ExitHeader);
			AssertNull("ExitControlMenu.ExitHeader", (plugIn.TopLevelMenu as ExitControlMenuItem).ExitHeader);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			plugIn.TopLevelMenu.PerformClick();

			AssertEquals("Are you sure that you want to create the Exit Declaration?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotNull("Exit Summary created", plugIn.ExitHeader);

			var header = (plugIn.TopLevelMenu as ExitControlMenuItem).ExitHeader;
			AssertNotNull("ExitControlMenu.ExitHeader", (plugIn.TopLevelMenu as ExitControlMenuItem).ExitHeader);
			AssertEquals("CXH_ParentID", declaration.PK, header.CXH_ParentID);
			AssertEquals("CXH_ParentTableCode", "JE", header.CXH_ParentTableCode);
			AssertEquals("CXH_JobReference", declaration.JE_DeclarationReference, header.CXH_JobReference);
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_ParentID = declaration.PK;
			exitHeader.CXH_ParentTableCode = declaration.TablePrefix;
			Factory.Save();
			plugInWrapper = GetTestableExitSummaryPlugInWrapper(declaration);
			return plugInWrapper.PlugIn;
		}

		ExitControlPlugIn GetExitControlPlugIn(JobDeclaration declaration)
		{
			Factory.Save();
			plugInWrapper = GetTestableExitSummaryPlugInWrapper(declaration);
			return plugInWrapper.PlugIn;
		}

		ExitControlPlugIn GetExitControlPlugIn(ForwardingShipment shipment)
		{
			Factory.Save();
			plugInWrapper = GetTestableExitSummaryPlugInWrapper(shipment);
			return plugInWrapper.PlugIn;
		}

		ExitControlPlugIn GetExitControlPlugIn(ForwardingConsol consol)
		{
			Factory.Save();
			plugInWrapper = GetTestableExitSummaryPlugInWrapper(consol);
			return plugInWrapper.PlugIn;
		}

		ITestableExitControlPlugIn GetTestableExitSummaryPlugInWrapper(JobDeclaration declaration) => new TestExitControlPlugIn(declaration);

		ITestableExitControlPlugIn GetTestableExitSummaryPlugInWrapper(ForwardingShipment shipment) => new TestExitControlPlugIn(shipment);

		ITestableExitControlPlugIn GetTestableExitSummaryPlugInWrapper(ForwardingConsol consol) => new TestExitControlPlugIn(consol);

		protected override void TearDown()
		{
			base.TearDown();
			plugIn?.Dispose();
		}

		ITestableExitControlPlugIn plugInWrapper;
		ExitControlPlugIn plugIn;

		sealed class TestExitControlPlugIn : ExitControlPlugIn, ITestableExitControlPlugIn
		{
			public TestExitControlPlugIn(IBusiness hostEntity)
				: base(hostEntity)
			{
			}

			public bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed() => QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();

			public bool EnabledExposed => Enabled;

			ExitControlPlugIn ITestableExitControlPlugIn.PlugIn => this;

			public void ChangeTheVisibilityExposed() => ChangeTheVisibility();

			public void CreateExitHeaderIfRequiredExposed() => CreateExitHeaderIfRequired();
		}

		#region Default Declaration Data

		sealed class ExitControlPlugInForTestingDefaultData : ExitControlPlugIn
		{
			public ExitControlPlugInForTestingDefaultData(IBusiness hostBusinessEntity) : base(hostBusinessEntity)
			{
			}

			protected override CusExitHeader NewExitHeader(BusinessObject hostEntity)
			{
				return hostEntity.Factory.New<CusExitHeaderForTestingDefaultData>();
			}

			public void CreateExitHeader() => CreateExitHeaderIfRequired();
		}

		sealed class CusExitHeaderForTestingDefaultData : CusExitHeader
		{
			public CusExitHeaderForTestingDefaultData(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public IReadOnlyCollection<bool> DefaultingFromDeclarationLogs => defaultingFromDeclarationLogs;
			readonly List<bool> defaultingFromDeclarationLogs = new List<bool>();

			public IReadOnlyCollection<bool> DefaultingFromShipmentLogs => defaultingFromShipmentLogs;
			readonly List<bool> defaultingFromShipmentLogs = new List<bool>();

			protected override void DefaultHeaderDataFromDeclaration(JobDeclaration declaration)
			{
				base.DefaultHeaderDataFromDeclaration(declaration);
				defaultingFromDeclarationLogs.Add(true);
			}

			protected override void DefaultHeaderDataFromShipment(ForwardingShipment shipment)
			{
				base.DefaultHeaderDataFromShipment(shipment);
				defaultingFromShipmentLogs.Add(true);
			}
		}

		#endregion
	}

	public interface ITestableExitControlPlugIn
	{
		bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
		bool EnabledExposed { get; }
		void ChangeTheVisibilityExposed();
		ExitControlPlugIn PlugIn { get; }
	}
}
