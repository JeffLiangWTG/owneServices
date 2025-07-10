using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5ArrivalMovementForm))]
	sealed class Phase5ArrivalMovementFormBaseOnlyTest : Phase5ArrivalMovementFormAbstractTest<NctsHeader>
	{
		public void TestMessagesUserControlType()
		{
			using (var form = new Phase5ArrivalMovementForm(header))
			{
				form.Show();
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectedTab = form.MessagesTabPage;
				var messagesTabDynamicUserControl = form.FindSingle<ZDynamicControlCreationUserControl>("MessagesTabDynamicUserControl");
				AssertType<MessagesTabUserControl>(messagesTabDynamicUserControl.HostedControl);
				AssertEquals("Messages", messagesTabDynamicUserControl.BindingSource.DataMember);
			}
		}

		public void TestMinimumSize()
		{
			using (var form = new Phase5ArrivalMovementForm(header))
			{
				AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1366, 725, true), form.MinimumSize);
			}
		}

		public void TestFormCaption()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				AssertContains("NCTS Transit Movement", form.Text);
			}
		}

		[RequiresSTA]
		public void TestMainTabPage()
		{
			using (var form = new Phase5ArrivalMovementForm(header))
			{
				form.Show();
				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				var mainTabPage = mainTabControl.GetTabPage("MainTabPage");
				AssertEquals("MainTabPage.AutoScroll", true, mainTabPage.AutoScroll);
				AssertEquals("MainTabPage.Caption", "Arrival Notification", mainTabPage.CaptionResourceString.Caption);

				var arrivalNotificationTabUserControl = form.ArrivalNotificationTabUserControl;
				AssertEquals("ArrivalNotificationTabUserControl is within MainTabPage", true, mainTabPage.Contains(arrivalNotificationTabUserControl));
				AssertEquals("ArrivalNotificationTabUserControl.Size", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1342, 603, true), arrivalNotificationTabUserControl.Size);
			}
		}

		public void TestIncidentsTabPage()
		{
			header.BH_ExportFlag = YesNoList.Codes.Yes;
			using (var form = new Phase5ArrivalMovementFormForTesting(header))
			{
				form.Show();
				var incidentsTabPage = form.IncidentsTabPage;
				AssertEquals("IncidentsTabPage.Caption", "Incidents", incidentsTabPage.CaptionResourceString.Caption);
			}
		}

		[RequiresSTA]
		public void TestIncidentsTabPage_Invisible()
		{
			header.BH_ExportFlag = YesNoList.Codes.No;
			using (var form = new Phase5ArrivalMovementFormForTesting(header))
			{
				form.Show();
				AssertEquals("TabVisible", false, form.IncidentsTabPage.TabVisible);
			}
		}

		[GuiTest]
		public void TestCustomFieldsControlTabControl_CustomFieldTab()
		{
			using (var form = new Phase5ArrivalMovementForm(header))
			{
				form.Show();

				var customFieldControl = form.CustomFieldTabPage;
				AssertNotNull(customFieldControl);
				Assert(customFieldControl.TabVisible);
			}
		}

		[RequiresSTA]
		public void TestMessagesTabPage()
		{
			using (var form = new Phase5ArrivalMovementForm(header))
			{
				AssertEquals("Caption", "Messages", form.MessagesTabPage.CaptionResourceString.Caption);
			}
		}

		public void TestUnloadingRemarksTabPage()
		{
			header.BH_ExportFlag = YesNoList.Codes.Yes;
			foreach (var status in ExpectedUnloadingAllowedOrCompleteStatusList)
			{
				header.ArrivalMovementHeader.BM_CustomsStatus = status;
				using (var form = new Phase5ArrivalMovementForm(header))
				{
					form.Show();
					var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
					var unloadingRemarksTabPage = form.UnloadingRemarksTabPage;
					CombineAssertions($"CustomsStatus: {status}", () =>
					{
						AssertEquals("UnloadingRemarksTabPage.Caption", "Unloading Remarks", unloadingRemarksTabPage.CaptionResourceString.Caption);
						AssertSame("SelectedTab", unloadingRemarksTabPage, mainTabControl.SelectedTab);

						var unloadingRemarksTabUserControl = form.UnloadingRemarksTabUserControl;
						AssertEquals("UnloadingRemarksTabUserControl is within UnloadingRemarksTabPage", true, unloadingRemarksTabPage.Contains(unloadingRemarksTabUserControl));
						AssertEquals("UnloadingRemarksTabUserControl.Dock", DockStyle.Fill, unloadingRemarksTabUserControl.Dock);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestUnloadingRemarksTabPageReadOnly()
		{
			var headerForTest = Factory.New<NctsHeaderForTest>();
			headerForTest.SetMovementType(NctsMovementType.Codes.Arrival);
			headerForTest.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			headerForTest.IsUnloadingAllowedOrCompleteForTest = true;
			headerForTest.IsUnloadingRemarksTabReadOnlyForTest = false;
			headerForTest.ArrivalMovementHeader.BM_NoChangesToReport = false;

			using (var form = new Phase5ArrivalMovementForm(headerForTest))
			{
				form.Show();
				var unloadingRemarksTabPage = form.UnloadingRemarksTabPage;
				var transportInfoGrid = unloadingRemarksTabPage.FindSingle<ZGrid>("TransportInfoGrid");
				CombineAssertions(() =>
				{
					AssertEquals("Unloading remarks tab children are not readonly", false, transportInfoGrid.ReadOnly);

					headerForTest.IsUnloadingRemarksTabReadOnlyForTest = true;
					headerForTest.ArrivalMovementHeader.BM_CustomsStatusInfo.RefreshBinding();
					AssertEquals("Unloading remarks tab children are readonly", true, transportInfoGrid.ReadOnly);
				});
			}
		}

		[RequiresSTA]
		public void TestUnloadingRemarksTabPage_Invisible()
		{
			var statusCodeList = new NctsTransitStatusList().GetAllCodes();
			CombineAssertions(() =>
			{
				foreach (var status in statusCodeList)
				{
					if (!ExpectedUnloadingAllowedOrCompleteStatusList.Contains(status))
					{
						header.ArrivalMovementHeader.BM_CustomsStatus = status;
						using (var form = new Phase5ArrivalMovementForm(header))
						{
							form.Show();
							AssertEquals($"TabVisible status {status}", false, form.UnloadingRemarksTabPage.TabVisible);
						}
					}
				}
			});
		}

		public void TestTabPagesOrder()
		{
			header.BH_ExportFlag = YesNoList.Codes.Yes;
			foreach (var status in ExpectedUnloadingAllowedOrCompleteStatusList)
			{
				header.ArrivalMovementHeader.BM_CustomsStatus = status;
				using (var form = new Phase5ArrivalMovementForm(header))
				{
					form.Show();

					var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
					AssertSequencesEqual($"TabPageNames status {status}", ExpectedTabPageNamesInOrder, mainTabControl.TabPages.Cast<ZTabPage>().Select(x => x.Name));
				}
			}
		}

		public static IEnumerable<string> ExpectedTabPageNamesInOrder = new[]
		{
			"MainTabPage", "IncidentsTabPage", "UnloadingRemarksTabPage", "MessagesTabPage", "CustomFieldsTabPage", "WorkflowTabPage", "BillingTabPage", "eDocsTabPage", "NotesTabPage", "LogsTabPage"
		};

		public void TestDefaultTabPage()
		{
			using (var form = new Phase5ArrivalMovementForm(header))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				AssertEquals("When no customs status is set, Arrival notification tab should be selected", form.FindSingle<ZTabPage>("MainTabPage"), mainTabControl.SelectedTab);
			}
		}

		public void TestDefaultTabPage_Status()
		{
			CombineAssertions(() =>
			{
				foreach (var status in ExpectedUnloadingAllowedOrCompleteStatusList)
				{
					header.ArrivalMovementHeader.BM_CustomsStatus = status;
					using (var form = new Phase5ArrivalMovementForm(header))
					{
						form.Show();

						var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");

						AssertEquals("When customs status is " + status + ", Property nctsHeader.IsUnloadingRemarksTabVisible should be true", true, header.IsUnloadingRemarksTabVisible);
						AssertEquals("When customs status is " + status + ", Unloading remarks tab should be selected", form.UnloadingRemarksTabPage.Name, mainTabControl.SelectedTab.Name);
					}
				}
			});
		}

		public void TestMenuItems()
		{
			using (var form = new Phase5ArrivalMovementForm(header))
			{
				form.Show();

				var menuItems = form.Menu.MenuItems;
				AssertSequencesEqual(new[] { "&File", "&Edit", "View", "Actio&ns", "&NCTS", "&Job Invoicing", "&Help" }, menuItems.Cast<MenuItem>().Select(x => x.Text));
			}
		}

		public void TestInitializeAdditionalArrivalTabs()
		{
			var nctsPhase5LayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new NctsPhase5LayoutProviderForAdditionalArrivalTabsTest()) }
			};

			using (ObjectFactory.Substitute("NctsPhase5LayoutProviders", nctsPhase5LayoutProviders))
			using (var form = new Phase5ArrivalMovementFormForTesting(header))
			{
				var mainTabControl = form.MainTabControl;
				CombineAssertions(() =>
				{
					AssertEquals("There are 2 additional tabs", 2, mainTabControl.AllTabPages.Count(t => t.Name.StartsWith("AdditionalTabPageForTest")));

					Phase5LayoutTestHelper.AssertAdditionalTabPage<AdditionalTabPageForTest1UserControl>(mainTabControl, nameof(AdditionalTabPageForTest1), 1);
					Phase5LayoutTestHelper.AssertAdditionalTabPage<AdditionalTabPageForTest2UserControl>(mainTabControl, nameof(AdditionalTabPageForTest2), 2);
				});
			}
		}

		public void TestInitializeReorderTabs()
		{
			var nctsPhase5LayoutProviders = new KeyObjectHandleDictionaryObject
			{
				{ "Default", new TestObjectHandle(new NctsPhase5LayoutProviderForReorderTabsTest()) }
			};

			header.BH_ExportFlag = YesNoList.Codes.Yes;

			using (ObjectFactory.Substitute("NctsPhase5LayoutProviders", nctsPhase5LayoutProviders))
			using (var form = new Phase5ArrivalMovementFormForTesting(header))
			{
				CombineAssertions(() =>
				{
					var mainTabControl = form.MainTabControl;
					AssertEquals("TabPageNames", "AdditionalTabPageForTest1, MainTabPage, IncidentsTabPage, UnloadingRemarksTabPage, CustomFieldsTabPage, WorkflowTabPage, AdditionalTabPageForTest2, NotesTabPage, LogsTabPage", string.Join(", ", mainTabControl.AllTabPages.Cast<ZTabPage>().Select(x => x.Name)));
				});
			}
		}

		public void TestDeleteIncidentsDialogBox()
		{
			CombineAssertions(() =>
			{
				header.BH_ExportFlag = YesNoList.Codes.Yes;
				using (var form = new Phase5ArrivalMovementForm(header))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					header.BH_ExportFlag = YesNoList.Codes.No;
					AssertEquals("User prompt should not have a question yet as there was no incident details to delete", false, UnitTestUserNotification.Instance.LastMessage.WasQuestion);

					header.BH_ExportFlag = YesNoList.Codes.Yes;
					var incident = header.EnRouteIncidents.AddNew();
					incident.BN_EndorsementDate = new CargoWise.Types.ZDateTime(2023, 10, 4);
					header.BH_ExportFlag = YesNoList.Codes.No;
					AssertEquals("User prompt has a question", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("Prompt text", "Incident details will be deleted. Are you sure you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			});
		}

		public void TestCanDisplayMessageBuilderMappingPath()
		{
			using var form = new Phase5ArrivalMovementForm(header);
			var devToolMessageBuilderPathConfigurator = form as IDevToolMessageBuilderMappingPathConfigurator;

			AssertNotNull("Phase5ArrivalMovementForm as Message Builder Mapping Path Configurator", devToolMessageBuilderPathConfigurator);
			AssertEquals("CanDisplayMessageBuilderMappingPath", expected: true, devToolMessageBuilderPathConfigurator.CanDisplayMessageBuilderMappingPath);
		}

		protected override void PerformExtraNctsHeaderConfiguration(NctsHeader header)
		{
			base.PerformExtraNctsHeaderConfiguration(header);
			header.ArrivalMovementHeader.BM_CustomsStatus = ExpectedUnloadingAllowedOrCompleteStatusList[0];
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		string[] ExpectedUnloadingAllowedOrCompleteStatusList => new string[] { "UAP", "ULR", "CL1", "CL3", "CD2", "CD4" };

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		}
		NctsHeader header;

		sealed class Phase5ArrivalMovementFormForTesting : Phase5ArrivalMovementForm
		{
			internal Phase5ArrivalMovementFormForTesting(NctsHeader nctsHeader) : base(nctsHeader) { }

			internal new ZTabControl MainTabControl => base.MainTabControl;
			internal new ZTabPage IncidentsTabPage => base.IncidentsTabPage;
		}

		sealed class NctsPhase5LayoutProviderForAdditionalArrivalTabsTest : NctsPhase5LayoutProvider, INctsPhase5LayoutProvider
		{
			IEnumerable<ITabPage> INctsPhase5LayoutProvider.AdditionalArrivalTabPages
			{
				get
				{
					yield return new AdditionalTabPageForTest1();
					yield return new AdditionalTabPageForTest2();
				}
			}
		}

		sealed class NctsPhase5LayoutProviderForReorderTabsTest : NctsPhase5LayoutProvider, INctsPhase5LayoutProvider
		{
			IEnumerable<ITabPage> INctsPhase5LayoutProvider.AdditionalArrivalTabPages
			{
				get
				{
					yield return new AdditionalTabPageForTest1();
					yield return new AdditionalTabPageForTest2();
				}
			}

			IEnumerable<string> INctsPhase5LayoutProvider.ReorderArrivalTabPagesNames(IEnumerable<string> defaultTabPageNames)
			{
				return new[] { "AdditionalTabPageForTest1" }.Union(defaultTabPageNames.Except(new[] { "MessagesTabPage" }));
			}
		}

		sealed class NctsHeaderForTest : NctsHeader
		{
			public NctsHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool IsUnloadingAllowedOrCompleteCore => IsUnloadingAllowedOrCompleteForTest;

			public bool IsUnloadingAllowedOrCompleteForTest { get; set; }

			protected override bool IsUnloadingRemarksTabReadOnlyCore => IsUnloadingRemarksTabReadOnlyForTest;

			public bool IsUnloadingRemarksTabReadOnlyForTest { get; set; }
		}
	}
}
