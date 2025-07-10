using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDetailsUserControlType()
		{
			AssertUserControlType(nameof(EntryInstructionDetailsUserControl.DetailsUserControl), typeof(EntryInstructionDetailBasicUserControl));
		}

		public void TestFiscalReferencesUserControlType()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionFiscalReferencesSupportConfiguration(declaration, true))
			{
				AssertUserControlType(nameof(EntryInstructionDetailsUserControl.FiscalReferencesUserControl), typeof(EntryInstructionFiscalReferencesUserControl));
			}
		}

		public void TestAuthorisationsUserControlType()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAuthorisationsSupportConfiguration(declaration, true))
			{
				AssertUserControlType(nameof(EntryInstructionDetailsUserControl.AuthorisationsUserControl), typeof(EntryInstructionAuthorisationsUserControl));
			}
		}

		public void TestGridUserControlType()
		{
			AssertUserControlType(nameof(EntryInstructionDetailsUserControl.EntryInstructionGridUserControl), typeof(EntryInstructionGridUserControl));
		}

		public void TestGuaranteesUserControlType()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionGuaranteesSupportConfiguration(declaration, true).disposable)
			{
				AssertUserControlType("GuaranteesUserControl", typeof(EntryInstructionGuaranteesUserControl));
			}
		}

		public void TestFiscalReferencesTabPage_Invisible()
		{
			AssertTabPageVisibility(false, nameof(EntryInstructionDetailsUserControl.FiscalReferencesTabPage));
		}

		public void TestFiscalReferencesTabPage_Visible()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionFiscalReferencesSupportConfiguration(declaration, true))
			{
				AssertTabPageVisibility(true, nameof(EntryInstructionDetailsUserControl.FiscalReferencesTabPage));
			}
		}

		public void TestAuthorisationsTabPage_Invisible()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAuthorisationsSupportConfiguration(declaration, false))
			{
				AssertTabPageVisibility(false, nameof(EntryInstructionDetailsUserControl.AuthorisationsTabPage));
			}
		}

		[RequiresSTA]
		public void TestAuthorisationsTabPage_Visible()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAuthorisationsSupportConfiguration(declaration, true))
			{
				AssertTabPageVisibility(true, nameof(EntryInstructionDetailsUserControl.AuthorisationsTabPage));
			}
		}

		public void TestSupplyChainActorTabPage_Caption()
		{
			using (var form = new ZForm())
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var tabPage = control.FindSingle<ZTabPage>("SupplyChainActorTabPage");
				AssertEquals("SupplyChainActorTabPage.Caption", "Add. Supply Chain Actors", tabPage.CaptionResourceString.Caption);
			}
		}

		public void TestAuthorisationsTabpage_Caption()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			using (var form = new ZForm())
			using (var control = new EntryInstructionDetailsUserControl())
			{
				declaration.JE_MessageType = "EXP";
				form.SetDataBinding(declaration, null);
				form.Controls.Add(control);
				form.Show();

				var tabPage = control.FindSingle<ZTabPage>("AuthorisationsTabPage");
				AssertEquals("AuthorisationsTabPage.Caption for UCC6 Export", "Authorizations", tabPage.CaptionResourceString.Caption);

				declaration.JE_MessageType = "IMP";
				AssertEquals("AuthorisationsTabPage.Caption for UCC6 Import", "[3/39] Authorizations", tabPage.CaptionResourceString.Caption);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true))
			using (var form = new ZForm())
			using (var control = new EntryInstructionDetailsUserControl())
			{
				declaration.JE_MessageType = "EXP";
				form.SetDataBinding(declaration, null);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(true, declaration.IsUCC5);
				var tabPage = control.FindSingle<ZTabPage>("AuthorisationsTabPage");
				AssertEquals("AuthorisationsTabPage.Caption for UCC5", "[UCC 3/39] Authorizations", tabPage.CaptionResourceString.Caption);
			}
		}

		public void TestDV1DetailsTabPage_Visible_Default()
		{
			AssertTabPageVisibility(false, nameof(EntryInstructionDetailsUserControl.DV1DetailsTabPage));

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.ZG_IsHighValueOvrd = true;

			AssertTabPageVisibility(false, nameof(EntryInstructionDetailsUserControl.DV1DetailsTabPage));
		}

		public void TestDV1DetailsTabPage_Visible_Enabled()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.ZG_IsHighValueOvrd = true;
				AssertTabPageVisibility(true, nameof(EntryInstructionDetailsUserControl.DV1DetailsTabPage));

				declaration.ZG_IsHighValueOvrd = false;
				AssertTabPageVisibility(false, nameof(EntryInstructionDetailsUserControl.DV1DetailsTabPage));
			}
		}

		public void TestDV1DetailsTabPage_Visible_DV1DetailFlag()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.ZG_IsHighValueOvrd = true;
				AssertTabPageVisibility(true, nameof(EntryInstructionDetailsUserControl.DV1DetailsTabPage));

				declaration.ZG_IsHighValueOvrd = false;
				AssertTabPageVisibility(false, nameof(EntryInstructionDetailsUserControl.DV1DetailsTabPage));
			}
		}

		public void TestDV1DetailsTabPage_Visible_MessageType()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.ZG_IsHighValueOvrd = true;
				AssertTabPageVisibility(true, nameof(EntryInstructionDetailsUserControl.DV1DetailsTabPage));

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertTabPageVisibility(false, nameof(EntryInstructionDetailsUserControl.DV1DetailsTabPage));
			}
		}

		public void TestAdditionalSupplyChainActorTabPage_Invisible()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalSupplyChainActorSupportConfiguration(declaration, false))
			{
				AssertTabPageVisibility(false, nameof(EntryInstructionDetailsUserControl.SupplyChainActorTabPage));
			}
		}

		public void TestAdditionalSupplyChainActorTabPage_Visible()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalSupplyChainActorSupportConfiguration(declaration, true))
			{
				AssertTabPageVisibility(true, nameof(EntryInstructionDetailsUserControl.SupplyChainActorTabPage));
			}
		}

		public void TestAdditionalSupplyChainActorTabPage_Caption()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			using (var form = new ZForm())
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.SetDataBinding(declaration, null);
				form.Controls.Add(control);
				form.Show();

				var tabPage = control.FindSingle<ZTabPage>("SupplyChainActorTabPage");
				AssertEquals("SupplyChainActorTabPage.Caption", "Add. Supply Chain Actors", tabPage.CaptionResourceString.Caption);
			}
		}

		public void TestSealsUserControlType()
		{
			AssertUserControlType(nameof(EntryInstructionDetailsUserControl.SealsUserControl), typeof(EntryInstructionExportSealsUserControl));
		}

		public void TestSealsTabPage_Invisible()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSealsSupportConfiguration(declaration, false))
			{
				AssertTabPageVisibility(false, nameof(EntryInstructionDetailsUserControl.SealsTabPage));
			}
		}

		public void TestSealsTabPage_Visible()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSealsSupportConfiguration(declaration, true))
			{
				AssertTabPageVisibility(true, nameof(EntryInstructionDetailsUserControl.SealsTabPage));
			}
		}

		public void TestSupportingDocumentsControlType()
		{
			using (var form = new ZForm())
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var tabPage = control.FindSingle<ZTabPage>(nameof(EntryInstructionDetailsUserControl.SupportingDocumentsTabPage));
				tabPage.Show();

				var foundUserControl = control.FindSingle<ZDynamicControlCreationUserControl>(nameof(EntryInstructionDetailsUserControl.SupportingDocumentsUserControl));
				AssertEquals("Supporting Documents", tabPage.CaptionResourceString.Caption);
				AssertEquals(typeof(InvoiceLayoutSupportingDocumentsUserControl), foundUserControl.UserControlType);
			}
		}

		public void TestSupportingDocumentsTabPage_Invisible()
		{
			AssertSupportingDocumentTabPageVisibility(supportingDocumentsSupport: false);
		}

		public void TestSupportingDocumentsTabPage_Visible()
		{
			AssertSupportingDocumentTabPageVisibility(supportingDocumentsSupport: true);
		}

		void AssertSupportingDocumentTabPageVisibility(bool supportingDocumentsSupport)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSupportingDocumentsSupportConfigurationAndReturnMock(declaration, supportingDocumentsSupport).disposable)
			{
				AssertTabPageVisibility(supportingDocumentsSupport, nameof(EntryInstructionDetailsUserControl.SupportingDocumentsTabPage));
			}
		}

		public void TestPreviousDocumentsControlType()
		{
			using (var form = new ZForm())
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var tabPage = control.FindSingle<ZTabPage>(nameof(EntryInstructionDetailsUserControl.PreviousDocumentsTabPage));
				tabPage.Show();

				var foundUserControl = control.FindSingle<ZDynamicControlCreationUserControl>(nameof(EntryInstructionDetailsUserControl.PreviousDocumentsUserControl));
				AssertEquals("Previous Documents", tabPage.CaptionResourceString.Caption);
				AssertEquals(typeof(LayoutPreviousDocumentsUserControl), foundUserControl.UserControlType);
			}
		}

		public void TestPreviousDocumentsTabPage_Invisible()
		{
			AssertPreviousDocumentTabPageVisibility(previousDocumentsSupport: false);
		}

		public void TestPreviousDocumentsTabPage_Visible()
		{
			AssertPreviousDocumentTabPageVisibility(previousDocumentsSupport: true);
		}

		void AssertPreviousDocumentTabPageVisibility(bool previousDocumentsSupport)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionPreviousDocumentsSupportConfigurationAndReturnMock(declaration, previousDocumentsSupport).disposable)
			{
				AssertTabPageVisibility(previousDocumentsSupport, nameof(EntryInstructionDetailsUserControl.PreviousDocumentsTabPage));
			}
		}

		public void TestSpecialProceduresControlType()
		{
			using (var form = new ZForm())
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var tabPage = control.FindSingle<ZTabPage>(nameof(EntryInstructionDetailsUserControl.SpecialProceduresTabPage));
				tabPage.Show();

				var foundUserControl = control.FindSingle<SpecialProceduresUserControl>(nameof(EntryInstructionDetailsUserControl.SpecialProceduresUserControl));
				CombineAssertions(() =>
				{
					AssertEquals("Special Procedures", tabPage.CaptionResourceString.Caption);
					AssertEquals(typeof(SpecialProceduresUserControl), foundUserControl.GetType());
				});
			}
		}

		public void TestSpecialProceduresTabPage_Invisible()
		{
			AssertSpecialProceduresTabPageVisibility(specialProceduresSupport: false);
		}

		public void TestSpecialProceduresTabPage_Visible()
		{
			AssertSpecialProceduresTabPageVisibility(specialProceduresSupport: true);
		}

		void AssertSpecialProceduresTabPageVisibility(bool specialProceduresSupport)
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionSpecialProceduresSupportConfigurationAndReturnMock(declaration, specialProceduresSupport).disposable)
			{
				AssertTabPageVisibility(specialProceduresSupport, nameof(EntryInstructionDetailsUserControl.SpecialProceduresTabPage));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
		}

		JobDeclaration declaration;

		public void TestGuaranteeTabPage_Invisible()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionGuaranteesSupportConfiguration(declaration, false).disposable)
			{
				AssertGuaranteeTabPageVisibility(false);
			}
		}

		public void TestGuaranteeTabPage_Visible()
		{
			ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);

			var declarationConfiguration = new DeclarationConfigurationForTest()
			{
				IsUCC6ForTest = true,
				InstructionConfigurationForTest = new InstructionConfigurationForTest() { SupportGuaranteesForTest = true },
			};

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(x => x.GetObject()).Returns(declarationConfiguration);

			var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var keyObjectHandleDictionaryObject = new KeyObjectHandleDictionaryObject
			{
				{ currentCountryCode, objectHandleMock.Object }
			};
			Factory.ClearCachedValue<DeclarationConfiguration>($"DeclarationConfiguration_{currentCountryCode}");

			using (ObjectFactory.Substitute(nameof(DeclarationConfiguration), keyObjectHandleDictionaryObject))
			{
				AssertGuaranteeTabPageVisibility(true);
			}
		}

		public void TestInitSupportingDocumentsUserControl()
		{
			using (var form = new ZForm())
			using (var userControl = new EntryInstructionDetailsUserControl())
			{
				userControl.SetDataBinding(declaration, "");
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var supportingDocumentTab = userControl.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
				userControl.EntryInstructionTabControl.SelectedTab = supportingDocumentTab;
				var grid = userControl.Controls.Find("SupportingDocumentsGrid", true).First() as ZGrid;
				AssertEquals("DataMember for SupportingDocumentsGrid", "CustomsEntryInstructions.SupportingDocuments", grid.DataMember);
			}
		}

		public void TestAdditionalInfoTabPage_Visibility()
		{
			var tabPageName = nameof(EntryInstructionDetailsUserControl.AdditionalInfoTabPage);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(declaration, false).disposable)
			{
				AssertTabPageVisibility(false, tabPageName);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(declaration, true).disposable)
			{
				AssertTabPageVisibility(true, tabPageName);
			}
		}

		[RequiresSTA]
		public void TestAdditionalInfosUserControlType()
		{
			var userControlName = nameof(EntryInstructionDetailsUserControl.AdditionalInfoUserControl);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionAdditionalInfosSupportConfigurationAndReturnMock(declaration, true).disposable)
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var additionalInfoTab = control.FindSingle<ZTabPage>("AdditionalInfoTabPage");
				control.EntryInstructionTabControl.SelectedTab = additionalInfoTab;

				var userControl = control.FindSingle<ZDynamicControlCreationUserControl>(userControlName);
				AssertEquals($"{userControlName}.UserControlType", typeof(AdditionalInfosUserControlWithGrid), userControl.UserControlType);
			}
		}

		public void TestInitAdditionalInfosUserControl()
		{
			using (var form = new ZForm())
			using (var userControl = new EntryInstructionDetailsUserControl())
			{
				userControl.SetDataBinding(declaration, "");
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				var additionalInfoTab = userControl.FindSingle<ZTabPage>("AdditionalInfoTabPage");
				userControl.EntryInstructionTabControl.SelectedTab = additionalInfoTab;
				var grid = userControl.Controls.Find("AdditionalInfosGrid", true).First() as ZGrid;
				AssertEquals("DataMember for AdditionalInfosGrid", "CustomsEntryInstructions.AdditionalInfos", grid.DataMember);

				var additionalInfosGroupBox = additionalInfoTab.FindSingle<ZGroupBox>("AdditionalInfosGroupBox");
				AssertEquals("AdditionalInfosGroupBox caption", "Additional Info", additionalInfosGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestSupplyChainActorReferencesUserControlDock()
		{
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var supplyChainActorReferencesUserControl = control.FindSingle<SupplyChainActorReferencesUserControl>("SupplyChainActorReferencesUserControl");

				AssertNotNull("SupplyChainActorReferencesUserControl", supplyChainActorReferencesUserControl);
				AssertEquals("SupplyChainActorReferencesUserControl.Dock", System.Windows.Forms.DockStyle.Fill, supplyChainActorReferencesUserControl.Dock);
			}
		}

		void AssertGuaranteeTabPageVisibility(ZBool shoulbeVisible)
		{
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				control.SetDataBinding(declaration, "");

				form.Controls.Add(control);
				control.SetTabPagesVisibility();

				form.Show();

				var entryInstructionGridUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("EntryInstructionGridUserControl").HostedControl as EntryInstructionGridUserControl;
				var entryInstructionsGrid = entryInstructionGridUserControl.FindSingle<ZGrid>("EntryInstructionsGrid");

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

				entryInstructionsGrid.ListManager.Position = 1;

				var guaranteesTabPage = control.FindSingleOrDefault<ZTabPage>("GuaranteesTabPage");
				if (shoulbeVisible)
				{
					AssertEquals("GuaranteesTabPage.Visible", shoulbeVisible, guaranteesTabPage?.TabVisible ?? false);
				}
				else
				{
					AssertNull("GuaranteesTabPage.Invisible", guaranteesTabPage);
				}
			}
		}

		void AssertUserControlType(ZString userControlName, Type userControlType)
		{
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var userControl = control.FindSingle<ZDynamicControlCreationUserControl>(userControlName);
				AssertEquals($"{userControlName}.UserControlType", userControlType, userControl.UserControlType);
			}
		}

		void AssertTabPageVisibility(ZBool visible, ZString tabPageName)
		{
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetTabPagesVisibility();
				var tabPage = control.FindSingleOrDefault<ZTabPage>(tabPageName);
				if (visible)
				{
					AssertEquals($"{tabPageName}.Visible", visible, tabPage.TabVisible);
				}
				else
				{
					AssertNull($"{tabPageName}.Invisible", tabPage);
				}
			}
		}

		#region DeclarationConfigurationForTest

		class DeclarationConfigurationForTest : DeclarationConfiguration
		{
			public ZBool IsUCC6ForTest { get; set; }
			public InstructionConfiguration InstructionConfigurationForTest { get; set; }

			protected override ZBool IsUCC6Core(BusinessObject businessObject)
			{
				return IsUCC6ForTest;
			}

			protected override InstructionConfiguration GetNewInstructionConfiguration()
			{
				return InstructionConfigurationForTest;
			}
		}

		class InstructionConfigurationForTest : InstructionConfiguration
		{
			public ZBool SupportGuaranteesForTest { get; set; }

			protected override ZBool GuaranteesSupportCore(JobDeclaration declaration, CusEntryInstruction entryInstruction)
			{
				return SupportGuaranteesForTest;
			}
		}

		#endregion
	}
}
