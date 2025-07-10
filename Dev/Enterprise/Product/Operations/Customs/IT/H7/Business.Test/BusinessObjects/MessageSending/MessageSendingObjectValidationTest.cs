using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(MessageSendingObjectValidation))]
sealed class MessageSendingObjectValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAmendmentReasonCode()
	{
		var sendingObject = new MessageSendingObject(Factory.New<AsycudaBill>());
		sendingObject.Action = ITH7MessageTypes.Codes.H7D;
		sendingObject.AmendmentReasonCode = ZString.Empty;
		sendingObject.Validation.ValidateAmendmentReasonCode();
		AssertNoMessageErrors("Empty Amendment reason code does not trigger validation error", sendingObject.AmendmentReasonCodeInfo);

		sendingObject.Action = ITH7MessageTypes.Codes.H7C;
		sendingObject.Validation.ValidateAmendmentReasonCode();
		AssertHasMessageError("Error when H7C and AmendmentReasonCode is Empty", sendingObject.AmendmentReasonCodeInfo, "You have not entered an Amendment/Cancellation Reason.");

		sendingObject.AmendmentReasonCode = Ucc6CancellationReasonList.Codes.IncorrectRegistration;
		sendingObject.Validation.ValidateAmendmentReasonCode();
		AssertNoMessageErrors("Validation not triggered if H7C and Amendment reason code is entered", sendingObject.AmendmentReasonCodeInfo);

		sendingObject.Action = ITH7MessageTypes.Codes.H7M;
		sendingObject.AmendmentReasonCode = ZString.Empty;
		sendingObject.Validation.ValidateAmendmentReasonCode();
		AssertHasMessageError("Error when H7M and AmendmentReasonCode is Empty", sendingObject.AmendmentReasonCodeInfo, "You have not entered an Amendment/Cancellation Reason.");

		sendingObject.AmendmentReasonCode = AmendmentReasonList.Codes.SubstantialSideRevision;
		sendingObject.Validation.ValidateAmendmentReasonCode();
		AssertNoMessageErrors("Validation not triggered if H7M and Amendment reason code is entered", sendingObject.AmendmentReasonCodeInfo);
	}

	public void TestCheckAmendmentReasonCode_ErrorMessageOnInvalidCode()
	{
		var sendingObject = new MessageSendingObject(Factory.New<AsycudaBill>());
		sendingObject.Action = ITH7MessageTypes.Codes.H7C;
		sendingObject.AmendmentReasonCode = "zz";
		sendingObject.Validation.ValidateAmendmentReasonCode();
		AssertHasMessageErrorContaining(sendingObject.AmendmentReasonCodeInfo, ListValidation.InvalidCodeMessageError);
	}

	public void TestCheckLegislativeReference()
	{
		var sendingObject = new MessageSendingObject(Factory.New<AsycudaBill>());
		sendingObject.Action = ITH7MessageTypes.Codes.H7D;
		sendingObject.LegislativeReference = ZString.Empty;
		sendingObject.Validation.ValidateLegislativeReference();
		AssertNoMessageErrors("Empty Legislative reference does not trigger validation error on H7D", sendingObject.LegislativeReferenceInfo);

		sendingObject.Action = ITH7MessageTypes.Codes.H7C;
		sendingObject.Validation.ValidateLegislativeReference();
		AssertHasMessageError("Error when H7C and Legislative reference is Empty", sendingObject.LegislativeReferenceInfo, "You have not entered a Legislative Reference.");

		sendingObject.LegislativeReference = Ucc6ImportCancellationAndAmendmentLegislativeReferenceList.Codes.CduArt117;
		sendingObject.Validation.ValidateLegislativeReference();
		AssertNoMessageErrors("Validation not triggered if H7M and Legislative Reference is entered", sendingObject.LegislativeReferenceInfo);

		sendingObject.Action = ITH7MessageTypes.Codes.H7M;
		sendingObject.LegislativeReference = ZString.Empty;
		sendingObject.Validation.ValidateLegislativeReference();
		AssertHasMessageError("Error when H7M and Legislative reference is Empty", sendingObject.LegislativeReferenceInfo, "You have not entered a Legislative Reference.");

		sendingObject.LegislativeReference = Ucc6ImportCancellationAndAmendmentLegislativeReferenceList.Codes.CduArt117;
		sendingObject.Validation.ValidateLegislativeReference();
		AssertNoMessageErrors("Validation not triggered if H7M and Legislative Reference is entered", sendingObject.LegislativeReferenceInfo);
	}

	public void TestCheckLegislativeReference_ErrorMessageOnInvalidCode()
	{
		var sendingObject = new MessageSendingObject(Factory.New<AsycudaBill>());
		sendingObject.Action = ITH7MessageTypes.Codes.H7C;
		sendingObject.LegislativeReference = "zz";
		sendingObject.Validation.ValidateLegislativeReference();
		AssertHasMessageErrorContaining(sendingObject.LegislativeReferenceInfo, ListValidation.InvalidCodeMessageError);
	}
}
