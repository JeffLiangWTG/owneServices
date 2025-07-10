using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ChargesComparerTest : TestCaseWithFactory
	{
		public void TestCompare()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var charge1 = invoiceHeader.Charges.AddNew();
			var charge2 = invoiceHeader.Charges.AddNew();
			var charge3 = invoiceHeader.Charges.AddNew();
			var charge4 = invoiceHeader.Charges.AddNew();

			charge1.J7_DistributeBy = ChargeDistributeByList.Codes.FOB;
			charge2.J7_DistributeBy = ChargeDistributeByList.Codes.FOB;
			charge3.J7_DistributeBy = ChargeDistributeByList.Codes.NetWeight;
			charge4.J7_DistributeBy = Common.ChargeDistributeByList.Codes.Weight;

			AssertEquals("FOB = FOB", 0, new ChargesComparer().Compare(charge1, charge2));
			AssertEquals("FOB > NWT", 1, new ChargesComparer().Compare(charge1, charge3));
			AssertEquals("FOB > WGT", 1, new ChargesComparer().Compare(charge1, charge4));
			AssertEquals("NWT < FOB", -1, new ChargesComparer().Compare(charge3, charge1));
			AssertEquals("WGT < FOB", -1, new ChargesComparer().Compare(charge4, charge1));
			AssertEquals("WGT = NWT", 0, new ChargesComparer().Compare(charge3, charge4));
		}
	}
}
