using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(CustomsBrokerageUserControl))]
	sealed class CustomsBrokerageUserControlBaseOnlyTest : CustomsBrokerageUserControlAbstractTest<CustomsBrokerageUserControl>
	{
		public void TestIsEquipmentSupported()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Ireland))
			{
				using (var userControl = new CustomsBrokerageUserControl())
				{
					AssertNotNull("Supported for IE", userControl.TransportEquipmentTabPage);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				using (var userControl = new CustomsBrokerageUserControl())
				{
					AssertNull("Not supported for DE", userControl.TransportEquipmentTabPage);
				}
			}
		}

		public void TestSetTransportEquipmentTabVisibility_Onload()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.IsUCC6) + "Core", true))
			{
				using (var form = new ZForm(declaration))
				using (var userControl = new CustomsBrokerageUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();

					var tabPage = userControl.TransportEquipmentTabPage;
					AssertEquals("Visible when EquipmentsRequired is true", true, tabPage.TabRelevant);
				}
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.IsUCC6) + "Core", false))
			{
				using (var form = new ZForm(declaration))
				using (var userControl = new CustomsBrokerageUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();

					var tabPage = userControl.TransportEquipmentTabPage;
					AssertEquals("Invisible when EquipmentsRequired is false", false, tabPage.TabRelevant);
				}
			}
		}

		public void TestSetTransportEquipmentTabVisibility_MessageTypeChanged()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.IsUCC6) + "Core", true))
			{
				var tabPage = control.TransportEquipmentTabPage;
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals("Invisible when EquipmentsRequired is false", false, tabPage.TabRelevant);

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					AssertEquals("Visible when EquipmentsRequired is true", true, tabPage.TabRelevant);
				});
			}
		}

		public void TestTransportEquipmentTabPage_Index()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_ContainerMode = ContainerModes.Containerised;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.IsUCC6) + "Core", true))
			{
				using (var form = new ZForm(declaration))
				using (var userControl = new CustomsBrokerageUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();

					userControl.MainTabControl.SelectTab(userControl.TransportEquipmentTabPage);
					var equipmentTabPageIndex = userControl.MainTabControl.SelectedIndex;
					CombineAssertions(() =>
					{
						AssertSame("Next to and behind ContainerTabPage", userControl.ContainerTabPage, userControl.MainTabControl.TabPages[equipmentTabPageIndex - 1]);
						AssertSame("Next to and in front of PackingTabPage", userControl.PackingTabPage, userControl.MainTabControl.TabPages[equipmentTabPageIndex + 1]);
					});
				}
			}
		}

		public void TestEntryInstructionsTabVisibleForCountry()
		{
			AssertEquals(true, control.EntryInstructionsTabVisibleForCountry);
		}

		public void TestDV1DetailTabPage_TabRelevant_MessageType()
		{
			declaration.ZG_IsHighValueOvrd = true;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
			{
				var tabPage = control.DV1DetailsTabPage;
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals("Import", true, tabPage.TabRelevant);

					declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
					AssertEquals("Miscellaneous", false, tabPage.TabRelevant);
				});
			}
		}

		public void TestDV1DetailTabPage_TabRelevant_DV1Flag()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
			{
				var tabPage = control.DV1DetailsTabPage;
				CombineAssertions(() =>
				{
					declaration.ZG_IsHighValueOvrd = false;
					AssertEquals("False", false, tabPage.TabRelevant);

					declaration.ZG_IsHighValueOvrd = true;
					AssertEquals("True", true, tabPage.TabRelevant);
				});
			}
		}

		public void TestDV1DetailTabPage_TabRelevant_Default()
		{
			var tabPage = control.DV1DetailsTabPage;
			AssertEquals("False by default", false, tabPage.TabRelevant);
		}

		public void TestDV1DetailTabPage_TabRelevant_Enabled()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.ZG_IsHighValueOvrd = true;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
			{
				using (var form = new ZForm(declaration))
				using (var userControl = new CustomsBrokerageUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();

					var tabPage = userControl.DV1DetailsTabPage;
					AssertEquals("Visible", true, tabPage.TabRelevant);
				}
			}
		}

		public void TestDV1DetailTabPage_Index()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.ZG_IsHighValueOvrd = true;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
			{
				using (var form = new ZForm(declaration))
				using (var userControl = new CustomsBrokerageUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();

					var tabPage = userControl.DV1DetailsTabPage;
					tabPage.Select();
					userControl.MainTabControl.SelectTab(tabPage);
					AssertEquals("Index", 1, userControl.MainTabControl.SelectedIndex);
				}
			}
		}

		public void TestSetJobDeclaration_ValueNull()
		{
			using (var userControl = new CustomsBrokerageUserControl())
			{
				AssertNoExceptionThrown(() => userControl.JobDeclaration = null);
			}
		}

		public void TestMiscOptionsUserControl()
		{
			control.MainTabControl.SelectedTab = control.MiscOptionsTabPage;
			CombineAssertions(() =>
			{
				AssertNotNull("DynamicMiscOptions should loaded if applied DynamicMiscOptionsLayout", control.DynamicMiscOptions);
				AssertNull("MiscOptions should not loaded if applied DynamicMiscOptionsLayout", control.MiscOptions);
			});
		}

		public void TestSelectEntryInstructionTabPageTriggersSetTabPagesVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var userControl = new CustomsBrokerageUserControlForTest())
			{
				form.Controls.Add(userControl);
				form.Show();

				userControl.MainTabControl.SelectTab(userControl.EntryInstructionDetailsTabPage);
				AssertEquals("When selecting the Entry instruction tab page, SetTabPagesVisibilityHitCount", 1, userControl.EntryInstructionDetailsUserControlForTest.SetTabPagesVisibilityHitCount);

				userControl.MainTabControl.SelectTab(userControl.DeclarationTabPage);
				AssertEquals("When selecting another tab page, SetTabPagesVisibilityHitCount", 1, userControl.EntryInstructionDetailsUserControlForTest.SetTabPagesVisibilityHitCount);

				userControl.MainTabControl.SelectTab(userControl.EntryInstructionDetailsTabPage);
				AssertEquals("When selecting -again- the Entry instruction tab page, SetTabPagesVisibilityHitCount", 2, userControl.EntryInstructionDetailsUserControlForTest.SetTabPagesVisibilityHitCount);
			}
		}

		public void TestPlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel_Visibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			using (var form = new JobDeclarationForm(declaration))
			using (var userControl = new CustomsBrokerageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var placeButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel = form.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel;
				AssertEquals(false, placeButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible);

				userControl.MainTabControl.SelectTab(userControl.EntryInstructionDetailsTabPage);
				AssertEquals(false, placeButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible);

				userControl.MainTabControl.SelectTab(userControl.InvoiceLinesTabPage);
				AssertEquals(true, placeButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible);

				userControl.MainTabControl.SelectTab(userControl.PackingTabPage);
				AssertEquals(false, placeButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.Visible);
			}
		}

		public void TestPlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel_CanBeNull()
		{
			using var form = new JobDeclarationForm(declaration);
			using var userControl = new CustomsBrokerageUserControl();
			form.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel = null;

			form.Controls.Add(userControl);
			form.Show();
			AssertNoExceptionThrown(() => userControl.MainTabControl.SelectTab(userControl.InvoiceLinesTabPage));
		}

		public void TestISupportMultipleResourceStringDataSupporterMembers()
		{
			var multipleResourceStringDataSupporter = control as ISupportMultipleResourceStringDataSupporter;
			AssertNotNull("UserControl as ISupportMultipleResourceStringDataSupporter", multipleResourceStringDataSupporter);
			AssertSame("JobDeclaration and SupportMultipleResourceStringData Member", declaration, multipleResourceStringDataSupporter.SupportMultipleResourceStringData);
		}

		public void TestInvoiceGroupingTabPageVisibility()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals(true, control.InvoiceGroupingTabPage.TabRelevant);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals(false, control.InvoiceGroupingTabPage.TabRelevant);
		}

		protected override Type JobDeclarationUserControlType => typeof(EUJobDeclarationUserControl);

		protected override Type ImportSupplierHeaderUserControlType => typeof(EUImportSupplierHeaderUserControl);

		protected override Type ExportSupplierHeaderUserControlType => typeof(EUExportSupplierHeaderUserControl);

		protected override Type ImportInvoiceLineUserControlType => typeof(EUImportInvoiceLineUserControl);

		protected override Type ExportInvoiceLineUserControlType => typeof(EUExportInvoiceLineUserControl);

		protected override Type MessageUserControlType => typeof(EntryMessageUserControl);

		protected override Type EntryInstructionDetailsUserControlType => typeof(EntryInstructionDetailsUserControl);

		class CustomsBrokerageUserControlForTest : CustomsBrokerageUserControl
		{
			public CustomsBrokerageUserControlForTest()
			{
			}

			public EntryInstructionDetailsUserControlForTest EntryInstructionDetailsUserControlForTest => (EntryInstructionDetailsUserControlForTest)fBaseCustomsEntryInstructionUserControl;

			protected override Customs.GUI.BaseCustomsEntryUserControl GetEntryInstructionUserControl() => new EntryInstructionDetailsUserControlForTest();
		}

		class EntryInstructionDetailsUserControlForTest : EntryInstructionDetailsUserControl
		{
			public EntryInstructionDetailsUserControlForTest()
			{
			}

			public int SetTabPagesVisibilityHitCount { get; private set; }

			protected override void SetTabPagesVisibilityCore() => SetTabPagesVisibilityHitCount++;
		}
	}
}
