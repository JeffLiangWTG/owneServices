using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ImportUCC5EntryCreationStrategyTest : EU.Business.Declaration.Testing.EntryCreationStrategyTest
	{
		public override void TestGetKeyForHeader()
		{
			base.TestGetKeyForHeader();

			invoice.JZ_IncoTerm = "FOB";
			invoice.ZG_AgreedPlaceCode = "HERE";
			invoice.JZ_IncoTermPlace = "BOB'S PLACE";
			invoice.JZ_AdditionalTerms = "THESE Terms";
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			invoice.JZ_ValuationCode = NatureOfTransactionList.Codes._11;
			invoice.JZ_UCR = "UCR123";

			var key = entryCreationStrategy.GetKeyForHeader(invoiceLine);

			CombineAssertions("MergeKeys for Header", () =>
			{
				Assert("JZ_IncoTerm", key.Contains(new ZString("FOB")));
				Assert("ZG_AgreedPlaceCode", key.Contains(new ZString("HERE")));
				Assert("JZ_IncoTermPlace", key.Contains(new ZString("BOB'S PLACE")));
				Assert("JZ_AdditionalTerms", key.Contains(new ZString("THESE Terms")));
				Assert("JZ_RX_NKInvoice_Currency", key.Contains(new ZString(Core.Constants.CurrencyCodes.EuropeanUnion)));
				Assert("JZ_ValuationCode", key.Contains(new ZString("11")));
				Assert("JZ_UCR", key.Contains(new ZString("UCR123")));
			});
		}

		public void TestMergeKeyForInvoiceLinesWithAtLeastOneExciseTax()
		{
			var dec = GetJobDeclarationForTest();
			var inv = dec.Invoices.AddNew();
			var invLine1 = inv.JobComInvoiceLines.AddNew();
			inv.JobComInvoiceLines.AddNew();

			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, dec.ActiveEntryHeaders[0].MergedLines.Count);

			invLine1.CusLineTariffDetails.AddNew();
			dec.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			AssertEquals(2, dec.ActiveEntryHeaders[0].MergedLines.Count);
		}

		protected override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTest()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			jobDeclaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;

			return jobDeclaration;
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryCreationStrategy = new ImportUCC5EntryCreationStrategy(declaration);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		ImportUCC5EntryCreationStrategy entryCreationStrategy;
	}
}
