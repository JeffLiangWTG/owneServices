using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CW1CusEntryLineFeeCollection))]
	sealed class CW1CusEntryLineFeeCollectionTest : Customs.Business.Testing.CusEntryLineFeeCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			CusEntryHeader entryHeader = testDec.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			return new CW1CusEntryLineFeeCollection(entryLine, Factory);
		}

		public void TestCF_SourceType()
		{
			var cusEntryLine = Factory.NewWithValidTestData<CusEntryLine>();
			var cw1CusEntryLineFee = Factory.New<CusEntryLineFee>();
			cw1CusEntryLineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			cw1CusEntryLineFee.CF_ChargeType = "GST";
			cw1CusEntryLineFee.CF_ChargeAmount = 1.1m;
			cw1CusEntryLineFee.CF_CL = cusEntryLine.PK;

			var cusEntryLineFee = Factory.New<CusEntryLineFee>();
			cusEntryLineFee.CF_ChargeType = "DTY";
			cusEntryLineFee.CF_ChargeAmount = 1.2m;
			cusEntryLineFee.CF_CL = cusEntryLine.PK;

			var cusCusEntryLineFee = Factory.New<CusEntryLineFee>();
			cusCusEntryLineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CUS;
			cusCusEntryLineFee.CF_ChargeType = "DTY";
			cusCusEntryLineFee.CF_ChargeAmount = 2.2m;
			cusCusEntryLineFee.CF_CL = cusEntryLine.PK;
			Factory.Save();

			var collection = new CW1CusEntryLineFeeCollection(cusEntryLine, Factory);
			collection.Load();
			AssertEquals(2, collection.Count);
			AssertContainsExactElementsInAnyOrder(new[] { cusEntryLineFee, cw1CusEntryLineFee }, collection);

			var fee = collection.AddNew();
			AssertEquals(CusEntryLineFeeSourceCodeList.Codes.CW1, fee.CF_Source);
		}
	}
}
