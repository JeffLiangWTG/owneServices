using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using AccConstants = Enterprise.Accounting.Business.AccountingUtils;

namespace Enterprise.Accounting.Business.Base.Unmatching.Testing
{
	[TestedType(typeof(ARMatchGroupFilterHelper))]
	public class ARMatchGroupFilterHelperTestCase : MatchGroupFilterHelperTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ARMatchGroupFilterHelper(Factory);
		}

		#region TestOrganisationFilter

		public void TestOrganisationFilter()
		{
			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			SetupDataSet();

			ARINV1.AH_OH = TestOrg1.PK;
			ARCRD1.AH_OH = TestOrg2.PK;
			ARINV2.AH_OH = TestOrg2.PK;
			ARCRD2.AH_OH = TestOrg2.PK;

			Matchlink1.AP_MatchGroupNum = "M00001000";
			Matchlink2.AP_MatchGroupNum = "M00001000";
			Matchlink3.AP_MatchGroupNum = "M00001001";
			Matchlink4.AP_MatchGroupNum = "M00001001";

			Factory.Save();

			FilterHelper.Organisation = TestOrg1.PK;

			DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);

			AssertEquals("There should be 1 match group", 1, dynBizOs.Count);
			AssertEquals("The match group number should be M00001000", "M00001000", dynBizOs[0][UnmatchingRow.Schema.MatchGroupNum]);

			FilterHelper.Organisation = TestOrg2.PK;
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 2 match groups", 2, dynBizOs.Count);
		}

		#endregion

		#region TestFilterMatchGroupNumAndTransactionNum

		public void TestFilterMatchGroupNumAndTransactionNum()
		{
			SetupDataSet("00001000", "00001002", "00001001", "00001003");
			Factory.Save();

			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink link1 = group.AddNew();
			link1.AP_AH = Matchlink1.AP_AH;
			link1.AP_MatchGroupNum = "M00001000";
			Matchlink1.Delete();

			TransactionMatchLink link2 = group.AddNew();
			link2.AP_AH = Matchlink2.AP_AH;
			link2.AP_MatchGroupNum = "M00001000";
			TestObjectCreator.SetupMatchLinkMatchDate(group);
			Matchlink2.Delete();

			TransactionMatchLink link3 = group.AddNew();
			link3.AP_AH = Matchlink3.AP_AH;
			link3.AP_MatchGroupNum = "M00001001";
			link3.AP_MatchDate = new ZDateTime(2004, 4, 3);
			Matchlink3.Delete();

			TransactionMatchLink link4 = group.AddNew();
			link4.AP_AH = Matchlink4.AP_AH;
			link4.AP_MatchGroupNum = "M00001001";
			link4.AP_MatchDate = new ZDateTime(2004, 4, 3);
			Matchlink4.Delete();

			Factory.Save();
			DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);

			AssertEquals("There should be 2 match groups", 2, dynBizOs.Count);
			FilterHelper.NumberFilter = "M00001001";
			FilterHelper.NumberType = AccConstants.NumberFilterTypes.MatchGroupNumber;

			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 1 match group", 1, dynBizOs.Count);
			AssertEquals("The match date should be 2004/4/3", new ZDateTime(2004, 4, 3), dynBizOs[0][UnmatchingRow.Schema.MatchDate]);

			FilterHelper.NumberType = AccConstants.NumberFilterTypes.TransactionNumber;
			FilterHelper.NumberFilter = "00001000";
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 1 match group", 1, dynBizOs.Count);
			AssertEquals("Number of the group should be M00001000", "M00001000", dynBizOs[0][UnmatchingRow.Schema.MatchGroupNum]);

			FilterHelper.NumberType = AccConstants.NumberFilterTypes.TransactionNumber;
			FilterHelper.NumberFilter = "00001002";
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be one match group", 1, dynBizOs.Count);
			AssertEquals("Number of the group should be M00001001", "M00001001", dynBizOs[0][UnmatchingRow.Schema.MatchGroupNum]);
		}

		#endregion

		#region TestDateFiltering

		public void TestDateFiltering()
		{
			SetupDataSet();
			ARINV1.AH_InvoiceDate = new ZDateTime(2004, 3, 4);
			ARCRD1.AH_InvoiceDate = new ZDateTime(2004, 5, 6);
			ARINV2.AH_InvoiceDate = new ZDateTime(2004, 7, 8);
			ARCRD2.AH_InvoiceDate = new ZDateTime(2004, 9, 10);

			Factory.Save();

			FilterHelper.DateType = AccConstants.DateFilterTypes.TransactionDate;
			FilterHelper.FromDateFilter = new ZDateTime(2004, 3, 3);
			FilterHelper.ToDateFilter = new ZDateTime(2004, 6, 6);

			DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);

			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 1 match group in the collection", 1, dynBizOs.Count);
			AssertEquals("The match group should be M00001000", "M00001000", dynBizOs[0][UnmatchingRow.Schema.MatchGroupNum]);

			FilterHelper.ToDateFilter = new ZDateTime(2004, 8, 9);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 2 match groups in the collection", 2, dynBizOs.Count);

			FilterHelper.ToDateFilter = new ZDateTime(2004, 3, 3);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be no match groups in the collection", 0, dynBizOs.Count);

			FilterHelper.ToDateFilter = new ZDateTime(2004, 8, 8);
			FilterHelper.FromDateFilter = new ZDateTime(2004, 6, 6);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 1 match group in the collection", 1, dynBizOs.Count);
			AssertEquals("the match group should be M00001001", "M00001001", dynBizOs[0][UnmatchingRow.Schema.MatchGroupNum]);
		}

		#endregion

		#region TestFilterStringGroupBy

		public void TestFilterStringGroupBy()
		{
			SetupDataForFilteringTest();

			DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 2 AR MatchGroup rows in the collection: M00001334 and M00001442", 2, dynBizOs.Count);

			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink link = group.AddNew();
			link.AP_AH = Matchlink4.AP_AH;
			link.AP_MatchDate = ZDateTime.Now.AddDays(1);
			Matchlink4.Delete();
			Factory.Save();

			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 3 elements in the collection due to the GROUP BY MatchDate", 3, dynBizOs.Count);
		}

		#endregion

		#region TestFilterDoesNotExcludePayment

		public void TestFilterDoesNotExcludePayment()
		{
			SetupDataForDoesNotExcludePaymentTests();

			DynamicBusinessObjectCollection dynBizOs = new DynamicBusinessObjectCollection(Factory);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 1 MatchGroup row in the collection: M00002551", 1, dynBizOs.Count);

			UnmatchingRowCollection testRows = new UnmatchingRowCollection(Factory);
			testRows.SetFilterHelper(FilterHelper);
			testRows.Load();
			Assert("Collection should contain MatchGroup M00002551", testRows.ContainsMatchGroupNumber("M00002551"));
		}

		#endregion
	}
}
