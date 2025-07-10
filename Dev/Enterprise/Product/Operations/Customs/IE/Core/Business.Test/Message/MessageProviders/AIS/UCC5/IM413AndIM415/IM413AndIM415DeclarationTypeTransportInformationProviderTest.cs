using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415DeclarationTypeTransportInformationProviderTest : DataProviderTestCase<IM413AndIM415DeclarationTypeTransportInformationProvider>
	{
		public void TestBorderTransportMode()
		{
			SetUpTestData();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("BorderTransportMode(Translated)", "1", Provider.BorderTransportMode);
		}

		public void TestActiveBorderTransportMeansNationality()
		{
			SetUpTestData();
			declaration.JE_RN_NKTransportNationality = Core.Constants.CountryCodes.Ireland;
			AssertEquals("ActiveBorderTransportMeansNationality", "IE", Provider.ActiveBorderTransportMeansNationality);
		}

		protected override IM413AndIM415DeclarationTypeTransportInformationProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415DeclarationTypeTransportInformationProvider(declaration);
		}

		void SetUpTestData()
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

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
