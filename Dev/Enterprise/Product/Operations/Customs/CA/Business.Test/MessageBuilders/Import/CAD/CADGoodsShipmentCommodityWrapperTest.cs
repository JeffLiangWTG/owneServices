using System.Linq;
using CargoWise.Customs.CA.MessageContracts.CAD;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	sealed class CADGoodsShipmentCommodityWrapperTest : TestCaseWithFactory
	{
		public void TestDutyTaxFeeForGST()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "CAD";
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.JI_Tariff = "1234567890";
			line1.CA_TreatmentCode = "22";
			var tax1 = line1.DutiesAndTaxes.AddNew();
			tax1.C1_Override = true;
			tax1.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax1.C1_Code = "001";
			tax1.C1_Amount = 100;

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			line2.JI_Tariff = "1234567891";
			line2.CA_TreatmentCode = "02";
			var tax2 = line2.DutiesAndTaxes.AddNew();
			tax2.C1_Override = true;
			tax2.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax2.C1_Code = "002";
			tax2.C1_ExemptCode = GSTStatusCodes.Codes.C48;

			declaration.DoMerge();
			var goodsItems = new CADMessageWrapper(declaration.B3EntryHeader).CADDocumentMetaData.Declaration.GoodsShipment.FirstOrDefault().GovernmentAgencyGoodsItem;
			AssertEquals(2, goodsItems.Count());
			var gst1 = goodsItems.FirstOrDefault().DutyTaxFee.FirstOrDefault(x => x.TypeCode == "GST");
			AssertEquals("01", gst1.DutyRegimeCode);
			AssertEquals(100m, gst1.Payment.PaymentAmount.Amount);
			AssertEquals("CAD", gst1.Payment.PaymentAmount.CurrencyCode);

			var gst2 = goodsItems.LastOrDefault().DutyTaxFee.FirstOrDefault(x => x.TypeCode == "GST");
			AssertEquals(GSTStatusCodes.Codes.C48, gst2.DutyRegimeCode);
		}

		public void TestGSTDutyTaxFeeForLVSJob()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "CAD";
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_LineNo = 1;
			line1.JI_Tariff = "1234567890";
			line1.CA_TreatmentCode = "22";
			var tax1 = line1.DutiesAndTaxes.AddNew();
			tax1.C1_Override = true;
			tax1.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax1.C1_Code = "001";
			tax1.C1_Amount = 100;

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = "CAD";
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_LineNo = 2;
			line2.JI_Tariff = "1234567891";
			line2.CA_TreatmentCode = "02";
			var tax2 = line2.DutiesAndTaxes.AddNew();
			tax2.C1_Override = true;
			tax2.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax2.C1_Code = "002";
			tax2.C1_ExemptCode = GSTStatusCodes.Codes.C48;

			declaration.DoMerge();
			var goodsShipments = new CADMessageWrapper(declaration.B3EntryHeader).CADDocumentMetaData.Declaration.GoodsShipment;
			AssertEquals(2, goodsShipments.Count());
			var gst1 = goodsShipments.FirstOrDefault().GovernmentAgencyGoodsItem.FirstOrDefault().DutyTaxFee.FirstOrDefault(x => x.TypeCode == "GST");
			AssertEquals("01", gst1.DutyRegimeCode);
			AssertEquals(100m, gst1.Payment.PaymentAmount.Amount);
			AssertEquals("CAD", gst1.Payment.PaymentAmount.CurrencyCode);

			var gst2 = goodsShipments.LastOrDefault().GovernmentAgencyGoodsItem.FirstOrDefault().DutyTaxFee.FirstOrDefault(x => x.TypeCode == "GST");
			AssertEquals(GSTStatusCodes.Codes.C48, gst2.DutyRegimeCode);
		}

		public void TestDutyTaxFeeForPreCARMJob()
		{
			var previousJob = Factory.New<JobDeclaration>();
			previousJob.JE_MessageType = JobMessageTypeList.Codes.Import;
			previousJob.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invoice = previousJob.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "CAD";

			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LineNo = 1;
			line.JI_Tariff = "1234567890";
			line.CA_TreatmentCode = "22";

			var tax1 = line.DutiesAndTaxes.AddNew();
			tax1.C1_Override = true;
			tax1.C1_TaxType = DutyAndTaxTypes.Codes.GST;
			tax1.C1_Code = "001";
			tax1.C1_Amount = 100;

			var tax2 = line.DutiesAndTaxes.AddNew();
			tax2.C1_Override = true;
			tax2.C1_TaxType = DutyAndTaxTypes.Codes.CustomsDuty;
			tax2.C1_Code = "002";
			tax2.C1_Amount = 200;

			var tax3 = line.DutiesAndTaxes.AddNew();
			tax3.C1_Override = true;
			tax3.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			tax3.C1_Code = "003";
			tax3.C1_Amount = 300;

			var tax4 = line.DutiesAndTaxes.AddNew();
			tax4.C1_Override = true;
			tax4.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			tax4.C1_Code = "004";
			tax4.C1_Amount = 400;

			previousJob.DoMerge();

			var newDeclaration = previousJob.GetNewCopyToPRECARMAdjustmentDeclaration();
			Factory.Save();
			AssertNotNull(newDeclaration.ParentRelatedDeclaration);
			AssertEquals(previousJob, newDeclaration.ParentRelatedDeclaration);
			AssertEquals(4, newDeclaration.InvoiceLines[0].DutiesAndTaxes.Count);
			newDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			newDeclaration.DoMerge();
			var goodsShipments = new CADMessageWrapper(newDeclaration.B3EntryHeader).CADDocumentMetaData.Declaration.GoodsShipment;
			var dutyTaxFee = goodsShipments.FirstOrDefault().GovernmentAgencyGoodsItem.FirstOrDefault().DutyTaxFee;
			var gst = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "GST");
			AssertNotNull(gst);
			AssertEquals("DutyRegimeCode", "01", gst.DutyRegimeCode);
			AssertEquals("RequestOverrideCode", ZString.Empty, gst.RequestOverrideCode);
			AssertEquals("PayMent Currency", Core.Constants.CurrencyCodes.Canada, gst.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("PayMent Amount", 100m, gst.Payment.PaymentAmount.Amount);

			var cud = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "CUD");
			AssertNotNull(cud);
			AssertEquals("DutyRegimeCode", "22", cud.DutyRegimeCode);
			AssertEquals("RequestOverrideCode", ZString.Empty, cud.RequestOverrideCode);
			AssertEquals("PayMent Currency", Core.Constants.CurrencyCodes.Canada, cud.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("PayMent Amount", 200m, cud.Payment.PaymentAmount.Amount);

			var oth = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "OTH");
			AssertNotNull(oth);
			AssertEquals("DutyRegimeCode", "003", oth.DutyRegimeCode);
			AssertEquals("RequestOverrideCode", "X", oth.RequestOverrideCode);
			AssertEquals("PayMent Currency", Core.Constants.CurrencyCodes.Canada, oth.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("PayMent Amount", 300m, oth.Payment.PaymentAmount.Amount);

			var sur = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "SUR");
			AssertNotNull(sur);
			AssertEquals("DutyRegimeCode", "004", sur.DutyRegimeCode);
			AssertEquals("RequestOverrideCode", "X", sur.RequestOverrideCode);
			AssertEquals("PayMent Currency", Core.Constants.CurrencyCodes.Canada, sur.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("PayMent Amount", 400m, sur.Payment.PaymentAmount.Amount);
		}

		public void TestExciseDuty_ExciseCodeIsE01OrE07()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			line1.JI_InvoiceQuantity = 132.0004;
			line1.JI_InvoiceUQ = "KG";
			line1.JI_LinePrice = 2000m;
			line1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			Factory.Save();

			var exs1 = line1.DutiesAndTaxes.AddNew();
			exs1.C1_Override = true;
			exs1.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			exs1.C1_Amount = 37m;
			exs1.Quantity = 38m;
			exs1.C1_UnitOfMeasure = "MIL";
			exs1.C1_Code = "E01";

			var exsFee = entryLine.Fees.AddNew();
			exsFee.CF_ChargeType = EntryChargeTypeList.Codes.TotalExciseTaxAmount;
			exsFee.CF_ChargeAmount = 37m;

			var goodsShipments = new CADMessageWrapper(entryHeader).CADDocumentMetaData.Declaration.GoodsShipment;
			var commodity = goodsShipments.First().GovernmentAgencyGoodsItem.First();
			var dutyTaxFees = commodity.DutyTaxFee;
			var fet = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.FET && x.DutyRegimeCode == "E01");
			AssertNotNull(fet);
			var exs = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.EXC);
			AssertEquals(37m, exs.Payment.PaymentAmount.Amount);

			exs1.C1_Code = "E07";
			exsFee.CF_ChargeAmount = 39m;

			goodsShipments = new CADMessageWrapper(entryHeader).CADDocumentMetaData.Declaration.GoodsShipment;
			commodity = goodsShipments.First().GovernmentAgencyGoodsItem.First();
			dutyTaxFees = commodity.DutyTaxFee;
			fet = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.FET && x.DutyRegimeCode == "E07");
			AssertNotNull(fet);
			exs = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.EXC);
			AssertEquals(39m, exs.Payment.PaymentAmount.Amount);
		}

		public void TestAdditionalInformation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 03, 18);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 03, 18);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			var entryLine1 = entry.AllEntryLines.AddNew();
			var entryLine2 = entry.AllEntryLines.AddNew();
			var entryLine3 = entry.AllEntryLines.AddNew();
			var entryLine4 = entry.AllEntryLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.CA_TimeLimit = 1;
			invoice1.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Year;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.CA_TimeLimit = 2;
			invoice2.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			var link2 = invoiceLine2.AdditionalEntryLineLinks.AddNew();
			link2.BU_CL = entryLine2.PK;
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.CA_TimeLimit = 3;
			invoice3.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Week;
			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			var link3 = invoiceLine3.AdditionalEntryLineLinks.AddNew();
			link3.BU_CL = entryLine3.PK;
			var invoice4 = declaration.Invoices.AddNew();
			invoice4.CA_TimeLimit = 4;
			invoice4.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			var invoiceLine4 = invoice4.JobComInvoiceLines.AddNew();
			var link4 = invoiceLine4.AdditionalEntryLineLinks.AddNew();
			link4.BU_CL = entryLine4.PK;

			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			var additionalInformation = goodsShipments[0].GovernmentAgencyGoodsItem.First().AdditionalInformation.ToArray();
			AssertAdditionalInformation(additionalInformation[0], string.Empty, AdditionalInformationTypeCodes.Codes.TLS, "20240318");
			AssertAdditionalInformation(additionalInformation[1], string.Empty, AdditionalInformationTypeCodes.Codes.TLE, "20250318");
			AssertAdditionalInformation(additionalInformation[2], "2", AdditionalInformationTypeCodes.Codes.TLT);
			additionalInformation = goodsShipments[1].GovernmentAgencyGoodsItem.First().AdditionalInformation.ToArray();
			AssertAdditionalInformation(additionalInformation[0], string.Empty, AdditionalInformationTypeCodes.Codes.TLS, "20240318");
			AssertAdditionalInformation(additionalInformation[1], string.Empty, AdditionalInformationTypeCodes.Codes.TLE, "20240518");
			AssertAdditionalInformation(additionalInformation[2], "2", AdditionalInformationTypeCodes.Codes.TLT);
			additionalInformation = goodsShipments[2].GovernmentAgencyGoodsItem.First().AdditionalInformation.ToArray();
			AssertAdditionalInformation(additionalInformation[0], string.Empty, AdditionalInformationTypeCodes.Codes.TLS, "20240318");
			AssertAdditionalInformation(additionalInformation[1], string.Empty, AdditionalInformationTypeCodes.Codes.TLE, "20240408");
			AssertAdditionalInformation(additionalInformation[2], "2", AdditionalInformationTypeCodes.Codes.TLT);
			additionalInformation = goodsShipments[3].GovernmentAgencyGoodsItem.First().AdditionalInformation.ToArray();
			AssertAdditionalInformation(additionalInformation[0], string.Empty, AdditionalInformationTypeCodes.Codes.TLS, "20240318");
			AssertAdditionalInformation(additionalInformation[1], string.Empty, AdditionalInformationTypeCodes.Codes.TLE, "20240322");
			AssertAdditionalInformation(additionalInformation[2], "2", AdditionalInformationTypeCodes.Codes.TLT);

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 04, 18);
			var dutyTax1 = invoiceLine1.DutiesAndTaxes.AddNew();
			dutyTax1.C1_PreviousTranLine = 11;
			var dutyTax2 = invoiceLine2.DutiesAndTaxes.AddNew();
			dutyTax2.C1_PreviousTranLine = 22;
			var dutyTax3 = invoiceLine3.DutiesAndTaxes.AddNew();
			dutyTax3.C1_PreviousTranLine = 33;
			var dutyTax4 = invoiceLine4.DutiesAndTaxes.AddNew();
			dutyTax4.C1_PreviousTranLine = 1234567;
			goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			var goodsItem = goodsShipments[0].GovernmentAgencyGoodsItem.First();
			additionalInformation = goodsItem.AdditionalInformation.ToArray();
			AssertAdditionalInformation(additionalInformation[0], string.Empty, AdditionalInformationTypeCodes.Codes.TLS, "20240418");
			AssertAdditionalInformation(additionalInformation[1], string.Empty, AdditionalInformationTypeCodes.Codes.TLE, "20250418");
			AssertAdditionalInformation(additionalInformation[2], "2", AdditionalInformationTypeCodes.Codes.TLT);
			var previousDocument = goodsItem.PreviousDocument;
			AssertEquals("CADGoodsShipmentCommodityWrapper - PreviousDocument - LineNumeric", "000011", previousDocument.LineNumeric);
			AssertEquals("CADGoodsShipmentCommodityWrapper - PreviousDocument - TypeCode", "632", previousDocument.TypeCode);
			goodsItem = goodsShipments[1].GovernmentAgencyGoodsItem.First();
			additionalInformation = goodsItem.AdditionalInformation.ToArray();
			AssertAdditionalInformation(additionalInformation[0], string.Empty, AdditionalInformationTypeCodes.Codes.TLS, "20240418");
			AssertAdditionalInformation(additionalInformation[1], string.Empty, AdditionalInformationTypeCodes.Codes.TLE, "20240618");
			AssertAdditionalInformation(additionalInformation[2], "2", AdditionalInformationTypeCodes.Codes.TLT);
			previousDocument = goodsItem.PreviousDocument;
			AssertEquals("CADGoodsShipmentCommodityWrapper - PreviousDocument - LineNumeric", "000022", previousDocument.LineNumeric);
			AssertEquals("CADGoodsShipmentCommodityWrapper - PreviousDocument - TypeCode", "632", previousDocument.TypeCode);
			goodsItem = goodsShipments[2].GovernmentAgencyGoodsItem.First();
			additionalInformation = goodsItem.AdditionalInformation.ToArray();
			AssertAdditionalInformation(additionalInformation[0], string.Empty, AdditionalInformationTypeCodes.Codes.TLS, "20240418");
			AssertAdditionalInformation(additionalInformation[1], string.Empty, AdditionalInformationTypeCodes.Codes.TLE, "20240509");
			AssertAdditionalInformation(additionalInformation[2], "2", AdditionalInformationTypeCodes.Codes.TLT);
			previousDocument = goodsItem.PreviousDocument;
			AssertEquals("CADGoodsShipmentCommodityWrapper - PreviousDocument - LineNumeric", "000033", previousDocument.LineNumeric);
			AssertEquals("CADGoodsShipmentCommodityWrapper - PreviousDocument - TypeCode", "632", previousDocument.TypeCode);
			goodsItem = goodsShipments[3].GovernmentAgencyGoodsItem.First();
			additionalInformation = goodsItem.AdditionalInformation.ToArray();
			AssertAdditionalInformation(additionalInformation[0], string.Empty, AdditionalInformationTypeCodes.Codes.TLS, "20240418");
			AssertAdditionalInformation(additionalInformation[1], string.Empty, AdditionalInformationTypeCodes.Codes.TLE, "20240422");
			AssertAdditionalInformation(additionalInformation[2], "2", AdditionalInformationTypeCodes.Codes.TLT);
			previousDocument = goodsItem.PreviousDocument;
			AssertEquals("CADGoodsShipmentCommodityWrapper - PreviousDocument - LineNumeric", "234567", previousDocument.LineNumeric);
			AssertEquals("CADGoodsShipmentCommodityWrapper - PreviousDocument - TypeCode", "632", previousDocument.TypeCode);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			goodsItem = goodsShipments[2].GovernmentAgencyGoodsItem.First();
			previousDocument = goodsItem.PreviousDocument;
			AssertEquals("CADGoodsShipmentCommodityWrapper - PreviousDocument - LineNumeric", "00033", previousDocument.LineNumeric);

			goodsItem = goodsShipments[3].GovernmentAgencyGoodsItem.First();
			previousDocument = goodsItem.PreviousDocument;
			AssertEquals("CADGoodsShipmentCommodityWrapper - PreviousDocument - LineNumeric", "34567", previousDocument.LineNumeric);
		}

		public void TestAdditionalInformationForLVSJob()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var simaTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, DutyAndTaxManager.SIMATariffType);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var antiDumpingRelTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "0123456789", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(antiDumpingRelTariff, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "NMB");

			var antiDumpingTariff = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, simaTariffType.PK, "AD1407", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, relatedTariffCode: "0123456789", description: "TEST(TS1)");
			var antiDumpingRelationShip = universalHelper.CreateTariffRelationship(antiDumpingTariff.PK, harmonizedTariffType.PK, "0123456789");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 03, 18);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 03, 18);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var entryLine1 = entry.AllEntryLines.AddNew();
			var entryLine2 = entry.AllEntryLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;

			var add = invoiceLine1.DutiesAndTaxes.AddNew();
			add.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add.C1_ExemptCode = SIMACodes.Codes.C10;
			add.C1_Code = "AD";

			var sur = invoiceLine1.DutiesAndTaxes.AddNew();
			sur.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur.C1_ExemptCode = SIMACodes.Codes.C20;

			var saf = invoiceLine1.DutiesAndTaxes.AddNew();
			saf.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			saf.C1_ExemptCode = SIMACodes.Codes.C50;

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 55m;
			invoiceLine2.JI_CustomsUnitQty = "KGM";
			invoiceLine2.CA_AuthorityNumber = "85-2955";
			invoiceLine2.JI_Tariff = "0123456789";
			invoiceLine2.CA_SIMADumpingNum = "AD1407";
			var link2 = invoiceLine2.AdditionalEntryLineLinks.AddNew();
			link2.BU_CL = entryLine2.PK;

			var cvd = invoiceLine2.DutiesAndTaxes.AddNew();
			cvd.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd.C1_ExemptCode = SIMACodes.Codes.C50;

			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			var goodsItem1 = goodsShipments[0].GovernmentAgencyGoodsItem.First();
			var additionalInformation1 = goodsItem1.AdditionalInformation.ToArray();
			AssertEquals(5, additionalInformation1.Length);
			AssertAdditionalInformation(additionalInformation1[0], "N", AdditionalInformationTypeCodes.Codes.ASJ);
			AssertAdditionalInformation(additionalInformation1[1], "AD", AdditionalInformationTypeCodes.Codes.MIF);
			AssertAdditionalInformation(additionalInformation1[2], "U", AdditionalInformationTypeCodes.Codes.BSJ);
			AssertAdditionalInformation(additionalInformation1[3], "S", AdditionalInformationTypeCodes.Codes.CSJ);
			AssertAdditionalInformation(additionalInformation1[4], "013", AdditionalInformationTypeCodes.Codes.VDC);

			var goodsItem2 = goodsShipments[0].GovernmentAgencyGoodsItem.Last();
			var additionalInformation2 = goodsItem2.AdditionalInformation.ToArray();
			AssertEquals(3, additionalInformation2.Length);
			AssertAdditionalInformation(additionalInformation2[0], "S", AdditionalInformationTypeCodes.Codes.ASJ);
			AssertAdditionalInformation(additionalInformation2[1], "TS1", AdditionalInformationTypeCodes.Codes.MIF);
			AssertAdditionalInformation(additionalInformation2[2], "013", AdditionalInformationTypeCodes.Codes.VDC);
		}

		void AssertAdditionalInformation(ICADMessageDeclarationAdditionalInformation information, string statementCode, string statementTypeCode, string limitDateTime = "", string statementDescription = "")
		{
			AssertEquals("CADGoodsShipmentCommodityWrapper - AdditionalInformation - StatementCode", statementCode, information.StatementCode);
			AssertEquals("CADGoodsShipmentCommodityWrapper - AdditionalInformation - StatementTypeCode", statementTypeCode, information.StatementTypeCode);
			AssertEquals("CADGoodsShipmentCommodityWrapper - AdditionalInformation - StatementDescription", statementDescription, information.StatementDescription);
			AssertEquals("CADGoodsShipmentCommodityWrapper - AdditionalInformation - LimitDateTime", limitDateTime, information.LimitDateTime);
		}

		public void TestAdditionalinformationWhenSurtaxExemptCode10()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 03, 18);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 03, 18);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			var entryLine1 = entry.AllEntryLines.AddNew();
			var entryLine2 = entry.AllEntryLines.AddNew();
			var entryLine3 = entry.AllEntryLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.CA_TimeLimit = 1;
			invoice1.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Year;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;

			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 04, 18);
			var dutyTax1 = invoiceLine1.DutiesAndTaxes.AddNew();
			dutyTax1.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			dutyTax1.C1_ExemptCode = SIMACodes.Codes.C51;
			dutyTax1.C1_PreviousTranLine = 11;
			var dutyTax2 = invoiceLine1.DutiesAndTaxes.AddNew();
			dutyTax2.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			dutyTax2.C1_ExemptCode = SIMACodes.Codes.C10;
			dutyTax2.C1_PreviousTranLine = 22;

			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			var goodsItem = goodsShipments[0].GovernmentAgencyGoodsItem.First();
			var additionalInformation = goodsItem.AdditionalInformation.ToArray();
			AssertAdditionalInformation(additionalInformation[0], "S", AdditionalInformationTypeCodes.Codes.ASJ);
			AssertAdditionalInformation(additionalInformation[1], "N", AdditionalInformationTypeCodes.Codes.BSJ);
		}

		public void TestInvoiceLineCharge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 03, 18);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 03, 18);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			var entryLine1 = entry.AllEntryLines.AddNew();
			var entryLine2 = entry.AllEntryLines.AddNew();
			var entryLine3 = entry.AllEntryLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 100m;
			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 30);
			var add1 = invoiceLine1.DutiesAndTaxes.AddNew();
			add1.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 200m;
			invoiceLine2.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 30);
			var add2 = invoiceLine2.DutiesAndTaxes.AddNew();
			add2.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			var link2 = invoiceLine2.AdditionalEntryLineLinks.AddNew();
			link2.BU_CL = entryLine2.PK;
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Canada;
			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 300m;
			invoiceLine3.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 30);
			var link3 = invoiceLine3.AdditionalEntryLineLinks.AddNew();
			link3.BU_CL = entryLine3.PK;
			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			var invoiceLineCharge1 = goodsShipments[0].GovernmentAgencyGoodsItem.First().InvoiceLineCharge;
			AssertEquals("line 1 amount", 130m, invoiceLineCharge1.Amount);
			AssertEquals("line 1 currency code", Core.Constants.CurrencyCodes.Canada, invoiceLineCharge1.CurrencyCode);
			var invoiceLineCharge2 = goodsShipments[1].GovernmentAgencyGoodsItem.First().InvoiceLineCharge;
			AssertEquals("line 2 amount", 230m, invoiceLineCharge2.Amount);
			AssertEquals("line 2 currency code", Core.Constants.CurrencyCodes.UnitedStates, invoiceLineCharge2.CurrencyCode);
			var invoiceLineCharge3 = goodsShipments[2].GovernmentAgencyGoodsItem.First().InvoiceLineCharge;
			AssertNull("Has no SIMA Duty", invoiceLineCharge3);
		}

		public void TestAdditionalInformation_SIMA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 03, 18);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 03, 18);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			var entryLine1 = entry.AllEntryLines.AddNew();
			var entryLine2 = entry.AllEntryLines.AddNew();
			var entryLine3 = entry.AllEntryLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.CA_TimeLimit = 1;
			invoice1.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Year;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.CA_TimeLimit = 2;
			invoice2.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			var link2 = invoiceLine2.AdditionalEntryLineLinks.AddNew();
			link2.BU_CL = entryLine2.PK;
			var invoice3 = declaration.Invoices.AddNew();
			invoice3.CA_TimeLimit = 3;
			invoice3.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Week;
			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			var link3 = invoiceLine3.AdditionalEntryLineLinks.AddNew();
			link3.BU_CL = entryLine3.PK;
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 04, 18);
			var dutyTax1 = invoiceLine1.DutiesAndTaxes.AddNew();
			dutyTax1.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			dutyTax1.C1_ExemptCode = SIMACodes.Codes.C10;
			dutyTax1.C1_PreviousTranLine = 11;
			var dutyTax2 = invoiceLine2.DutiesAndTaxes.AddNew();
			dutyTax2.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			dutyTax2.C1_ExemptCode = SIMACodes.Codes.C20;
			dutyTax2.C1_PreviousTranLine = 22;
			var dutyTax3 = invoiceLine3.DutiesAndTaxes.AddNew();
			dutyTax3.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			dutyTax3.C1_ExemptCode = SIMACodes.Codes.C30;
			dutyTax3.C1_PreviousTranLine = 33;

			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			var goodsItem = goodsShipments[0].GovernmentAgencyGoodsItem.First();
			var additionalInformation = goodsItem.AdditionalInformation.ToArray();
			AssertAdditionalInformation(additionalInformation[0], "N", AdditionalInformationTypeCodes.Codes.ASJ);

			goodsItem = goodsShipments[1].GovernmentAgencyGoodsItem.First();
			additionalInformation = goodsItem.AdditionalInformation.ToArray();
			AssertAdditionalInformation(additionalInformation[0], "U", AdditionalInformationTypeCodes.Codes.BSJ);

			goodsItem = goodsShipments[2].GovernmentAgencyGoodsItem.First();
			additionalInformation = goodsItem.AdditionalInformation.ToArray();
			AssertAdditionalInformation(additionalInformation[0], "S", AdditionalInformationTypeCodes.Codes.CSJ);
		}

		public void TestDutyTaxFee_SUR()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = entryHeader.AllEntryLines.AddNew();

			var line1 = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
			line1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine);
			line1.JI_InvoiceQuantity = 132.0004;
			line1.JI_InvoiceUQ = "KG";
			line1.JI_LinePrice = 2000m;
			line1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Australia;
			Factory.Save();

			var sur1 = line1.DutiesAndTaxes.AddNew();
			sur1.C1_Override = true;
			sur1.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur1.C1_Amount = 37m;
			sur1.Quantity = 38m;
			sur1.C1_UnitOfMeasure = "M3";
			sur1.C1_Code = "24187A";

			var goodsShipments = new CADMessageWrapper(entryHeader).CADDocumentMetaData.Declaration.GoodsShipment;
			var commodity = goodsShipments.First().GovernmentAgencyGoodsItem.First();
			var dutyTaxFees = commodity.DutyTaxFee;
			var sur = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.SUR);
			AssertEquals("DutyRegimeCode for SUR", "24187A", sur.DutyRegimeCode);
			AssertEquals("Should not populate SpecificTaxBaseQuantity for SUR", 0m, sur.SpecificTaxBaseQuantity);
			AssertEquals("Should not populate SpecificTaxBaseQtyUnit for SUR", ZString.Empty, sur.SpecificTaxBaseQtyUnit);

			sur1.C1_Code = "BBBA";
			sur1.Quantity = 0m;
			sur1.C1_UnitOfMeasure = ZString.Empty;

			goodsShipments = new CADMessageWrapper(entryHeader).CADDocumentMetaData.Declaration.GoodsShipment;
			commodity = goodsShipments.First().GovernmentAgencyGoodsItem.First();
			dutyTaxFees = commodity.DutyTaxFee;
			sur = dutyTaxFees.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.SUR);
			AssertEquals("Populate DutyRegimeCode even when there’s no UoM or Quantity", "BBBA", sur.DutyRegimeCode);
			AssertEquals("Should not populate SpecificTaxBaseQuantity for SUR", 0m, sur.SpecificTaxBaseQuantity);
			AssertEquals("Should not populate SpecificTaxBaseQtyUnit for SUR", ZString.Empty, sur.SpecificTaxBaseQtyUnit);
		}

		public void TestDutyTaxFee_EXD()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var dedCharge = invoice.Charges.AddNew();
			dedCharge.J7_ChargeType = CustomsChargeTypeList.Codes.DeductionCharge;
			dedCharge.J7_Amount = 100m;
			dedCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.UnitedStates;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var entryLine2 = entryHeader.AllEntryLines.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 20m;
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine1);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 10m;
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine2);
			Factory.Save();

			var governmentAgencyGoodsItems = new CADMessageWrapper(entryHeader).CADDocumentMetaData.Declaration.GoodsShipment.First().GovernmentAgencyGoodsItem;
			var dutyTaxFees1 = governmentAgencyGoodsItems.First().DutyTaxFee;
			var exd = dutyTaxFees1.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.EXD);
			AssertEquals(66.67m, exd.DeductAmount.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, exd.DeductAmount.CurrencyCode);

			var dutyTaxFees2 = governmentAgencyGoodsItems.Last().DutyTaxFee;
			exd = dutyTaxFees2.First(x => x.TypeCode == CADDutyTaxFeeTypeCodes.Codes.EXD);
			AssertEquals(33.33m, exd.DeductAmount.Amount);
			AssertEquals(Core.Constants.CurrencyCodes.UnitedStates, exd.DeductAmount.CurrencyCode);
		}

		public void TestDutyTaxFee_WarehouseCARM()
		{
			var entry = GetCusEntryHeader(JobMessageTypeList.Codes.Import);
			entry.Declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			var dutyTaxFee = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.FirstOrDefault().GovernmentAgencyGoodsItem.FirstOrDefault().DutyTaxFee;
			var tot = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "TOT" && x.DutyTaxFeeAssessmentBasis.FirstOrDefault().CurrencyCode == "CAD");
			AssertNotNull(tot);
		}

		public void TestCountQuantity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 03, 18);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 03, 18);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			var entryLine1 = entry.AllEntryLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;

			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			AssertEquals(1m, goodsShipments[0].GovernmentAgencyGoodsItem.First().CountQuantity);
			AssertEquals("KGM", goodsShipments[0].GovernmentAgencyGoodsItem.First().CountQtyUnit);

			invoiceLine1.JI_CustomsQuantity = 1.0004m;
			goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			AssertEquals(1.000m, goodsShipments[0].GovernmentAgencyGoodsItem.First().CountQuantity);

			invoiceLine1.JI_CustomsQuantity = 1.0005m;
			goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			AssertEquals(1.001m, goodsShipments[0].GovernmentAgencyGoodsItem.First().CountQuantity);

			invoiceLine1.JI_InvoiceQuantity = 2m;
			invoiceLine1.JI_InvoiceUQ = "PKG";
			invoiceLine1.JI_CustomsQuantity = 0;
			goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			AssertEquals(2m, goodsShipments[0].GovernmentAgencyGoodsItem.First().CountQuantity);
			AssertEquals("", goodsShipments[0].GovernmentAgencyGoodsItem.First().CountQtyUnit);
		}

		public void TestCountQuantityForLVSJob()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 03, 18);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 03, 18);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var entryLine1 = entry.AllEntryLines.AddNew();
			var entryLine2 = entry.AllEntryLines.AddNew();
			var entryLine3 = entry.AllEntryLines.AddNew();
			var entryLine4 = entry.AllEntryLines.AddNew();
			entryLine4.CL_AdValoremTariff = "7616999090";
			var entryLine5 = entry.AllEntryLines.AddNew();
			entryLine5.CL_AdValoremTariff = "7616999091";
			var entryLine6 = entry.AllEntryLines.AddNew();
			entryLine6.CL_AdValoremTariff = "7616999092";
			var entryLine7 = entry.AllEntryLines.AddNew();
			entryLine7.CL_AdValoremTariff = "7616999093";
			var entryLine8 = entry.AllEntryLines.AddNew();
			entryLine8.CL_AdValoremTariff = "7616999094";

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;

			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CustomsQuantity = 2m;
			invoiceLine2.JI_CustomsUnitQty = "KGM";
			var link2 = invoiceLine2.AdditionalEntryLineLinks.AddNew();
			link2.BU_CL = entryLine1.PK;

			var invoice3 = declaration.Invoices.AddNew();
			var invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CustomsQuantity = 55m;
			invoiceLine3.JI_CustomsUnitQty = "KGM";
			invoiceLine3.CA_AuthorityNumber = "85-2955";
			var link3 = invoiceLine3.AdditionalEntryLineLinks.AddNew();
			link3.BU_CL = entryLine2.PK;

			var invoice4 = declaration.Invoices.AddNew();
			var invoiceLine4 = invoice4.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_CustomsQuantity = 2m;
			invoiceLine4.JI_CustomsUnitQty = "KGM";
			var link4 = invoiceLine4.AdditionalEntryLineLinks.AddNew();
			link4.BU_CL = entryLine3.PK;
			var invoiceLine5 = invoice4.JobComInvoiceLines.AddNew();
			invoiceLine5.JI_CustomsQuantity = 2m;
			invoiceLine5.JI_CustomsUnitQty = "KGM";
			var link5 = invoiceLine5.AdditionalEntryLineLinks.AddNew();
			link5.BU_CL = entryLine3.PK;

			var invoice5 = declaration.Invoices.AddNew();
			var invoiceLine6 = invoice5.JobComInvoiceLines.AddNew();
			invoiceLine6.JI_Tariff = "7616999090";
			invoiceLine6.JI_CustomsQuantity = 33m;
			invoiceLine6.JI_CustomsUnitQty = "KGM";
			var surtax = invoiceLine6.DutiesAndTaxes.AddNew();
			surtax.C1_Override = true;
			surtax.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			surtax.C1_Amount = 37m;
			surtax.Quantity = 38m;
			surtax.C1_UnitOfMeasure = "M3";
			surtax.C1_Code = "24187A";
			var link6 = invoiceLine6.AdditionalEntryLineLinks.AddNew();
			link6.BU_CL = entryLine4.PK;

			var invoiceLine7 = invoice5.JobComInvoiceLines.AddNew();
			invoiceLine7.JI_Tariff = "7616999091";
			invoiceLine7.JI_CustomsQuantity = 26m;
			invoiceLine7.JI_CustomsUnitQty = "KGM";
			var add = invoiceLine7.DutiesAndTaxes.AddNew();
			add.C1_Override = true;
			add.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add.C1_Amount = 46m;
			add.Quantity = 37m;
			add.C1_UnitOfMeasure = "M3";
			add.C1_Code = "24187B";
			var link7 = invoiceLine7.AdditionalEntryLineLinks.AddNew();
			link7.BU_CL = entryLine5.PK;

			var invoiceLine8 = invoice5.JobComInvoiceLines.AddNew();
			invoiceLine8.JI_Tariff = "7616999092";
			invoiceLine8.JI_CustomsQuantity = 16m;
			invoiceLine8.JI_CustomsUnitQty = "KGM";
			var cvd = invoiceLine8.DutiesAndTaxes.AddNew();
			cvd.C1_Override = true;
			cvd.C1_TaxType = DutyAndTaxTypes.Codes.CVD;
			cvd.C1_Amount = 46m;
			cvd.Quantity = 37m;
			cvd.C1_UnitOfMeasure = "M3";
			cvd.C1_Code = "24187C";
			var link8 = invoiceLine8.AdditionalEntryLineLinks.AddNew();
			link8.BU_CL = entryLine6.PK;

			var invoiceLine9 = invoice5.JobComInvoiceLines.AddNew();
			invoiceLine9.JI_Tariff = "7616999093";
			invoiceLine9.JI_CustomsQuantity = 216m;
			invoiceLine9.JI_CustomsUnitQty = "KGM";
			var excise = invoiceLine9.DutiesAndTaxes.AddNew();
			excise.C1_Override = true;
			excise.C1_TaxType = DutyAndTaxTypes.Codes.ExciseTax;
			excise.C1_Amount = 146m;
			excise.Quantity = 27m;
			excise.C1_UnitOfMeasure = "M3";
			excise.C1_Code = "24187D";
			var link9 = invoiceLine9.AdditionalEntryLineLinks.AddNew();
			link9.BU_CL = entryLine7.PK;

			var invoiceLine10 = invoice5.JobComInvoiceLines.AddNew();
			invoiceLine10.JI_Tariff = "7616999094";
			invoiceLine10.JI_CustomsQuantity = 56m;
			invoiceLine10.JI_CustomsUnitQty = "KGM";
			var safeguard = invoiceLine10.DutiesAndTaxes.AddNew();
			safeguard.C1_Override = true;
			safeguard.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			safeguard.C1_Amount = 16m;
			safeguard.Quantity = 17m;
			safeguard.C1_UnitOfMeasure = "M3";
			safeguard.C1_Code = "24187E";
			var link10 = invoiceLine10.AdditionalEntryLineLinks.AddNew();
			link10.BU_CL = entryLine8.PK;

			declaration.Invoices.AddNew();

			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			var items = goodsShipments[0].GovernmentAgencyGoodsItem.Where(x => x.Classification.Any(x => x.ID == "0000999900"));
			AssertEquals(2, items.Count());
			AssertNotNull(items.FirstOrDefault(x => x.CountQuantity == 1m && string.IsNullOrEmpty(x.CountQtyUnit)));
			AssertNotNull(items.FirstOrDefault(x => x.CountQuantity == 2m && string.IsNullOrEmpty(x.CountQtyUnit)));

			var item2 = goodsShipments[0].GovernmentAgencyGoodsItem.FirstOrDefault(x => x.Classification.All(x => x.ID != "0000999900"));
			AssertEquals(55m, item2.CountQuantity);
			AssertEquals("KGM", item2.CountQtyUnit);

			var item3 = goodsShipments[0].GovernmentAgencyGoodsItem.FirstOrDefault(x => x.Classification.All(x => x.ID == "7616999090"));
			AssertEquals(33m, item3.CountQuantity);
			AssertEquals("KGM", item3.CountQtyUnit);

			var item4 = goodsShipments[0].GovernmentAgencyGoodsItem.FirstOrDefault(x => x.Classification.All(x => x.ID == "7616999091"));
			AssertEquals(26m, item4.CountQuantity);
			AssertEquals("KGM", item4.CountQtyUnit);

			var item5 = goodsShipments[0].GovernmentAgencyGoodsItem.FirstOrDefault(x => x.Classification.All(x => x.ID == "7616999092"));
			AssertEquals(16m, item5.CountQuantity);
			AssertEquals("KGM", item5.CountQtyUnit);

			var item6 = goodsShipments[0].GovernmentAgencyGoodsItem.FirstOrDefault(x => x.Classification.All(x => x.ID == "7616999093"));
			AssertEquals(216m, item6.CountQuantity);
			AssertEquals("KGM", item6.CountQtyUnit);

			var item7 = goodsShipments[0].GovernmentAgencyGoodsItem.FirstOrDefault(x => x.Classification.All(x => x.ID == "7616999094"));
			AssertEquals(56m, item7.CountQuantity);
			AssertEquals("KGM", item7.CountQtyUnit);
		}

		public void TestSequenceNumericForLVSJob()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			var entryLine1 = entry.AllEntryLines.AddNew();
			var entryLine2 = entry.AllEntryLines.AddNew();
			var entryLine3 = entry.AllEntryLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.CA_TreatmentCode = "02";
			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;
			var invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2.CA_TreatmentCode = "10";
			var link2 = invoiceLine2.AdditionalEntryLineLinks.AddNew();
			link2.BU_CL = entryLine2.PK;

			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.OrderBy(x => x.SequenceNumeric).ToArray();
			AssertEquals(2, goodsShipments.Length);
			AssertEquals(1, goodsShipments[0].SequenceNumeric);
			AssertEquals(2, goodsShipments[1].SequenceNumeric);
		}

		public void TestDutyTaxFeeForLVSJob()
		{
			var entry = GetCusEntryHeader(JobMessageTypeList.Codes.LowValueShipments);

			var dutyTaxFee = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.FirstOrDefault().GovernmentAgencyGoodsItem.FirstOrDefault().DutyTaxFee;
			var tot = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "TOT");
			AssertNotNull(tot);
			AssertEquals("PayMent Currency", Core.Constants.CurrencyCodes.Canada, tot.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("PayMent Amount", 3.4m, tot.Payment.PaymentAmount.Amount);
			AssertEquals(2, tot.DutyTaxFeeAssessmentBasis.Count());
			AssertNotNull(tot.DutyTaxFeeAssessmentBasis.FirstOrDefault(x => x.Amount == 121m && x.CurrencyCode == Core.Constants.CurrencyCodes.Canada));
			AssertNotNull(tot.DutyTaxFeeAssessmentBasis.FirstOrDefault(x => x.Amount == 30.58m && x.CurrencyCode == Core.Constants.CurrencyCodes.Canada));

			var cud = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "CUD");
			AssertNotNull(cud);
			AssertEquals("PayMent Currency", Core.Constants.CurrencyCodes.Canada, cud.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("PayMent Amount", 1.1m, cud.Payment.PaymentAmount.Amount);

			var sur = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "SUR");
			AssertNotNull(sur);
			AssertEquals("DutyRegimeCode", "24187A", sur.DutyRegimeCode);
			AssertEquals("RequestOverrideCode", "X", sur.RequestOverrideCode);
			AssertEquals("PayMent Currency", Core.Constants.CurrencyCodes.Canada, sur.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("PayMent Amount", 37m, sur.Payment.PaymentAmount.Amount);

			var saf = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "OTH");
			AssertNotNull(saf);
			AssertEquals("DutyRegimeCode", "24187B", saf.DutyRegimeCode);
			AssertEquals("RequestOverrideCode", "X", saf.RequestOverrideCode);
			AssertEquals("PayMent Currency", Core.Constants.CurrencyCodes.Canada, saf.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("PayMent Amount", 44m, saf.Payment.PaymentAmount.Amount);

			entry = GetCusEntryHeader(JobMessageTypeList.Codes.Import);
			dutyTaxFee = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.FirstOrDefault().GovernmentAgencyGoodsItem.FirstOrDefault().DutyTaxFee;
			tot = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "TOT");
			AssertNotNull(tot);
			AssertNull(tot.Payment.PaymentAmount);
			AssertEquals(1, tot.DutyTaxFeeAssessmentBasis.Count());
			cud = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "CUD");
			AssertNotNull(cud);
			AssertNull(cud.Payment.PaymentAmount);

			var parentDeclaration = Factory.New<JobDeclaration>();
			parentDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			parentDeclaration.TransactionNumber.AccountSecurityCode = "10207";
			parentDeclaration.TransactionNumber.SequentialNumber = "50000000";
			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1ID = parentDeclaration.PK;
			pivot.XX_Relation2ID = entry.Declaration.PK;
			Factory.Save();
			dutyTaxFee = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.FirstOrDefault().GovernmentAgencyGoodsItem.FirstOrDefault().DutyTaxFee;
			tot = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "TOT");
			AssertNotNull(tot);
			AssertEquals("PayMent Currency", Core.Constants.CurrencyCodes.Canada, tot.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("PayMent Amount", 3.4m, tot.Payment.PaymentAmount.Amount);
			AssertEquals(2, tot.DutyTaxFeeAssessmentBasis.Count());
			AssertNotNull(tot.DutyTaxFeeAssessmentBasis.FirstOrDefault(x => x.Amount == 121m && x.CurrencyCode == Core.Constants.CurrencyCodes.Canada));
			AssertNotNull(tot.DutyTaxFeeAssessmentBasis.FirstOrDefault(x => x.Amount == 30.58m && x.CurrencyCode == Core.Constants.CurrencyCodes.Canada));

			sur = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "SUR");
			AssertNotNull(sur);
			AssertEquals("RequestOverrideCode", "X", sur.RequestOverrideCode);
			AssertEquals("PayMent Currency", Core.Constants.CurrencyCodes.Canada, sur.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("PayMent Amount", 37m, sur.Payment.PaymentAmount.Amount);

			saf = dutyTaxFee.FirstOrDefault(x => x.TypeCode == "OTH");
			AssertNotNull(saf);
			AssertEquals("RequestOverrideCode", "X", saf.RequestOverrideCode);
			AssertEquals("PayMent Currency", Core.Constants.CurrencyCodes.Canada, saf.Payment.PaymentAmount.CurrencyCode);
			AssertEquals("PayMent Amount", 44m, saf.Payment.PaymentAmount.Amount);
		}

		public void TestCountrySubDivisionCode()
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "TestOrg";
			var consignorAddress = consignor.Addresses.AddNew();
			consignorAddress.Address1 = "Test Address1";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Supplier = consignor.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.SupplierDocumentaryAddress.E2_OA_Address = consignorAddress.PK;
			invoice.SupplierDocumentaryAddress.E2_AddressOverride = true;
			invoice.SupplierDocumentaryAddress.E2_CompanyName = "TEST COMPANY";
			invoice.SupplierDocumentaryAddress.E2_State = "NY";
			invoice.SupplierDocumentaryAddress.E2_RN_NKCountryCode = "CA";

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			var entryLine1 = entry.AllEntryLines.AddNew();

			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			AssertEquals("NY", goodsShipments[0].Seller.SellerAddress.CountrySubDivisionCode);
		}

		public void TestStripOutInvalidCharacters()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();

			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Description = "ORGANIZER FOR OFFICE !÷ü😊測試";
			line1.JI_Tariff = "1111111111";
			line1.JI_CustomsQuantity = 1m;
			line1.JI_CustomsUnitQty = "KGM";

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			var entryLine1 = entry.AllEntryLines.AddNew();
			var link1 = line1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;

			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			AssertEquals("ORGANIZER FOR OFFICE", goodsShipments[0].GovernmentAgencyGoodsItem.First().Description);
		}

		public void TestGoodsShipmentCommodityDescriptionLVS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 03, 18);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 03, 18);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var entryLine1 = entry.AllEntryLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine1.JI_Description = "test desc";
			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;

			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			AssertEquals("LVS", goodsShipments[0].GovernmentAgencyGoodsItem.First().Description);
			invoiceLine1.CA_AuthorityNumber = "123456";
			AssertEquals("test desc", goodsShipments[0].GovernmentAgencyGoodsItem.First().Description);
			invoiceLine1.CA_AuthorityNumber = ZString.Empty;
			var add = invoiceLine1.DutiesAndTaxes.AddNew();
			add.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add.C1_ExemptCode = SIMACodes.Codes.C10;
			add.C1_Code = "AD";
			AssertEquals("test desc", goodsShipments[0].GovernmentAgencyGoodsItem.First().Description);
		}

		public void TestGoodsShipmentCommodityOriginLVS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 03, 18);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 03, 18);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var entryLine1 = entry.AllEntryLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine1.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			invoiceLine1.JI_CountryOfOrigin = "TW";
			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;

			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, goodsShipments[0].GovernmentAgencyGoodsItem.First().Origin.CountryCode);
			invoiceLine1.CA_AuthorityNumber = "123456";
			AssertEquals("TW", goodsShipments[0].GovernmentAgencyGoodsItem.First().Origin.CountryCode);
			invoiceLine1.CA_AuthorityNumber = ZString.Empty;
			var add = invoiceLine1.DutiesAndTaxes.AddNew();
			add.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add.C1_ExemptCode = SIMACodes.Codes.C10;
			add.C1_Code = "AD";
			AssertEquals("TW", goodsShipments[0].GovernmentAgencyGoodsItem.First().Origin.CountryCode);
		}

		public void TestGoodsShipmentCommodityExportCountryLVS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 03, 18);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 03, 18);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var entryLine1 = entry.AllEntryLines.AddNew();

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			invoiceLine1.CA_TreatmentCode = TariffTreatmentCodes.Codes.UnitedStates;
			invoiceLine1.CA_RN_NKExport = "TW";
			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;

			var goodsShipments = new CADMessageWrapper(entry).CADDocumentMetaData.Declaration.GoodsShipment.ToArray();
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, goodsShipments[0].GovernmentAgencyGoodsItem.First().ExportCountry.CountryCode);
			invoiceLine1.CA_AuthorityNumber = "123456";
			AssertEquals("TW", goodsShipments[0].GovernmentAgencyGoodsItem.First().ExportCountry.CountryCode);
			invoiceLine1.CA_AuthorityNumber = ZString.Empty;
			var add = invoiceLine1.DutiesAndTaxes.AddNew();
			add.C1_TaxType = DutyAndTaxTypes.Codes.ADD;
			add.C1_ExemptCode = SIMACodes.Codes.C10;
			add.C1_Code = "AD";
			AssertEquals("TW", goodsShipments[0].GovernmentAgencyGoodsItem.First().ExportCountry.CountryCode);
		}

		CusEntryHeader GetCusEntryHeader(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2024, 03, 18);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2024, 03, 18);

			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_Status = MessageStatusList.Codes.NotSent;
			entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_CustomsValue = 121m;

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.JI_CustomsUnitQty = "KGM";
			var sur1 = invoiceLine1.DutiesAndTaxes.AddNew();
			sur1.C1_Override = true;
			sur1.C1_TaxType = DutyAndTaxTypes.Codes.SUR;
			sur1.C1_Amount = 37m;
			sur1.Quantity = 38m;
			sur1.C1_UnitOfMeasure = "M3";
			sur1.C1_Code = "24187A";

			var saf1 = invoiceLine1.DutiesAndTaxes.AddNew();
			saf1.C1_Override = true;
			saf1.C1_TaxType = DutyAndTaxTypes.Codes.SAF;
			saf1.C1_Amount = 44m;
			saf1.Quantity = 254m;
			saf1.C1_UnitOfMeasure = "M3";
			saf1.C1_Code = "24187B";

			var link1 = invoiceLine1.AdditionalEntryLineLinks.AddNew();
			link1.BU_CL = entryLine1.PK;

			var cvcLineFee = Factory.New<CusEntryLineFee>();
			cvcLineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			cvcLineFee.CF_ChargeType = "CVC";
			cvcLineFee.CF_ChargeAmount = 22m;
			cvcLineFee.CF_CL = entryLine1.PK;

			var cudLineFee = Factory.New<CusEntryLineFee>();
			cudLineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			cudLineFee.CF_ChargeType = "DTY";
			cudLineFee.CF_ChargeAmount = 1.1m;
			cudLineFee.CF_CL = entryLine1.PK;

			var gstLineFee = Factory.New<CusEntryLineFee>();
			gstLineFee.CF_Source = CusEntryLineFeeSourceCodeList.Codes.CW1;
			gstLineFee.CF_ChargeType = "GST";
			gstLineFee.CF_ChargeAmount = 2.3m;
			gstLineFee.CF_CL = entryLine1.PK;

			return entry;
		}
	}
}
