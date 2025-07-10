using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class AUAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAQISCommodityCodesValidationForInvoiceLine()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISCommodityCode commodityCode = invoiceLine.AQISCommodityCodes.AddNew();
			commodityCode.Code = ",";
			invoiceLine.AddInfo.ReBuildAQISCommodityCodes();
			invoiceLine.AddInfo.Validation.ValidateZA_AQISCommCodes_Hidden();
			AssertHasErrors("Has errors", invoiceLine.AddInfo.ZA_AQISCommCodes_HiddenInfo);

			invoiceLine.AddInfo.ZA_AQISCommCodes_Hidden = "123";
			AssertHasMessageErrors("Has Message Errors", invoiceLine.AddInfo.ZA_AQISCommCodes_HiddenInfo);
		}

		public void TestAQISCommodityCodesValidationForInvoiceHeader()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			AQISCommodityCode commodityCode = invoiceHeader.AQISCommodityCodes.AddNew();
			commodityCode.Code = ",";
			invoiceHeader.AddInfo.ReBuildAQISCommodityCodes();
			invoiceHeader.AddInfo.Validation.ValidateZA_AQISCommCodes_Hidden();
			AssertHasErrors("Has errors", invoiceHeader.AddInfo.ZA_AQISCommCodes_HiddenInfo);

			invoiceHeader.AddInfo.ZA_AQISCommCodes_Hidden = "123";
			AssertHasMessageErrors("Has Message Errors", invoiceHeader.AddInfo.ZA_AQISCommCodes_HiddenInfo);
		}

		public void TestAQISCommodityCodesValidationForDeclaration()
		{
			AQISCommodityCode commodityCode = declaration.AQISCommodityCodes.AddNew();
			commodityCode.Code = ",";
			declaration.AddInfo.ReBuildAQISCommodityCodes();
			declaration.AddInfo.Validation.ValidateZA_AQISCommCodes_Hidden();
			AssertHasErrors("Has errors", declaration.AddInfo.ZA_AQISCommCodes_HiddenInfo);

			declaration.AddInfo.ZA_AQISCommCodes_Hidden = "123";
			AssertHasMessageErrors("Has Message Errors", declaration.AddInfo.ZA_AQISCommCodes_HiddenInfo);
		}

		public void TestAQISEntityIdValidationForInvoiceLine()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISEntityId entityId = invoiceLine.AQISEntityIds.AddNew();
			entityId.Code = ",";
			invoiceLine.AddInfo.ReBuildAQISEntityIds();
			invoiceLine.AddInfo.Validation.ValidateZA_AQISEntityIds_Hidden();
			AssertHasErrors("Has errors", invoiceLine.AddInfo.ZA_AQISEntityIds_HiddenInfo);

			invoiceLine.AddInfo.ZA_AQISEntityIds_Hidden = "123";
			AssertHasMessageErrors("Has Message Errors", invoiceLine.AddInfo.ZA_AQISEntityIds_HiddenInfo);
		}

		public void TestAQISEntityIdValidationForInvoiceHeader()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			AQISEntityId entityId = invoiceHeader.AQISEntityIds.AddNew();
			entityId.Code = ",";
			invoiceHeader.AddInfo.ReBuildAQISEntityIds();
			invoiceHeader.AddInfo.Validation.ValidateZA_AQISEntityIds_Hidden();
			AssertHasErrors("Has errors", invoiceHeader.AddInfo.ZA_AQISEntityIds_HiddenInfo);

			invoiceHeader.AddInfo.ZA_AQISEntityIds_Hidden = "123";
			AssertHasMessageErrors("Has Message Errors", invoiceHeader.AddInfo.ZA_AQISEntityIds_HiddenInfo);
		}

		public void TestAQISEntityIdValidationForDeclaration()
		{
			AQISEntityId entityId = declaration.AQISEntityIds.AddNew();
			entityId.Code = ",";
			declaration.AddInfo.ReBuildAQISEntityIds();
			declaration.AddInfo.Validation.ValidateZA_AQISEntityIds_Hidden();
			AssertHasErrors("Has errors", declaration.AddInfo.ZA_AQISEntityIds_HiddenInfo);

			declaration.AddInfo.ZA_AQISEntityIds_Hidden = "123";
			AssertHasMessageErrors("Has Message Errors", declaration.AddInfo.ZA_AQISEntityIds_HiddenInfo);
		}

		public void TestAQISProducerCodeValidationForInvoiceLine()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISProducerCode producerCode = invoiceLine.AQISProducerCodes.AddNew();
			producerCode.Code = ",";
			invoiceLine.AddInfo.ReBuildAQISProducerCodes();
			invoiceLine.AddInfo.Validation.ValidateZA_AQISProducerCodes_Hidden();
			AssertHasErrors("Has errors", invoiceLine.AddInfo.ZA_AQISProducerCodes_HiddenInfo);

			invoiceLine.AddInfo.ZA_AQISProducerCodes_Hidden = "123";
			AssertHasMessageErrors("Has Message Errors", invoiceLine.AddInfo.ZA_AQISProducerCodes_HiddenInfo);
		}

		public void TestAQISProducerCodeValidationForInvoiceHeader()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			AQISProducerCode producerCode = invoiceHeader.AQISProducerCodes.AddNew();
			producerCode.Code = ",";
			invoiceHeader.AddInfo.ReBuildAQISProducerCodes();
			invoiceHeader.AddInfo.Validation.ValidateZA_AQISProducerCodes_Hidden();
			AssertHasErrors("Has errors", invoiceHeader.AddInfo.ZA_AQISProducerCodes_HiddenInfo);

			invoiceHeader.AddInfo.ZA_AQISProducerCodes_Hidden = "123";
			AssertHasMessageErrors("Has Message Errors", invoiceHeader.AddInfo.ZA_AQISProducerCodes_HiddenInfo);
		}

		public void TestAQISProducerCodeValidationForDeclaration()
		{
			AQISProducerCode producerCode = declaration.AQISProducerCodes.AddNew();
			producerCode.Code = ",";
			declaration.AddInfo.ReBuildAQISProducerCodes();
			declaration.AddInfo.Validation.ValidateZA_AQISProducerCodes_Hidden();
			AssertHasErrors("Has errors", declaration.AddInfo.ZA_AQISProducerCodes_HiddenInfo);

			declaration.AddInfo.ZA_AQISProducerCodes_Hidden = "123";
			AssertHasMessageErrors("Has Message Errors", declaration.AddInfo.ZA_AQISProducerCodes_HiddenInfo);
		}

		public void TestAQISPermitIdValidationForInvoiceLine()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			AQISPermitId permitId = invoiceLine.AQISPermitIds.AddNew();
			permitId.Code = ",";
			invoiceLine.AddInfo.ReBuildAQISPermitIds();
			invoiceLine.AddInfo.Validation.ValidateZA_AQISPermitIds_Hidden();
			AssertHasErrors("Has errors", invoiceLine.AddInfo.ZA_AQISPermitIds_HiddenInfo);
		}

		public void TestAQISPermitIdValidationForInvoiceHeader()
		{
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();

			AQISPermitId permitId = invoiceHeader.AQISPermitIds.AddNew();
			permitId.Code = ",";
			invoiceHeader.AddInfo.ReBuildAQISPermitIds();
			invoiceHeader.AddInfo.Validation.ValidateZA_AQISPermitIds_Hidden();
			AssertHasErrors("Has errors", invoiceHeader.AddInfo.ZA_AQISPermitIds_HiddenInfo);
		}

		public void TestAQISPermitIdValidationForDeclaration()
		{
			AQISPermitId permitId = declaration.AQISPermitIds.AddNew();
			permitId.Code = ",";
			declaration.AddInfo.ReBuildAQISPermitIds();
			declaration.AddInfo.Validation.ValidateZA_AQISPermitIds_Hidden();
			AssertHasErrors("Has errors", declaration.AddInfo.ZA_AQISPermitIds_HiddenInfo);
		}

		public void TestUnaccompaniedPersonalEffectsValidation()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "CMR";

			declaration.AddInfo.ZA_UPEIndicator_Hidden = false;
			declaration.AddInfo.Validation.ValidateAll();

			AssertNoMessageErrors(declaration.AddInfo.ZA_UPEImporterPassportCountry_HiddenInfo);
			AssertNoMessageErrors(declaration.AddInfo.ZA_UPEImporterPassportNumber_HiddenInfo);
			AssertNoMessageErrors(declaration.AddInfo.ZA_UPEImporterDOB_HiddenInfo);
			AssertNoMessageErrors(declaration.AddInfo.ZA_UPEImporterSex_HiddenInfo);
			AssertNoMessageErrors(declaration.AddInfo.ZA_UPESpouseName_HiddenInfo);
			AssertNoMessageErrors(declaration.AddInfo.ZA_UPESpousePassportCountry_HiddenInfo);
			AssertNoMessageErrors(declaration.AddInfo.ZA_UPESpousePassportNumber_HiddenInfo);

			declaration.AddInfo.ZA_UPEIndicator_Hidden = true;
			declaration.AddInfo.Validation.ValidateAll();

			AssertHasMessageError(declaration.AddInfo.ZA_UPEImporterPassportCountry_HiddenInfo, "You have not entered a Country/Region.");
			AssertHasMessageError(declaration.AddInfo.ZA_UPEImporterPassportNumber_HiddenInfo, "You have not entered a Number.");
			AssertHasMessageError(declaration.AddInfo.ZA_UPEImporterDOB_HiddenInfo, "You have not entered a DOB.");
			AssertHasMessageError(declaration.AddInfo.ZA_UPEImporterSex_HiddenInfo, "You have not entered a Sex.");
			AssertNoMessageErrors(declaration.AddInfo.ZA_UPESpouseName_HiddenInfo);
			AssertNoMessageErrors(declaration.AddInfo.ZA_UPESpousePassportCountry_HiddenInfo);
			AssertNoMessageErrors(declaration.AddInfo.ZA_UPESpousePassportNumber_HiddenInfo);

			declaration.AddInfo.ZA_UPEImporterPassportCountry_Hidden = "AU";
			declaration.AddInfo.Validation.ValidateAll();

			AssertHasMessageError(declaration.AddInfo.ZA_UPEImporterPassportCountry_HiddenInfo, "The code you have selected is not in the list.");

			declaration.AddInfo.ZA_UPEImporterPassportCountry_Hidden = "AUS";
			declaration.AddInfo.Validation.ValidateAll();

			AssertNoMessageErrors(declaration.AddInfo.ZA_UPEImporterPassportCountry_HiddenInfo);

			declaration.AddInfo.ZA_UPEImporterSex_Hidden = "Y";
			declaration.AddInfo.Validation.ValidateAll();

			AssertHasMessageError(declaration.AddInfo.ZA_UPEImporterSex_HiddenInfo, "The code you have selected is not in the list.");

			declaration.AddInfo.ZA_UPEImporterSex_Hidden = "M";
			declaration.AddInfo.Validation.ValidateAll();

			AssertNoMessageErrors(declaration.AddInfo.ZA_UPEImporterSex_HiddenInfo);

			declaration.AddInfo.ZA_UPESpouseName_Hidden = "BOB";
			declaration.AddInfo.Validation.ValidateAll();

			AssertNoMessageErrors(declaration.AddInfo.ZA_UPESpouseName_HiddenInfo);
			AssertHasMessageErrors(declaration.AddInfo.ZA_UPESpousePassportCountry_HiddenInfo);
			AssertHasMessageErrors(declaration.AddInfo.ZA_UPESpousePassportNumber_HiddenInfo);

			declaration.AddInfo.ZA_UPESpouseName_Hidden = ZString.Empty;
			declaration.AddInfo.ZA_UPESpousePassportCountry_Hidden = "AUS";
			declaration.AddInfo.Validation.ValidateAll();

			AssertHasMessageErrors(declaration.AddInfo.ZA_UPESpouseName_HiddenInfo);
			AssertNoMessageErrors(declaration.AddInfo.ZA_UPESpousePassportCountry_HiddenInfo);
			AssertHasMessageErrors(declaration.AddInfo.ZA_UPESpousePassportNumber_HiddenInfo);

			declaration.AddInfo.ZA_UPESpousePassportCountry_Hidden = "";
			declaration.AddInfo.ZA_UPESpousePassportNumber_Hidden = "1234567890ABCD";
			declaration.AddInfo.Validation.ValidateAll();

			AssertHasMessageErrors(declaration.AddInfo.ZA_UPESpouseName_HiddenInfo);
			AssertHasMessageErrors(declaration.AddInfo.ZA_UPESpousePassportCountry_HiddenInfo);
			AssertNoMessageErrors(declaration.AddInfo.ZA_UPESpousePassportNumber_HiddenInfo);

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";
			JobComInvoiceLine invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "9999.40.15 41";
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "9999.40.15 42";

			invoiceHeader.Validation.ValidateAll();
			AssertNoMessageErrors(invoiceHeader.JZ_InvoiceNumberInfo);

			invoiceLine1.JI_Tariff = "9999.40.15 40";

			invoiceHeader.Validation.ValidateAll();
			AssertHasMessageError(invoiceHeader.JZ_InvoiceNumberInfo, "There must be at least ONE tariff line entered that has a Tariff Classification of 9999 4015 and the statistical code of 41.");

			declaration.AddInfo.ZA_UPEImporterPassportCountry_Hidden = "DEU";
			declaration.AddInfo.Validation.ValidateAll();
			AssertNoMessageErrors(declaration.AddInfo.ZA_UPEImporterPassportCountry_HiddenInfo);

			declaration.AddInfo.ZA_UPEImporterPassportCountry_Hidden = "D G";
			declaration.AddInfo.Validation.ValidateAll();
			AssertHasMessageError(declaration.AddInfo.ZA_UPEImporterPassportCountry_HiddenInfo, "The code you have selected is not in the list.");
		}

		public void TestOriginValidationForCMR()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "CMR";
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.AddInfo.ZA_ORG = "ZZ";
			AssertHasMessageErrors("Origin has a message error", invoiceHeader.AddInfo.ZA_ORGInfo);

			invoiceHeader.AddInfo.ZA_ORG = "AUST";
			AssertHasMessageErrors("Origin has a message error", invoiceHeader.AddInfo.ZA_ORGInfo);

			invoiceHeader.AddInfo.ZA_ORG = "AU";
			AssertNoMessageErrors("Origin has no message error", invoiceHeader.AddInfo.ZA_ORGInfo);

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_ORG = "ZZ";
			AssertHasMessageErrors("Origin has a message error", invoiceLine.AddInfo.ZA_ORGInfo);

			invoiceLine.AddInfo.ZA_ORG = "AUST";
			AssertHasMessageErrors("Origin has a message error", invoiceLine.AddInfo.ZA_ORGInfo);

			invoiceLine.AddInfo.ZA_ORG = "AU";
			AssertNoMessageErrors("Origin has no message error", invoiceLine.AddInfo.ZA_ORGInfo);
		}

		public void TestAddInfoNotificationsPropagateToAddInfoLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invLine = invHeader.JobComInvoiceLines.AddNew();
			var addInfo = invLine.AddInfo;

			AssertEquals(false, addInfo.ZA_DCXInfo.HasMessageErrors());
			addInfo.ZA_DCX = "ZZZZ";
			AssertEquals(true, addInfo.ZA_DCXInfo.HasMessageErrors());
			ZString messageError = addInfo.ZA_DCXInfo.GetMessageErrors().GetFirstMessage();
			addInfo.Validation.ValidateAddInfoLine();
			AssertEquals(true, addInfo.AddInfoLineInfo.HasMessageError(messageError));
			addInfo.ZA_DCX = "";
			addInfo.Validation.ValidateAddInfoLine();
			AssertEquals(false, addInfo.AddInfoLineInfo.HasMessageError(messageError));
			addInfo.AddInfoLine = "DCX=ZXZX";
			AssertEquals(true, addInfo.AddInfoLineInfo.HasMessageError(messageError));
			addInfo.AddInfoLine = "";
			AssertEquals(false, addInfo.AddInfoLineInfo.HasMessageError(messageError));
			addInfo.AddInfoLine = "DCX=ZXZX";
			addInfo.RunPreSaveValidation();
			AssertEquals(true, addInfo.AddInfoLineInfo.HasMessageError(messageError));

			addInfo.PRI_InstrumentNo = "";
			addInfo.PRI_InstrumentType = CMRInstrumentTypeList.Codes.TariffQuota;
			AssertEquals(true, addInfo.PRI_InstrumentTypeInfo.HasMessageErrors());
			messageError = addInfo.PRI_InstrumentTypeInfo.GetMessageErrors().GetFirstMessage();
			addInfo.Validation.ValidateAddInfoLine();
			AssertEquals(true, addInfo.AddInfoLineInfo.HasMessageError(messageError));
		}

		public void TestCheckZA_WRNWhereDecIsNotNature30()
		{
			AssertEquals("Declaration is not a nature 30", false, declaration.IsExWarehouse);

			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			AssertNoMessageErrors("WRN", header.AddInfo.ZA_WRNInfo);

			header.AddInfo.ZA_WRN = "123456789";
			AssertHasMessageErrors("WRN", header.AddInfo.ZA_WRNInfo);

			header.AddInfo.ZA_WRN = "";
			AssertNoMessageErrors("WRN", header.AddInfo.ZA_WRNInfo);

			JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
			AssertNoMessageErrors("WRN", invoiceLine.AddInfo.ZA_WRNInfo);

			invoiceLine.AddInfo.ZA_WRN = "123456789";
			AssertHasMessageErrors("WRN", invoiceLine.AddInfo.ZA_WRNInfo);

			invoiceLine.AddInfo.ZA_WRN = "";
			AssertNoMessageErrors("WRN", invoiceLine.AddInfo.ZA_WRNInfo);
		}

		public void TestCheckZA_WRNWhereDecIsNature30()
		{
			declaration.JE_MessageType = "EXW";
			AssertEquals("Declaration is a nature 30", true, declaration.IsExWarehouse);

			JobComInvoiceHeader header = declaration.Invoices.AddNew();
			AssertNoMessageErrors("WRN", header.AddInfo.ZA_WRNInfo);

			header.AddInfo.ZA_WRN = "123456789";
			AssertNoMessageErrors("WRN", header.AddInfo.ZA_WRNInfo);

			header.AddInfo.ZA_WRN = "";
			AssertNoMessageErrors("WRN", header.AddInfo.ZA_WRNInfo);

			JobComInvoiceLine invoiceLine = header.JobComInvoiceLines.AddNew();
			AssertNoMessageErrors("WRN", invoiceLine.AddInfo.ZA_WRNInfo);

			invoiceLine.AddInfo.ZA_WRN = "123456789";
			AssertNoMessageErrors("WRN", invoiceLine.AddInfo.ZA_WRNInfo);
		}

		[TestDate(2005, 10, 20)]
		public void TestInstrumentCode()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;

			var instrument = CMRInstrument.New(Factory);
			instrument.IN_Number = "111X22";
			instrument.IN_StartDate = new ZDateTime(2005, 1, 1);
			instrument.IN_EndDate = new ZDateTime(2005, 1, 2);

			var instrument2 = CMRInstrument.New(Factory);
			instrument2.IN_Number = "111X23";
			instrument2.IN_StartDate = new ZDateTime(2005, 1, 1);
			instrument2.IN_EndDate = ZDateTime.Today.AddDays(1);
			instrument2.IN_Type = "TC";
			instrument2.IN_TariffValidationType = CMRInstrument.InstrumentValidationType;

			var instrument3 = CMRInstrument.New(Factory);
			instrument3.IN_Number = "111X24";
			instrument3.IN_StartDate = new ZDateTime(2005, 1, 1);
			instrument3.IN_RevocationDate = new ZDateTime(2005, 1, 3);

			var instrument4 = CMRInstrument.New(Factory);
			instrument4.IN_Number = "111X25";
			instrument4.IN_StartDate = new ZDateTime(2005, 1, 1);
			instrument4.IN_RevocationDate = new ZDateTime(2005, 1, 4);
			instrument4.IN_EndDate = new ZDateTime(2005, 1, 5);

			var instrumentTariff = CMRInstrumentTariffGroup.New(Factory);
			instrumentTariff.IG_InstrumentNumber = "111X23";
			instrumentTariff.IG_InstrumentType = "TC";
			instrumentTariff.IG_TariffGroupItem = "12345678";

			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 20);
			AssertEquals("PreCondition:EffectiveDutyDate", new ZDateTime(2005, 10, 20), testDec.EffectiveDutyDate);

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.AddInfo.ZA_InstrumentCode_Hidden = "111X22";
			AssertHasMessageError("Instrument code is expired for this job", invoiceLine.AddInfo.ZA_InstrumentCode_HiddenInfo, "The selected instrument code has expired on " + instrument.IN_EndDate.ToShortDateString());

			testDec.JE_EntrySubmittedDate = new ZDateTime(2005, 1, 1);
			testDec.JE_DateOfFirstArrival = new ZDateTime(2005, 1, 2);
			var entry = testDec.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("EffectiveDutyDate is the latter of EntrySubmittedDate or DateOfFirstArrival", new ZDateTime(2005, 1, 2), testDec.EffectiveDutyDate);
			invoiceLine.AddInfo.ZA_InstrumentCode_Hidden = "111X22";
			AssertNoMessageError(invoiceLine.AddInfo.ZA_InstrumentCode_HiddenInfo, "The selected instrument code has expired on " + instrument.IN_EndDate.ToShortDateString());

			var classification = Factory.New<Classification>();
			classification.AddInfo.ZA_InstrumentCode_Hidden = "111X22";
			AssertEquals("Instrument has expired for today's date", false, instrument.IsValidForThisDate(ZDateTime.Today));
			AssertHasMessageError("Instrument code is expired for this job", classification.AddInfo.ZA_InstrumentCode_HiddenInfo, "The selected instrument code has expired on " + instrument.IN_EndDate.ToShortDateString());

			invoiceLine.AddInfo.ZA_InstrumentCode_Hidden = "111X23";
			invoiceLine.AddInfo.ZA_InstrumentType_Hidden = "TC";
			invoiceLine.JI_Tariff = "1234.56.77";
			AssertHasMessageErrorContaining(invoiceLine.AddInfo.ZA_InstrumentCode_HiddenInfo, "The instrument is not valid for the tariff number.");

			invoiceLine.JI_Tariff = "1234.56.78";
			AssertNoMessageErrorContaining(invoiceLine.AddInfo.ZA_InstrumentCode_HiddenInfo, "The instrument is not valid for the tariff number.");

			classification.AddInfo.ZA_InstrumentCode_Hidden = "111X24";
			AssertHasMessageError("Instrument code revoked", classification.AddInfo.ZA_InstrumentCode_HiddenInfo, "The selected instrument code was revoked on " + instrument3.IN_RevocationDate.ToShortDateString());

			classification.AddInfo.ZA_InstrumentCode_Hidden = "111X25";
			AssertHasMessageError("Instrument code expired and revoked", classification.AddInfo.ZA_InstrumentCode_HiddenInfo, "The selected instrument code has expired on " + instrument4.IN_EndDate.ToShortDateString() +
				 " and it was revoked on " + instrument4.IN_RevocationDate.ToShortDateString());

			invoice.AddInfo.ZA_EFD = "301005";
			instrument4.IN_RevocationDate = ZDateTime.Now.AddDays(1);
			instrument4.IN_EndDate = ZDateTime.Now.AddDays(2);
			invoiceLine.AddInfo.ZA_InstrumentCode_Hidden = "111X25";
			AssertHasMessageError("Instrument code will expire and be revoked", invoiceLine.AddInfo.ZA_InstrumentCode_HiddenInfo, "The selected instrument code will expire on " + instrument4.IN_EndDate.ToShortDateString() +
				 " and it will be revoked on " + instrument4.IN_RevocationDate.ToShortDateString());
		}

		public void TestValuationBasisNotRequiredForExWarehouse()
		{
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.ExWarehouse;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceHeader.AddInfo.ZA_ValuationBasis_Hidden = ZString.Empty;
			invoiceLine.AddInfo.ZA_ValuationBasis_Hidden = ZString.Empty;
			Assert(!invoiceLine.AddInfo.ZA_ValuationBasis_HiddenInfo.HasMessageErrors());
		}

		public void TestValidateZA_ManifestClientIDOverride_Hidden()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.AddInfo.ZA_ManifestClientIDOverride_Hidden = "";
			Assert(!declaration.AddInfo.ZA_ManifestClientIDOverride_HiddenInfo.HasMessageErrors());
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			declaration.AddInfo.Validation.ValidateZA_ManifestClientIDOverride_Hidden();
			Assert(declaration.AddInfo.ZA_ManifestClientIDOverride_HiddenInfo.HasMessageErrors());

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			declaration.AddInfo.Validation.ValidateZA_ManifestClientIDOverride_Hidden();
			AssertEquals("Mainfest ID should be only for Edifice", false, declaration.AddInfo.ZA_ManifestClientIDOverride_HiddenInfo.HasMessageErrors());

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			declaration.AddInfo.Validation.ValidateZA_ManifestClientIDOverride_Hidden();
			Assert(declaration.AddInfo.ZA_ManifestClientIDOverride_HiddenInfo.HasMessageErrors());

			JobComInvoiceHeader invHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invHeader.AddInfo.Validation.ValidateZA_ManifestClientIDOverride_Hidden();
			Assert("NoMessageErrorsOnInvoiceHeader", !invHeader.AddInfo.ZA_ManifestClientIDOverride_HiddenInfo.HasMessageErrors());
		}

		/// <summary>
		/// Format SCN=(YTS)NNNN
		/// Y - year
		/// T - type of Security ( D / C / S / T / B / E / M / X / Y
		/// S - State code
		/// NNNN - Unique Number starting at 0001
		/// </summary>
		public virtual void TestSecurityNumber()
		{
			addInfo.ZA_SCN = "";
			AssertNoNotifications(addInfo.ZA_SCNInfo);
			addInfo.ZA_SCN = "4C10014";
			AssertNoNotifications(addInfo.ZA_SCNInfo);
			addInfo.ZA_SCN = "3F10001";
			AssertHasMessageErrors(addInfo.ZA_SCNInfo);
			addInfo.ZA_SCN = "3C90001";
			AssertHasMessageErrors(addInfo.ZA_SCNInfo);
			addInfo.ZA_SCN = "SECRET";
			AssertHasMessageErrors(addInfo.ZA_SCNInfo);
			addInfo.ZA_SCN = "14C10031";
			AssertHasMessageErrors(addInfo.ZA_SCNInfo);
			addInfo.ZA_SCN = "4C100A1";
			AssertHasMessageErrors(addInfo.ZA_SCNInfo);
		}

		public void TestValuationAdviceNumber()
		{
			addInfo.ZA_VAN = "";
			AssertNoNotifications(addInfo.ZA_VANInfo);
			addInfo.ZA_VAN = "1234";
			AssertNoNotifications(addInfo.ZA_VANInfo);
			addInfo.ZA_VAN = "12";
			AssertNoNotifications(addInfo.ZA_VANInfo);
			addInfo.ZA_VAN = "-123";
			AssertHasMessageErrors(addInfo.ZA_VANInfo);
			addInfo.ZA_VAN = "abcd";
			AssertHasMessageErrors(addInfo.ZA_VANInfo);
		}

		public void TestGSTE()
		{
			var code = Factory.New<CMRCodeLists>();
			code.CI_Code = "418";
			code.CI_CodeType = "GSTX";
			code.CI_Startdate = new ZDateTime(2013, 3, 1);

			var code2 = Factory.New<CMRCodeLists>();
			code2.CI_Code = "423";
			code2.CI_CodeType = "GSTX";
			code2.CI_Startdate = new ZDateTime(2010, 3, 1);
			code2.CI_EndDate = new ZDateTime(2013, 2, 28);

			addInfo.ZA_GSTE = "423";
			AssertHasMessageErrorContaining(addInfo.ZA_GSTEInfo, AUAddInfoValidation.EnterValidGSTECode);
			addInfo.ZA_GSTE = "418";
			AssertNoMessageErrorContaining(addInfo.ZA_GSTEInfo, AUAddInfoValidation.EnterValidGSTECode);
		}

		public void TestOrigin()
		{
			declaration.JE_ApplicationCode = "LEG";
			addInfo.ZA_ORG = "43G4";
			AssertHasMessageErrors(addInfo.ZA_ORGInfo);
			addInfo.ZA_ORG = "USA";
			AssertNoNotifications(addInfo.ZA_ORGInfo);
			addInfo.ZA_ORG = "WSAM";
			AssertNoNotifications(addInfo.ZA_ORGInfo);
			addInfo.ZA_ORG = "INIA";
			AssertNoNotifications(addInfo.ZA_ORGInfo);
			addInfo.ZA_ORG = "AU";
			AssertNoNotifications(addInfo.ZA_ORGInfo);
			addInfo.ZA_ORG = "XX";
			AssertHasMessageErrors(addInfo.ZA_ORGInfo);
		}

		public void TestAmberEntryProcessing()
		{
			// Allowed values are any combination of the following
			// C Classification
			// O Origin
			// P Preference
			// Q Quantity
			// T Treatment Code
			// V Valuation
			// D Dumping / Countervailing
			addInfo.ZA_AMB = "C";
			AssertNoNotifications(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "X";
			AssertHasMessageErrors(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "O";
			AssertNoNotifications(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "P";
			AssertNoNotifications(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "Q";
			AssertNoNotifications(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "T";
			AssertNoNotifications(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "V";
			AssertNoNotifications(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "D";
			AssertNoNotifications(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "CC";
			AssertHasMessageErrors(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "CO";
			AssertNoNotifications(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "X";
			AssertHasMessageErrors(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "COPQTVD";
			AssertNoNotifications(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "ZZ";
			AssertHasMessageErrors(addInfo.ZA_AMBInfo);
			addInfo.ZA_AMB = "DVTQPOC";
			AssertNoNotifications(addInfo.ZA_AMBInfo);
		}

		public void TestCheckZA_PST()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 01, 01);
			CMRPreferenceSchemePeriodSnapshot preference = CMRPreferenceSchemePeriodSnapshot.New(Factory);
			preference.PF_StartDate = new ZDateTime(2005, 01, 01);
			preference.PF_SchemeType = "CA";
			preference.PF_Description = "Description";

			CMRPreferenceSchemePeriodCountry preferenceSchemeCountry = CMRPreferenceSchemePeriodCountry.New(Factory);
			preferenceSchemeCountry.PC_CountryCode = "CA";
			preferenceSchemeCountry.PC_PreferenceSchemePeriodSnapshotSchemeType = "CA";

			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JE_DateAtFinalDestination = new ZDateTime(2005, 01, 01);
			invoiceHeader.AddInfo.ZA_PST = "111";
			invoiceHeader.AddInfo.Validation.ValidateZA_PST();
			AssertEquals("Preference Scheme Type has no error", false, invoiceHeader.AddInfo.ZA_PSTInfo.HasMessageErrors());

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceHeader.AddInfo.Validation.ValidateZA_PST();
			AssertEquals("Preference Scheme Type has an error", true, invoiceHeader.AddInfo.ZA_PSTInfo.HasMessageErrors());

			invoiceHeader.AddInfo.ZA_PST = "CA";
			invoiceHeader.AddInfo.Validation.ValidateZA_PST();
			AssertEquals("Preference Scheme Type has no error", false, invoiceHeader.AddInfo.ZA_PSTInfo.HasMessageErrors());

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_PST = "111";
			invoiceLine.AddInfo.Validation.ValidateZA_PST();
			AssertEquals("Preference Scheme Type has an error", true, invoiceLine.AddInfo.ZA_PSTInfo.HasMessageErrors());

			CMRTariffRatePeriodSnapshot tariffScheme = CMRTariffRatePeriodSnapshot.New(Factory);
			tariffScheme.TT_PreferenceSchemeType = "CA";
			tariffScheme.TT_StartDate = new ZDateTime(2005, 1, 1);
			tariffScheme.TT_TariffClassificationNumber = "00000001";
			invoiceLine.JI_Tariff = "00000001 00";
			invoiceLine.AddInfo.ZA_POC = "CA";
			invoiceLine.AddInfo.ZA_PST = "CA";
			invoiceLine.AddInfo.Validation.ValidateZA_PST();
			AssertEquals("Preference Scheme Type has no error", false, invoiceLine.AddInfo.ZA_PSTInfo.HasMessageErrors());
		}

		public void TestCheckZA_POC()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JE_DateAtFinalDestination = new ZDateTime(2005, 01, 01);
			invoiceHeader.AddInfo.ZA_POC = "ZZ";
			AssertEquals("Preference Scheme Type has no error", false, invoiceHeader.AddInfo.ZA_POCInfo.HasMessageErrors());

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			invoiceHeader.AddInfo.Validation.ValidateZA_POC();
			AssertEquals("Preference Origin Country/Region Code has an error", true, invoiceHeader.AddInfo.ZA_POCInfo.HasMessageErrors());

			invoiceHeader.AddInfo.ZA_POC = "";
			AssertEquals("Preference Origin Countr/Region has an error", false, invoiceHeader.AddInfo.ZA_POCInfo.HasMessageErrors());

			invoiceHeader.AddInfo.ZA_PST = "MY";
			invoiceHeader.AddInfo.Validation.ValidateZA_POC();
			AssertEquals("Preference Origin Country/Region has an error", true, invoiceHeader.AddInfo.ZA_POCInfo.HasMessageErrors());

			invoiceHeader.AddInfo.ZA_POC = "IT";
			invoiceHeader.AddInfo.Validation.ValidateZA_POC();
			AssertEquals("Preference Origin Country/Region has no error", false, invoiceHeader.AddInfo.ZA_POCInfo.HasMessageErrors());

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_POC = "ZZ";
			invoiceLine.AddInfo.Validation.ValidateZA_POC();
			AssertEquals("Preference Origin Country/Region Code has an error", true, invoiceLine.AddInfo.ZA_POCInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_POC = "";
			AssertEquals("Preference Origin Country/Region has an error", false, invoiceLine.AddInfo.ZA_POCInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_PST = "MY";
			invoiceLine.AddInfo.Validation.ValidateZA_POC();
			AssertEquals("Preference Origin Country/Region has an error", true, invoiceLine.AddInfo.ZA_POCInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_POC = "AU";
			invoiceLine.AddInfo.Validation.ValidateZA_POC();
			AssertEquals("Preference Origin Country/Region has no error", false, invoiceLine.AddInfo.ZA_POCInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_PST = AUAddInfo.GeneralPreferenceRate;
			invoiceLine.AddInfo.ZA_POC = "";
			invoiceLine.AddInfo.Validation.ValidateZA_POC();
			AssertEquals("Empty for General rate is acceptable", false, invoiceLine.AddInfo.ZA_POCInfo.HasMessageErrors());
		}

		public void TestCheckZA_PRT()
		{
			CMRPreferenceRulePeriodSnapshot preference = CMRPreferenceRulePeriodSnapshot.New(Factory);
			preference.PU_StartDate = new ZDateTime(2004, 03, 03);
			preference.PU_RuleType = "P50";
			preference.PU_Description = "Description";

			CMRPreferenceSchemeRule schemeRule = CMRPreferenceSchemeRule.New(Factory);
			schemeRule.PR_RuleType = "P50";
			schemeRule.PR_PreferenceSchemePeriodSnapshotSchemeType = "MY";

			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			JobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			declaration.JE_DateAtFinalDestination = new ZDateTime(2005, 01, 01);
			invoiceHeader.AddInfo.ZA_PRT = "ZZ";
			AssertEquals("Preference Scheme Type has no error", false, invoiceHeader.AddInfo.ZA_PRTInfo.HasMessageErrors());

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			invoiceHeader.AddInfo.Validation.ValidateZA_PRT();
			AssertEquals("Preference Rule Type has an error", true, invoiceHeader.AddInfo.ZA_PRTInfo.HasMessageErrors());

			invoiceHeader.AddInfo.ZA_PRT = "";
			AssertEquals("Preference Rule Type has no error", false, invoiceHeader.AddInfo.ZA_PRTInfo.HasMessageErrors());

			invoiceHeader.AddInfo.ZA_PST = "MY";
			invoiceHeader.AddInfo.Validation.ValidateZA_PRT();
			AssertEquals("Preference Rule Type has an error", true, invoiceHeader.AddInfo.ZA_PRTInfo.HasMessageErrors());

			invoiceHeader.AddInfo.ZA_PRT = "P50";
			AssertEquals("Preference Rule Type has no error", false, invoiceHeader.AddInfo.ZA_PRTInfo.HasMessageErrors());

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_PRT = "ZZ";
			AssertEquals("Preference Rule Type has an error", true, invoiceLine.AddInfo.ZA_PRTInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_PRT = "";
			AssertEquals("Preference Rule Type has no error", false, invoiceLine.AddInfo.ZA_PRTInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_PST = "MY";
			invoiceLine.AddInfo.Validation.ValidateZA_PRT();
			AssertEquals("Preference Rule Type has an error", true, invoiceLine.AddInfo.ZA_PRTInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_PRT = "P50";
			AssertEquals("Preference Rule Type has no error", false, invoiceLine.AddInfo.ZA_PRTInfo.HasMessageErrors());

			invoiceLine.AddInfo.ZA_PST = AUAddInfo.GeneralPreferenceRate;
			invoiceLine.AddInfo.ZA_PRT = "";
			AssertEquals("Empty Rule for General acceptable", false, invoiceLine.AddInfo.ZA_PRTInfo.HasMessageErrors());
		}

		[ExpectNoExceptions()]
		public void TestCheckZA_PRFWithNoInvoiceHeader()
		{
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			declaration.JE_DateOfFirstArrival = new ZDateTime(2005, 01, 01);
			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_PRF = "X";
			invoiceLine.AddInfo.Validation.ValidateZA_PST();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			addInfo = declaration.AddInfo;
		}

		protected AUAddInfo addInfo;
		protected JobDeclaration declaration;
	}
}
