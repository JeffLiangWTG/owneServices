using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseEntryCreationStrategyTest : EntryCreationStrategyTest<ImportLicenseEntryCreationForTesting>
	{
		public void TestGetKeyForLine()
		{
			var supplier = Factory.New<OrgHeader>();
			var manufacturer = Factory.New<OrgHeader>();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoice.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			invoice.ExchangeHedgeType = ExchangeHedgeList.Codes._1;

			invoice.ExchangeHedgeReason = "1";
			invoice.ExchangeHedgePaymentMethod = "1";
			invoice.ExchangeHedgeReason = "1";
			invoice.ExchangeHedgeFinancialInstitution = "1";
			invoice.ExchangeHedgePaymentDeadline = 1;

			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.ImportLicenseNumber = "123456";
			invoiceLine.NaladiHs = "NaladiHs";
			invoiceLine.JI_UsedMaterialRegime = "1";
			invoiceLine.JI_UsedMaterialOperationType = "XX";
			invoiceLine.DrawbackModality = "1";
			invoiceLine.DrawbackCANumber = "123";
			invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._2;
			invoiceLine.ManufacturerDocAddressPK = manufacturer.MainAddress.PK;
			invoiceLine.DutyTaxRegime = "1";
			invoiceLine.DutyLegalBase = "A";
			invoiceLine.JI_CountryOfOrigin = "CV";
			invoiceLine.JI_SecondaryPreference = "EGITO";

			var consentingProcess = invoiceLine.ConsentingProcessCollection.AddNew();
			consentingProcess.CSI_ReferenceNumber = "RefNumber";
			consentingProcess.CSI_CustomsOffice = "EPJ1101";

			var tariffDetach = invoiceLine.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "999";

			var mergeKeys = new ImportLicenseEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine).Keys;
			CombineAssertions("Merge Keys for NON", () =>
			{
				AssertEquals(4, mergeKeys.Count);
				Assert("Keys contain JI_PK", mergeKeys.Contains(invoiceLine.PK));
			});

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;
			mergeKeys = new ImportLicenseEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine).Keys;

			CombineAssertions("Merge Keys for TRD", () =>
			{
				AssertEquals(26, mergeKeys.Count);

				AssertEquals("Keys contain JI_Tariff", new ZString("01010101"), mergeKeys[4]);
				AssertEquals("Keys contain JZ_IncoTerm", BRIncoTermList.Codes.FOB, mergeKeys[5]);
				AssertEquals("Keys contain JZ_RX_NKInvoice_Currency", Core.Constants.CurrencyCodes.Brazil, mergeKeys[6]);
				AssertEquals("Keys contain NaladiHs", new ZString("NaladiHs"), mergeKeys[7]);
				AssertEquals("Keys contain JI_UsedMaterialRegime", new ZString("1"), mergeKeys[8]);
				AssertEquals("Keys contain JI_UsedMaterialOperationType", new ZString("XX"), mergeKeys[9]);
				AssertEquals("Keys contain DrawbackModality", new ZString("1"), mergeKeys[10]);
				AssertEquals("Keys contain DrawbackCANumber", new ZString("123"), mergeKeys[11]);
				AssertEquals("Keys contain TariffDetachConcatenated", new ZString("999"), mergeKeys[12]);
				AssertEquals("Keys contain ConsentingProcessConcatenated", new ZString("RefNumber|EPJ1101"), mergeKeys[13]);
				AssertEquals("Keys contain ManufacturerDocAddressPK", manufacturer.MainAddress.PK, mergeKeys[14]);
				AssertEquals("Keys contain JZ_OA_SupplierAddress", supplier.MainAddress.PK, mergeKeys[15]);
				AssertEquals("Keys contain ExchangeHedgeType", new ZString("1"), mergeKeys[16]);
				AssertEquals("Keys contain ExchangeHedgePaymentMethod", new ZString("1"), mergeKeys[17]);
				AssertEquals("Keys contain ExchangeHedgeReason", new ZString("1"), mergeKeys[18]);
				AssertEquals("Keys contain ExchangeHedgePaymentDeadline", new ZDecimal(1), mergeKeys[19]);
				AssertEquals("Keys contain ExchangeHedgeFinancialInstitution", new ZString("1"), mergeKeys[20]);
				AssertEquals("Keys contain DutyTaxRegime", new ZString("1"), mergeKeys[21]);
				AssertEquals("Keys contain DutyLegalBase", new ZString("A"), mergeKeys[22]);
				AssertEquals("Keys contain JI_ManufacturerIndicator", new ZString("2"), mergeKeys[23]);
				AssertEquals("Keys contain Empty string when JI_ManufacturerIndicator != 3", ZString.Empty, mergeKeys[24]);
				AssertEquals("Keys contain JI_SecondaryPreference", new ZString("EGITO"), mergeKeys[25]);

				invoiceLine.JI_ManufacturerIndicator = ManufacturerIndicatorList.Codes._3;
				mergeKeys = new ImportLicenseEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine).Keys;
				AssertEquals("Keys contains JI_CountryOfOrigin when JI_ManufacturerIndicator == 3", new ZString("CV"), mergeKeys[24]);
			});
		}

		public void TestOnlyForGetMergeKey()
		{
			var supplier = Factory.New<OrgHeader>();
			var manufacturer = Factory.New<OrgHeader>();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			invoice.JZ_OH_Supplier = supplier.PK;
			invoice.JZ_OA_SupplierAddress = supplier.MainAddress.PK;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoice.JZ_IncoTerm = BRIncoTermList.Codes.FOB;

			invoice.ExchangeHedgeType = ExchangeHedgeList.Codes._1;

			invoice.ExchangeHedgeReason = "1";
			invoice.ExchangeHedgePaymentMethod = "1";
			invoice.ExchangeHedgeReason = "1";
			invoice.ExchangeHedgeFinancialInstitution = "1";
			invoice.ExchangeHedgePaymentDeadline = 1;

			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.ImportLicenseNumber = "123456";
			invoiceLine.NaladiHs = "NaladiHs";
			invoiceLine.JI_UsedMaterialRegime = "1";
			invoiceLine.JI_UsedMaterialOperationType = "XX";
			invoiceLine.DrawbackModality = "1";
			invoiceLine.DrawbackCANumber = "123";
			invoiceLine.ManufacturerDocAddressPK = manufacturer.MainAddress.PK;

			var consentingProcess = invoiceLine.ConsentingProcessCollection.AddNew();
			consentingProcess.CSI_ReferenceNumber = "RefNumber";
			consentingProcess.CSI_CustomsOffice = "EPJ1101";

			var tariffDetach = invoiceLine.TariffDetachs.AddNew();
			tariffDetach.CY_Code = "999";

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.TariffAndDescription;

			var mergeKeys = new ImportLicenseEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine);
			AssertEquals(invoiceLine.LastMergeKeyForImportLicenseEntry, mergeKeys);

			invoiceLine.JI_UsedMaterialRegime = "2";
			mergeKeys = new ImportLicenseEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine);
			AssertEquals(invoiceLine.LastMergeKeyForImportLicenseEntry, mergeKeys);

			invoiceLine.JI_UsedMaterialRegime = "1";
			mergeKeys = new ImportLicenseEntryCreationStrategy(declaration, true).GetKeyForLine(invoiceLine);
			AssertNotEquals(invoiceLine.LastMergeKeyForImportLicenseEntry, mergeKeys);
		}

		protected override ImportLicenseEntryCreationForTesting CreateNewEntryCreationStrategy(JobDeclaration declaration)
		{
			return new ImportLicenseEntryCreationForTesting(declaration);
		}

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.ImportLicense;

		protected override ZString ExpectedMessageTypeToNewEntryHeader => MessageTypeList.Codes.LIC;
	}

	class ImportLicenseEntryCreationForTesting : ImportLicenseEntryCreationStrategy, IEntryCreationStrategyForTesting
	{
		public ImportLicenseEntryCreationForTesting(JobDeclaration declaration) : base(declaration)
		{
		}

		public bool UseAdditionalEntryLineLinksExposed => base.UseAdditionalEntryLineLinks;

		public bool IsActiveExposed => base.IsActiveCore;

		public string MessageTypeToNewEntryHeaderExposed => base.CH_MessageTypeToNewEntryHeader;

		public void ClearReferenceToEntryLineWhenLineIsNotValidForMergeExposed(BaseJobComInvoiceLine invoiceLine) => ClearReferenceToEntryLineWhenLineIsNotValidForMerge(invoiceLine);
	}
}
