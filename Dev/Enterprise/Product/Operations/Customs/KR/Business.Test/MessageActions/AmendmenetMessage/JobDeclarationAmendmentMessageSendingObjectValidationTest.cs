using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class JobDeclarationAmendmentMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2021, 7, 15)]
		public void TestCheckShouldSend()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5BASnapshot();
			var parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5BB);
			parent.ShouldSend = true;
			AssertHasError(parent.ShouldSendInfo, "There are no changes to send.");

			entry.EntryInstruction.CEI_AgreedDutyRate = 3.21m;
			parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5BB);
			parent.ShouldSend = true;
			AssertNoError(parent.ShouldSendInfo, "There are no changes to send.");
		}

		public void TestCH_StatusIsCAPOrCAB()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms;
			var amendParent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DR);

			amendParent.ShouldSend = true;
			var errorMsg = string.Format(JobDeclarationMessageSendingObjectTest.CancellationApprovedByCustoms, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationApprovedByCustoms(entry.CH_MessageType));
			AssertHasError(amendParent.ShouldSendInfo, errorMsg);

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationByCustoms;
			amendParent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DR);
			amendParent.ShouldSend = true;
			errorMsg = string.Format(JobDeclarationMessageSendingObjectTest.CancellationByCustoms, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationByCustoms(entry.CH_MessageType));
			AssertHasError(amendParent.ShouldSendInfo, errorMsg);

			entry.CH_MessageType = JobMessageTypeList.Codes.Import;
			var originalParent = new JobDeclarationMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._929);
			originalParent.ShouldSend = true;
			errorMsg = string.Format(JobDeclarationMessageSendingObjectTest.CancellationByCustoms, ElectronicDocumentTypeList.GetMessageTypeSettingEntryToCancellationByCustoms(entry.CH_MessageType));
			AssertHasError(originalParent.ShouldSendInfo, errorMsg);
		}

		public void TestCH_StatusIsCDC()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationDeclined;
			entry.Declaration.JE_ExporterType = "A";
			entry.Declaration.JE_ExportGoodsType = "10";
			entry.Declaration.JE_ReturnReason = "AB";
			var entryLine1 = entry.MergedLines[0];
			entryLine1.CL_CustomsValue = 100m;
			var invoiceLine1 = entryLine1.RandomLine;
			invoiceLine1.JI_Model = "HYUNDAI ROBEX3000LC-7A-CHANGED";
			var parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS);
			parent.ShouldSend = true;
			AssertNoErrors(parent.ShouldSendInfo);
		}

		public void TestCheckReasonCode()
		{
			var entry = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS);

			parent.ShouldSend = true;
			parent.ReasonCode = "";
			parent.Validation.ValidateReasonCode();
			AssertHasMessageErrorContaining(parent.ReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);

			parent.ReasonCode = "99";
			parent.Validation.ValidateReasonCode();
			AssertHasMessageErrorContaining(parent.ReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			parent.ReasonCode = ExportAmendmentReasonCodeList.Codes._11;
			parent.Validation.ValidateReasonCode();
			AssertNoMessageErrors(parent.ReasonCodeInfo);

			parent.ReasonCode = ExportAmendmentReasonCodeList.Codes._12;
			parent.Validation.ValidateReasonCode();
			AssertNoMessageErrors(parent.ReasonCodeInfo);
		}

		public void TestCheckFaultParty()
		{
			var entry = Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS);
			parent.ShouldSend = true;
			parent.FaultParty = "";
			parent.Validation.ValidateFaultParty();
			AssertHasMessageErrorContaining(parent.FaultPartyInfo, MandatoryValidation.YouHaveNotEntered);

			parent.FaultParty = "I";
			parent.Validation.ValidateFaultParty();
			AssertHasMessageErrorContaining(parent.FaultPartyInfo, ListValidation.InvalidCodeMessageError);

			parent.FaultParty = ExportImputationReasonCodeList.Codes.A;
			parent.Validation.ValidateFaultParty();
			AssertNoMessageErrors(parent.FaultPartyInfo);

			parent.FaultParty = ExportImputationReasonCodeList.Codes.B;
			parent.Validation.ValidateFaultParty();
			AssertNoMessageErrors(parent.FaultPartyInfo);

			parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			parent.ShouldSend = true;
			parent.FaultParty = "";
			parent.Validation.ValidateFaultParty();
			AssertHasMessageErrorContaining(parent.FaultPartyInfo, MandatoryValidation.YouHaveNotEntered);

			parent.FaultParty = "XX";
			parent.Validation.ValidateFaultParty();
			AssertHasMessageErrorContaining(parent.FaultPartyInfo, ListValidation.InvalidCodeMessageError);

			parent.FaultParty = ImputationReasonCodeList.Codes._01;
			parent.Validation.ValidateFaultParty();
			AssertNoMessageErrors(parent.FaultPartyInfo);
		}

		[TestDate(2022, 11, 14)]
		public void TestDateOfFinalPrice()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._15;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			var parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5AS);
			parent.ShouldSend = true;
			AssertNoMessageErrors(parent.DateOfFinalPriceInfo);

			parent.ReasonCode = ExportAmendmentReasonCodeList.Codes._28;

			parent.Validation.ValidateDateOfFinalPrice();
			AssertHasMessageErrorContaining(parent.DateOfFinalPriceInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._11;
			parent.Validation.ValidateDateOfFinalPrice();
			AssertNoMessageErrors(parent.DateOfFinalPriceInfo);

			declaration.JE_ExportGoodsType = TransactionTypeCodeList.Codes._31;
			parent.Validation.ValidateDateOfFinalPrice();
			AssertHasMessageErrorContaining(parent.DateOfFinalPriceInfo, MandatoryValidation.YouHaveNotEntered);

			parent.DateOfFinalPrice = CargoWise.Types.ZDate.Today;
			AssertNoMessageErrors(parent.DateOfFinalPriceInfo);

			parent.DateOfFinalPrice = new ZDate(2022, 11, 15);
			AssertHasMessageErrorContaining(parent.DateOfFinalPriceInfo, parent.Validation.DateOfFinalPriceMessageErr);

			parent.DateOfFinalPrice = new ZDate(2022, 11, 13);
			AssertNoMessageErrors(parent.DateOfFinalPriceInfo);
		}

		public void TestShouldSendByLocalExport5DS()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5DQSnapshot();
			var parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DS);
			AssertEquals(0, parent.AmendedItems.Count);
			parent.ShouldSend = true;
			AssertHasErrorContaining(parent.ShouldSendInfo, "There are no changes to send.");

			entry.Declaration.JE_SubLocationOfGoods = "새 창고";
			parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DS);
			AssertEquals(1, parent.AmendedItems.Count);
			parent.ShouldSend = true;
			AssertNoErrorContaining(parent.ShouldSendInfo, "There are no changes to send.");

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationDeclined;
			parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DS);
			parent.ShouldSend = true;
			AssertNoErrors(parent.ShouldSendInfo);
		}

		public void TestShouldSendByLocalExport5DR()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas5DPSnapshot();
			var parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DR);
			AssertEquals(0, parent.AmendedItems.Count);
			parent.ShouldSend = true;
			AssertHasErrorContaining(parent.ShouldSendInfo, "There are no changes to send.");

			entry.Declaration.JE_SubLocationOfGoods = "새 창고";
			parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DR);
			AssertEquals(1, parent.AmendedItems.Count);
			parent.ShouldSend = true;
			AssertNoErrorContaining(parent.ShouldSendInfo, "There are no changes to send.");

			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationDeclined;
			parent = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5DR);
			parent.ShouldSend = true;
			AssertNoErrors(parent.ShouldSendInfo);
		}

		public void TestCheckShouldSendBy5SC105()
		{
			var entry5SC = SetTestData(Core.Constants.CountryCodes.Australia);
			AssertCheckOrginalFTAMessage(entry5SC, ElectronicDocumentTypeList.Codes._5SC, ElectronicDocumentTypeList.Codes._DHR, "You have selected a DHR message, but none of invoice lines have required country of origins.");

			SetAcceptedData(entry5SC, ElectronicDocumentTypeList.Codes._5SC);
			entry5SC.Declaration.JE_ExportDate = ZDateTime.Today;
			AssertCheckAmendmentFTAMessage(entry5SC, ElectronicDocumentTypeList.Codes._5SC, ElectronicDocumentTypeList.Codes._105, ElectronicDocumentTypeList.Codes._DHS);
		}
		public void TestCheckShouldSendByDHRDHS()
		{
			var entryDHR = SetTestData(Core.Constants.CountryCodes.China);
			AssertCheckOrginalFTAMessage(entryDHR, ElectronicDocumentTypeList.Codes._DHR, ElectronicDocumentTypeList.Codes._5SC, "You have selected a 5SC message, but there are some invoice lines which require a detailed FTA message(DHR).");

			SetAcceptedData(entryDHR, ElectronicDocumentTypeList.Codes._DHR);
			entryDHR.Declaration.JE_ExportDate = ZDateTime.Today;
			AssertCheckAmendmentFTAMessage(entryDHR, ElectronicDocumentTypeList.Codes._DHR, ElectronicDocumentTypeList.Codes._DHS, ElectronicDocumentTypeList.Codes._105);
		}

		void AssertCheckOrginalFTAMessage(CusEntryHeader entry, string correctType, string wrongType, string errorMessage)
		{
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var ftaMiscMessageSendingObject = new FTAMessageSendingObject(entry, correctType);
			ftaMiscMessageSendingObject.ShouldSend = true;
			AssertNoErrorContaining(ftaMiscMessageSendingObject.ShouldSendInfo, errorMessage);

			ftaMiscMessageSendingObject = new FTAMessageSendingObject(entry, wrongType);
			ftaMiscMessageSendingObject.ShouldSend = true;
			AssertHasErrorContaining(ftaMiscMessageSendingObject.ShouldSendInfo, errorMessage);
		}

		void SetAcceptedData(CusEntryHeader entry, ZString entryType)
		{
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = entryType;
			entryNum.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;

			Stream stream = null;
			if (entryType == ElectronicDocumentTypeList.Codes._DHR)
			{
				ImportDHRHeader header = new ImportDHRCreator().Create(entry);
				stream = KRXmlObjectSerializer.Serialize(header);
			}
			else
			{
				ImportFTAHeader header = new ImportFTACreator().Create(entry);
				stream = KRXmlObjectSerializer.Serialize(header);
			}
			AccumulativeAmendmentManager.CreateNewSnapshot(entry, entryType, stream);
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, entryType);
		}

		void AssertCheckAmendmentFTAMessage(CusEntryHeader entry, string originalType, string correctType, string wrongType)
		{
			var fTAamendmentMessageSendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, correctType);
			fTAamendmentMessageSendingObject.ShouldSend = true;
			AssertNoErrorContaining(fTAamendmentMessageSendingObject.ShouldSendInfo, "You cannot send this message now. Its status indicates Customs has never accepted an original message of this type.");

			fTAamendmentMessageSendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, wrongType);
			fTAamendmentMessageSendingObject.ShouldSend = true;
			AssertHasErrorContaining(fTAamendmentMessageSendingObject.ShouldSendInfo, "You cannot send this message now. Its status indicates Customs has never accepted an original message of this type.");
		}

		CusEntryHeader SetTestData(string country)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.DetailedFTACountries, "Detailed FTA Countries (DHR)");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "CN", "중국", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "ID", "인도네시아", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "IN", "인도", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.DetailedFTACountries, "VN", "베트남", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CountryOfOrigin = country;
			return entry;
		}

		public void TestSendNew5FEMessageWhenPrevious5FEIncludedPenaltyExemptionRequest()
		{
			SetupDataFor5UA(out var entry);

			var message5FE = entry.Messages.AddNew();
			message5FE.EM_MessageOwner = "1";
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;

			var message5FK = entry.Messages.AddNew();
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ANT;
			message5FK.EM_ApplicationReference = message5FE.EM_MessageNum;
			message5FK.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5FE;
			message5FK.EM_MessageType = ElectronicDocumentTypeList.Codes._5FK;

			var charge_DTY = entry.Charges.FirstOrDefault(x => x.C1_ChargeType == "DTY");
			charge_DTY.C1_ChargeAmount = 3000000000000m;

			var messageSendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			messageSendingObject.PenaltyExemptionIndicator = Constants.YesNo.No;

			messageSendingObject.ShouldSend = true;
			AssertNoError(messageSendingObject.ShouldSendInfo, "You have indicated to include 5UA in 5FE. The previous 5UA request included in the last 5FE has not been reviewed by Customs. Please wait until its 5UB arrives.");
			messageSendingObject.ShouldSend = false;
			AssertNoError(messageSendingObject.ShouldSendInfo, "You have indicated to include 5UA in 5FE. The previous 5UA request included in the last 5FE has not been reviewed by Customs. Please wait until its 5UB arrives.");

			messageSendingObject.PenaltyExemptionIndicator = Constants.YesNo.Yes;
			messageSendingObject.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A3;

			messageSendingObject.ShouldSend = true;
			AssertHasError(messageSendingObject.ShouldSendInfo, "You have indicated to include 5UA in 5FE. The previous 5UA request included in the last 5FE has not been reviewed by Customs. Please wait until its 5UB arrives.");
			messageSendingObject.ShouldSend = false;
			AssertNoError(messageSendingObject.ShouldSendInfo, "You have indicated to include 5UA in 5FE. The previous 5UA request included in the last 5FE has not been reviewed by Customs. Please wait until its 5UB arrives.");

			var message5UB = entry.Messages.AddNew();
			message5UB.EM_ApplicationReference = message5FE.EM_MessageNum;
			message5UB.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5FE;
			message5UB.EM_MessageType = ElectronicDocumentTypeList.Codes._5UB;

			messageSendingObject.ShouldSend = true;
			AssertNoError(messageSendingObject.ShouldSendInfo, "You have indicated to include 5UA in 5FE. The previous 5UA request included in the last 5FE has not been reviewed by Customs. Please wait until its 5UB arrives.");
		}

		public void TestSendNew5FEMessageWhenStandAlone5UAIsSent()
		{
			SetupDataFor5UA(out var entry);

			var message5FE = entry.Messages.AddNew();
			message5FE.EM_MessageNum = "1";
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;

			var message5FK = entry.Messages.AddNew();
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			entry.CH_EntryStatus = CustomsEntryStatusTypeList.Codes.ANT;
			message5FK.EM_ApplicationReference = message5FE.EM_MessageNum;
			message5FK.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5FE;
			message5FK.EM_MessageType = ElectronicDocumentTypeList.Codes._5FK;

			var charge_DTY = entry.Charges.FirstOrDefault(x => x.C1_ChargeType == "DTY");
			charge_DTY.C1_ChargeAmount = 3000000000000m;

			var message5UA = entry.Messages.AddNew();
			message5UA.EM_MessageNum = "2";
			message5UA.EM_MessageOwner = message5FE.EM_MessageNum;
			message5UA.EM_ApplicationReference = "1";
			message5UA.EM_MessageType = ElectronicDocumentTypeList.Codes._5UA;
			var entryNum5UA = entry.EntryNumbers.AddNew();
			entryNum5UA.CE_EntryLineReference = "1";
			entryNum5UA.CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalSent;

			var messageSendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			messageSendingObject.PenaltyExemptionIndicator = Constants.YesNo.No;

			messageSendingObject.ShouldSend = true;
			AssertNoError(messageSendingObject.ShouldSendInfo, "You have indicated to include 5UA in 5FE. The previous 5UA request has not been reviewed by Customs. Please wait until its 5UB arrives.");
			messageSendingObject.ShouldSend = false;
			AssertNoError(messageSendingObject.ShouldSendInfo, "You have indicated to include 5UA in 5FE. The previous 5UA request has not been reviewed by Customs. Please wait until its 5UB arrives.");

			messageSendingObject.PenaltyExemptionIndicator = Constants.YesNo.Yes;
			messageSendingObject.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A3;

			messageSendingObject.ShouldSend = true;
			AssertHasError(messageSendingObject.ShouldSendInfo, "You have indicated to include 5UA in 5FE. The previous 5UA request has not been reviewed by Customs. Please wait until its 5UB arrives.");
			messageSendingObject.ShouldSend = false;
			AssertNoError(messageSendingObject.ShouldSendInfo, "You have indicated to include 5UA in 5FE. The previous 5UA request has not been reviewed by Customs. Please wait until its 5UB arrives.");

			var message5UB = entry.Messages.AddNew();
			message5UB.EM_ApplicationReference = message5UA.EM_MessageNum;
			message5UB.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5UA;
			message5UB.EM_MessageType = ElectronicDocumentTypeList.Codes._5UB;

			messageSendingObject.ShouldSend = true;
			AssertNoError(messageSendingObject.ShouldSendInfo, "You have indicated to include 5UA in 5FE. The previous 5UA request has not been reviewed by Customs. Please wait until its 5UB arrives.");
			messageSendingObject.ShouldSend = false;
			AssertNoError(messageSendingObject.ShouldSendInfo, "You have indicated to include 5UA in 5FE. The previous 5UA request has not been reviewed by Customs. Please wait until its 5UB arrives.");
		}

		void SetupDataFor5UA(out CusEntryHeader entry)
		{
			entry = new TestDataSetupHelper(Factory).GetEntry929FullData(PaymentMethodCodeList.Codes._00, ZBool.True);
			var statement929 = Factory.New<CusStatementHeader>();
			statement929.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
			statement929.B2_PaymentAuthorizationDate = new ZDateTime(2025, 01, 14);
			statement929.B2_GC = entry.Declaration.JE_GC;

			var statementLine = statement929.StatementLines.AddNew();
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			statementLine.B3_EntryNum = entry.EntryNumber;
			using var stream = KRXmlObjectSerializer.Serialize(new ImportEntryHeaderCreator().Create(entry));
			AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
			Factory.Save();
		}

		public void TestCheckTaxPenaltyCause()
		{
			var messageSendingObject = SetAmendmentMessageSendingObject();
			AssertNoMessageErrors(messageSendingObject.TaxPenaltyCauseInfo);

			messageSendingObject.TaxPenaltyCause = "XX";
			AssertHasMessageErrorContaining(messageSendingObject.TaxPenaltyCauseInfo, ListValidation.InvalidCodeMessageError);

			messageSendingObject.TaxPenaltyCause = TaxPenaltyTypeCodeList.Codes._01;
			AssertNoMessageErrors(messageSendingObject.TaxPenaltyCauseInfo);
		}

		public void TestCheckDutyPenaltyCause()
		{
			var messageSendingObject = SetAmendmentMessageSendingObject();
			AssertNoMessageErrors(messageSendingObject.DutyPenaltyCauseInfo);

			messageSendingObject.DutyPenaltyCause = "XX";
			AssertHasMessageErrorContaining(messageSendingObject.DutyPenaltyCauseInfo, ListValidation.InvalidCodeMessageError);

			messageSendingObject.DutyPenaltyCause = TaxPenaltyTypeCodeList.Codes._01;
			AssertNoMessageErrors(messageSendingObject.DutyPenaltyCauseInfo);
		}

		public void TestCheckApplyDutyPenaltyReduction()
		{
			var messageSendingObject = SetAmendmentMessageSendingObject_HasPenaltyExemptionSessionalData();
			AssertNoMessageErrors(messageSendingObject.ApplyDutyPenaltyReductionInfo);

			messageSendingObject.ApplyDutyPenaltyReduction = "X";
			AssertHasMessageErrorContaining(messageSendingObject.ApplyDutyPenaltyReductionInfo, ListValidation.InvalidCodeMessageError);

			messageSendingObject.ApplyDutyPenaltyReduction = DutyPenaltyReducedYNCodeList.Codes.Y;
			AssertNoMessageErrors(messageSendingObject.ApplyDutyPenaltyReductionInfo);
		}

		public void TestCheckPenaltyExemptionIndicator()
		{
			var messageSendingObject = SetAmendmentMessageSendingObject_HasPenaltyExemptionSessionalData();
			messageSendingObject.Validation.ValidatePenaltyExemptionIndicator();
			AssertHasMessageErrorContaining(messageSendingObject.PenaltyExemptionIndicatorInfo, MandatoryValidation.YouHaveNotEntered);

			messageSendingObject.PenaltyExemptionIndicator = "A";
			AssertHasMessageErrorContaining(messageSendingObject.PenaltyExemptionIndicatorInfo, ListValidation.InvalidCodeMessageError);

			messageSendingObject.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.Y;
			AssertNoMessageErrors(messageSendingObject.PenaltyExemptionIndicatorInfo);

			messageSendingObject.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.N;
			AssertNoMessageErrors(messageSendingObject.PenaltyExemptionIndicatorInfo);

			messageSendingObject.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.X;
			AssertHasMessageErrorContaining(messageSendingObject.PenaltyExemptionIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckPenaltyExemptionReasonCode()
		{
			var messageSendingObject = SetAmendmentMessageSendingObject_HasPenaltyExemptionSessionalData();
			messageSendingObject.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.Y;
			messageSendingObject.Validation.ValidatePenaltyExemptionReasonCode();
			AssertHasMessageErrorContaining(messageSendingObject.PenaltyExemptionReasonCodeInfo, MandatoryValidation.YouHaveNotEntered);

			messageSendingObject.PenaltyExemptionReasonCode = "XX";
			AssertHasMessageErrorContaining(messageSendingObject.PenaltyExemptionReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			messageSendingObject.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A1;
			AssertNoMessageErrors(messageSendingObject.PenaltyExemptionReasonCodeInfo);

			messageSendingObject.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.X;
			messageSendingObject.PenaltyExemptionReasonCode = ZString.Empty;
			AssertNoMessageErrors(messageSendingObject.PenaltyExemptionReasonCodeInfo);

			messageSendingObject.PenaltyExemptionReasonCode = "XX";
			AssertHasMessageErrorContaining(messageSendingObject.PenaltyExemptionReasonCodeInfo, ListValidation.InvalidCodeMessageError);

			messageSendingObject.PenaltyExemptionIndicator = DutyPenaltyExemptionCodeList.Codes.N;
			messageSendingObject.PenaltyExemptionReasonCode = ZString.Empty;
			AssertNoMessageErrors(messageSendingObject.PenaltyExemptionReasonCodeInfo);

			messageSendingObject.PenaltyExemptionReasonCode = "XX";
			AssertHasMessageErrorContaining(messageSendingObject.PenaltyExemptionReasonCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckPenaltyExemptionReason()
		{
			var messageSendingObject = SetAmendmentMessageSendingObject_HasPenaltyExemptionSessionalData();
			AssertEquals(false, messageSendingObject.IsPenaltyExemptionRequested);
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(messageSendingObject.PenaltyExemptionReasonCode));
			messageSendingObject.Validation.ValidatePenaltyExemptionReason();
			AssertNoMessageErrors(messageSendingObject.PenaltyExemptionReasonInfo);

			messageSendingObject.PenaltyExemptionIndicator = YesNo.Yes;
			messageSendingObject.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A3;
			AssertEquals(true, messageSendingObject.IsPenaltyExemptionRequested);
			Assert(PenaltyExemptionReasonCodeList.IsLegalReasonCode(messageSendingObject.PenaltyExemptionReasonCode));
			messageSendingObject.Validation.ValidatePenaltyExemptionReason();
			AssertHasMessageErrorContaining(messageSendingObject.PenaltyExemptionReasonInfo, MandatoryValidation.YouHaveNotEntered);

			messageSendingObject.PenaltyExemptionReason = "Test Value";
			AssertNoMessageErrors(messageSendingObject.PenaltyExemptionReasonInfo);

			messageSendingObject.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A1;
			Assert(!PenaltyExemptionReasonCodeList.IsLegalReasonCode(messageSendingObject.PenaltyExemptionReasonCode));
			messageSendingObject.Validation.ValidatePenaltyExemptionReason();
			AssertHasMessageErrorContaining(messageSendingObject.PenaltyExemptionReasonInfo, MandatoryValidation.DoNotEntered);
		}

		public void TestRefundRequestSubmissionYN()
		{
			var messageSendingObject = SetAmendmentMessageSendingObject_HasPenaltyExemptionSessionalData();
			AssertEquals("AX", messageSendingObject.AmendmentType);
			messageSendingObject.Validation.ValidateRefundRequestSubmissionYN();
			AssertNoMessageErrors(messageSendingObject.RefundRequestSubmissionYNInfo);

			messageSendingObject.RefundRequestSubmissionYN = "X";
			AssertNoMessageErrors("Not Check", messageSendingObject.RefundRequestSubmissionYNInfo);

			var entry = messageSendingObject.Header;
			var vat = entry.Charges.FirstOrDefault();
			vat.C1_ChargeAmount -= (vat.C1_ChargeAmount + 1);
			messageSendingObject = new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals("CX", messageSendingObject.AmendmentType);

			messageSendingObject.Validation.ValidateRefundRequestSubmissionYN();
			AssertHasMessageErrorContaining(messageSendingObject.RefundRequestSubmissionYNInfo, MandatoryValidation.YouHaveNotEntered);

			messageSendingObject.RefundRequestSubmissionYN = "X";
			AssertHasMessageErrorContaining(messageSendingObject.RefundRequestSubmissionYNInfo, ListValidation.InvalidCodeMessageError);

			messageSendingObject.RefundRequestSubmissionYN = "Y";
			AssertNoMessageErrors(messageSendingObject.RefundRequestSubmissionYNInfo);

			messageSendingObject.RefundRequestSubmissionYN = "N";
			AssertNoMessageErrors(messageSendingObject.RefundRequestSubmissionYNInfo);
		}

		JobDeclarationAmendmentMessageSendingObject SetAmendmentMessageSendingObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			return new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
		}

		JobDeclarationAmendmentMessageSendingObject SetAmendmentMessageSendingObject_HasPenaltyExemptionSessionalData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.EntryNumber = "1234520000045M";
			var vat = entry.Charges.AddNew();
			vat.C1_ChargeType = "VAT";
			vat.C1_ChargeAmount = 10;

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementType = StatementHeaderTypeList.Codes.Invoice;
			statement.B2_ProcessDate = ZDateTime.Today;
			statement.B2_PaymentAuthorizationDate = ZDateTime.Today.AddDays(-5);
			statement.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;
			var statementLine = statement.StatementLines.AddNew();
			statementLine.B3_EntryNum = "1234520000045M";
			statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
			Factory.Save();

			vat.C1_ChargeAmount = 11;

			return new JobDeclarationAmendmentMessageSendingObject(entry, ElectronicDocumentTypeList.Codes._5FE);
		}
	}
}
