using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportEntryCreationStrategyTest : EntryCreationStrategyTest<ImportEntryCreationForTesting>
	{
		public void TestGetKeyForLine()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			var supplier = Factory.New<OrgHeader>();
			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoice.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			invoice.JZ_ValuationCode = ValuationCodeList.Codes._01;
			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.NaladiHs = "01010000";
			invoiceLine.NaladiNcca = "01010002";
			invoiceLine.FMMBenefit = "E";

			var nve = invoiceLine.NVECusCodeDataCollection.AddNew();
			nve.CY_Code = "AA";
			nve.CY_Data = "0001";
			nve.CY_Order = 6;

			var tariffDetach = invoiceLine.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "999";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var mergeKey = new ImportEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine);
			CombineAssertions("Merge Keys for NON", () =>
			{
				AssertEquals(4, mergeKey.Keys.Count);
				Assert("Keys contain JI_PK", mergeKey.Keys.Contains(invoiceLine.PK));
			});

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;
			mergeKey = new ImportEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine);
			CombineAssertions("Merge Keys for CLS", () =>
			{
				AssertEquals(14, mergeKey.Keys.Count);

				AssertEquals("Keys contain JI_Tariff", new ZString("01010101"), mergeKey.Keys[4]);
				AssertEquals("Keys contain JZ_OH_Supplier", supplier.PK, mergeKey.Keys[5]);
				AssertEquals("Keys contain JZ_RX_NKInvoice_Currency", new ZString("BRL"), mergeKey.Keys[6]);
				AssertEquals("Keys contain JZ_IncoTerm", new ZString("FOB"), mergeKey.Keys[7]);
				AssertEquals("Keys contain JZ_ValuationCode", new ZString(ValuationCodeList.Codes._01), mergeKey.Keys[8]);
				AssertEquals("Keys contain TariffDetachConcatenated", new ZString("999"), mergeKey.Keys[9]);
				AssertEquals("Keys contain NVEConcatenated", new ZString("6|AA|0001"), mergeKey.Keys[10]);
				AssertEquals("Keys contain NaladiHs", new ZString("01010000"), mergeKey.Keys[11]);
				AssertEquals("Keys contain NaladiNcca", new ZString("01010002"), mergeKey.Keys[12]);
				AssertEquals("Keys contain FMMBenefit", new ZString("E"), mergeKey.Keys[13]);
			});
		}

		protected override ImportEntryCreationForTesting CreateNewEntryCreationStrategy(JobDeclaration declaration)
		{
			return new ImportEntryCreationForTesting(declaration);
		}

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.Import;

		protected override ZString ExpectedMessageTypeToNewEntryHeader => MessageTypeList.Codes.CDI;

		protected override ZBool ExpectedAdditionalEntryLineLinks => false;
	}

	class ImportEntryCreationForTesting : ImportEntryCreationStrategy, IEntryCreationStrategyForTesting
	{
		public ImportEntryCreationForTesting(JobDeclaration declaration) : base(declaration)
		{
		}

		public bool UseAdditionalEntryLineLinksExposed => base.UseAdditionalEntryLineLinks;

		public bool IsActiveExposed => base.IsActiveCore;

		public string MessageTypeToNewEntryHeaderExposed => base.CH_MessageTypeToNewEntryHeader;

		public void ClearReferenceToEntryLineWhenLineIsNotValidForMergeExposed(BaseJobComInvoiceLine invoiceLine) => ClearReferenceToEntryLineWhenLineIsNotValidForMerge(invoiceLine);
	}
}
