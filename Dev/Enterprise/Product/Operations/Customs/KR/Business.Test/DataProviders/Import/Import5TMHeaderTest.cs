using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class Import5TMHeaderTest : XMLMessageTestHelper<Import5TMHeaderTest>
	{
		[TestDate(2021, 04, 01)]
		public void TestEmptyData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "1111111111", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "품명규격1");
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "2222222222", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "품명규격2");

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForForeigner, "KR0843652", Core.Constants.CountryCodes.KoreaSouth);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.JE_OH_DutyPayer = orgHeader.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1234520100523X";

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "1111111111";
			entryLine1.CL_ValueForVAT = 10.9m;
			var entryLine1_Fee = entryLine1.Fees.AddNew();
			entryLine1_Fee.CF_ChargeAmount = 10.1m;
			entryLine1_Fee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1_1 = invoice.InvoiceLines.AddNew();
			invoiceLine1_1.JI_CL = entryLine1.PK;
			invoiceLine1_1.JI_Tariff = "1111111111";
			invoiceLine1_1.JI_NetWeight = 10.8m;
			invoiceLine1_1.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1_1.JI_SequenceNumber = 1;

			var invoiceLine1_2 = invoice.InvoiceLines.AddNew();
			invoiceLine1_2.JI_CL = entryLine1.PK;
			invoiceLine1_2.JI_Tariff = "1111111111";
			invoiceLine1_2.JI_NetWeight = 11000m;
			invoiceLine1_2.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			invoiceLine1_2.JI_SequenceNumber = 2;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "2222222222";
			entryLine2.CL_ValueForVAT = 20.9m;
			var entryLine2_Fee = entryLine2.Fees.AddNew();
			entryLine2_Fee.CF_ChargeAmount = 11.1m;
			entryLine2_Fee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Tariff = "2222222222";
			invoiceLine2.JI_NetWeight = 12.8m;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine2.JI_SequenceNumber = 1;

			var miscMessageSendingObject = new JobDeclarationMiscMessageSendingObject(entry, "5TM", MessageFunctionCode.Original, x => true);
			AssertEquals(2, miscMessageSendingObject.MessageSendingEntryLines.Count);
			miscMessageSendingObject.MessageSendingEntryLines[0].IsGoldOrItsProduct = true;
			miscMessageSendingObject.MessageSendingEntryLines[1].IsGoldOrItsProduct = true;

			var import5TM = new Import5TMHeaderCreator().Create(entry, miscMessageSendingObject);
			var result = new GOVCBR5TMMessageBuilder(import5TM).GenerateMessage();
			var fileReader = new TestFileReader(typeof(Import5TMHeaderTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5TM_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		[TestDate(2021, 04, 01)]
		public void TestRealData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Universal.Constants.TariffTypes.HarmonizedSystem);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "0102399000", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "Gold 99.9999");

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			orgHeader.CustomsCodes.AddNew(IdentificationType.CorporationCode, "1028141234", Core.Constants.CountryCodes.KoreaSouth);
			orgHeader.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForResident, "01012345678912", Core.Constants.CountryCodes.KoreaSouth);
			orgHeader.CustomsCodes.AddNew(IdentificationType.KoreanRegNoForForeigner, "KR0177360", Core.Constants.CountryCodes.KoreaSouth);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_PaidBy = PaidByCodeList.Codes.OTH;
			declaration.JE_OH_DutyPayer = orgHeader.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "1235621012542M";

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "0102399000";
			entryLine1.CL_ValueForVAT = 100m;
			var entryLine1_Fee = entryLine1.Fees.AddNew();
			entryLine1_Fee.CF_ChargeAmount = 10m;
			entryLine1_Fee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1_1 = invoice.InvoiceLines.AddNew();
			invoiceLine1_1.JI_CL = entryLine1.PK;
			invoiceLine1_1.JI_Tariff = "0102399000";
			invoiceLine1_1.JI_NetWeight = 10m;
			invoiceLine1_1.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1_1.JI_SequenceNumber = 1;

			var invoiceLine1_2 = invoice.InvoiceLines.AddNew();
			invoiceLine1_2.JI_CL = entryLine1.PK;
			invoiceLine1_2.JI_Tariff = "0102399000";
			invoiceLine1_2.JI_NetWeight = 11000m;
			invoiceLine1_2.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			invoiceLine1_2.JI_SequenceNumber = 2;

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_AdValoremTariff = "0102399000";
			entryLine2.CL_ValueForVAT = 98m;
			var entryLine2_Fee = entryLine2.Fees.AddNew();
			entryLine2_Fee.CF_ChargeAmount = 9m;
			entryLine2_Fee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Tariff = "0102399000";
			invoiceLine2.JI_NetWeight = 20000m;
			invoiceLine2.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			invoiceLine2.JI_SequenceNumber = 1;

			var entryLine3 = entry.MergedLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			entryLine3.CL_AdValoremTariff = "0101319000";
			entryLine3.CL_ValueForVAT = 33m;
			var entryLine3_Fee = entryLine3.Fees.AddNew();
			entryLine3_Fee.CF_ChargeAmount = 3m;
			entryLine3_Fee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Tariff = "0101319000";
			invoiceLine3.JI_NetWeight = 30000m;
			invoiceLine3.JI_NetWeightUQ = Core.Constants.Weight.Grams;
			invoiceLine3.JI_SequenceNumber = 1;

			var miscMessageSendingObject = new JobDeclarationMiscMessageSendingObject(entry, "5TM", MessageFunctionCode.Original, x => true);
			AssertEquals(3, miscMessageSendingObject.MessageSendingEntryLines.Count);
			miscMessageSendingObject.MessageSendingEntryLines[0].IsGoldOrItsProduct = true;
			miscMessageSendingObject.MessageSendingEntryLines[1].IsGoldOrItsProduct = true;
			miscMessageSendingObject.MessageSendingEntryLines[2].IsGoldOrItsProduct = false;

			var import5TM = new Import5TMHeaderCreator().Create(entry, miscMessageSendingObject);
			var result = new GOVCBR5TMMessageBuilder(import5TM).GenerateMessage();
			var fileReader = new TestFileReader(typeof(Import5TMHeaderTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5TM_D2.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public void TestDecimalplaces()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "TestDecimalplaces";

			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_ValueForVAT = 99.999m;
			var entryLine1_Fee = entryLine1.Fees.AddNew();
			entryLine1_Fee.CF_ChargeAmount = 99.999m;
			entryLine1_Fee.CF_ChargeType = Core.Constants.Customs.CusEntryFeeTypes.VAT;

			var invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_NetWeight = 99.999m;
			invoiceLine1.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine1.JI_SequenceNumber = 1;

			var miscMessageSendingObject = new JobDeclarationMiscMessageSendingObject(entry, "5TM", MessageFunctionCode.Original, x => true);
			miscMessageSendingObject.MessageSendingEntryLines[0].IsGoldOrItsProduct = true;
			var import5TM = new Import5TMHeaderCreator().Create(entry, miscMessageSendingObject);
			AssertEquals("TestDecimalplaces", import5TM.ImportDeclarationNumber);
			//AssertEquals("ValueForVAT decimalplaces is 0.", 99m, import5TM.EntryLines[0].ValueForVAT);
			//AssertEquals("VAT decimalplaces is 0.", 99m, import5TM.EntryLines[0].VAT);
			//AssertEquals("NetWeightKG decimalplaces is 3.", 99.999m, import5TM.EntryLines[0].NetWeightInKG);		//soon to be used.

			var result = new GOVCBR5TMMessageBuilder(import5TM).GenerateMessage();
			//AssertEquals(99m, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value);
			//AssertEquals(99m, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.Payment.TaxAssessedAmount.Value);
			//AssertEquals(99.999m, result.GoodsShipment[0].GovernmentAgencyGoodsItem.Commodity.GoodsMeasure.NetNetWeightMeasure.Value);		//soon to be used.
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
