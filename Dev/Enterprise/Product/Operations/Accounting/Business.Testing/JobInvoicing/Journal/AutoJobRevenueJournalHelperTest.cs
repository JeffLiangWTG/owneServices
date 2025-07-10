using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class AutoJobRevenueJournalHelperTest : TestCaseWithFactory
	{
		#region TestIsExcludedFromAutoJRJ

		public void TestIsExcludedFromAutoJRJ_AutoJRJRegistryIsNo_TaxRegNumAreDifferent()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(false);
			AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly();

			AssertIsExcludedFromAutoJRJ(false);
			AssertIsExcludedFromAutoJRJ_NullCostOrSellOrg(false);
			AssertIsExcludedFromAutoJRJ_NullOrEmptyCountryCode(false);
			AssertIsExcludedFromAutoJRJ_NullBranch(false);
		}

		public void TestIsExcludedFromAutoJRJ_AutoJRJRegistryIsNo_TaxRegNumAreTheSame()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(true);
			AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly();

			AssertIsExcludedFromAutoJRJ(false);
			AssertIsExcludedFromAutoJRJ_NullCostOrSellOrg(false);
			AssertIsExcludedFromAutoJRJ_NullOrEmptyCountryCode(false);
			AssertIsExcludedFromAutoJRJ_NullBranch(false);
		}

		public void TestIsExcludedFromAutoJRJ_AutoJRJRegistryIsYes_TaxRegNumAreDifferent()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(false);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			AssertIsExcludedFromAutoJRJ(false);
			AssertIsExcludedFromAutoJRJ_NullCostOrSellOrg(false);
			AssertIsExcludedFromAutoJRJ_NullOrEmptyCountryCode(false);
			AssertIsExcludedFromAutoJRJ_NullBranch(false);
		}

		public void TestIsExcludedFromAutoJRJ_AutoJRJRegistryIsYes_TaxRegNumAreTheSame()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(true);
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();

			AssertIsExcludedFromAutoJRJ(false);
			AssertIsExcludedFromAutoJRJ_NullCostOrSellOrg(false);
			AssertIsExcludedFromAutoJRJ_NullOrEmptyCountryCode(false);
			AssertIsExcludedFromAutoJRJ_NullBranch(false);
		}

		public void TestIsExcludedFromAutoJRJ_AutoJRJRegistryIsTax_TaxRegNumAreDifferent()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(false);
			AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly();

			AssertIsExcludedFromAutoJRJ(true);
			AssertIsExcludedFromAutoJRJ_NullCostOrSellOrg(true);
			AssertIsExcludedFromAutoJRJ_NullOrEmptyCountryCode(true);
			AssertIsExcludedFromAutoJRJ_NullBranch(true);
		}

		public void TestIsExcludedFromAutoJRJ_AutoJRJRegistryIsTax_TaxRegNumAreTheSame()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(true);
			AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly();

			AssertIsExcludedFromAutoJRJ(false);
			AssertIsExcludedFromAutoJRJ_NullCostOrSellOrg(true);
			AssertIsExcludedFromAutoJRJ_NullOrEmptyCountryCode(true);
			AssertIsExcludedFromAutoJRJ_NullBranch(true);
		}

		public void TestIsExcludedFromAutoJRJ_AutoJRJRegistryIsTax_BranchHasNoOrgProxy()
		{
			TestObjectCreator.PrepareAutoJRJTestEnvironment(true);
			AutoJRJRegistryStatusHelper.SetAutoJRJWithTaxRegistrationNumberEnabled_ForTestOnly();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			GlbBranch.CurrentBranch.Factory.Save();

			AssertIsExcludedFromAutoJRJ(true, "Charge branch has no org proxy, while charge creditor/debtor has non empty tax registration number");

			TestObjectCreator.AutoJRJCreditorOrDebtor.PrimaryRegistrationNumber.Number = null;

			AssertIsExcludedFromAutoJRJ(false, "Charge branch has no org proxy, while charge creditor/debtor has empty tax registration number");
		}

		void AssertIsExcludedFromAutoJRJ(bool expectedResult, string assertMessage = null)
		{
			var costOrSellAccount = TestObjectCreator.AutoJRJCreditorOrDebtor;
			var branch = GlbBranch.CurrentBranch;
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals(assertMessage, expectedResult, helperForTest.IsExcludedFromAutoJRJ(branch, costOrSellAccount, countryCode));
		}

		void AssertIsExcludedFromAutoJRJ_NullCostOrSellOrg(bool expectedResult)
		{
			var costOrSellAccount = (OrgHeader)null;
			var branch = GlbBranch.CurrentBranch;
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals(expectedResult, helperForTest.IsExcludedFromAutoJRJ(branch, costOrSellAccount, countryCode));
		}

		void AssertIsExcludedFromAutoJRJ_NullOrEmptyCountryCode(bool expectedResult)
		{
			var costOrSellAccount = TestObjectCreator.AutoJRJCreditorOrDebtor;
			var branch = GlbBranch.CurrentBranch;
			var countryCode = (string)null;

			AssertEquals(expectedResult, helperForTest.IsExcludedFromAutoJRJ(branch, costOrSellAccount, countryCode));

			countryCode = string.Empty;

			AssertEquals(expectedResult, helperForTest.IsExcludedFromAutoJRJ(branch, costOrSellAccount, countryCode));
		}

		void AssertIsExcludedFromAutoJRJ_NullBranch(bool expectedResult)
		{
			var costOrSellAccount = TestObjectCreator.AutoJRJCreditorOrDebtor;
			var branch = (GlbBranch)null;
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AssertEquals(expectedResult, helperForTest.IsExcludedFromAutoJRJ(branch, costOrSellAccount, countryCode));
		}

		#endregion

		public void TestIsAutoJRJEnabled()
		{
			AssertEquals("Default value", false, helperForTest.IsAutoJRJEnabled);

			AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Yes);
			AssertEquals("When registry is set to YES", true, helperForTest.IsAutoJRJEnabled);

			AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax);
			AssertEquals("When registry is set to TAX", true, helperForTest.IsAutoJRJEnabled);

			AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Non);
			AssertEquals("When registry is set to NON", false, helperForTest.IsAutoJRJEnabled);
		}

		protected override void SetUp()
		{
			base.SetUp();
			helperForTest = new AutoJobRevenueJournalHelper();
		}

		AutoJobRevenueJournalHelper helperForTest;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
