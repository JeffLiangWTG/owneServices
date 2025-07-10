using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	public class MessageSendingEntryLineObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckShouldSend_MessageTypeIs5FN()
		{
			var invoice = entry.Declaration.Invoices.AddNew();
			CreateEntryLine(entry, 1, invoice, "A093000004");
			CreateEntryLine(entry, 2, invoice, "A1070001");

			var messageSendingObjectParent = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original);
			AssertEquals(1, messageSendingObjectParent.SendingObjectsCollection.Count);
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
			var entryLines = messageSendingObject.MessageSendingEntryLines;
			AssertEquals(2, entryLines.Count);

			#region Send not 929
			entryLines[0].ShouldSend = true;
			entryLines[1].ShouldSend = true;
			AssertHasErrorContaining(entryLines[0].ShouldSendInfo, "You cannot send this message. The status of '5FN' indicates Customs has never accepted an original message of '929'. Please send a message of '929' before trying to send this message.");
			AssertHasErrorContaining(entryLines[1].ShouldSendInfo, "You cannot send this message. The status of '5FN' indicates Customs has never accepted an original message of '929'. Please send a message of '929' before trying to send this message.");
			#endregion

			#region Send 929 and not received R99
			entry.EntryNumber = "6N00221000001M";
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
			messageSendingObject = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original).SendingObjectsCollection[0];
			entryLines = messageSendingObject.MessageSendingEntryLines;
			entryLines[0].ShouldSend = true;
			entryLines[1].ShouldSend = true;
			AssertHasErrorContaining(entryLines[0].ShouldSendInfo, "You cannot send this message. Its status indicates the last electronic document is still waiting for a response.");
			AssertHasErrorContaining(entryLines[1].ShouldSendInfo, "You cannot send this message. Its status indicates the last electronic document is still waiting for a response.");
			#endregion

			#region Send 929 and received R99
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			messageSendingObjectParent = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original);
			messageSendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
			entryLines = messageSendingObject.MessageSendingEntryLines;
			entryLines[0].ShouldSend = true;
			entryLines[1].ShouldSend = true;
			AssertNoErrors(entryLines[0].ShouldSendInfo);
			AssertNoErrors(entryLines[1].ShouldSendInfo);

			new GOVCBR5FNSender(messageSendingObjectParent.ObjectsToSend, Factory).Send();
			var created5FNMessages = entry.Messages.Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5FN).OrderBy(x => x.EM_MessageNum).ToList();
			var created5FNEntryNum = entry.EntryNumbers.Where(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5FN).OrderBy(x => x.CE_EntryNum).ToList();
			AssertEquals("6N00221000001M", created5FNEntryNum[0].CE_EntryNum);
			AssertEquals("1", created5FNEntryNum[0].CE_EntryLineReference);
			AssertEquals("6N00221000001M", created5FNEntryNum[1].CE_EntryNum);
			AssertEquals("2", created5FNEntryNum[1].CE_EntryLineReference);
			#endregion

			#region Send 5FN and not received R99 or R20
			messageSendingObject = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original).SendingObjectsCollection[0];
			entryLines = messageSendingObject.MessageSendingEntryLines;
			entryLines[0].ShouldSend = true;
			entryLines[1].ShouldSend = true;
			AssertHasErrorContaining(entryLines[0].ShouldSendInfo, "You cannot send this message. Its status indicates the last electronic document is still waiting for a response.");
			AssertHasErrorContaining(entryLines[1].ShouldSendInfo, "You cannot send this message. Its status indicates the last electronic document is still waiting for a response.");
			#endregion

			#region Send 5FN and received R99 or R20
			created5FNEntryNum[0].CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			created5FNEntryNum[1].CE_EntryStatus = CustomsMessageStatusTypeList.Codes.OriginalRejected;
			messageSendingObject = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original).SendingObjectsCollection[0];
			entryLines = messageSendingObject.MessageSendingEntryLines;
			entryLines[0].ShouldSend = true;
			entryLines[1].ShouldSend = true;
			AssertHasErrorContaining(entryLines[0].ShouldSendInfo, "You cannot send this message. Its status indicates Customs has already accepted a message of this type.");
			AssertNoErrors(entryLines[1].ShouldSendInfo);
			#endregion

			#region Received 5BG about 929
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationApprovedByCustoms;
			messageSendingObject = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original).SendingObjectsCollection[0];
			entryLines = messageSendingObject.MessageSendingEntryLines;
			entryLines[0].ShouldSend = true;
			entryLines[1].ShouldSend = true;
			AssertHasErrorContaining(entryLines[0].ShouldSendInfo, "You cannot send this message. Its status indicates the cancellation of a declaration has been accepted by Customs.");
			AssertHasErrorContaining(entryLines[1].ShouldSendInfo, "You cannot send this message. Its status indicates the cancellation of a declaration has been accepted by Customs.");
			#endregion

			#region Received 023 about 929
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationByCustoms;
			messageSendingObject = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original).SendingObjectsCollection[0];
			entryLines = messageSendingObject.MessageSendingEntryLines;
			entryLines[0].ShouldSend = true;
			entryLines[1].ShouldSend = true;
			AssertHasErrorContaining(entryLines[0].ShouldSendInfo, "This entry has been declined by Customs. Please refer to the recent 023 message. You can no longer send further messages on this entry.");
			AssertHasErrorContaining(entryLines[1].ShouldSendInfo, "This entry has been declined by Customs. Please refer to the recent 023 message. You can no longer send further messages on this entry.");
			#endregion
		}

		public void TestCheckShouldSendWithMessageErrors_MessageTypeIs5FN()
		{
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryLine_HasMessageError = CreateEntryLine(entry, 1, invoice, "A093000004");
			var entryLine_NoMessageError = CreateEntryLine(entry, 2, invoice, "A1070001");
			declaration.Validation.ValidateAll();
			entryLine_HasMessageError.Validation.ValidateAll();
			Assert(entryLine_HasMessageError.HasMessageErrors);
			Assert(!entryLine_NoMessageError.HasMessageErrors);

			var messageSendingObjectParent = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original);
			AssertEquals(1, messageSendingObjectParent.SendingObjectsCollection.Count);
			Factory.Save();
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection[0];
			var entryLines = messageSendingObject.MessageSendingEntryLines;
			AssertEquals(2, entryLines.Count);
			entryLines[0].DecorateFromCusEntryLine(entryLine_HasMessageError, ElectronicDocumentTypeList.Codes._5FN);
			entryLines[0].ShouldSend = true;
			entryLines[1].DecorateFromCusEntryLine(entryLine_NoMessageError, ElectronicDocumentTypeList.Codes._5FN);
			entryLines[1].ShouldSend = true;
			AssertHasErrorContaining(entryLines[0].ShouldSendInfo, "The selected entry line has message errors. Unless you fix them, you will not be able to send a message as you don't have the security right to send with message errors.");
			AssertNoWarningContaining(entryLines[0].ShouldSendInfo, "The selected entry line has message errors. Please review them. However you will be able to send a message as you have the security right to send with message errors.");
			AssertHasErrorContaining(entryLines[1].ShouldSendInfo, "The selected entry line has message errors. Unless you fix them, you will not be able to send a message as you don't have the security right to send with message errors.");
			AssertNoWarningContaining(entryLines[1].ShouldSendInfo, "The selected entry line has message errors. Please review them. However you will be able to send a message as you have the security right to send with message errors.");

			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
			entryLines = new JobDeclarationMiscMessageSendingObjectParent(entry.Declaration, ElectronicDocumentTypeList.Codes._5FN, MessageFunctions.MessageFunctionCode.Original).SendingObjectsCollection[0].MessageSendingEntryLines;
			entryLines[0].ShouldSend = true;
			entryLines[1].ShouldSend = true;
			AssertNoErrorContaining(entryLines[0].ShouldSendInfo, "The selected entry line has message errors. Unless you fix them, you will not be able to send a message as you don't have the security right to send with message errors.");
			AssertHasWarningContaining(entryLines[0].ShouldSendInfo, "The selected entry line has message errors. Please review them. However you will be able to send a message as you have the security right to send with message errors.");
			AssertNoErrorContaining(entryLines[1].ShouldSendInfo, "The selected entry line has message errors. Unless you fix them, you will not be able to send a message as you don't have the security right to send with message errors.");
			AssertHasWarningContaining(entryLines[1].ShouldSendInfo, "The selected entry line has message errors. Please review them. However you will be able to send a message as you have the security right to send with message errors.");
		}

		public void TestKoreanInputColumn()
		{
			var invoice = entry.Declaration.Invoices.AddNew();
			CreateEntryLine(entry, 1, invoice);

			var messageSendingObjects = new ExtendReExportDateMessageSendingObjectCollection(entry.Declaration);
			AssertEquals(1, messageSendingObjects.Count);
			AssertEquals(1, messageSendingObjects[0].D72EntryLines.Count);
			AssertEquals(1, messageSendingObjects[0].MessageSendingInvoiceLines.Count);

			var entryLineObject = messageSendingObjects[0].D72EntryLines[0];
			entryLineObject.Remark = "한글, 특수문자(ℓ) 입력가능";
			AssertNoErrors(entryLineObject.RemarkInfo);

			entryLineObject.UseCodeDescription = "한글, 특수문자(ℓ) 입력가능";
			AssertNoErrors(entryLineObject.UseCodeDescriptionInfo);

			var invoiceLineObject = messageSendingObjects[0].MessageSendingInvoiceLines[0];
			invoiceLineObject.Remark = "한글, 특수문자(ℓ) 입력가능";
			AssertNoErrors(invoiceLineObject.RemarkInfo);

			invoiceLineObject.HSDescription = "한글, 특수문자(ℓ) 입력가능";
			AssertNoErrors(invoiceLineObject.HSDescriptionInfo);
		}

		CusEntryLine CreateEntryLine(CusEntryHeader entry, ZShort entryLineNo, JobComInvoiceHeader invoice, string secondaryPreference = "")
		{
			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_LineNumber = entryLineNo;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_SecondaryPreference = secondaryPreference;

			return entryLine;
		}

		CusEntryHeader entry;
		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A093000004", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제93조제4호 해당물품");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.IsDutyExempt, YesNo.Yes, tariff1);
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A1070001", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제107조 제1항 해당물품");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = KRJobMessageTypeList.Codes.Import;
		}
	}
}
