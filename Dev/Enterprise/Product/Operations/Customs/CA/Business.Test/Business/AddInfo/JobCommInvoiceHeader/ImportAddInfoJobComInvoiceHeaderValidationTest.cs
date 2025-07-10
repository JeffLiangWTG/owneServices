using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.CA.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ImportAddInfoJobComInvoiceHeaderValidationTest : ComonImportAddInfoJobComInvoiceHeaderValidationTest
	{
		#region TestCheckCA_PortOfClearance

		public void TestCheckCA_PortOfClearance()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "X234", "Nanaimo", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.CA_PortOfClearanceInfo, "BLA", "X234");
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.CA_PortOfClearanceInfo, "BLA", "X234");
			invoiceHeader.CA_PortOfClearance = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.CA_PortOfClearanceInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#endregion

		#region TestCheckCA_RN_NKExport

		public void TestCheckCA_RN_NKExport()
		{
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			ValidationTestHelper.SetTotalValueForDuty(JobComInvoiceHeaderTest.VFDOverLimit, declaration);
			invoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			AssertHasMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);

			declaration.JE_TransportMode = "ROA";
			var entryHeader = declaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
			var line = entryHeader.MergedLines.AddNew();
			line.CL_CustomsValue = 100m;
			var collection = new DefaultFreightPercentageCollection();
			var item = collection.AddNew();
			item.ModeofTransport = "ROA";
			item.FreightPercentage = 2.3m;
			CACustomsDataRegistry.Instance.DefaultFreightPercentages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			CACustomsDataRegistry.Instance.DefaultFreightPercentages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DefaultFreightPercentageCollection());
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertHasMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);

			var oft = invoiceHeader.Charges.AddNew();
			oft.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;
			oft.J7_Amount = 24m;
			oft.J7_RX_NKCurrency = "CAD";
			invoiceHeader.CA_RN_NKExport = ZString.Empty;
			invoiceHeader.CA_RN_NKExport = Core.Constants.CountryCodes.UnitedStates;
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);

			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceHeader.CA_RN_NKExportInfo, "11", Constants.CountryCodes.Canada);
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.CA_RN_NKExportInfo, "11", Constants.CountryCodes.Canada);
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ReWarehouse13;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse21;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse22;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);

			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.AddInfoValidation.ValidateCA_RN_NKExport();
			});

			entryHeader.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse131;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse132;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse212;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse213;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse214;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse215;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse216;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse22;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			invoiceHeader.AddInfoValidation.ValidateCA_RN_NKExport();
			AssertNoMessageError(invoiceHeader.CA_RN_NKExportInfo, ImportAddInfoJobComInvoiceHeaderValidation.FreightChargeIsRequired);
		}

		#endregion

		#region TestCheckCA_RL_NKLastPort

		public void TestCheckCA_RL_NKLastPort()
		{
			declaration.JE_RL_NKPortOfLoading = invoiceHeader.AddInfoLookups.LastPorts[0].RL_Code;
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.CA_RL_NKLastPortInfo, "11123", invoiceHeader.AddInfoLookups.LastPorts[0].RL_Code);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageErrorForValidationType(invoiceHeader.CA_RL_NKLastPortInfo, ImportAddInfoJobComInvoiceHeaderValidation.LastPortRequired, ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_RL_NKLastPortInfo);
			declaration.JE_RL_NKPortOfLoading = invoiceHeader.AddInfoLookups.LastPorts[0].RL_Code;
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_RL_NKLastPortInfo);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine1.CA_GACInd = "Y";
			invoiceHeader.CA_RL_NKLastPort = ZString.Empty;
			AssertHasMessageErrorContaining(invoiceHeader.CA_RL_NKLastPortInfo, ImportAddInfoJobComInvoiceHeaderValidation.LastPortRequiredIIDWithGAC);
			invoiceHeader.CA_RL_NKLastPort = invoiceHeader.AddInfoLookups.LastPorts[0].RL_Code;
			AssertNoMessageErrorContaining(invoiceHeader.CA_RL_NKLastPortInfo, ImportAddInfoJobComInvoiceHeaderValidation.LastPortRequiredIIDWithGAC);

			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "JPG1S";
			unloco.RL_HasAirport = true;
			unloco.RL_IATA = "OOO";
			unloco.RL_IsSystem = false;
			Factory.Save();
			invoiceHeader.CA_RL_NKLastPort = unloco.RL_Code;
			AssertHasMessageErrorContaining(invoiceHeader.CA_RL_NKLastPortInfo, ImportAddInfoJobComInvoiceHeaderValidation.SystemDefinedUNLOCORequired);
			unloco.RL_IsSystem = true;
			Factory.Save();
			invoiceHeader.CA_RL_NKLastPort = unloco.RL_Code;
			AssertNoMessageErrorContaining(invoiceHeader.CA_RL_NKLastPortInfo, ImportAddInfoJobComInvoiceHeaderValidation.SystemDefinedUNLOCORequired);

			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.AddInfoValidation.ValidateCA_RL_NKLastPort();
			});
		}

		#endregion

		#region TestCheckCA_RN_NKTranshipment

		public void TestCheckCA_RN_NKTranshipment()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.CA_RN_NKTranshipmentInfo, "11", Constants.CountryCodes.Canada);
		}

		#endregion

		#region TestCheckCA_TradeZone

		public void TestCheckCA_TradeZone()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Constants.CountryCodes.Canada);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Codes.CACTradeZone, "CA Trade Zone", Constants.CountryCodes.Canada);
			helper.CreateNewOrGetExistingCusCodeList(Constants.CountryCodes.Canada, UniversalReferenceConstants.RefCusCodeListType.Codes.CACTradeZone, "86A", "Tacoma Boatbuilding Company (closed)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.CA_TradeZoneInfo, "A", "86A");
		}

		#endregion

		#region TestCheckCA_USPortOfExit

		public void TestCheckCA_USPortOfExit()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2813", "2813Desc", startDate, endDate);
			newFactory.Save();

			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.CA_USPortOfExitInfo, "A", "2813");
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			invoiceHeader.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageErrorForValidationType(invoiceHeader.CA_USPortOfExitInfo, ValidateForMessageType.B3CUSDEC, declaration);
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.ACROSS, declaration);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			invoiceHeader.CA_RN_NKExport = Constants.CountryCodes.Australia;
			ValidationTestHelper.SetForValidationMessageType(ValidateForMessageType.B3CUSDEC, declaration);
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);

			ValidationTestHelper.SetTotalValueForDuty(JobComInvoiceHeaderTest.VFDOverLimit, declaration);
			invoiceHeader.CA_TradeZone = "101";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageErrorForValidationType(invoiceHeader.CA_USPortOfExitInfo, ValidateForMessageType.B3CUSDEC, declaration);

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			invoiceHeader.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ReWarehouse13;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse20;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse21;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ExWarehouse22;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.TransferOfGoods30;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);

			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			standAloneInvoice.CA_RN_NKExport = Constants.CountryCodes.UnitedStates;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.AddInfoValidation.ValidateCA_USPortOfExit();
			});

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse131;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ReWarehouse132;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse201;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse211;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse212;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse213;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse214;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse215;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse216;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.ExWarehouse22;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods301;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
			declaration.JE_MessageSubType = CADEntryTypeList.Codes.TransferOfGoods302;
			ValidationTestHelper.AssertFieldIsNotMandatory(invoiceHeader.CA_USPortOfExitInfo);
		}

		#endregion

		#region TestCheckCA_TreatmentCode

		public void TestCheckCA_TreatmentCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreatePreferenceForCountry(TariffTreatmentCodes.Codes.UnitedStates, TariffTreatmentCodes.Codes.UnitedStates, Constants.CountryCodes.Canada);

			ValidationTestHelper.AssertInvalidCodeMessageErrorForValidationType(invoiceHeader.CA_TreatmentCodeInfo, "A", TariffTreatmentCodes.Codes.UnitedStates, ValidateForMessageType.B3CUSDEC, declaration);
			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			standAloneInvoice.CA_TreatmentCode = TariffTreatmentCodes.Codes.Norway;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.AddInfoValidation.ValidateCA_TreatmentCode();
			});
		}

		#endregion

		#region TestCheckCA_TimeLimit

		public override void TestCheckCA_TimeLimit()
		{
			base.TestCheckCA_TimeLimit();

			invoiceHeader.CA_TimeLimit = 1;
			invoiceHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;
			var messageError = "A monthly time limit must be specified with 1/60 Remission calculation method.";
			line.CA_CalculationMethod = CalculationMethods.Codes.OneSixtiethRemission;
			AssertHasMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, messageError);

			invoiceHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			AssertNoMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, messageError);

			invoiceHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Day;

			messageError = "A monthly time limit must be specified with 1/120 Remission calculation method.";
			line.CA_CalculationMethod = CalculationMethods.Codes.OneOneTwentiethRemission;
			AssertHasMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, messageError);
			invoiceHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			AssertNoMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, messageError);

			messageError = "A monthly time limit must be specified with 1/60 Remission calculation method.";

			line.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			AssertNoMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, messageError);

			invoiceHeader.CA_TimeLimitCode = TimeLimitUnitCodes.Codes.Month;
			AssertNoMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, messageError);

			line.CA_CalculationMethod = CalculationMethods.Codes.NoRemission;
			AssertNoMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, messageError);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Warehouse10;
			invoiceHeader.CA_TimeLimit = 0;
			AssertHasMessageError(invoiceHeader.CA_TimeLimitInfo, "Time Limits are mandatory on Warehouse Type 10 Jobs.");
			invoiceHeader.CA_TimeLimit = 1;
			AssertNoMessageError(invoiceHeader.CA_TimeLimitInfo, "Time Limits are mandatory on Warehouse Type 10 Jobs.");

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.CashD;
			invoiceHeader.CA_TimeLimit = 0;
			AssertHasMessageError(invoiceHeader.CA_TimeLimitInfo, "Time Limits are mandatory on Sight Type D Jobs.");
			invoiceHeader.CA_TimeLimit = 1;
			AssertNoMessageError(invoiceHeader.CA_TimeLimitInfo, "Time Limits are mandatory on Sight Type D Jobs.");

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ConfirmingSight;
			invoiceHeader.CA_TimeLimit = 0;
			AssertHasMessageError(invoiceHeader.CA_TimeLimitInfo, "Time Limits are mandatory on Confirming Sight Type AD Jobs.");
			invoiceHeader.CA_TimeLimit = 1;
			AssertNoMessageError(invoiceHeader.CA_TimeLimitInfo, "Time Limits are mandatory on Confirming Sight Type AD Jobs.");

			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.AddInfoValidation.ValidateCA_TimeLimit();
			});

			var cadEntry = declaration.ActiveEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			invoiceHeader.CA_TimeLimit = 0;
			AssertHasMessageError(invoiceHeader.CA_TimeLimitInfo, "Time Limits are mandatory on Warehouse Type 10 Jobs.");
			invoiceHeader.CA_TimeLimit = 1;
			AssertNoMessageError(invoiceHeader.CA_TimeLimitInfo, "Time Limits are mandatory on Warehouse Type 10 Jobs.");

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;
			invoiceHeader.CA_TimeLimit = 0;
			AssertHasMessageError(invoiceHeader.CA_TimeLimitInfo, "Time Limits are mandatory on Warehouse Type 10 Jobs.");
			invoiceHeader.CA_TimeLimit = 1;
			AssertNoMessageError(invoiceHeader.CA_TimeLimitInfo, "Time Limits are mandatory on Warehouse Type 10 Jobs.");
		}

		public void TestCheckCA_TimeLimitForTemporaryImport()
		{
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.ConfirmingSight;
			invoiceHeader.CA_TimeLimit = 0;
			invoiceHeader.CA_TimeLimitCode = ZString.Empty;
			line.CA_CalculationMethod = ZString.Empty;
			var validation = invoiceHeader.AddInfoValidation;
			validation.ValidateCA_TimeLimit();
			AssertNoMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			validation.ValidateCA_TimeLimit();
			AssertNoMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, MandatoryValidation.YouHaveNotEntered);

			line.CA_CalculationMethod = CalculationMethods.Codes.OneOneTwentiethRemission;
			validation.ValidateCA_TimeLimit();
			AssertHasMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceHeader.CA_TimeLimit = 1;
			validation.ValidateCA_TimeLimit();
			AssertNoMessageErrorContaining(invoiceHeader.CA_TimeLimitInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#endregion

		#region TestCheckCA_ValueForDutyCode

		public void TestCheckCA_ValueForDutyCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageErrorForValidationType(invoiceHeader.CA_ValueForDutyCodeInfo, "A", ValueForDutyCodes.Codes.UnrelatedFirmsIdenticalGoods, ValidateForMessageType.B3CUSDEC, declaration);
			var standAloneInvoice = Factory.New<JobComInvoiceHeader>();
			standAloneInvoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
			AssertNoExceptionThrown(() =>
			{
				standAloneInvoice.AddInfoValidation.ValidateCA_ValueForDutyCode();
			});
		}

		#endregion

		#region TestCheckCA_CasualImportCommodity

		public void TestCheckCA_CasualImportCommodity()
		{
			new UniversalReferenceTestDataHelper(Factory).CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Canada, RefCusCodeListType.Codes.CasualImportCommodity,
				"Cigarettes", "Cigarettes", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributes.Names.Type, CasualImportConstants.CasualImpCommodityType.Tobacco);
			Factory.Save();

			ValidationTestHelper.AssertErrorIfInvalidCode(invoiceHeader.CA_CasualImportCommodityInfo, "?", "Cigarettes");
		}

		#endregion

		#region TestCheckCA_CasualImportDestinationProvince

		public void TestCheckCA_CasualImportDestinationProvince()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(invoiceHeader.CA_CasualImportDestinationProvinceInfo, "??", CanadianProvinceList.Codes.Alberta);

			var consignee = Factory.New<OrgHeader>();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "CAYUL";
			consignee.MainAddress.OA_City = "MONTREAL";
			consignee.MainAddress.OA_State = "QC";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			invoice.JZ_OH_Consignee = consignee.PK;
			invoice.CA_IsCasualImport = true;
			AssertEquals("QC", invoice.CA_CasualImportDestinationProvince);
			AssertNoWarning(invoice.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceHeaderValidation.DestinationProvinceDoesNotMatch);
			invoice.CA_CasualImportDestinationProvince = "ON";
			AssertHasWarning(invoice.CA_CasualImportDestinationProvinceInfo, ImportAddInfoJobComInvoiceHeaderValidation.DestinationProvinceDoesNotMatch);
		}

		#endregion

		#region TestCheckCA_CarrierCode

		public void TestCheckCA_CarrierCode()
		{
			var carrier = Factory.New<Universal.ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "4646";
			carrier.ZZ4_Description = "Carrier Name";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.ZZ4_IsAir = true;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertNoErrors(invoiceHeader.CA_LVSCarrierInfo);
			var carrierCode = Factory.New<Universal.ZZRefCarrierCombined>();
			carrierCode.ZZ4_Code = "4646";
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.CA_LVSCarrierInfo, "??", "4646");

			invoiceHeader.CA_LVSCarrier = "4646";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			invoiceHeader.AddInfoValidation.ValidateCA_CarrierCode();
			AssertHasWarningContaining(invoiceHeader.CA_LVSCarrierInfo, "Carrier not allowed for mode of transport 'SEA'");

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			invoiceHeader.AddInfoValidation.ValidateCA_CarrierCode();
			AssertNoWarningContaining(invoiceHeader.CA_LVSCarrierInfo, "Carrier not allowed for mode of transport 'SEA'");
		}

		public void TestCheckCA_CarrierCode_CarriteAttribute()
		{
			var carrier = Factory.New<Universal.ZZRefCarrierCombined>();
			carrier.ZZ4_Code = "4646";
			carrier.ZZ4_Description = "Carrier Name";
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Canada;
			carrier.Attributes.AddNew(Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER);
			carrier.Attributes.AddNew(TransportTypeList.Codes.Air, TransportTypeList.Codes.Air);
			Factory.Save();

			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			AssertNoErrors(invoiceHeader.CA_LVSCarrierInfo);
			var carrierCode = Factory.New<Universal.ZZRefCarrierCombined>();
			carrierCode.ZZ4_Code = "4646";
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceHeader.CA_LVSCarrierInfo, "??", "4646");

			invoiceHeader.CA_LVSCarrier = "4646";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			invoiceHeader.AddInfoValidation.ValidateCA_CarrierCode();
			AssertHasWarningContaining(invoiceHeader.CA_LVSCarrierInfo, "Carrier not allowed for mode of transport 'SEA'");

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;

			invoiceHeader.AddInfoValidation.ValidateCA_CarrierCode();
			AssertNoWarningContaining(invoiceHeader.CA_LVSCarrierInfo, "Carrier not allowed for mode of transport 'SEA'");
		}

		#endregion

		#region Implementation

		protected override AddInfoJobComInvoiceHeader GetNewAddInfo()
		{
			return new AddInfoJobComInvoiceHeader(invoiceHeader.JZ_AddInfoInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			line = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
			line.CA_CalculationMethod = CalculationMethods.Codes.OneSixtiethRemission;
		}

		protected override ZString GetMessageType()
		{
			return JobMessageTypeList.Codes.Import;
		}

		JobComInvoiceLine line;

		#endregion
	}
}
