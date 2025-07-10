using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class EDIMessageExtensionTest : TestCaseWithFactory
{
	public void TestSignatureRequiresAmendmentMetadata_WhenMessageTypeIsAmendmentAndApplicationReferenceIsTemporaryStorage_ReturnsTrue()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = EDIMessageTypeList.Codes.Amendment;
		message.EM_ApplicationReference = EDIMessageApplicationReferenceList.Codes.TemporaryStorage;

		var result = message.SignatureRequiresAmendmentMetadata();

		AssertEquals("Should return true when EM_MessageType is Amendment and EM_ApplicationReference is TemporaryStorage", true, result);
	}

	public void TestSignatureRequiresAmendmentMetadata_WhenMessageTypeIsNotAmendment_ReturnsFalse()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = EDIMessageTypeList.Codes.NewDeclaration;
		message.EM_ApplicationReference = EDIMessageApplicationReferenceList.Codes.TemporaryStorage;

		var result = message.SignatureRequiresAmendmentMetadata();

		AssertEquals("Should return false when EM_MessageType is not Amendment", false, result);
	}

	public void TestSignatureRequiresAmendmentMetadata_WhenApplicationReferenceIsNotTemporaryStorage_ReturnsFalse()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = EDIMessageTypeList.Codes.Amendment;
		message.EM_ApplicationReference = EDIMessageApplicationReferenceList.Codes.Import;

		var result = message.SignatureRequiresAmendmentMetadata();

		AssertEquals("Should return false when EM_ApplicationReference is not TemporaryStorage", false, result);
	}

	public void TestSignatureRequiresAmendmentMetadata_WhenBothConditionsNotMet_ReturnsFalse()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = EDIMessageTypeList.Codes.Cancellation;
		message.EM_ApplicationReference = EDIMessageApplicationReferenceList.Codes.Export;

		var result = message.SignatureRequiresAmendmentMetadata();

		AssertEquals("Should return false when neither condition is met", false, result);
	}

	public void TestSignatureRequiresAmendmentMetadata_WhenMessageTypeIsNull_ReturnsFalse()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = null;
		message.EM_ApplicationReference = EDIMessageApplicationReferenceList.Codes.TemporaryStorage;

		var result = message.SignatureRequiresAmendmentMetadata();

		AssertEquals("Should return false when EM_MessageType is null", false, result);
	}

	public void TestSignatureRequiresAmendmentMetadata_WhenApplicationReferenceIsNull_ReturnsFalse()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = EDIMessageTypeList.Codes.Amendment;
		message.EM_ApplicationReference = null;

		var result = message.SignatureRequiresAmendmentMetadata();

		AssertEquals("Should return false when EM_ApplicationReference is null", false, result);
	}

	public void TestIsCancellationRequest_WhenMessageTypeIsCancellation_ReturnsTrue()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = EDIMessageTypeList.Codes.Cancellation;

		var result = message.IsCancellationRequest();

		AssertEquals("Should return true when EM_MessageType is Cancellation", true, result);
	}

	public void TestIsCancellationRequest_WhenMessageTypeIsNotCancellation_ReturnsFalse()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = EDIMessageTypeList.Codes.Amendment;

		var result = message.IsCancellationRequest();

		AssertEquals("Should return false when EM_MessageType is not Cancellation", false, result);
	}

	public void TestIsCancellationRequest_WhenMessageTypeIsNull_ReturnsFalse()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_MessageType = null;

		var result = message.IsCancellationRequest();

		AssertEquals("Should return false when EM_MessageType is null", false, result);
	}
}
