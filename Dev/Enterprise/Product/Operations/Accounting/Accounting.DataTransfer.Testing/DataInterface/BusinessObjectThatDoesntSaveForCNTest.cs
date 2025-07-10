using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataInterface.Testing
{
	[TestedType(typeof(BusinessObjectThatDoesntSaveForCN))]
	class BusinessObjectThatDoesntSaveForCNTest : NonPersistentBusinessObjectTestCase
	{
		public void TestChartType()
		{
			BusinessObjectThatDoesntSaveForCN bizo = new BusinessObjectThatDoesntSaveForCN(Factory) { ChartType = true };
			AssertEquals("ChartType", true, bizo.ChartType);

			bizo.ChartType = false;
			AssertEquals("ChartType", false, bizo.ChartType);
		}

		public void TestBranchCode()
		{
			var biz = new BusinessObjectThatDoesntSaveForCN(Factory);
			AssertEquals("BranchCode", ZString.Empty, biz.BranchCode);
			biz.BranchCode = "ABC";
			AssertEquals("BranchCode", "ABC", biz.BranchCode);
			ZGuid id = ZGuid.NewZGuid();
			var bizo = new BusinessObjectThatDoesntSaveForCN(Factory) { BranchPK = id, BranchCode = "BRH" };
			AssertEquals("BranchCode", "BRH", bizo.BranchCode);
			AssertEquals("BranchPK", id, bizo.BranchPK);
		}

		[TestDate(2012, 07, 22)]
		public void TestPeriod()
		{
			BusinessObjectThatDoesntSaveForCN bizo = new BusinessObjectThatDoesntSaveForCN(Factory) { Period = 200603 };
			AssertEquals("Period", 200603, bizo.Period);

			bizo.Period = 200909;
			AssertEquals("Period", 200909, bizo.Period);
		}

		[TestDate(2012, 04, 17)]
		public void TestToFromDates()
		{
			BusinessObjectThatDoesntSaveForCN bizo = new BusinessObjectThatDoesntSaveForCN(Factory) { FromDate = new ZDateTime(2006, 03, 1, 0, 0, 0), ToDate = new ZDateTime(2006, 04, 1, 0, 0, 0) };
			AssertEquals("From date", new ZDateTime(2006, 03, 1, 0, 0, 0), bizo.FromDate);
			AssertEquals("To date", new ZDateTime(2006, 04, 1, 0, 0, 0), bizo.ToDate);

			bizo.FromDate = new ZDateTime(2009, 4, 6, 4, 56, 0);
			bizo.ToDate = new ZDateTime(2009, 4, 6, 14, 56, 0);
			AssertEquals("From date", new ZDateTime(2009, 4, 6, 4, 56, 0), bizo.FromDate);
			AssertEquals("To date", new ZDateTime(2009, 4, 6, 14, 56, 00), bizo.ToDate);
		}
	}
}
