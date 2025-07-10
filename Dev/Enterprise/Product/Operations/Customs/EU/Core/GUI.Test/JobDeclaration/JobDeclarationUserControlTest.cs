using System.Collections;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(EUJobDeclarationUserControl))]
	class JobDeclarationUserControlTest : BaseCustomsDeclarationUserControlAbstractTest<EUJobDeclarationUserControl, JobDeclaration>
	{
		public void TestExportDeclarationNumberBoundTextBoxCaptionResourceString()
		{
			using (var control = new EUJobDeclarationUserControl())
			{
				Assert("We do not force the CaptionResourceString of this control", control.ExportDeclarationNumberBoundTextBox.CaptionResourceString.IsEmpty());
			}
		}

		public void TestIncoTermDropEditCaptionResourceString()
		{
			using (var control = new EUJobDeclarationUserControl())
			{
				Assert("We do not force the CaptionResourceString of this control", control.IncoTermDropEdit.CaptionResourceString.IsEmpty());
			}
		}

		public void TestImporterDocAddressType()
		{
			using (var userControl = new EUJobDeclarationUserControl())
			{
				AssertType<MasterFiles.GUI.ZDocAddressControl>(userControl.ImporterDocAddress);
			}
		}

		public void TestSupplierDocAddressType()
		{
			using (var userControl = new EUJobDeclarationUserControl())
			{
				AssertType<MasterFiles.GUI.ZDocAddressControl>(userControl.SupplierDocAddress);
			}
		}

		public void TestWhenUnwantedControlsAreNotVisibleForDifferentJobMessageTypes()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			declaration.AircraftRegistrationNumberVisibleExposed = false;
			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("userControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible", false, userControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
				AssertEquals("userControl.JE_ContainerCountCalcEdit.Visible", false, userControl.JE_ContainerCountCalcEdit.Visible);
				AssertEquals("userControl.DepartureTransportIDTextBox.Visible", true, userControl.DepartureTransportIDTextBox.Visible);
				AssertEquals("ContainerModeBoundDropDownEdit always visible", true, userControl.ContainerModeBoundDropDownEdit.Visible);
				AssertEquals("BadgeCodeDropEdit never visible", false, userControl.BadgeCodeDropEdit.Visible);
				AssertEquals("AircraftRegistrationNumberTextBox never visible", false, userControl.AircraftRegistrationNumberTextBox.Visible);
				AssertEquals("JE_IATALoadPortCodeFindBox never visible", false, userControl.JE_IATALoadPortCodeFindBox.Visible);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("userControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible", false, userControl.JE_TotalNoOfPiecesBoundCalcEdit.Visible);
				AssertEquals("userControl.JE_ContainerCountCalcEdit.Visible", false, userControl.JE_ContainerCountCalcEdit.Visible);
				AssertEquals("userControl.DepartureTransportIDTextBox.Visible", true, userControl.DepartureTransportIDTextBox.Visible);
				AssertEquals("ContainerModeBoundDropDownEdit always visible", true, userControl.ContainerModeBoundDropDownEdit.Visible);
				AssertEquals("BadgeCodeDropEdit never visible", false, userControl.BadgeCodeDropEdit.Visible);
				AssertEquals("AircraftRegistrationNumberTextBox never visible", false, userControl.AircraftRegistrationNumberTextBox.Visible);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("userControl.DepartureTransportIDTextBox.Visible", false, userControl.DepartureTransportIDTextBox.Visible);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("userControl.DepartureTransportIDTextBox.Visible", true, userControl.DepartureTransportIDTextBox.Visible);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("userControl.DepartureTransportIDTextBox.Visible", false, userControl.DepartureTransportIDTextBox.Visible);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				AssertEquals("userControl.DepartureTransportIDTextBox.Visible", true, userControl.DepartureTransportIDTextBox.Visible);
				AssertEquals("ContainerModeBoundDropDownEdit always visible", true, userControl.ContainerModeBoundDropDownEdit.Visible);

				declaration.AircraftRegistrationNumberVisibleExposed = true;
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("AircraftRegistrationNumberTextBox visible", true, userControl.AircraftRegistrationNumberTextBox.Visible);
			}
		}

		public void TestControlsNotRequiredForRoadAndRailAreNeverVisible()
		{
			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Road;
				AssertEquals("userControl.JE_VoyageFlightNoBoundTextBox.Visible", false, userControl.JE_VoyageFlightNoBoundTextBox.Visible);

				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Rail;
				AssertEquals("userControl.JE_VoyageFlightNoBoundTextBox.Visible", false, userControl.JE_VoyageFlightNoBoundTextBox.Visible);
			}
		}

		public void TestExportUCC6ControlsCaptions()
		{
			declaration.JE_MessageType = ZString.Empty;
			using (ObjectFactory.Substitute("DeclarationFormLayoutProviders", new Hashtable()))
			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("userControl.FinalDestinationFindBox.Caption - Export UCC6", "Destination", userControl.FinalDestinationFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("userControl.OriginFindBox.Caption - Export UCC6", "Dispatch", userControl.OriginFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertNotEquals("userControl.FinalDestinationFindBox.Caption - Import UCC6", "Destination", userControl.FinalDestinationFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
					AssertNotEquals("userControl.OriginFindBox.Caption - Import UCC6", "Dispatch", userControl.OriginFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				}
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertNotEquals("userControl.FinalDestinationFindBox.Caption - Export Non-UCC6", "Destination", userControl.FinalDestinationFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
					AssertNotEquals("userControl.OriginFindBox.Caption - Export Non-UCC6", "Dispatch", userControl.OriginFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				}
			}
		}

		public void TestCustomsWarehouseTitle()
		{
			using (var control = new EUJobDeclarationUserControl())
			{
				AssertEquals("[49] Customs Warehouse", control.BondedWarehouseDocAddressControl.CaptionResourceString.Caption);
			}
		}

		public void TestBondedWarehouseVisibility()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				Application.DoEvents();
				userControl.RightTabControl.SelectedTab = userControl.OrganisationsTabPage;
				Application.DoEvents();

				declaration.AreMultipleEntryInstructionsAllowedExposed = false;
				CombineAssertions("BondedWarehouseDocAddressControl visible", () =>
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals(true, userControl.BondedWarehouseDocAddressControl.Visible);

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					AssertEquals(true, userControl.BondedWarehouseDocAddressControl.Visible);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					AssertEquals(false, userControl.BondedWarehouseDocAddressControl.Visible);
				});

				declaration.AreMultipleEntryInstructionsAllowedExposed = true;
				CombineAssertions("BondedWarehouseDocAddressControl visible", () =>
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals(false, userControl.BondedWarehouseDocAddressControl.Visible);

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					AssertEquals(false, userControl.BondedWarehouseDocAddressControl.Visible);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					AssertEquals(false, userControl.BondedWarehouseDocAddressControl.Visible);
				});
			}
		}

		public void TestOriganizationUserControlVisibility_Import()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.RightTabControl.SelectedTab = userControl.OrganisationsTabPage;

				CombineAssertions(() =>
				{
					AssertEquals("Import User Control", true, userControl.OrganizationImportUserControl.Visible);
					AssertEquals("Export User Control", false, userControl.OrganizationExportUserControl.Visible);
				});
			}
		}

		public void TestOriganizationUserControlVisibility_Export()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.RightTabControl.SelectedTab = userControl.OrganisationsTabPage;

				CombineAssertions(() =>
				{
					AssertEquals("Import User Control", false, userControl.OrganizationImportUserControl.Visible);
					AssertEquals("Export User Control", true, userControl.OrganizationExportUserControl.Visible);
				});
			}
		}

		public void TestBox18Visibility()
		{
			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				AssertEquals("Box18TransportIDTextBox should be shown", true, userControl.Box18TransportIDTextBox.Visible);
				AssertEquals("Box18TrNationalityFindBox should be shown", true, userControl.Box18TrNationalityFindBox.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				AssertEquals("Box18TransportIDTextBox should be shown", true, userControl.Box18TransportIDTextBox.Visible);
				AssertEquals("Box18TrNationalityFindBox should be shown", true, userControl.Box18TrNationalityFindBox.Visible);

				declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
				AssertEquals("Box18TransportIDTextBox should be shown", true, userControl.Box18TransportIDTextBox.Visible);
				AssertEquals("Box18TrNationalityFindBox should be shown", true, userControl.Box18TrNationalityFindBox.Visible);
			}
		}

		public void TestAircraftRegNumberCaption()
		{
			using (var userControl = new EUJobDeclarationUserControl())
			{
				AssertEquals("Aircraft Registration Number", userControl.AircraftRegistrationNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestGoodsDestinationVisibility()
		{
			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				var goodsDestinationDropEdit = (ZDropEdit)(form.Controls.Find("GoodsDestinationDropEdit", true).Single());
				form.Show();

				AssertEquals("GoodsDestinationDropEdit should always be shown", true, goodsDestinationDropEdit.Visible);
			}
		}

		public void TestGoodsOriginVisibility()
		{
			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				var goodsOriginDropEdit = (ZDropEdit)(form.Controls.Find("GoodsOriginDropEdit", true).Single());
				form.Show();

				AssertEquals("GoodsDestinationDropEdit should always be shown", true, goodsOriginDropEdit.Visible);
			}
		}

		public void TestControlsNotRequiredAreNeverVisible()
		{
			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Show();

				AssertUnwantedControlsAreNotVisible(userControl);

				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
				AssertEquals("Precondition: declaration.IsAir", true, declaration.IsAir);
				AssertUnwantedControlsAreNotVisible(userControl);

				declaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
				AssertEquals("Precondition: declaration.IsSea", true, declaration.IsSea);
				AssertUnwantedControlsAreNotVisible(userControl);
			}
		}

		public void TestIsHighValueOvrdCheckBoxVisibility_Disabled()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", false))
			{
				using (var form = new ZForm(declaration))
				using (var userControl = new EUJobDeclarationUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals("IsHighValueOvrdCheckBox not Visible by default", false, userControl.IsHighValueOvrdCheckBox.Visible);
				}
			}
		}

		public void TestIsHighValueOvrdCheckBoxVisibility_Enabled()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
			{
				using (var form = new ZForm(declaration))
				using (var userControl = new EUJobDeclarationUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();

					declaration.JE_MessageType = MessageTypeList.Codes.Import;

					AssertEquals("IsHighValueOvrdCheckBox Visible, if Import", true, userControl.IsHighValueOvrdCheckBox.Visible);

					declaration.JE_MessageType = MessageTypeList.Codes.Export;

					AssertEquals("IsHighValueOvrdCheckBox not Visible, if Export", false, userControl.IsHighValueOvrdCheckBox.Visible);
				}
			}
		}

		public void TestInlandTransportCodeDropEdit()
		{
			using (var userControl = new EUJobDeclarationUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("BindTo", "JE_TransportMeans", userControl.InlandTransportCodeDropEdit.BindTo);
					AssertEquals("Caption", "[18] Code", userControl.InlandTransportCodeDropEdit.CaptionResourceString.Caption);
					AssertEquals("FullDescription", "[18] Inland Transport Code", userControl.InlandTransportCodeDropEdit.CaptionResourceString.FullDescription);
					AssertEquals("PreBoundMaxLength", 2, userControl.InlandTransportCodeDropEdit.PreBoundMaxLength);
					AssertEquals("ShowDescriptionBox", false, userControl.InlandTransportCodeDropEdit.ShowDescriptionBox);
				});
			}
		}

		public void TestInlandTransportCodeDropEditVisibilityWhenUCC6IsActive()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("InlandTransportCodeDropEdit visibility if import", true, userControl.InlandTransportCodeDropEdit.Visible);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("InlandTransportCodeDropEdit visibility if not import", false, userControl.InlandTransportCodeDropEdit.Visible);
			}
		}

		public void TestInlandTransportCodeDropEditVisibilityWhenUCC6IsNotActive()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
			using (var form = new ZForm(declaration))
			using (var userControl = new EUJobDeclarationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("InlandTransportCodeDropEdit visibility if import", false, userControl.InlandTransportCodeDropEdit.Visible);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("InlandTransportCodeDropEdit visibility if not import", false, userControl.InlandTransportCodeDropEdit.Visible);
			}
		}

		void AssertUnwantedControlsAreNotVisible(EUJobDeclarationUserControl userControl)
		{
			AssertEquals("userControl.FolioNumberTextBox.Visible", false, userControl.FolioNumberTextBox.Visible);
			AssertEquals("userControl.JE_ContainerCountCalcEdit.Visible", false, userControl.JE_ContainerCountCalcEdit.Visible);
			AssertEquals("userControl.RegionOfDestinationDropEdit.Visible", false, userControl.RegionOfDestinationDropEdit.Visible);
			AssertNotContains("userControl.ConsolVoyageLabel", "Folio", userControl.JE_VoyageFlightNoBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
		}

		JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
	}

	[PropertyDescriptorCollection(typeof(AddInfoPropertyDescriptorCollection))]
	public class JobDeclarationForTest : JobDeclaration
	{
		public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZBool AircraftRegistrationNumberVisible => AircraftRegistrationNumberVisibleExposed;

		public ZBool AircraftRegistrationNumberVisibleExposed { get; set; }

		public override ZBool AreMultipleEntryInstructionsAllowed => AreMultipleEntryInstructionsAllowedExposed;

		public ZBool AreMultipleEntryInstructionsAllowedExposed { get; set; }
	}
}
