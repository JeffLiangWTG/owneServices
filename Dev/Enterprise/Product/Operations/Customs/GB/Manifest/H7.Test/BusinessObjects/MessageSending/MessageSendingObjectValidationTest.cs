using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	public class MessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckMessageType()
		{
			var sendingObject = new MessageSendingObject(Factory.New<AsycudaBill>());
			sendingObject.Action = H7EDIMessageTypeList.Codes.ArrivalNotification;

			sendingObject.Validation.ValidateAction();
			AssertHasError(sendingObject.ActionInfo, "An arrival notification cannot be sent without a Movement Reference Number (MRN)");

			sendingObject.Action = H7EDIMessageTypeList.Codes.CancelDeclaration;
			sendingObject.Validation.ValidateAction();
			AssertHasError(sendingObject.ActionInfo, "A cancel declaration cannot be sent without a Movement Reference Number (MRN)");
		}

		public void TestCheckAmedmentReasonCode()
		{
			var sendingObject = new MessageSendingObject(Factory.New<AsycudaBill>());
			sendingObject.Action = H7EDIMessageTypeList.Codes.CancelDeclaration;

			sendingObject.Validation.ValidateAmendmentReasonCode();
			AssertHasMessageErrorContaining(sendingObject.AmendmentReasonCodeInfo, "Amendment reason code must be entered when canceling a declaration");
			AssertHasMessageErrorContaining(sendingObject.AmendmentReasonCodeInfo, "This entry is not applicable to a cancellation because it does not have valid MRN or cancellation code");

			sendingObject.AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.C_GoodsPresentationNotice;
			sendingObject.Validation.ValidateAmendmentReasonCode();
			AssertHasMessageError(sendingObject.AmendmentReasonCodeInfo, "This entry is not applicable to a cancellation because it does not have valid MRN or cancellation code");
		}

		public void TestCheckAmendmentReason()
		{
			var sendingObject = new MessageSendingObject(Factory.New<AsycudaBill>());
			sendingObject.Action = H7EDIMessageTypeList.Codes.CancelDeclaration;

			sendingObject.Validation.ValidateAmendmentInvalidationReason();
			AssertHasError(sendingObject.AmendmentInvalidationReasonInfo, "Amendment reason must be entered when canceling a declaration");
		}

		public void TestCheckQueryByReferenceType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var sendingObject = new MessageSendingObject(bill);
			sendingObject.Action = H7EDIMessageTypeList.Codes.QueryDeclaration;
			sendingObject.QueryType = string.Empty;
			AssertHasError(sendingObject.QueryTypeInfo, "Please enter a Query By Reference.");

			sendingObject.QueryType = "123";
			AssertHasError(sendingObject.QueryTypeInfo, "Enter a valid Query By Reference.");

			AssertQueryTypeHasError(sendingObject, H7QueryTypeList.Codes.MRNSummary);
			AssertQueryTypeHasError(sendingObject, H7QueryTypeList.Codes.MRNSnapshot);
			bill.MovementReferenceNumber = "MRN123";
			AssertQueryTypeHasNoError(sendingObject, H7QueryTypeList.Codes.MRNSummary);
			AssertQueryTypeHasNoError(sendingObject, H7QueryTypeList.Codes.MRNSnapshot);

			AssertQueryTypeHasError(sendingObject, H7QueryTypeList.Codes.DUCR);
			var document = bill.PreviousDocuments.AddNew();
			document.CSI_Code = "740";
			AssertQueryTypeHasError(sendingObject, H7QueryTypeList.Codes.DUCR);
			document.CSI_Code = "DCR";
			AssertQueryTypeHasNoError(sendingObject, H7QueryTypeList.Codes.DUCR);

			AssertQueryTypeHasError(sendingObject, H7QueryTypeList.Codes.UCR);
			bill.ABL_UCRNumber = "UCR123";
			AssertQueryTypeHasNoError(sendingObject, H7QueryTypeList.Codes.UCR);
		}

		void AssertQueryTypeHasError(MessageSendingObject sendingObject, string referenceType)
		{
			sendingObject.QueryType = referenceType;
			AssertHasError(sendingObject.QueryTypeInfo, $"This entry does not have a value for its {referenceType}");
		}

		void AssertQueryTypeHasNoError(MessageSendingObject sendingObject, string referenceType)
		{
			sendingObject.QueryType = referenceType;
			AssertNoErrors(sendingObject.QueryTypeInfo);
		}
	}
}
