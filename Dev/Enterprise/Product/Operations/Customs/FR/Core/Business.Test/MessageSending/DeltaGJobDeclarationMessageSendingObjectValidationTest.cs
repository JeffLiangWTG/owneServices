using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;
using CusEntryInstruction = Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.FR.Business.MessageSending.Testing
{
	public class JobDeclarationMessageSendingObjectValidationTest : MessageSendingObjectValidationTest
	{
		public void TestTriggeringPointForValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var configuration = new TriggerPointsConfiguration();
			configuration.EnableAutomatedValidation = true;
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, configuration))
			{
				var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				entryHeader.CH_TriggeringPointForValidation = ZString.Empty;
				sendingObject.MessageType = EntryActionCodeList.Codes.ANT;
				AssertHasErrorContaining("MessageType = ANT + TriggeringPointForValidation = empty should trigger an error.", sendingObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.VaaTriggeringPointMissing);

				sendingObject.MessageType = EntryActionCodeList.Codes.VAL;
				AssertNoErrorContaining(sendingObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.VaaTriggeringPointMissing);

				entryHeader.CH_TriggeringPointForValidation = TriggerPointsCodeList.Codes.NUL;
				sendingObject.MessageType = EntryActionCodeList.Codes.ANT;
				AssertNoMessageError(sendingObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.Doc355Or337Missing);

				entryHeader.CH_TriggeringPointForValidation = TriggerPointsCodeList.Codes.PAB;
				sendingObject.MessageType = EntryActionCodeList.Codes.ANT;
				AssertHasMessageError("MessageType = ANT + TriggeringPointForValidation = PAB +  no previous doc 355/337 found should trigger an error.", sendingObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.Doc355Or337Missing);

				var documentOnDeclaration = declaration.PreviousDocuments.AddNew();
				documentOnDeclaration.CSI_Code = PreviousDocumentCodeList.Codes._355;
				sendingObject.Validation.ValidateMessageType();
				AssertNoMessageError("If we have a 355/337 previous document on declaration, we can set triggering point to PAB.", sendingObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.Doc355Or337Missing);
				declaration.PreviousDocuments.RemoveAndDelete(documentOnDeclaration);

				var documentOnInvoiceHeader = invoiceHeader.PreviousDocuments.AddNew();
				documentOnInvoiceHeader.CSI_Code = PreviousDocumentCodeList.Codes._355;
				sendingObject.Validation.ValidateMessageType();
				AssertNoMessageError("If we have a 355/337 previous document on invoice header, we can set triggering point to PAB.", sendingObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.Doc355Or337Missing);
				invoiceHeader.PreviousDocuments.RemoveAndDelete(documentOnInvoiceHeader);

				var documentOnInvoiceLine = invoiceLine.PreviousDocuments.AddNew();
				documentOnInvoiceLine.CSI_Code = PreviousDocumentCodeList.Codes._355;
				sendingObject.Validation.ValidateMessageType();
				AssertNoMessageError("If we have a 355/337 previous document on invoice line, we can set triggering point to PAB.", sendingObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.Doc355Or337Missing);

				documentOnInvoiceLine.CSI_Code = PreviousDocumentCodeList.Codes._337;
				sendingObject.Validation.ValidateMessageType();
				AssertNoMessageError("If we have a 355/337 previous document on invoice line, we can set triggering point to PAB.", sendingObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.Doc355Or337Missing);
			}

			configuration.EnableAutomatedValidation = false;
			using (FRCustomsDataRegistry.Instance.TriggerPointsConfiguration.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, configuration))
			{
				var sendingObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);
				entryHeader.CH_TriggeringPointForValidation = TriggerPointsCodeList.Codes.VAQ;
				sendingObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);
				sendingObject.MessageType = EntryActionCodeList.Codes.ANT;
				AssertHasWarning("TriggeringPointForValidation = VAQ and EnableAutomatedValidation = false should trigger an error.", sendingObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.AutomatedValidationTurnedOff);

				entryHeader.CH_TriggeringPointForValidation = TriggerPointsCodeList.Codes.PAB;
				sendingObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);
				sendingObject.MessageType = EntryActionCodeList.Codes.ANT;
				AssertHasWarning("TriggeringPointForValidation = PAB and EnableAutomatedValidation = false should trigger an error.", sendingObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.AutomatedValidationTurnedOff);

				entryHeader.CH_TriggeringPointForValidation = TriggerPointsCodeList.Codes.NUL;
				sendingObject = new DeltaGJobDeclarationMessageSendingObject(entryHeader);
				sendingObject.MessageType = EntryActionCodeList.Codes.ANT;
				AssertNoWarning("TriggeringPointForValidation = NUL and EnableAutomatedValidation = false should not trigger an error.", sendingObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.AutomatedValidationTurnedOff);
			}
		}

		public void TestCheckChangeAcknowledgementIndicator()
		{
			var testItem = GenerateDeclarationSendingObject(OrgCusAccountDeltaGTypeList.Codes.G1, EntrySubstyleCodePairList.Codes.D);
			testItem.MessageType = EntryActionCodeList.Codes.REC;
			AssertEquals(Customs.FR.Business.ReasonCodeList.Codes.C173, testItem.ChangeAcknowledgementIndicator);
			testItem.Validation.ValidateMessageType();
			AssertNoErrorContaining(testItem.ChangeAcknowledgementIndicatorInfo, MandatoryValidation.MustBeEntered);

			testItem.ChangeAcknowledgementIndicator = "";
			testItem.Validation.ValidateMessageType();
			AssertHasErrorContaining(testItem.ChangeAcknowledgementIndicatorInfo, MandatoryValidation.MustBeEntered);

			testItem.MessageType = EntryActionCodeList.Codes.INV;
			AssertEquals(Customs.FR.Business.ReasonCodeList.Codes.C174, testItem.ChangeAcknowledgementIndicator);
			testItem.Validation.ValidateMessageType();
			AssertNoErrorContaining(testItem.ChangeAcknowledgementIndicatorInfo, MandatoryValidation.MustBeEntered);

			testItem.ChangeAcknowledgementIndicator = "";
			testItem.Validation.ValidateMessageType();
			AssertHasErrorContaining(testItem.ChangeAcknowledgementIndicatorInfo, MandatoryValidation.MustBeEntered);

			testItem.ChangeAcknowledgementIndicator = ZString.Empty;
			testItem.MessageType = EntryActionCodeList.Codes.VAL;
			testItem.Validation.ValidateMessageType();
			AssertNoErrorContaining(testItem.ChangeAcknowledgementIndicatorInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestEntrySubstyleIsEmpty()
		{
			var testItem = GenerateDeclarationSendingObject(OrgCusAccountDeltaGTypeList.Codes.G1, "");
			testItem.MessageType = EntryActionCodeList.Codes.VAL;
			testItem.Validation.ValidateMessageType();
			AssertNoErrors(testItem.MessageTypeInfo);

			testItem.Header.EntryInstruction.CEI_SubStyle = EntrySubstyleCodePairList.Codes.D;
			testItem.Validation.ValidateMessageType();
			AssertNoErrors(testItem.MessageTypeInfo);
		}

		public void TestDoNotRecalculateEntrySubstyleIsPrelodged()
		{
			var testItem = GenerateDeclarationSendingObject(OrgCusAccountDeltaGTypeList.Codes.G1, EntrySubstyleCodePairList.Codes.D);
			var messageError = "The entry Sub Style will be recalculated to [A] and this value will be both saved on the job and sent in the message. To disable this and send [D] verbatim, tick 'Keep Entered Sub Style'";

			testItem.DoNotRecalculateEntrySubstyle = true;
			testItem.MessageType = EntryActionCodeList.Codes.VAL;
			AssertNoWarning(testItem.MessageTypeInfo, messageError);

			testItem.MessageType = EntryActionCodeList.Codes.ANT;
			AssertNoWarning(testItem.MessageTypeInfo, messageError);

			testItem.DoNotRecalculateEntrySubstyle = false;
			testItem.MessageType = EntryActionCodeList.Codes.ANT;
			AssertNoWarning(testItem.MessageTypeInfo, messageError);

			testItem.MessageType = EntryActionCodeList.Codes.VAL;
			AssertHasWarning(testItem.MessageTypeInfo, messageError);
		}

		public void TestDoNotRecalculateEntrySubstyleIsLodged()
		{
			var testItem = GenerateDeclarationSendingObject(OrgCusAccountDeltaGTypeList.Codes.G1, EntrySubstyleCodePairList.Codes.A);
			var messageError = "The entry Sub Style will be recalculated to [D] and this value will be both saved on the job and sent in the message. To disable this and send [A] verbatim, tick 'Keep Entered Sub Style'";

			testItem.DoNotRecalculateEntrySubstyle = true;

			testItem.MessageType = EntryActionCodeList.Codes.VAL;
			AssertNoWarning(testItem.MessageTypeInfo, messageError);

			testItem.MessageType = EntryActionCodeList.Codes.ANT;
			AssertNoWarning(testItem.MessageTypeInfo, messageError);

			testItem.DoNotRecalculateEntrySubstyle = false;

			testItem.MessageType = EntryActionCodeList.Codes.VAL;
			AssertNoWarning(testItem.MessageTypeInfo, messageError);

			testItem.MessageType = EntryActionCodeList.Codes.ANT;
			AssertHasWarning(testItem.MessageTypeInfo, messageError);

			testItem = GenerateDeclarationSendingObject(OrgCusAccountDeltaGTypeList.Codes.G1, EntrySubstyleCodePairList.Codes.Y);
			testItem.MessageType = EntryActionCodeList.Codes.VAL;
			AssertNoWarnings(testItem.MessageTypeInfo);

			testItem.MessageType = EntryActionCodeList.Codes.ANT;
			AssertNoWarnings(testItem.MessageTypeInfo);

			testItem.DoNotRecalculateEntrySubstyle = false;

			testItem.MessageType = EntryActionCodeList.Codes.VAL;
			AssertNoWarnings(testItem.MessageTypeInfo);

			testItem.MessageType = EntryActionCodeList.Codes.VAL;
			AssertNoWarnings(testItem.MessageTypeInfo);
		}

		public void TestErrorWhenSendingWrongSubStyleAfterBAE_G1()
		{
			var g1SendingObj = GenerateDeclarationSendingObject(OrgCusAccountDeltaGTypeList.Codes.G1, EntrySubstyleCodePairList.Codes.D);
			var expectedError = "The sub type [D] is not valid for message REC. Use [A] instead please.";

			g1SendingObj.DoNotRecalculateEntrySubstyle = false;

			g1SendingObj.Header.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES010;
			g1SendingObj.MessageType = EntryActionCodeList.Codes.ANT;
			AssertNoError(g1SendingObj.MessageTypeInfo, expectedError);

			g1SendingObj.Header.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			g1SendingObj.MessageType = EntryActionCodeList.Codes.REC;
			AssertHasError("It's not allowed to send D after BAE.", g1SendingObj.MessageTypeInfo, expectedError);

			g1SendingObj.DoNotRecalculateEntrySubstyle = true;

			g1SendingObj.Header.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES010;
			g1SendingObj.MessageType = EntryActionCodeList.Codes.VAL;
			AssertNoError(g1SendingObj.MessageTypeInfo, expectedError);

			g1SendingObj.Header.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			g1SendingObj.MessageType = EntryActionCodeList.Codes.REC;
			AssertHasError("It's not allowed to send D after BAE even if user tick 'Keep sub style'.", g1SendingObj.MessageTypeInfo, expectedError);
		}

		public void TestErrorWhenSendingWrongSubStyleAfterBAE_G2()
		{
			var g2SendingObj = GenerateDeclarationSendingObject(OrgCusAccountDeltaGTypeList.Codes.G2, EntrySubstyleCodePairList.Codes.F);
			var expectedError = "The sub type [F] is not valid for message REC. Use [C] instead please.";

			g2SendingObj.DoNotRecalculateEntrySubstyle = false;

			g2SendingObj.Header.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES010;
			g2SendingObj.MessageType = EntryActionCodeList.Codes.ANT;
			AssertNoError(g2SendingObj.MessageTypeInfo, expectedError);

			g2SendingObj.Header.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			g2SendingObj.MessageType = EntryActionCodeList.Codes.REC;
			AssertHasError("It's not allowed to send F after BAE.", g2SendingObj.MessageTypeInfo, expectedError);

			g2SendingObj.DoNotRecalculateEntrySubstyle = true;

			g2SendingObj.Header.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES010;
			g2SendingObj.MessageType = EntryActionCodeList.Codes.VAL;
			AssertNoError(g2SendingObj.MessageTypeInfo, expectedError);

			g2SendingObj.Header.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES100;
			g2SendingObj.MessageType = EntryActionCodeList.Codes.REC;
			AssertHasError("It's not allowed to send F after BAE even if user tick 'Keep sub style'.", g2SendingObj.MessageTypeInfo, expectedError);
		}

		public void TestCheckMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var messageObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			AssertNoErrors(messageObject.MessageTypeInfo);
			messageObject.MessageType = "XXX";
			AssertHasErrors(messageObject.MessageTypeInfo);
			messageObject.MessageType = ZString.Empty;
			AssertHasErrors(messageObject.MessageTypeInfo);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			AssertNoMessageErrorContaining(messageObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.NoMessageShouldbeSentWhenCancelled);
			AssertNoErrors(messageObject.MessageTypeInfo);

			entry.CH_EntryStatus = EntryStatusDescriptionCodeList.Codes.ES090;
			Assert("PreReq - is cancelled", entry.IsCancelledWithCustoms);
			messageObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			messageObject.ShouldSend = true;
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			AssertHasMessageErrorContaining(messageObject.MessageTypeInfo, DeltaGJobDeclarationMessageSendingObjectValidation.NoMessageShouldbeSentWhenCancelled);
		}

		public void TestCheckAvailableBalanceOnA12Account()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			SetUpAi2Permit(dec, ZString.Empty, 601m, 600m);
			var entry1 = SetUpAi2Amount(dec);
			var entry2 = SetUpAi2Amount(dec);
			var messageErrorTheAvailableBalanceLessThanAi2Amount = "The available balance on the AI2 account is ";
			var messageObject = new DeltaGJobDeclarationMessageSendingObject(entry1);
			var info = messageObject.MessageTypeInfo;
			messageObject.ShouldSend = true;
			messageObject.MessageType = EntryActionCodeList.Codes.VAL;
			AssertHasMessageErrorContaining(info, messageErrorTheAvailableBalanceLessThanAi2Amount);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			AssertNoMessageErrorContaining("message type is non-VAL", info, messageErrorTheAvailableBalanceLessThanAi2Amount);
			messageObject.ShouldSend = false;
			messageObject.MessageType = EntryActionCodeList.Codes.VAL;
			AssertNoMessageErrorContaining(info, messageErrorTheAvailableBalanceLessThanAi2Amount);

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			SetUpAi2Permit(dec2, ZString.Empty, 1200, 600m);
			var entry3 = SetUpAi2Amount(dec);
			var entry4 = SetUpAi2Amount(dec);
			var messageObject2 = new DeltaGJobDeclarationMessageSendingObject(entry1);
			var info2 = messageObject2.MessageTypeInfo;
			messageObject2.ShouldSend = true;
			messageObject2.MessageType = EntryActionCodeList.Codes.VAL;
			AssertNoMessageErrorContaining(info, messageErrorTheAvailableBalanceLessThanAi2Amount);
		}

		CusEntryHeader SetUpAi2Amount(JobDeclaration declaration)
		{
			var toDay = ZDate.Today;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("ZZ1", 10.0m, "FR", 0m, 0m, GuaranteeTypeList.Codes.AI2, toDay.AddDays(-1), toDay.AddDays(1), "Test AI2");
			helper.CreateTaxOrFee("ZZ2", 20.0m, "FR", 0m, 0m, Constants.Customs.CusEntryFeeTypes.VAT, toDay.AddDays(-1), toDay.AddDays(1), "Test VAT");
			helper.CreateTaxOrFee("ZZ3", 40.0m, "FR", 0m, 0m, Constants.Customs.CusEntryFeeTypes.DutyAmount, toDay.AddDays(-1), toDay.AddDays(1), "Test DTY");
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_ValuationDateOverride = toDay;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.AllEntryLines.AddNew();

			invoiceLine.JI_CL = cusEntryLine.PK;

			var fee1 = cusEntryLine.Fees.AddNew();
			fee1.NationalFeeTypeCode = "ZZ1";
			fee1.CF_ChargeAmount = 100m;

			var fee2 = cusEntryLine.Fees.AddNew();
			fee2.NationalFeeTypeCode = "ZZ2";
			fee2.CF_ChargeAmount = 200m;

			var fee3 = cusEntryLine.Fees.AddNew();
			fee3.NationalFeeTypeCode = "ZZ3";
			fee3.CF_ChargeAmount = 400m;

			var fee4 = cusEntryLine.Fees.AddNew();
			fee4.NationalFeeTypeCode = "";
			fee4.CF_ChargeAmount = 1m;

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;

			AssertEquals(300m, cusEntryHeader.Ai2Amount);
			return cusEntryHeader;
		}

		void SetUpAi2Permit(JobDeclaration declaration, ZString currency, ZDecimal openingBalance, ZDecimal pendingBalance)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.FillWithValidTestData();
			orgAddress.OA_Address1 = "Address1";
			orgAddress.OA_Address2 = "Address2";

			var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader1.CPH_Type = GuaranteeTypeList.Codes.AI2;
			guaranteeHeader1.CPH_Number = "0000001";
			if (!currency.IsEmpty)
			{
				guaranteeHeader1.CPH_UnitOfMeasure = currency;
			}
			guaranteeHeader1.CPH_OH_PermitHolder = orgHeader.PK;
			var openingBalanceTransaction = guaranteeHeader1.CusGuaranteeLineTransactions.AddNew();
			openingBalanceTransaction.CPL_Reference = "Ref";
			openingBalanceTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			openingBalanceTransaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Confirmed;
			openingBalanceTransaction.CPL_TranValue = openingBalance;

			guaranteeHeader1.CPH_Balance = openingBalance - pendingBalance;
			Factory.Save();

			declaration.ZG_VATDeferType = VATProcedureList.Codes._2;
			declaration.ZG_VATDeferNumber = "0000001";
			declaration.JE_OH_Importer = orgHeader.PK;
			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			var ai2Permit = declaration.Ai2Permit;
			AssertNotNull(declaration.Ai2Permit);
			AssertEquals(guaranteeHeader1.PK, ai2Permit.PK);
		}

		public void TestCheckAmendmentReason()
		{
			const string errorMessage = "Amendment reason must be entered when amending or canceling a declaration";
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;

			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var messageObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			CombineAssertions(() =>
			{
				messageObject.MessageType = EntryActionCodeList.Codes.ANT;
				messageObject.Validation.ValidateVOCReason();
				AssertNoErrorContaining("Creation", messageObject.VOCReasonInfo, errorMessage);

				messageObject.MessageType = EntryActionCodeList.Codes.REC;
				messageObject.Validation.ValidateVOCReason();
				AssertHasErrorContaining("Amendment", messageObject.VOCReasonInfo, errorMessage);

				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
				messageObject.Validation.ValidateVOCReason();
				AssertHasErrorContaining("Not Delta D", messageObject.VOCReasonInfo, errorMessage);

				declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G2;
				messageObject.MessageType = EntryActionCodeList.Codes.INV;
				messageObject.Validation.ValidateVOCReason();
				AssertHasErrorContaining("Cancellation", messageObject.VOCReasonInfo, errorMessage);

				messageObject.VOCReason = "GOOD REASON";
				AssertNoErrorContaining("Reason Entered", messageObject.VOCReasonInfo, errorMessage);
			});
		}

		public void TestCheckReplacementDeclarationType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var messageObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			messageObject.MessageType = EntryActionCodeList.Codes.REC;
			AssertExceptionThrown(typeof(MaxLengthExceededException), () => { messageObject.ReplacementDeclarationType = "TestTestTestTestTestTestTestTestTest"; });
			ErrorReporter.Clear();

			messageObject.ShouldSend = true;
			messageObject.ReplacementDeclarationType = ZString.Empty;
			messageObject.Validation.ValidateReplacementDeclarationType();
			AssertHasErrorContaining(messageObject.ReplacementDeclarationTypeInfo, MandatoryValidation.MustBeEntered);
			AssertNoMessageErrorContaining(messageObject.ReplacementDeclarationTypeInfo, ListValidation.InvalidCodeMessageError);

			messageObject.MessageType = EntryActionCodeList.Codes.INV;
			messageObject.Validation.ValidateReplacementDeclarationType();
			AssertHasErrorContaining(messageObject.ReplacementDeclarationTypeInfo, MandatoryValidation.MustBeEntered);
			AssertNoMessageErrorContaining(messageObject.ReplacementDeclarationTypeInfo, ListValidation.InvalidCodeMessageError);

			messageObject.MessageType = EntryActionCodeList.Codes.VAL;
			messageObject.Validation.ValidateReplacementDeclarationType();
			AssertNoErrorContaining(messageObject.ReplacementDeclarationTypeInfo, MandatoryValidation.MustBeEntered);
			AssertNoMessageErrorContaining(messageObject.ReplacementDeclarationTypeInfo, ListValidation.InvalidCodeMessageError);

			messageObject.MessageType = EntryActionCodeList.Codes.INV;
			messageObject.ReplacementDeclarationType = "IST";
			messageObject.Validation.ValidateReplacementDeclarationType();
			AssertNoErrorContaining(messageObject.ReplacementDeclarationTypeInfo, MandatoryValidation.MustBeEntered);
			AssertNoMessageErrorContaining(messageObject.ReplacementDeclarationTypeInfo, ListValidation.InvalidCodeMessageError);

			messageObject.ReplacementDeclarationType = "TST";
			messageObject.Validation.ValidateReplacementDeclarationType();
			AssertNoErrorContaining(messageObject.ReplacementDeclarationTypeInfo, MandatoryValidation.MustBeEntered);
			AssertHasMessageErrorContaining(messageObject.ReplacementDeclarationTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckShouldSend()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.DeltaGFallbackStatus = "PDS";

			var fallbackSetting = new FallbackSettings();
			fallbackSetting.End = ZDateTime.Today.AddDays(1);
			fallbackSetting.Start = ZDateTime.Today.AddDays(-1);
			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, fallbackSetting);

			var messageObject = new DeltaGJobDeclarationMessageSendingObject(entry);
			messageObject.ShouldSend = true;
			AssertHasErrorContaining(messageObject.ShouldSendInfo, "Delta G fallback is active and a message for this entry has already been created during fallback.");

			messageObject.ShouldSend = false;
			AssertNoErrorContaining(messageObject.ShouldSendInfo, "Delta G fallback is active and a message for this entry has already been created during fallback.");

			entry.DeltaGFallbackStatus = "";
			messageObject.ShouldSend = true;
			AssertNoErrorContaining(messageObject.ShouldSendInfo, "Delta G fallback is active and a message for this entry has already been created during fallback.");

			FRCustomsDataRegistry.Instance.DeltaGMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new FallbackSettings());
			entry.DeltaGFallbackStatus = "PDS";
			messageObject.Validation.ValidateShouldSend();
			AssertNoErrorContaining(messageObject.ShouldSendInfo, "Delta G fallback is active and a message for this entry has already been created during fallback.");
		}

		public void TestCheckAvailableBalanceOnA12AccountWithLocalCurrency()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			SetUpAi2Permit(dec, dec.LocalCurrencyCode, 601m, 302m);
			var entry1 = SetUpAi2Amount(dec);
			var messageErrorTheAvailableBalanceLessThanAi2Amount = "The available balance on the AI2 account is ";
			var messageObject = new DeltaGJobDeclarationMessageSendingObject(entry1);
			var info = messageObject.MessageTypeInfo;
			messageObject.ShouldSend = true;
			messageObject.MessageType = EntryActionCodeList.Codes.VAL;
			Assert(dec.Ai2Permit.CPH_Balance == 299m);
			Assert(dec.Ai2Permit.CPH_Calc_TotalBalanceIncludingPending.Amount == 299m);
			Assert(dec.Ai2Permit.CPH_Calc_UsedBalance == 302m);
			AssertHasMessageErrorContaining(info, messageErrorTheAvailableBalanceLessThanAi2Amount);

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			SetUpAi2Permit(dec2, dec2.LocalCurrencyCode, 803m, 302m);
			var entry2 = SetUpAi2Amount(dec2);
			var messageObject2 = new DeltaGJobDeclarationMessageSendingObject(entry2);
			var info2 = messageObject.MessageTypeInfo;
			messageObject2.ShouldSend = true;
			messageObject2.MessageType = EntryActionCodeList.Codes.VAL;
			Assert(dec2.Ai2Permit.CPH_Balance == 501m);
			Assert(dec2.Ai2Permit.CPH_Calc_TotalBalanceIncludingPending.Amount == 501m);
			Assert(dec2.Ai2Permit.CPH_Calc_UsedBalance == 302m);
			AssertHasMessageErrorContaining(info2, messageErrorTheAvailableBalanceLessThanAi2Amount);
		}

		public void TestCheckAvailableBalanceOnA12AccountWithAbroadCurrency()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			SetUpAi2Permit(dec, "AUD", 601m, 300m);
			var entry1 = SetUpAi2Amount(dec);
			var messageErrorTheAvailableBalanceLessThanAi2Amount = "The available balance on the AI2 account is ";
			var messageObject = new DeltaGJobDeclarationMessageSendingObject(entry1);
			var info = messageObject.MessageTypeInfo;
			messageObject.ShouldSend = true;
			messageObject.MessageType = EntryActionCodeList.Codes.VAL;
			Assert(dec.Ai2Permit.CPH_Balance == 301m);
			Assert(dec.Ai2Permit.CPH_Calc_TotalBalanceIncludingPending.Amount == 301m);
			Assert(dec.Ai2Permit.CPH_Calc_UsedBalance == 300m);
			AssertHasMessageErrorContaining(info, messageErrorTheAvailableBalanceLessThanAi2Amount);

			SetUpAi2Permit(dec, "AUD", 800m, 1m);
			messageObject.ShouldSend = true;
			Assert(dec.Ai2Permit.CPH_Balance == 799m);
			Assert(dec.Ai2Permit.CPH_Calc_TotalBalanceIncludingPending.Amount == 799m);
			Assert(dec.Ai2Permit.CPH_Calc_UsedBalance == 1m);
			messageObject.MessageType = EntryActionCodeList.Codes.VAL;
			AssertNoMessageErrorContaining(info, messageErrorTheAvailableBalanceLessThanAi2Amount);
		}

		public DeltaGJobDeclarationMessageSendingObject GenerateDeclarationSendingObject(string deltaMode, string ceiSubStyle)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = MessageSubTypeList.Codes.IMC;
			declaration.JE_DeltaMode = deltaMode;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var line = invoice.InvoiceLines.AddNew();
			line.JI_JZ = invoice.PK;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = "ACC";
			entry.MovementReferenceNumberSetter("ABC", ZDateTime.BrettsBirthday);
			entry.CH_CustomsMessageRemarks = "Amendment";
			entry.CH_BGMReference = "1GB945390992000-B00001002";

			var entryinstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			entry.CH_CEI_Instruction = entryinstruction.PK;
			entry.EntryInstruction.CEI_SubStyle = ceiSubStyle;
			entryinstruction.CEI_JE = declaration.PK;

			var testItem = new DeltaGJobDeclarationMessageSendingObject(entry);

			return testItem;
		}
	}
}
