using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestTabPagesOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var cusEntryInstruction = declaration.CustomsEntryInstructions[0];
			cusEntryInstruction.CEI_Style = "H1";
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetTabPagesVisibility();

				var tabOrderThatMustBeMaintained = new ZString[] { "DetailsTabPage", "SupplyChainActorTabPage", "SupportingDocumentsTabPage", "PreviousDocumentsTabPage", "AdditionalInfoTabPage", "SpecialProceduresTabPage" };
				var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
				var tabPagesOrder = instructionTabControl.AllTabPages.OfType<ZTabPage>().Where(tp => tp.TabVisible && tabOrderThatMustBeMaintained.Contains(tp.Name)).Select(tp => new ZString(tp.Name)).ToArray();

				AssertArrayEqualsByElements(tabOrderThatMustBeMaintained, tabPagesOrder);
			}
		}

		public void TestDetailsUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(typeof(LayoutEntryInstructionDetailBasicUserControl), control.FindSingle<ZDynamicControlCreationUserControl>("DetailsUserControl").UserControlType);
			}
		}

		public void TestSupplyChainActorTabPageVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				control.SetTabPagesVisibility();
				AssertEquals("SupplyChainActorTabPage TabVisible Export", true, control.FindSingle<ZTabPage>("SupplyChainActorTabPage").TabVisible);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				control.SetTabPagesVisibility();
				AssertEquals("SupplyChainActorTabPage TabVisible Import", true, control.FindSingle<ZTabPage>("SupplyChainActorTabPage").TabVisible);
			}
		}

		public void TestFiscalReferencesTabPageVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				control.SetTabPagesVisibility();
				AssertNull("FiscalReferencesTabPage should be invisible for Export", control.FindSingleOrDefault<ZTabPage>("FiscalReferencesTabPage"));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				control.SetTabPagesVisibility();
				AssertNull("FiscalReferencesTabPage should be invisible for Import UCC5", control.FindSingleOrDefault<ZTabPage>("FiscalReferencesTabPage"));

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				control.SetTabPagesVisibility();
				AssertEquals("FiscalReferencesTabPage TabVisible Import UCC6", true, control.FindSingle<ZTabPage>("FiscalReferencesTabPage").TabVisible);
			}
		}

		public void TestPreviousDocumentsTabPage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			CombineAssertions("PreviousDocumentsTabPage", () =>
			{
				DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<EntryInstructionDetailsUserControl>(
					businessObject: declaration,
					tabPageName: "PreviousDocumentsTabPage",
					userControlName: "PreviousDocumentsUserControl",
					expectedCaption: "Previous Documents",
					expectedUserControlType: typeof(InvoiceHeaderExportPreviousDocumentsUserControl)
				);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<EntryInstructionDetailsUserControl>(
					businessObject: declaration,
					tabPageName: "PreviousDocumentsTabPage",
					userControlName: "PreviousDocumentsUserControl",
					expectedCaption: "[2/1] Previous Documents",
					expectedUserControlType: typeof(UCC5ImportPreviousDocumentsUserControl)
				);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<EntryInstructionDetailsUserControl>(
					businessObject: declaration,
					tabPageName: "PreviousDocumentsTabPage",
					userControlName: "PreviousDocumentsUserControl",
					expectedCaption: "Previous Documents",
					expectedUserControlType: typeof(InvoiceHeaderExportPreviousDocumentsUserControl)
				);
			});
		}

		public void TestAdditionalInfoTabPage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<EntryInstructionDetailsUserControl>(declaration, "AdditionalInfoTabPage", "AdditionalInfoUserControl", "Additional Documents", typeof(AdditionalInfosUserControlWithGrid));

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<EntryInstructionDetailsUserControl>(declaration, "AdditionalInfoTabPage", "AdditionalInfoUserControl", "Additional Documents", typeof(ImportAdditionalInfosUserControlWithGrid));
		}

		public void TestSupportingDocumentsTabPage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;

			CombineAssertions("SupportingDocumentsTabPage", () =>
			{
				DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<EntryInstructionDetailsUserControl>(
					businessObject: declaration,
					tabPageName: "SupportingDocumentsTabPage",
					userControlName: "SupportingDocumentsUserControl",
					expectedCaption: "Supporting Documents",
					expectedUserControlType: typeof(InvoiceLayoutSupportingDocumentsUserControl)
				);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<EntryInstructionDetailsUserControl>(
					businessObject: declaration,
					tabPageName: "SupportingDocumentsTabPage",
					userControlName: "SupportingDocumentsUserControl",
					expectedCaption: "[2/3] Supporting Documents",
					expectedUserControlType: typeof(InvoiceLayoutSupportingDocumentsUserControl)
				);

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
				DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<EntryInstructionDetailsUserControl>(
					businessObject: declaration,
					tabPageName: "SupportingDocumentsTabPage",
					userControlName: "SupportingDocumentsUserControl",
					expectedCaption: "Supporting Documents",
					expectedUserControlType: typeof(InvoiceLayoutSupportingDocumentsUserControl)
				);
			});
		}

		public void TestGuaranteesUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.CustomsEntryInstructions.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetTabPagesVisibility();

				var userControl = (ZDynamicControlCreationUserControl)form.Controls.Find("GuaranteesUserControl", true).First();
				AssertEquals("Enterprise.Customs.IE.GUI.IEEntryInstructionGuaranteesUserControl", userControl.UserControlType.FullName);
			}
		}

		public void TestSetAuthorisationsTabPageCaption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("[3/39] Authorizations", control.FindSingle<ZTabPage>("AuthorisationsTabPage").CaptionResourceString.Caption);
			}
		}

		public void TestSetAuthorisationsTabPageCaption_Other()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.Interfaced;
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Authorizations", control.FindSingle<ZTabPage>("AuthorisationsTabPage").CaptionResourceString.Caption);
			}
		}

		public void TestSetSupplyChainActorTabPageCaption_ImportV1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("[3/37] Add. Supply Chain Actors", control.FindSingle<ZTabPage>("SupplyChainActorTabPage").CaptionResourceString.Caption);
			}
		}

		public void TestSetSupplyChainActorTabPageCaption_Other()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Add. Supply Chain Actors", control.FindSingle<ZTabPage>("SupplyChainActorTabPage").CaptionResourceString.Caption);
			}
		}

		public void TestSpecialProceduresUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.AddNew();
			using (var form = new ZForm(declaration))
			using (var control = new EntryInstructionDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(typeof(SpecialProceduresUserControl), control.FindSingle<ZDynamicControlCreationUserControl>("SpecialProceduresUserControl").UserControlType);
			}
		}
	}
}
