using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryLineFeeCollection))]
	public class CusEntryLineFeeCollectionTest : Customs.Business.Testing.CusEntryLineFeeCollectionTest
	{
		public void TestGetTotalPayableDutyAndTax()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			CusEntryLineFee countervailingDuty = entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingDuty, 100m);
			CusEntryLineFee countervailingSecurityAmount = entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.CountervailingSecurityAmount, 110m);
			countervailingSecurityAmount.CF_IsLandedCostOnly = true;
			CusEntryLineFee dumpingDuty = entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DumpingDuty, 120m);
			CusEntryLineFee dutyAmount = entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.DutyAmount, 130m);
			CusEntryLineFee gSTDeferred = entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.GSTDeferred, 140m);//not payable
			gSTDeferred.CF_IsLandedCostOnly = true;
			CusEntryLineFee lCTAmount = entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.LCTAmount, 150m);
			CusEntryLineFee totalDutyTaxForLine = entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.TotalDutyTaxForLine, 160m);
			totalDutyTaxForLine.CF_IsLandedCostOnly = true;
			CusEntryLineFee gSTAmount = entryLine.Fees.AddOrUpdate(CusEntryChargeTypeList.Codes.GSTAmount, 100m);
			gSTAmount.CF_IsLandedCostOnly = true;

			AssertEquals("IsPayable, CountervailingDuty", true, countervailingDuty.IsPayableToCustomsForLine);
			AssertEquals("IsPayable, CountervailingSecurityAmount", false, countervailingSecurityAmount.IsPayableToCustomsForLine);
			AssertEquals("IsPayable, DumpingDuty", true, dumpingDuty.IsPayableToCustomsForLine);
			AssertEquals("IsPayable, DutyAmount", true, dutyAmount.IsPayableToCustomsForLine);
			AssertEquals("IsPayable, GSTDeferred", false, gSTDeferred.IsPayableToCustomsForLine);
			AssertEquals("IsPayable, LCTAmount", true, lCTAmount.IsPayableToCustomsForLine);
			AssertEquals("IsPayable, TotalDutyTaxForLine", false, totalDutyTaxForLine.IsPayableToCustomsForLine);
			AssertEquals("IsPayable, GSTAmount", true, gSTAmount.IsPayableToCustomsForLine);

			AssertEquals("GetTotalPayableDutyTax", 500m, entryLine.Fees.GetTotalPayableDutyTax());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			return new CusEntryLineFeeCollection(entryLine, entryLine.Factory);
		}
	}
}
