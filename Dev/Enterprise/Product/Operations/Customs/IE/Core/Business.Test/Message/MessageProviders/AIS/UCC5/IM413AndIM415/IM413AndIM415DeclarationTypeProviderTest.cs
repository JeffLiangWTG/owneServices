using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	class IM413AndIM415DeclarationTypeProviderTest : DataProviderTestCase<IM413AndIM415DeclarationTypeProvider>
	{
		public void TestMessageType()
		{
			SetUpTestData();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			AssertEquals("MessageType", "H1", Provider.MessageType);
		}

		public void TestDeclarationType()
		{
			SetUpTestData();
			declaration.JE_EntryStyle = MessageSubTypeListExp.Codes.ExportOrReExportOfGoodsOutsideOfTheCustomsTerritoryOfTheUnion;
			AssertEquals("DeclarationType", "EX", Provider.DeclarationType);
		}

		public void TestAdditionalDeclarationType()
		{
			SetUpTestData();
			entryInstruction.CEI_SubStyle = "C";
			AssertEquals("AdditionalDeclarationType", "C", Provider.AdditionalDeclarationType);
		}

		public void TestLRN()
		{
			SetUpTestData();
			entryHeader.CH_BGMReference = "LRN2343234242";
			AssertEquals("LRN", "LRN2343234242", Provider.LRN);
		}

		public void TestValuationInformation()
		{
			SetUpTestData();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoiceLine.JI_LinePrice = 150m;
			AssertEquals("ValuationInformation.InvoiceCurrency", Core.Constants.CurrencyCodes.Australia, Provider.ValuationInformation.InvoiceCurrency);
		}

		public void TestGoodsInformation()
		{
			SetUpTestData();
			declaration.JE_TotalNoOfPacks = 22;
			AssertEquals("GoodsInformation.TotalPackageNumber", "22", Provider.GoodsInformation.TotalPackageNumber);
		}

		public void TestTransportInformation()
		{
			SetUpTestData();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TransportInformation.BorderTransportMode(Translated)", "1", Provider.TransportInformation.BorderTransportMode);
		}

		public void TestCustomsOffices()
		{
			SetUpTestData();
			declaration.JE_CustomsOffice = "IEDUB100";
			AssertEquals("CustomsOffices.CustomsOfficeLodgement", "IEDUB100", Provider.CustomsOffices.CustomsOfficeLodgement);
		}

		public void TestPaymentMethod()
		{
			SetUpTestData();
			declaration.JE_PaymentMethod = "E";
			AssertEquals("Payment Method", "E", Provider.PaymentMethod);
		}

		public void TestParties()
		{
			AssertType<IM413AndIM415DeclarationTypePartiesProvider>(Provider.Parties);
		}

		public void TestAuth8F()
		{
			SetUpTestData();
			entryInstruction.DetailsOfPlannedActivities = "Sample text";
			AssertEquals("Auth8F", "Sample text", Provider.Auth8F.DetailsOfPlannedActivities);
		}

		protected override IM413AndIM415DeclarationTypeProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415DeclarationTypeProvider(new EntryHeaderWrapper(entryHeader), false);
		}

		protected void SetUpTestData()
		{
			if (entryHeader == null)
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
			}
		}

		protected JobDeclaration declaration;
		protected CusEntryInstruction entryInstruction;
		protected CusEntryHeader entryHeader;
		protected CusEntryLine entryLine;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;
	}
}
