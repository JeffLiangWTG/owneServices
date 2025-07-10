using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5FEAmendmentSenderTest : CusEntryHeaderAmendmentMessageSenderTest<GOVCBR5FEAmendmentSender>
	{
		protected override IEnumerable<JobDeclarationAmendmentMessageSendingObject> GetMessageParents() => MessageSendingObjects;

		protected override MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5FEAmendmentSenderForTest(Parents, Factory) : new GOVCBR5FEAmendmentSender(Parents, Factory);

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> Parents => parents ?? (parents = GetMessageParents());
		IEnumerable<JobDeclarationAmendmentMessageSendingObject> parents;

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					var entry = new TestDataSetupHelper(Factory).GetEntry929FullData(ZString.Empty, ZBool.True);
					entry.CH_VersionID = 1;
					entry.EntryInstruction.CEI_ApplyDutyPenaltyReduction = "N";

					var statement929 = Factory.New<CusStatementHeader>();
					statement929.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;
					statement929.B2_PaymentAuthorizationDate = new ZDateTime(2025, 01, 14);
					statement929.B2_GC = entry.Declaration.JE_GC;
					statement929.B2_StatementNumber = "1234567890123456789";
					statement929.B2_PaymentStatus = StatementHeaderPaymentStatusList.Codes.PYC;

					var statementLine = statement929.StatementLines.AddNew();
					statementLine.B3_AssociatedEntry = "9876543210987654321";
					statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
					statementLine.B3_EntryNum = entry.EntryNumber;

					using var stream = KRXmlObjectSerializer.Serialize(new ImportEntryHeaderCreator().Create(entry));
					AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
					AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
					Factory.Save();

					var entryNum5UA = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UA);
					entryNum5UA.CE_EntryLineReference = "1";

					var chargeVersion1 = entry.Charges.AddNew();
					chargeVersion1.C1_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
					chargeVersion1.C1_ChargeAmount = 1000m;
					chargeVersion1.C1_RateOverrideReasonCode = "1";
					var chargeVersion2 = entry.Charges.AddNew();
					chargeVersion2.C1_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
					chargeVersion2.C1_ChargeAmount = 500m;
					var entryLine = entry.MergedLines.AddNew();
					entryLine.CL_LineNumber = 1;
					var feeVersion1 = entryLine.Fees.AddNew();
					feeVersion1.CF_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
					feeVersion1.CF_ChargeAmount = 1000m;
					feeVersion1.CF_RateOverrideReasonCode = "1";
					var feeVersion2 = entryLine.Fees.AddNew();
					feeVersion2.CF_ChargeType = ChargeTypeList.Codes.SpecialConsumptionTax;
					feeVersion2.CF_ChargeAmount = 500m;

					var sendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, MessageType);
					var messageSendingObject = sendingObjectParent.SendingObjectsCollection[0];
					entry.EntryInstruction.AmendmentSessionalDataCollection.First().PenaltyExemptionSessionalData.CSI_Value = 100m;
					messageSendingObject.PenaltyExemptionReasonCode = PenaltyExemptionReasonCodeList.Codes.A1;
					messageSendingObject.PenaltyExemptionReason = "";

					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>();
				}
				return messageSendingObjects;
			}
		}

		protected override ZString MessageType => ElectronicDocumentTypeList.Codes._5FE;

		IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects;

		public override void TestStatusIsUpdated()
		{
			GetMessageSender().Send();
			foreach (JobDeclarationAmendmentMessageSendingObject parent in Parents)
			{
				AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentSent, GetStatusField(parent.Header));
			}
		}

		public void TestChargesVersionUpdate()
		{
			GetMessageSender().Send();
			foreach (JobDeclarationAmendmentMessageSendingObject parent in Parents)
			{
				var entry = parent.Header;
				AssertNotNull(entry.Charges.Cast<CusEntryHeaderCharges>().Where(x => x.C1_RateOverrideReasonCode == "1"));
				AssertNotNull(entry.Charges.Cast<CusEntryHeaderCharges>().Where(x => x.C1_RateOverrideReasonCode == "2"));
				AssertNotNull(entry.MergedLines[0].Fees.Cast<CusEntryLineFee>().Where(x => x.CF_RateOverrideReasonCode == "1"));
				AssertNotNull(entry.MergedLines[0].Fees.Cast<CusEntryLineFee>().Where(x => x.CF_RateOverrideReasonCode == "2"));
			}
		}

		public override void TestSendMessage()
		{
			var entry = Parents.Single().Header;
			entry.CH_VersionID = 1;
			var entryNum5UA = GetEntryNumberAndAssert5FESendMessageWith5UAData(entry, Constants.YesNo.No, PenaltyExemptionReasonCodeList.Codes.A1, "2", 1, "1", ZString.Empty, ZString.Empty);

			entry.CH_VersionID = 2;
			var entryNum5UAVersion1 = GetEntryNumberAndAssert5FESendMessageWith5UAData(entry, Constants.YesNo.Yes, PenaltyExemptionReasonCodeList.Codes.A3, "3", 2, "2", CustomsMessageStatusTypeList.Codes.OriginalSent, "2");
			AssertNotEquals(entryNum5UA, entryNum5UAVersion1);

			var entryNum5UAVersion2 = GetEntryNumberAndAssert5FESendMessageWith5UAData(entry, Constants.YesNo.Yes, PenaltyExemptionReasonCodeList.Codes.A4, "3", 3, "3", CustomsMessageStatusTypeList.Codes.OriginalSent, "3");
			AssertNotEquals(entryNum5UAVersion1.PK, entryNum5UAVersion2.PK);
		}

		CusEntryNumber GetEntryNumberAndAssert5FESendMessageWith5UAData(CusEntryHeader entry, ZString penaltyExemptionIndicator, ZString penaltyExemptionReasonCode, ZString amendmentVersionNumber, ZInt entryNumCount, ZString maxVersionNumber5UA, ZString status5UA, ZString versionNumber5UAIn5FE)
		{
			var sendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, MessageType);
			var messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>();
			messageSendingObjects.FirstOrDefault().PenaltyExemptionIndicator = penaltyExemptionIndicator;
			messageSendingObjects.FirstOrDefault().PenaltyExemptionReasonCode = penaltyExemptionReasonCode;

			new GOVCBR5FEAmendmentSender(messageSendingObjects, Factory).Send();
			var message5FE = entry.Messages.Cast<EDIMessage>().LastOrDefault(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals("5FE version number", amendmentVersionNumber, message5FE.EM_ApplicationReference);
			AssertEquals("5UA entrynum count", entryNumCount, entry.EntryNumbers.Cast<CusEntryNumber>().Count(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UA));

			var entryNumber = entry.EntryNumbers.GetCusEntryNumWithMaxVersionNumber(ElectronicDocumentTypeList.Codes._5UA);
			AssertEquals("5UA version number", maxVersionNumber5UA, entryNumber.CE_EntryLineReference);
			AssertEquals("5UA status", status5UA, entryNumber.CE_EntryStatus);
			AssertEquals("5UA version number in 5FE EDIMessage.EM_MessageOwner", versionNumber5UAIn5FE, message5FE.EM_MessageOwner);
			return entryNumber;
		}

		public void TestSend5ULTogether()
		{
			var entry = Parents.Single().Header;
			Factory.Save();

			var sendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, MessageType);
			var messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>();
			messageSendingObjects.FirstOrDefault().RefundRequestSubmissionYN = Constants.YesNo.Yes;

			new GOVCBR5FEAmendmentSender(messageSendingObjects, Factory).Send();

			entry.Reload();
			var entryNumber5UL = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL && x.CE_EntryLineReference == "1234567890123456789");

			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalSent, entryNumber5UL.CE_EntryStatus);
			AssertEquals("1234567890123456789", entryNumber5UL.CE_EntryLineReference);
		}

		public new void TestSendException()
		{
			IsExceptionTest = ZBool.True;
			var messages = Parents.FirstOrDefault().Header.Messages.Cast<EDIMessage>();
			AssertEquals("Two messages exits before sending a message.", 0, messages.Count());
			AssertExceptionThrown<Exception>(() => GetMessageSender().Send());
			AssertEquals("Exception occurred when sending a message, and no new message has been created.", 0, messages.Count());
		}

		[TestDate(2025, 01, 01)]
		public void TestRefundRequestSubmissionYN()
		{
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");

			var declaration = Factory.New<JobDeclaration>();
			CreateCusEntryNumAndCustomsDisbursementBills("1111", "I", "0127030112200237051", ChargeTypeList.Codes.Duty, -10m, RefundCauseCodeList.Codes._01, RefundTypeList.Codes.A, RefundReasonCodeList.Codes._02);
			CreateCusEntryNumAndCustomsDisbursementBills("2222", "D", "0127030112200237052", ChargeTypeList.Codes.LiquorTax, -20m, RefundCauseCodeList.Codes._02, RefundTypeList.Codes.B, RefundReasonCodeList.Codes._01);
			CreateCusEntryNumAndCustomsDisbursementBills("3333", "I", "0127030112200237053", ChargeTypeList.Codes.SpecialConsumptionTax, -30m, RefundCauseCodeList.Codes._03, RefundTypeList.Codes.C, RefundReasonCodeList.Codes._02);
			CreateCusEntryNumAndCustomsDisbursementBills("4444", "D", "0127030112200237054", ChargeTypeList.Codes.TransportationTax, -40m, RefundCauseCodeList.Codes._04, RefundTypeList.Codes.D, RefundReasonCodeList.Codes._03);
			Factory.Save();

			var sendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5FE);
			AssertEquals(4, sendingObjectParent.SendingObjectsCollection.Count);

			var sendingObject1 = sendingObjectParent.SendingObjectsCollection[0];
			sendingObject1.RefundRequestSubmissionYN = ZString.Empty;
			sendingObject1.ShouldSend = true;

			var sendingObject2 = sendingObjectParent.SendingObjectsCollection[1];
			sendingObject2.RefundRequestSubmissionYN = YesNoList.Codes.No;
			sendingObject2.ShouldSend = true;

			var sendingObject3 = sendingObjectParent.SendingObjectsCollection[2];
			sendingObject3.RefundRequestSubmissionYN = YesNoList.Codes.Yes;
			sendingObject3.ShouldSend = true;

			var sendingObject4 = sendingObjectParent.SendingObjectsCollection[3];
			sendingObject4.RefundRequestSubmissionYN = YesNoList.Codes.Yes;
			sendingObject4.ShouldSend = true;
			new GOVCBR5FEAmendmentSender(sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>(), Factory).Send();
			Factory.Save();

			AssertWhenRefundRequestSubmissionYNIsNotY(sendingObject1.Header, "0127030112200237051");
			AssertWhenRefundRequestSubmissionYNIsNotY(sendingObject2.Header, "0127030112200237052");
			AssertWhenRefundRequestSubmissionYNIsY(sendingObject3.Header, "0127030112200237053", "6N0022500001U", RefundCauseCodeList.Codes._03, RefundTypeList.Codes.C, RefundReasonCodeList.Codes._02, 30m);
			AssertWhenRefundRequestSubmissionYNIsY(sendingObject4.Header, "0127030112200237054", "6N0022500002U", RefundCauseCodeList.Codes._04, RefundTypeList.Codes.D, RefundReasonCodeList.Codes._03, 40m);

			void CreateCusEntryNumAndCustomsDisbursementBills(string entryNum, string statementType, string statementNumber, string chargeType, decimal chargeAmount, string refundCauseCode, string refundType, string refundReasonCode)
			{
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.EntryNumber = entryNum;
				entry.Charges.AddNew(ChargeTypeList.Codes.PenaltyForMissedDeclaration, chargeAmount);

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.AmendmentSessionalDataCollection.AddNew();
				instruction.CEI_RefundCauseCode = refundCauseCode;
				instruction.CEI_RefundType = refundType;
				instruction.CEI_RefundReasonCode = refundReasonCode;
				entry.CH_CEI_Instruction = instruction.PK;

				var statementHeader = Factory.New<CusStatementHeader>();
				statementHeader.B2_StatementType = statementType;
				statementHeader.B2_StatementNumber = statementNumber;
				var statementLine = statementHeader.StatementLines.AddNew();
				statementLine.B3_EntryType = KRJobMessageTypeList.Codes.Import;
				statementLine.B3_AssociatedEntry = statementNumber;
				statementLine.B3_EntryNum = entryNum;
			}

			void AssertWhenRefundRequestSubmissionYNIsNotY(CusEntryHeader entry, string customsDisbursementBillNo)
			{
				Assert(!entry.Messages.Cast<EDIMessage>().Any(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UL));
				Assert(!entry.EntryNumbers.Cast<CusEntryNumber>().Any(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL));

				var refundSessionalData = entry.GetOrCreateRefundSessionalDataOriginalSendable(customsDisbursementBillNo);
				AssertEquals(customsDisbursementBillNo, refundSessionalData.CSI_ReferenceNumber2);
				AssertEquals(ZString.Empty, refundSessionalData.CSI_Code);
				AssertEquals(ZString.Empty, refundSessionalData.CSI_Procedure);
				AssertEquals(ZString.Empty, refundSessionalData.CSI_SubType);
				AssertEquals(ZDecimal.Zero, refundSessionalData.CSI_Value);
			}

			void AssertWhenRefundRequestSubmissionYNIsY(CusEntryHeader entry, string customsDisbursementBillNo, string refundDeclarationNumber, string csi_procedure, string csi_subType, string csi_issuerType, decimal csi_value)
			{
				var messages = entry.Messages.Cast<EDIMessage>();
				var message5FE = messages.FirstOrDefault(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5FE);
				var message5UL = messages.FirstOrDefault(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UL);
				AssertNotNull(message5UL);
				AssertEquals(customsDisbursementBillNo, message5UL.EM_ApplicationReference);
				AssertEquals(refundDeclarationNumber, message5UL.EM_MessageOwner);
				AssertEquals(RefundRequestType.Simultaneous, message5UL.EM_MessageSubType);

				var entryNum5UL = entry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UL);
				AssertNotNull(entryNum5UL);
				AssertEquals(customsDisbursementBillNo, entryNum5UL.CE_EntryLineReference);

				var refundSessionalData = entry.GetOrCreateRefundSessionalDataOriginalSendable(customsDisbursementBillNo);
				AssertEquals("Y", refundSessionalData.CSI_Code);
				AssertEquals(csi_procedure, refundSessionalData.CSI_Procedure);
				AssertEquals(csi_subType, refundSessionalData.CSI_SubType);
				AssertEquals(csi_issuerType, refundSessionalData.CSI_IssuerType);
				AssertEquals(csi_value, refundSessionalData.CSI_Value);
				AssertEquals(refundDeclarationNumber, refundSessionalData.CSI_ReferenceNumber);
				AssertEquals(customsDisbursementBillNo, refundSessionalData.CSI_ReferenceNumber2);
			}
		}

		public void TestNoExceptionWhenAmendForTax()
		{
			var entry = new TestDataSetupHelper(Factory).Create929SnapShot(ZString.Empty);
			var vat = entry.Charges.FirstOrDefault(x => x.C1_ChargeType == ChargeTypeList.Codes.VAT);
			AssertEquals(6000m, vat.C1_ChargeAmount);

			vat.C1_ChargeAmount = 8000m;
			var sendingObjectParent = new JobDeclarationAmendmentMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5FE);
			var sendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<JobDeclarationAmendmentMessageSendingObject>();
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			new GOVCBR5FEAmendmentSender(sendingObjects, Factory).Send();
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		protected override ZString GetStatusField(CusEntryHeader entry)
		{
			return entry.CH_Status;
		}

		class GOVCBR5FEAmendmentSenderForTest : GOVCBR5FEAmendmentSender
		{
			public GOVCBR5FEAmendmentSenderForTest(IEnumerable<JobDeclarationAmendmentMessageSendingObject> messageSendingObjects, BusinessObjectFactory factory)
			: base(messageSendingObjects, factory)
			{
			}

			protected override Import5FEHeader GetMessageDataProvider(CusEntryHeader parent, ImportEntryHeader currentSnapshot, AmendedItemCollection amendedItems) => throw new Exception();
		}
	}
}
