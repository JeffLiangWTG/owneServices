using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportUsageFeeEntryCreationStrategyTest : EntryCreationStrategyTest<ImportUsageFeeEntryCreationStrategyForTesting>
	{
		public void TestGetKeyForLine()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.MainAddress.CompanyName = "MANUFACTURER";
			manufacturer.MainAddress.OA_Email = "MANUFACTURER@TEST.COM";
			manufacturer.MainAddress.OA_Address1 = "MANUFACTURER ADDRESS 1";
			manufacturer.MainAddress.PrimaryOrgAddressAdditionalInfoDetail = "MANUFACTURER ADDITIONAL ADDRESS";
			manufacturer.MainAddress.OA_City = "MANUFACTURER CITY";
			manufacturer.MainAddress.OA_State = "MS";
			manufacturer.MainAddress.OA_RN_NKCountryCode = "CV";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceLine.JI_GoodsApplication = GoodsApplicationTypeList.Codes.Resale;
			invoiceLine.JI_GoodsCondition = GoodsConditionTypeList.Codes.GoodsMadeToOrder;

			var supplier = Factory.New<OrgHeader>();
			invoice.JZ_OH_Supplier = manufacturer.PK;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Brazil;
			invoice.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			invoice.JZ_ValuationCode = ValuationCodeList.Codes._01;
			invoice.JZ_OA_SupplierAddress = manufacturer.MainAddress.PK;
			invoice.ExchangeHedgeType = ExchangeHedgeList.Codes._4;

			AssertMergeKeys(OrgConstants.MergeInvoiceLines.NotMerge);
			AssertMergeKeys(OrgConstants.MergeInvoiceLines.TariffAndDescription);

			void AssertMergeKeys(ZString mergeBy)
			{
				declaration.JE_MergeBy = mergeBy;
				var mergeKeys = new ImportUsageFeeEntryCreationStrategy(declaration).GetKeyForLine(invoiceLine).Keys;
				var keyCount = 0;
				CombineAssertions($"Merge Keys for {mergeBy}", () =>
				{
					AssertEquals("Merge keys count", 11, mergeKeys.Count);
					AssertEquals("Keys contain Message Type", new ZString("SUF"), mergeKeys[keyCount++]);
					AssertEquals("Keys not contain Valuation Date", ZDate.Empty, mergeKeys[keyCount++]);
					AssertEquals("Keys contain JI_CEI", invoiceLine.JI_CEI, mergeKeys[keyCount++]);
					AssertEquals("Keys contain JZ_IncoTerm", new ZString("FOB"), mergeKeys[keyCount++]);
					AssertEquals("Keys contain JZ_ValuationCode", new ZString(ValuationCodeList.Codes._01), mergeKeys[keyCount++]);
					AssertEquals("Keys contain JZ_OA_SupplierAddress", manufacturer.MainAddress.PK, mergeKeys[keyCount++]);
					AssertEquals("Keys contain ExchangeHedgeType", ExchangeHedgeList.Codes._4, mergeKeys[keyCount++]);
					AssertEquals("Keys contain JI_Tariff", "01010101", mergeKeys[keyCount++]);
					AssertEquals("Keys contain JI_OA_ManufacturerAddress", manufacturer.MainAddress.PK, mergeKeys[keyCount++]);
					AssertEquals("Keys contain JI_GoodsApplication", GoodsApplicationTypeList.Codes.Resale, mergeKeys[keyCount++]);
					AssertEquals("Keys contain JI_GoodsCondition", GoodsConditionTypeList.Codes.GoodsMadeToOrder, mergeKeys[keyCount++]);
				});
			}
		}

		protected override ImportUsageFeeEntryCreationStrategyForTesting CreateNewEntryCreationStrategy(JobDeclaration declaration)
		{
			return new ImportUsageFeeEntryCreationStrategyForTesting(declaration);
		}

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.Import;

		protected override ZString ExpectedMessageTypeToNewEntryHeader => MessageTypeList.Codes.SUF;

		protected override ZBool ExpectedAdditionalEntryLineLinks => true;
	}

	class ImportUsageFeeEntryCreationStrategyForTesting : ImportUsageFeeEntryCreationStrategy, IEntryCreationStrategyForTesting
	{
		public ImportUsageFeeEntryCreationStrategyForTesting(JobDeclaration declaration) : base(declaration)
		{
		}

		public bool UseAdditionalEntryLineLinksExposed => base.UseAdditionalEntryLineLinks;

		public bool IsActiveExposed => base.IsActiveCore;

		public string MessageTypeToNewEntryHeaderExposed => base.CH_MessageTypeToNewEntryHeader;

		public void ClearReferenceToEntryLineWhenLineIsNotValidForMergeExposed(BaseJobComInvoiceLine invoiceLine) => ClearReferenceToEntryLineWhenLineIsNotValidForMerge(invoiceLine);
	}
}
