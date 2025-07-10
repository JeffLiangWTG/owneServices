using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.JAS.Business.Cognos;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.GUI.Cognos.Testing
{
	class CognosAccGLAccountDescriptorFormTest : TestCaseWithFactory
	{
		public void TestSubClassificationTabPageVisibility()
		{
			CognosAccountForm.Show();
			CognosAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			CognosAccount.AJ_ReportCategory = Core.Constants.AccountType.BalanceSheetAccount;
			CognosAccount.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			Assert("Not a Cognos Sub-Classification account, should hide SubClassificationTabPage", !CognosAccountForm.InternalMainTabControlTest.TabPages.Contains(CognosAccountForm.InternalSubClassificationTabPageTest));
			CognosAccount.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			AssertEquals("ReportType is Sub-Classification, should show SubClassificationTabPage", 2, CognosAccountForm.InternalMainTabControlTest.TabPages.IndexOf(CognosAccountForm.InternalSubClassificationTabPageTest));
		}

		public void TestInternalCognosTabPageTestVisibility()
		{
			CognosAccountForm.Show();
			CognosAccount.AJ_Language = Core.Constants.GLLanguages.Afrikaans;
			Assert("Not a Cognos GL Language account, should hide InternalCognosTabPageTest", !CognosAccountForm.InternalMainTabControlTest.TabPages.Contains(CognosAccountForm.InternalCognosTabPageTest));
			CognosAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			AssertEquals("Language is ZZZ, should show InternalCognosTabPageTest", 1, CognosAccountForm.InternalMainTabControlTest.TabPages.IndexOf(CognosAccountForm.InternalCognosTabPageTest));
		}

		public void TestDebtorModuleButtonGridVisibility()
		{
			CognosAccountForm.Show();
			CognosAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			CognosAccount.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			CognosAccount.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			CognosAccountForm.InternalMainTabControlTest.SelectedIndex = 2;
			CognosAccount.ExtraInfo.IsSubClassifiedByDebtor = true;
			Assert("Should not be visible if not grouped by creditor", !CognosAccountForm.InternalCreditorModuleButtonGridTest.Visible);
			Assert("Should not be visible if not grouped by age", !CognosAccountForm.InternalAccountAgeDropEditTest.Visible);
			Assert("Should be visible if grouped by debtor", CognosAccountForm.InternalDebtorModuleButtonGridTest.Visible);
		}

		public void TestInternalCreditorModuleButtonGridTestVisibility()
		{
			CognosAccountForm.Show();
			CognosAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			CognosAccount.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			CognosAccount.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			CognosAccountForm.InternalMainTabControlTest.SelectedIndex = 2;
			CognosAccount.ExtraInfo.IsSubClassifiedByCreditor = true;
			Assert("Should be visible if grouped by creditor", CognosAccountForm.InternalCreditorModuleButtonGridTest.Visible);
			Assert("Should not be visible if not grouped by debtor", !CognosAccountForm.InternalDebtorModuleButtonGridTest.Visible);
			Assert("Should not be visible if not grouped by age", !CognosAccountForm.InternalAccountAgeDropEditTest.Visible);
		}

		public void TestInternalAccountAgeDropEditTestVisibility()
		{
			CognosAccountForm.Show();
			CognosAccount.AJ_Language = Core.Constants.GLLanguages.ZZZ_ExternalLinkToGeneralLedger;
			CognosAccount.AJ_ReportCategory = CognosAccGLAccountDescriptor.CognosSubClassificationAccountType;
			CognosAccount.AJ_ReportType = CognosAccGLAccountDescriptor.ReportTypeCOA;
			CognosAccountForm.InternalMainTabControlTest.SelectedIndex = 2;
			CognosAccount.ExtraInfo.IsSubClassifiedByAge = true;
			Assert("Should be visible if grouped by age", CognosAccountForm.InternalAccountAgeDropEditTest.Visible);
			Assert("Should not be visible if grouped by creditor", !CognosAccountForm.InternalCreditorModuleButtonGridTest.Visible);
			Assert("Should not be visible if not grouped by debtor", !CognosAccountForm.InternalDebtorModuleButtonGridTest.Visible);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CognosAccount = Factory.New<CognosAccGLAccountDescriptor>();
			CognosAccountForm = new CognosAccGLAccountDescriptorForm(CognosAccount);
		}

		protected override void TearDown()
		{
			CognosAccountForm.Dispose();
			base.TearDown();
		}

		[TestedType(typeof(CognosAccGLAccountDescriptorForm))]
		class BasherTest : ZFormBasherTest
		{
			protected override Form GetFormToBashCore()
			{
				CognosAccGLAccountDescriptor accountDescriptor = Factory.New<CognosAccGLAccountDescriptor>();
				return new CognosAccGLAccountDescriptorForm(accountDescriptor);
			}
		}

		CognosAccGLAccountDescriptor CognosAccount;
		CognosAccGLAccountDescriptorForm CognosAccountForm;
	}
}
