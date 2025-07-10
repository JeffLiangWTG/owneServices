using System;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class RF415HeaderProviderTest : DataProviderTestCase<RF415HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Message sending object missing", () => new RF415HeaderProvider(null));
		}

		public void TestHeader()
		{
			var header = Provider.Header;
			AssertType<RF415HeaderTypeProvider>(header);
			AssertSame("Cached", header, Provider.Header);
		}

		public void TestAttachedDocuments()
		{
			var attachedDocuments = Provider.AttachedDocuments;
			AssertType<RF415AttachedDocumentProvider[]>(attachedDocuments);
			AssertEquals(1, attachedDocuments.Count);
		}

		public void TestParties()
		{
			var parties = Provider.Parties;
			AssertType<PartiesProvider>(parties);
			AssertSame("Cached", parties, Provider.Parties);
		}

		public void TestDatesPlaces()
		{
			var datesPlaces = Provider.DatesPlaces;
			AssertType<DatesPlacesProvider>(datesPlaces);
			AssertSame("Cached", datesPlaces, Provider.DatesPlaces);
		}

		public void TestLegalBasisCodes()
		{
			sendingObject.LegalBasis = "Legal Basis";
			AssertEquals("LegalBasisCodes", "Legal Basis", Provider.LegalBasisCodes);
		}

		public void TestDescriptionOfGrounds()
		{
			sendingObject.DescriptionOfGrounds = "Description of grounds";
			AssertEquals("DescriptionOfGrounds", "Description of grounds", Provider.DescriptionOfGrounds);
		}

		public void TestBankDetails()
		{
			sendingObject.BankDetails = "Bank and account details";
			AssertEquals("BankDetails", "Bank and account details", Provider.BankDetails);
		}

		public void TestGoodsInformation()
		{
			var asycudaBill = sendingObject.Bill;
			asycudaBill.PackedItems.AddNew();
			asycudaBill.PackedItems.AddNew();
			var goodsInformation = Provider.GoodsInformation;
			AssertType<RF415GoodsInformationProvider[]>(goodsInformation);
			AssertEquals("Count", 2, goodsInformation.Count);
			AssertSame("Cached", goodsInformation, Provider.GoodsInformation);
		}

		public void TestCustomsProcedure()
		{
			AssertNull("CustomsProcedure", Provider.CustomsProcedure);
		}

		public void TestAmountOfDutiesToBeRepaid()
		{
			sendingObject.Amount = 10.24m;
			var amountOfDutiesToBeRepaid = Provider.AmountOfDutiesToBeRepaid;
			AssertEquals("Amount", 10.24m, amountOfDutiesToBeRepaid.Amount);
			AssertEquals("Currency", "EUR", amountOfDutiesToBeRepaid.Currency);
			AssertSame("Cached", amountOfDutiesToBeRepaid, Provider.AmountOfDutiesToBeRepaid);
		}

		public void TestAdditionalInformation()
		{
			sendingObject.AdditionalInformation = "test additional information";
			AssertEquals("AdditionalInformation", "test additional information", Provider.AdditionalInformation);
		}

		public void TestFallbackProcedure()
		{
			AssertNull("To be added in future workflow", Provider.FallbackProcedure);
		}

		[TestDate(2024, 05, 15, 0, 0, 0)]
		public void TestPreparationDateAndTime()
		{
			AssertEquals("Preparation date and time", new DateTime(2024, 05, 15, 0, 0, 0), Provider.PreparationDateAndTime);
		}

		public void TestMRN()
		{
			bill.MovementReferenceNumber = "TestMRN";
			AssertEquals("MRN", "TestMRN", Provider.MRN);
		}
		protected override RF415HeaderProvider GetProvider()
		{
			return new RF415HeaderProvider(sendingObject);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();

			sendingObject = new RF415MessageSendingObject(bill);
			sendingObject.DocumentSendingObjectCollection.AddNew();
		}

		RF415MessageSendingObject sendingObject;
		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
