using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class RF415HeaderProviderTest : DataProviderTestCase<RF415HeaderProvider>
	{
		public void TestIRF415Header()
		{
			Assert("Should implement IRF415Header", Provider is IRF415Header);
		}

		public void TestHeader()
		{
			var header = Provider.Header;
			AssertType<RF415HeaderTypeProvider>(header);
			AssertSame("Cached", header, Provider.Header);
		}

		public void TestAttachedDocuments()
		{
			AssertType<AttachedDocumentProvider[]>(Provider.AttachedDocuments);
		}

		public void TestParties()
		{
			var parties = Provider.Parties;
			AssertType<RF415PartiesProvider>(parties);
			AssertSame("Cached", parties, Provider.Parties);
		}

		public void TestDatesPlaces()
		{
			var datesPlaces = Provider.DatesPlaces;
			AssertType<RF415DatesPlacesTypeProvider>(datesPlaces);
			AssertSame("Cached", datesPlaces, Provider.DatesPlaces);
		}

		public void TestMRN()
		{
			SendingObject.Bill.MovementReferenceNumber = "TestMRN";
			var mrn = Provider.MRN;
			AssertEquals("TestMRN", mrn);
		}

		public void TestLegalBasisCodes()
		{
			SendingObject.LegalBasis = "A01";
			AssertEquals("A01", Provider.LegalBasisCodes);
		}

		public void TestDescriptionOfGrounds()
		{
			SendingObject.DescriptionOfGrounds = "Description of Grounds";
			AssertEquals("Description of Grounds", Provider.DescriptionOfGrounds);
		}

		public void TestBankDetails()
		{
			SendingObject.BankDetails = "Bank and account details";
			AssertEquals("Bank and account details", Provider.BankDetails);
		}

		public void TestGoodsInformation()
		{
			var asycudaBill = SendingObject.Bill;
			asycudaBill.PackedItems.AddNew();
			asycudaBill.PackedItems.AddNew();

			var goodsInformation = Provider.GoodsInformation;
			AssertEquals("Count", 2, goodsInformation.Count);
			AssertType<RF415GoodsInformationProvider[]>(goodsInformation);
			AssertSame("Cached", goodsInformation, Provider.GoodsInformation);
		}

		public void TestCustomsProcedure()
		{
			AssertNull(Provider.CustomsProcedure);
		}

		public void TestAmountOfDutiesToBeRepaid()
		{
			SendingObject.Amount = 10.241m;
			var amountOfDutiesToBeRepaid = Provider.AmountOfDutiesToBeRepaid;
			AssertEquals("Amount", 10.24m, amountOfDutiesToBeRepaid.Amount);
			AssertEquals("Currency", "EUR", amountOfDutiesToBeRepaid.Currency);
			AssertSame("Cached", amountOfDutiesToBeRepaid, Provider.AmountOfDutiesToBeRepaid);
		}

		public void TestAdditionalInformation()
		{
			SendingObject.AdditionalInformation = "Additional Information";
			AssertEquals("Additional Information", Provider.AdditionalInformation);
		}

		protected override RF415HeaderProvider GetProvider() => new RF415HeaderProvider(SendingObject);

		RF415MessageSendingObject SendingObject => sendingObject ?? (sendingObject = new RF415MessageSendingObject(Factory.New<AsycudaManifestHeader>().Bills.AddNew()));
		RF415MessageSendingObject sendingObject;
	}
}
