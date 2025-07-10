using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class RF415HeaderProviderTest : DataProviderTestCase<RF415HeaderProvider>
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
			AssertType<PartiesProvider>(parties);
			AssertSame("Cached", parties, Provider.Parties);
		}

		public void TestDatesPlaces()
		{
			var datesPlaces = Provider.DatesPlaces;
			AssertType<DatesPlacesProvider>(datesPlaces);
			AssertSame("Cached", datesPlaces, Provider.DatesPlaces);
		}

		public void TestMRN()
		{
			SetUpTestData();
			entryHeader.MovementReferenceNumberSetter("12MRN345CDEFG678R9");
			AssertEquals("12MRN345CDEFG678R9", Provider.MRN);
		}

		public void TestLegalBasisCodes()
		{
			SetUpTestData();
			sendingAction.LegalBasis = "A01";
			AssertEquals("A01", Provider.LegalBasisCodes);
		}

		public void TestDescriptionOfGrounds()
		{
			SetUpTestData();
			sendingAction.DescriptionOfGrounds = "Description of Grounds";
			AssertEquals("Description of Grounds", Provider.DescriptionOfGrounds);
		}

		public void TestBankDetails()
		{
			SetUpTestData();
			sendingAction.BankDetails = "Bank and account details";
			AssertEquals("Bank and account details", Provider.BankDetails);
		}

		public void TestGoodsInformation()
		{
			SetUpTestData();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var goodsInformation = Provider.GoodsInformation;
			AssertEquals("Count", 2, goodsInformation.Count);
			AssertType<RF415GoodsInformationProvider[]>(goodsInformation);
			AssertSame("Cached", goodsInformation, Provider.GoodsInformation);
		}

		public void TestCustomsProcedure()
		{
			SetUpTestData();
			entryInstruction.CEI_Style = "H1";
			AssertEquals("CustomsProcedure", "H1", Provider.CustomsProcedure);
		}

		public void TestAmountOfDutiesToBeRepaid()
		{
			SetUpTestData();
			sendingAction.Amount = 10.24m;
			var amountOfDutiesToBeRepaid = Provider.AmountOfDutiesToBeRepaid;
			AssertEquals("Amount", 10.24m, amountOfDutiesToBeRepaid.Amount);
			AssertEquals("Currency", "EUR", amountOfDutiesToBeRepaid.Currency);
		}

		public void TestAdditionalInformation()
		{
			SetUpTestData();
			sendingAction.AdditionalInformation = "Additional Information";
			AssertEquals("Additional Information", Provider.AdditionalInformation);
		}

		public void TestFallbackProcedure()
		{
			AssertNull(Provider.FallbackProcedure);
		}

		protected override RF415HeaderProvider GetProvider()
		{
			SetUpTestData();
			return new RF415HeaderProvider(sendingAction);
		}

		void SetUpTestData()
		{
			if (sendingAction == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				sendingAction = new RefundApplicationMessageSendingAction(entryHeader);
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		RefundApplicationMessageSendingAction sendingAction;
	}
}
