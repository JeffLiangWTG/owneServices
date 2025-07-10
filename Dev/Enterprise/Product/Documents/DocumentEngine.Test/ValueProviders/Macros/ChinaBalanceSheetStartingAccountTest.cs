using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ChinaBalanceSheetStartingAccount))]
	sealed class ChinaBalanceSheetStartingAccountTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			AssertEquals("should not match <>", false, ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			AssertEquals("should not match <Chinaa Balance Sheet Starting Account>", false, ValueProviderToTest.IsResponsibleForReplacing("<Chinaa Balance Sheet Starting Account>", Passes.FirstPass));
			AssertEquals("should match <China Balance Sheet Starting Account>", true, ValueProviderToTest.IsResponsibleForReplacing("<China Balance Sheet Starting Account>", Passes.FirstPass));
			AssertEquals("should match <ChinaBalanceSheetStartingAccount>", true, ValueProviderToTest.IsResponsibleForReplacing("<ChinaBalanceSheetStartingAccount>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			var test = new ChinaBalanceSheetStartingAccount();
			AssertEquals(test.GetBalanceSheetStartingAccount(), ValueProviderToTest.GetReplacement("<ChinaBalanceSheetStartingAccount>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ChinaBalanceSheetStartingAccount();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var accounting = ObjectFactory.Get<IAccounting>();
			var id = accounting.ReportOrder_GLAccountSecondReportStartsFrom(Core.SharedConstants.Languages.ChineseSimplified, Core.Constants.CountryCodes.China);
			if (id.IsEmpty)
			{
				id = ZGuid.NewZGuid();
				var filter = new ZQuery();
				filter.MaximumRows = 4;
				filter.OrderBy = AccGLHeaderSchema.Constants.AG_AccountNum;
				filter.AddToFilter(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.ProfitAndLossAccount);
				var header = Factory.Load<AccGLHeader>(filter)[2];
				var gLHeaderToAdd = Factory.NewWithPrimaryKey<AccGLAccountDescriptor>(id.ToGuid());
				gLHeaderToAdd.AJ_Language = Core.SharedConstants.Languages.ChineseSimplified;
				gLHeaderToAdd.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
				gLHeaderToAdd.AJ_AccountDescription = "Chinese1";
				gLHeaderToAdd.AJ_LocalAccountNumber = "1020.30.40";
				gLHeaderToAdd.AJ_DebitCredit = "DR";
				gLHeaderToAdd.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
				gLHeaderToAdd.AJ_ReportCategory = "HDR";
				gLHeaderToAdd.ParentGLHeaderPK = header.PK;

				var gLHeaderToAdd2 = Factory.New<AccGLAccountDescriptor>();
				gLHeaderToAdd2.AJ_Language = Core.SharedConstants.Languages.ChineseSimplified;
				gLHeaderToAdd2.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
				gLHeaderToAdd2.AJ_AccountDescription = "Chinese2";
				gLHeaderToAdd2.AJ_LocalAccountNumber = "1010.30.40";
				gLHeaderToAdd2.AJ_DebitCredit = "DR";
				gLHeaderToAdd2.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
				gLHeaderToAdd2.AJ_ReportCategory = "HDR";
				gLHeaderToAdd2.ParentGLHeaderPK = header.PK;
				Factory.Save();
				accounting.AddReportOrder_AccountsOrderValue(Core.SharedConstants.Languages.ChineseSimplified, Core.Constants.CountryCodes.China, nameof(AccountOrderType.BalanceSheet), id);
			}
		}
	}
}
