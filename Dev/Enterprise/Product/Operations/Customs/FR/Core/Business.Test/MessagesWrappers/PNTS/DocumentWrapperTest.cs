using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class DocumentWrapperTest : Customs.Business.Testing.DataProviderTestCase<DocumentWrapper>
	{
		public void TestReferenceNumber()
		{
			AssertEquals("ReferenceNumber should equal ABL_BillNumber.", "1X", Provider.ReferenceNumber);
		}

		public void TestType()
		{
			AssertEquals("Type should equal TypeOfBillDocument.", "N701", Provider.Type);
		}

		protected override DocumentWrapper GetProvider()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "1X";
			bill.ABL_SpecialCargoCode = "111";
			bill.TypeOfBillDocument = "N701";

			return DocumentWrapper.New(bill);
		}
	}
}
