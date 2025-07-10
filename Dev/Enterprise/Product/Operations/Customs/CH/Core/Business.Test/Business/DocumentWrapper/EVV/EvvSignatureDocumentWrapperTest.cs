using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(EvvSignatureDocumentWrapper))]
internal class EvvSignatureDocumentWrapperTest : NonPersistentBusinessObjectTestCase
{
	public void TestDocumentType()
	{
		RefCusCodeTestHelper.CreateTransportationTypeList(Factory);

		var document = CreateEVVSignatureWrapper(TestingData.InputEvvResponseAllDTY());

		AssertEquals("Document Type", "taxationDecisionCustomsDuties", document.DocumentType);
	}

	[TestDate(2024, 1, 24)]
	public void TestSignatureValidationResult()
	{
		CombineAssertions(() =>
		{
			var document = CreateEVVSignatureWrapper(TestingData.InputEvvResponseVATValidSignature);
			AssertEquals("Signature Validation Result", "OK", document.SignatureValidationText);
			AssertEquals("Validation Summary Result", "OK", document.ValidationSummaryText);

			document = CreateEVVSignatureWrapper(TestingData.InputEvvResponseVATInvalidSignature);
			AssertEquals("Signature Validation Result", "not OK", document.SignatureValidationText);
			AssertEquals("Validation Summary Result", "not OK", document.ValidationSummaryText);
		});
	}

	[TestDate(2024, 1, 24)]
	public void TestCertificateDateValidationResult()
	{
		CombineAssertions(() =>
		{
			var document = CreateEVVSignatureWrapper(TestingData.InputEvvResponseVATValidSignature);
			AssertEquals("Certificate Date Validation Result", "OK", document.CertificateDateValidationText);
			AssertEquals("Validation Summary Result", "OK", document.ValidationSummaryText);

			document = CreateEVVSignatureWrapper(TestingData.InputEvvResponseVATExpiredCertificate);
			AssertEquals("Certificate Date Validation Result", "not OK", document.CertificateDateValidationText);
			AssertEquals("Validation Summary Result", "not OK", document.ValidationSummaryText);
		});
	}

	[TestDate(2024, 1, 24)]
	public void TestCertificateRevocationListValidationResult()
	{
		CombineAssertions(() =>
		{
			var document = CreateEVVSignatureWrapper(TestingData.InputEvvResponseVATValidSignature);
			AssertEquals("Certificate Revocation List Result", "OK", document.CertificateRevocationListValidationText);
			AssertEquals("Validation Summary Result", "OK", document.ValidationSummaryText);

			document = CreateEVVSignatureWrapper(TestingData.InputEvvResponseVATInvalidSwissCustomsCertificate);
			AssertEquals("Certificate Revocation List Result", "not OK", document.CertificateRevocationListValidationText);
			AssertEquals("Validation Summary Result", "not OK", document.ValidationSummaryText);
		});
	}

	[TestDate(2024, 1, 24)]
	public void TestCertificateChainValidationResult()
	{
		var document = CreateEVVSignatureWrapper(TestingData.InputEvvResponseVATValidSignature);
		AssertEquals("Certificate Chain Validation Result", "OK", document.CertificateChainValidationText);
		AssertEquals("Validation Summary Result", "OK", document.ValidationSummaryText);

		document = CreateEVVSignatureWrapper(TestingData.InputEvvResponseVATInvalidSwissCustomsCertificate);
		AssertEquals("Certificate Chain Validation Result", "not OK", document.CertificateChainValidationText);
		AssertEquals("Validation Summary Result", "not OK", document.ValidationSummaryText);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return CreateEVVSignatureWrapper(TestingData.InputEvvResponseVATValidSignature);
	}

	EvvSignatureDocumentWrapper CreateEVVSignatureWrapper(ZString messageText)
	{
		return EvvSignatureDocumentWrapper.New(CreateEDIMessage(messageText), Factory);
	}

	CHEDIMessage CreateEDIMessage(ZString messageText)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var message = (CHEDIMessage)entryHeader.Messages.AddNew();
		message.EM_MessageText = messageText;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageType = MessageTypeCodeList.Codes.EVV;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		return message;
	}
}
