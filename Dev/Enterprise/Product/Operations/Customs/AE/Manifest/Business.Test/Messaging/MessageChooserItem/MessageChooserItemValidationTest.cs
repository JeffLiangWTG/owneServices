using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class MessageChooserItemValidationTest : TestCaseWithFactory
{
	public void TestCheckEntryType_Mandatory()
	{
		ValidationTestHelper.AssertErrorIfNotEntered(MessageChooserItem.EntryTypeInfo);
	}

	public void TestCheckEntryType_ValidCode()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(MessageChooserItem.EntryTypeInfo, "!", EntryTypes.Codes.Original);
	}

	public void TestCheckSubjectCode()
	{
		const string messageErrorOriginal = "Subject Code must not be captured for Original Manifest Messages";
		const string messageErrorCancellation = "Subject Code must be CXL for Cancellation Manifest Messages";
		const string messageErrorEmptyChange = "Subject Code must be selected for Change Manifest Messages";
		const string messageErrorInvalidChange = "Subject Code cannot be CXL for Change Manifest Messages";
		CombineAssertions(() =>
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(MessageChooserItem.SubjectCodeInfo, "!@#", SubjectCodes.Codes.ProvideJustificationForNotSubmittingAdditionalInformation);

			var targetInfo = MessageChooserItem.SubjectCodeInfo;
			MessageChooserItem.EntryType = EntryTypes.Codes.Original;
			MessageChooserItem.SubjectCode = "ABC";
			AssertHasMessageError("Original message, Subject Code not empty", targetInfo, messageErrorOriginal);
			MessageChooserItem.SubjectCode = ZString.Empty;
			AssertNoMessageError("Original message, Subject Code empty", targetInfo, messageErrorOriginal);

			MessageChooserItem.EntryType = EntryTypes.Codes.Cancellation;
			MessageChooserItem.SubjectCode = "ABC";
			AssertHasMessageError("Cancellation message, Subject Code invalid", targetInfo, messageErrorCancellation);
			MessageChooserItem.SubjectCode = SubjectCodes.Codes.ProvideJustificationForCanceling;
			AssertNoMessageError("Cancellation message, valid Subject Code", targetInfo, messageErrorCancellation);

			MessageChooserItem.EntryType = EntryTypes.Codes.Change;
			AssertHasMessageError("Change message, invalid Subject Code", targetInfo, messageErrorInvalidChange);
			MessageChooserItem.SubjectCode = ZString.Empty;
			AssertHasMessageError("Change message, empty Subject Code", targetInfo, messageErrorEmptyChange);
			MessageChooserItem.SubjectCode = SubjectCodes.Codes.UpdatePreviouslySubmittedInformation;
			AssertNoMessageErrors("Change message, valid Subject Code", targetInfo);
		});
	}

	public void TestCheckSubject()
	{
		const string shouldBeEmptyMessageError = "Subject must not be captured for Original Manifest Messages";
		const string shouldNotBeEmptyMessageError = "A Subject/Reason must be supplied";
		CombineAssertions(() =>
		{
			var targetInfo = MessageChooserItem.SubjectInfo;
			MessageChooserItem.EntryType = EntryTypes.Codes.Original;
			MessageChooserItem.Subject = "XYZ";
			AssertHasMessageError("Original message, Subject not empty", targetInfo, shouldBeEmptyMessageError);
			MessageChooserItem.Subject = ZString.Empty;
			AssertNoMessageError("Original message, Subject empty", targetInfo, shouldBeEmptyMessageError);

			MessageChooserItem.EntryType = EntryTypes.Codes.Cancellation;
			AssertHasMessageErrorContaining("Cancellation message, Subject empty", targetInfo, shouldNotBeEmptyMessageError);
			MessageChooserItem.EntryType = EntryTypes.Codes.Change;
			AssertHasMessageErrorContaining("Change message, Subject empty", targetInfo, shouldNotBeEmptyMessageError);
			MessageChooserItem.Subject = "XYZ";
			AssertNoMessageErrorContaining("Subject not empty", targetInfo, shouldNotBeEmptyMessageError);
		});
	}

	public void TestCheckEntryTypeAndSubmissionOriginal()
	{
		var message = "This manifest has been submitted before.";

		var bill = MessageChooserItem.Bill;
		var ediMessage = Factory.New<AEEDIMessage>();
		ediMessage.EM_Status = Messaging.Integration.EDIMessageStatusList.Codes.Sent;
		ediMessage.EM_MessageType = AEConstants.Messaging.MessageTypes.CUSCAR;
		bill.Messages.Add(ediMessage);

		CombineAssertions(() =>
		{
			AssertNoMessageErrorContaining(messageChooserItem.EntryTypeInfo, message);

			MessageChooserItem.EntryType = EntryTypes.Codes.Original;
			AssertHasMessageError(messageChooserItem.EntryTypeInfo, message);

			ediMessage.EM_MessageType = "QQQ";
			messageChooserItem.Validation.ValidateEntryType();
			AssertNoMessageErrorContaining(messageChooserItem.EntryTypeInfo, message);

			ediMessage.EM_MessageType = AEConstants.Messaging.MessageTypes.CUSCAR;
			MessageChooserItem.EntryType = EntryTypes.Codes.Change;
			AssertNoMessageErrorContaining(messageChooserItem.EntryTypeInfo, message);
		});
	}

	public void TestCheckEntryTypeAndSubmissionChangeOrCancellation()
	{
		var message = "Amendment/Cancellation cannot be done on an unaccepted entry.";

		var bill = MessageChooserItem.Bill;
		var ediMessage = Factory.New<AEEDIMessage>();
		ediMessage.EM_Status = Messaging.Integration.EDIMessageStatusList.Codes.Acknowledged;
		ediMessage.EM_MessageType = AEConstants.Messaging.MessageTypes.CUSCAR;
		bill.Messages.Add(ediMessage);

		CombineAssertions(() =>
		{
			AssertNoError(messageChooserItem.EntryTypeInfo, message);

			MessageChooserItem.EntryType = EntryTypes.Codes.Change;
			AssertNoMessageErrorContaining(messageChooserItem.EntryTypeInfo, message);

			ediMessage.EM_MessageType = "QQQ";
			messageChooserItem.Validation.ValidateEntryType();
			AssertHasMessageError($"Message Type:{ediMessage.EM_MessageType}", messageChooserItem.EntryTypeInfo, message);

			bill.ABL_MessageStatus = AEConstants.Messaging.MessageTypes.CUSRES;
			bill.ABL_BillStatus = AEManifestConstants.CustomsStatus.RFI;
			messageChooserItem.Validation.ValidateEntryType();
			AssertNoMessageErrorContaining($"Message Type:{ediMessage.EM_MessageType}, MessageStatus {bill.ABL_MessageStatus}, BillStatus {bill.ABL_BillStatus}", messageChooserItem.EntryTypeInfo, message);

			bill.ABL_MessageStatus = AEConstants.Messaging.MessageTypes.CONTRL;
			messageChooserItem.Validation.ValidateEntryType();
			AssertHasMessageError($"Message Type:{ediMessage.EM_MessageType}, MessageStatus {bill.ABL_MessageStatus}, BillStatus {bill.ABL_BillStatus}", messageChooserItem.EntryTypeInfo, message);

			bill.ABL_BillStatus = AEManifestConstants.CustomsStatus.ACT;
			MessageChooserItem.EntryType = EntryTypes.Codes.Cancellation;
			AssertNoMessageErrorContaining($"Message Type:{ediMessage.EM_MessageType}, MessageStatus {bill.ABL_MessageStatus}, BillStatus {bill.ABL_BillStatus}, entryType {MessageChooserItem.EntryType}", messageChooserItem.EntryTypeInfo, message);

			ediMessage.EM_MessageType = AEConstants.Messaging.MessageTypes.CUSCAR;
			MessageChooserItem.EntryType = EntryTypes.Codes.Original;
			AssertNoMessageErrorContaining($"Message Type:{ediMessage.EM_MessageType}, MessageStatus {bill.ABL_MessageStatus}, BillStatus {bill.ABL_BillStatus}, entryType {MessageChooserItem.EntryType}", messageChooserItem.EntryTypeInfo, message);
		});
	}

	MessageChooserItem MessageChooserItem => messageChooserItem ??= GetNewMessageChooserItem();
	MessageChooserItem messageChooserItem;

	MessageChooserItem GetNewMessageChooserItem()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var bill = header.Bills.AddNew();
		var chooser = new MessageChooser(header, new ISelectionItem[] { bill }, true);
		return chooser.ChooserItems[0];
	}
}
