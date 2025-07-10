using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GenericChargeFilterBusinessObject))]
	public class GenericChargeFilterBusinessObjectTest : AccountingFilterStripBusinessObjectTestCase
	{
		public void TestFilterValueForDescription()
		{
			SetUpGLHeaderAndChargeCodesForFilters();

			Factory.Save();

			var descriptionFilter = (ModuleTextFilter)FilterBizO["Description"];
			descriptionFilter.IsActive = true;
			descriptionFilter.Property = "";
			var results = new GenericChargeCollection(Factory, FilterBizO.Filter);
			results.Load(FilterBizO.Filter);
			AssertEquals("Empty filter should find charges from set up", 4, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK, chargeCode2.PK, glHeader1.PK, glHeader2.PK }, results.GetPKs());

			descriptionFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			descriptionFilter.Property = "BANK";
			results.Load(FilterBizO.Filter);
			AssertEquals("Should find GL and Charge code where the description mentions bank", 2, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode2.PK, glHeader1.PK }, results.GetPKs());

			descriptionFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			descriptionFilter.Property = "BANK";
			results.Load(FilterBizO.Filter);
			AssertEquals(1, results.Count);
			AssertEquals("Only GLHeader1 has a description starting with bank", glHeader1.PK, results[0].PK);

			descriptionFilter.Property = "1";
			results.Load(FilterBizO.Filter);
			AssertEquals("No descriptions actually start with the number 1", 0, results.Count);

			descriptionFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			descriptionFilter.Property = "1";
			results.Load(FilterBizO.Filter);
			AssertEquals(1, results.Count);
			AssertEquals("Only chargeCode1 has a description containing the number 1", chargeCode1.PK, results[0].PK);
		}

		public void TestChargeCodeFilter()
		{
			SetUpGLHeaderAndChargeCodesForFilters();
			var chargeCodeFilter = (ModuleTextFilter)FilterBizO["Charge Code"];
			chargeCodeFilter.IsActive = true;
			var results = new GenericChargeCollection(Factory, FilterBizO.Filter);
			results.Load(FilterBizO.Filter);
			AssertEquals("Empty filter should find all charge codes and GL header added for this test", 4, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK, chargeCode2.PK, glHeader1.PK, glHeader2.PK }, results.GetPKs());

			chargeCodeFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			chargeCodeFilter.Property = "1";
			results.Load(FilterBizO.Filter);
			AssertEquals("Once charge code and one GL account header contain the number 1", 2, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK, glHeader1.PK }, results.GetPKs());

			chargeCodeFilter.Property = "99";
			results.Load(FilterBizO.Filter);
			AssertEquals(2, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { glHeader1.PK, glHeader2.PK }, results.GetPKs());

			chargeCodeFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			chargeCodeFilter.Property = "A";
			results.Load(FilterBizO.Filter);
			AssertEquals(1, results.Count);
			AssertEquals("Only chargeCode1's code begins with A", chargeCode1.PK, results[0].PK);
		}

		public void TestLocalAccountCodeFilter()
		{
			SetUpTestDataForLocalAccountFilter();

			var results = new GenericChargeCollection(Factory, FilterBizO.Filter);
			var localAccountFilter = (ModuleTextFilter)FilterBizO["Local Account Code"];

			results.Load(FilterBizO.Filter);
			AssertEquals("Empty filter should find all charge codes and GL header added for this test", 4, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK, chargeCode2.PK, glHeader1.PK, glHeader2.PK }, results.GetPKs());

			localAccountFilter.Property = "0000";
			localAccountFilter.IsActive = true;
			results.Load(FilterBizO.Filter);
			AssertEquals("2 GL Headers with local account should be found", 2, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { glHeader1.PK, glHeader2.PK }, results.GetPKs());

			localAccountFilter.Property = "00000001";
			localAccountFilter.IsActive = true;
			results.Load(FilterBizO.Filter);
			AssertEquals("The GL Header with local account code '00000001' should be found", 1, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { glHeader1.PK }, results.GetPKs());

			localAccountFilter.Property = "3333";
			localAccountFilter.IsActive = true;
			results.Load(FilterBizO.Filter);
			AssertEquals("No GL Header's local account code starts with '3333'", 0, results.Count);
		}

		public void TestLocalAccountDescriptionFilter()
		{
			SetUpTestDataForLocalAccountFilter();

			var results = new GenericChargeCollection(Factory, FilterBizO.Filter);
			var localAccountFilter = (ModuleTextFilter)FilterBizO["Local Account Description"];

			results.Load(FilterBizO.Filter);
			AssertEquals("Empty filter should find all charge codes and GL header added for this test", 4, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK, chargeCode2.PK, glHeader1.PK, glHeader2.PK }, results.GetPKs());

			localAccountFilter.Property = "Local Account Description";
			localAccountFilter.IsActive = true;
			results.Load(FilterBizO.Filter);
			AssertEquals("2 GL Headers with local account should be found", 2, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { glHeader1.PK, glHeader2.PK }, results.GetPKs());

			localAccountFilter.Property = "Local Account Description 1";
			localAccountFilter.IsActive = true;
			results.Load(FilterBizO.Filter);
			AssertEquals("The GL Header with local account description 'Local Account Description 1' should be found", 1, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { glHeader1.PK }, results.GetPKs());

			localAccountFilter.Property = "Global Account Description";
			localAccountFilter.IsActive = true;
			results.Load(FilterBizO.Filter);
			AssertEquals("No GL Header's local account description starts with 'Global Account Description'", 0, results.Count);
		}

		public void TestFilterSubQueries()
		{
			var expectedQuery1 = string.Format("VC_GC = CONVERT('{0}', 'System.Guid') or VC_GC is null", GlbCompany.CurrentCompany.PK);
			var expectedQuery2 = "(" + expectedQuery1 + ") and VC_IsGLAccount = {0}";

			SetUpGLHeaderAndChargeCodesForFilters();
			var chargeCodeOnlyFilterBizO = new GenericChargeFilterBusinessObject(GenericChargeFilterBusinessObject.ElementType.ChargeCode);
			var glOnlyFilterBizO = new GenericChargeFilterBusinessObject(GenericChargeFilterBusinessObject.ElementType.GeneralLedger);
			var noneFilterBizO = new GenericChargeFilterBusinessObject(GenericChargeFilterBusinessObject.ElementType.None);
			var fullFilterBizO = new GenericChargeFilterBusinessObject(GenericChargeFilterBusinessObject.ElementType.All);

			AssertEquals("Full query.", expectedQuery1, fullFilterBizO.Filter.LiteralTextADO);
			AssertEquals("ChargeCode Only.", string.Format(expectedQuery2, 0), chargeCodeOnlyFilterBizO.Filter.LiteralTextADO);
			AssertEquals("GL Only.", string.Format(expectedQuery2, 1), glOnlyFilterBizO.Filter.LiteralTextADO);
			AssertEquals("No result query.", true, noneFilterBizO.Filter.IsNoResultQuery);
		}

		void SetUpTestDataForLocalAccountFilter()
		{
			SetUpGLHeaderAndChargeCodesForFilters();

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			GlbStaff.CurrentUser.GS_WorkingLanguage = Core.SharedConstants.Languages.ChineseSimplified;

			var testGLAccountDescriptor1 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testGLAccountDescriptor1.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			testGLAccountDescriptor1.AJ_Language = Core.SharedConstants.Languages.ChineseSimplified;
			testGLAccountDescriptor1.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			testGLAccountDescriptor1.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			testGLAccountDescriptor1.ParentGLHeaderPK = glHeader1.PK;
			testGLAccountDescriptor1.AJ_LocalAccountNumber = "00000001";
			testGLAccountDescriptor1.AJ_AccountDescription = "Local Account Description 1";

			var testGLAccountDescriptor2 = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			testGLAccountDescriptor2.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			testGLAccountDescriptor2.AJ_Language = Core.SharedConstants.Languages.ChineseSimplified;
			testGLAccountDescriptor2.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			testGLAccountDescriptor2.AJ_ReportCategory = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			testGLAccountDescriptor2.ParentGLHeaderPK = glHeader2.PK;
			testGLAccountDescriptor2.AJ_LocalAccountNumber = "00000002";
			testGLAccountDescriptor2.AJ_AccountDescription = "Local Account Description 2";

			Factory.Save();
		}

		public void TestAlternateAccount()
		{
			SetUpTestDataForAlternateAccountFilter();

			var results = new GenericChargeCollection(Factory, FilterBizO.Filter);
			var alternateGLAccountFilter = (ModuleTextFilter)FilterBizO["Alternate Account"];

			results.Load(FilterBizO.Filter);
			AssertEquals("Empty filter should find all charge codes and GL header added for this test", 4, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK, chargeCode2.PK, glHeader1.PK, glHeader2.PK }, results.GetPKs());

			alternateGLAccountFilter.Property = "10.00.1000";
			alternateGLAccountFilter.IsActive = true;
			results.Load(FilterBizO.Filter);
			AssertEquals("The GL Header with Alternate Account '10.00.1000' should be found", 1, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { glHeader1.PK }, results.GetPKs());

			alternateGLAccountFilter.Property = "20.00.1000";
			alternateGLAccountFilter.IsActive = true;
			results.Load(FilterBizO.Filter);
			AssertEquals("The GL Header with Alternate Account '20.00.1000' should be found", 1, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { glHeader2.PK }, results.GetPKs());

			alternateGLAccountFilter.Property = "3333";
			alternateGLAccountFilter.IsActive = true;
			results.Load(FilterBizO.Filter);
			AssertEquals("No GL Header's Alternate Account starts with '3333'", 0, results.Count);
		}

		public void TestAlternateAccountName()
		{
			SetUpTestDataForAlternateAccountFilter();

			var results = new GenericChargeCollection(Factory, FilterBizO.Filter);
			var alternateGLAccountNameFilter = (ModuleTextFilter)FilterBizO["Alternate Account Name"];

			results.Load(FilterBizO.Filter);
			AssertEquals("Empty filter should find all charge codes and GL header added for this test", 4, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { chargeCode1.PK, chargeCode2.PK, glHeader1.PK, glHeader2.PK }, results.GetPKs());

			alternateGLAccountNameFilter.Property = "AlternateGLAccount For GLHeader1";
			alternateGLAccountNameFilter.IsActive = true;
			results.Load(FilterBizO.Filter);
			AssertEquals("The GL Header with Alternate Account Name 'AlternateGLAccount For GLHeader1' should be found", 1, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { glHeader1.PK }, results.GetPKs());

			alternateGLAccountNameFilter.Property = "AlternateGLAccount For GLHeader2";
			alternateGLAccountNameFilter.IsActive = true;
			results.Load(FilterBizO.Filter);
			AssertEquals("The GL Header with Alternate Account Name 'AlternateGLAccount For GLHeader2' should be found", 1, results.Count);
			AssertContainsExactElementsInAnyOrder(new[] { glHeader2.PK }, results.GetPKs());

			alternateGLAccountNameFilter.Property = "3333";
			alternateGLAccountNameFilter.IsActive = true;
			results.Load(FilterBizO.Filter);
			AssertEquals("No GL Header's Alternate Account Name starts with '3333'", 0, results.Count);
		}

		void SetUpTestDataForAlternateAccountFilter()
		{
			SetUpGLHeaderAndChargeCodesForFilters();

			var testObjectCreator = new TestObjectCreator(Factory);
			var selectionChart = testObjectCreator.CreateAlternateChart("MGT", "Management Reporting");
			testObjectCreator.CreateAccAlternateChartFormat(selectionChart, 1, "X", "tier 1");
			var nonSelectionChart = testObjectCreator.CreateAlternateChart("TRR", "Management Reporting");
			testObjectCreator.CreateAccAlternateChartFormat(nonSelectionChart, 1, "X", "tier 1");
			Factory.Save();

			var alternateGLAccountForGLHeader1 = testObjectCreator.CreateAccAlternateGlAccount(selectionChart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount For GLHeader1");
			testObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccountForGLHeader1, glHeader1.PK, 1, "OCG", "OCG");
			var alternateGLAccountForGLHeader2 = testObjectCreator.CreateAccAlternateGlAccount(selectionChart.PK, "20.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount For GLHeader2");
			testObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccountForGLHeader2, glHeader2.PK, 1, "OCG", "OCG");
			var alternateGLAccountForGLHeader3 = testObjectCreator.CreateAccAlternateGlAccount(nonSelectionChart.PK, "30.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount For GLHeader3");
			testObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccountForGLHeader3, glHeader2.PK, 1, "OCG", "OCG");
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, selectionChart.PK.ToGuid());
		}

		#region Implementation

		void SetUpGLHeaderAndChargeCodesForFilters()
		{
			TestCaseHelper.ClearTable(AccChargeCodeSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccGLHeaderSchema.Constants.TableName);

			chargeCode1 = Factory.New<AccChargeCode>();
			chargeCode1.AC_Code = "ABCHRG1";
			chargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			chargeCode1.AC_Desc = "Auto-Breaking Charge 1";

			chargeCode2 = Factory.New<AccChargeCode>();
			chargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			chargeCode2.AC_Code = "BACHRG";
			chargeCode2.AC_Desc = "A BANK CHARGE";

			glHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader1.AG_AccountNum = "1991.01.10";
			glHeader1.AG_Description = "BANK ACCOUNT";

			glHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			glHeader2.AG_AccountNum = "9799.77.99";
			glHeader2.AG_Description = "CHEQUE ACCOUNT";

			Factory.Save();
		}

		AccChargeCode chargeCode1;
		AccChargeCode chargeCode2;
		AccGLHeader glHeader1;
		AccGLHeader glHeader2;

		protected GenericChargeFilterBusinessObject FilterBizO
		{
			get { return CachedBusinessObject as GenericChargeFilterBusinessObject; }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GenericChargeFilterBusinessObject(GenericChargeFilterBusinessObject.ElementType.All);
		}

		#endregion
	}
}
