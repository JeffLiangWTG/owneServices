using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.CH;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

public class TransportDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCSI_Code()
	{
		RefCusCodeTestHelper.CreateTransportDocumentCodes(Factory);

		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(transportDocument.CSI_CodeInfo, RefCusCodeTestHelper.InvalidTransportDocumentCode, RefCusCodeTestHelper.ValidTransportDocumentCode);
	}

	public void TestCheckCSI_ReferenceNumberIsMandatory() => CombineAssertions(() =>
	{
		transportDocument.Validation.ValidateCSI_ReferenceNumber();
		AssertHasMessageErrorContaining("Reference Number mandatory", transportDocument.CSI_ReferenceNumberInfo, "You have not entered");

		transportDocument.CSI_ReferenceNumber = "1234";
		AssertNoMessageErrorContaining("Reference Number mandatory - entered", transportDocument.CSI_ReferenceNumberInfo, "You have not entered");
	});

	public void TestCheckCSI_ReferenceNumber_NS30065()
	{
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		transportDocument.CSI_Code = TransportDocumentTypeCodes.MAWB;
		string messageInvalidFormat = PassarValidationMessages.MessageNS30065_InvalidFormat;
		string messageInvalidAirline = PassarValidationMessages.MessageNS30065_InvalidAirline;
		string messageInvalidCheckDigit = PassarValidationMessages.MessageNS30065_InvalidCheckdigit(5);
		var allMessages = new[] { messageInvalidFormat, messageInvalidAirline, messageInvalidCheckDigit };
		CombineAssertions(() =>
		{
			transportDocument.CSI_Code = "Test";
			transportDocument.CSI_ReferenceNumber = "1234567890";
			AssertNoMessageErrorContaining("MAWB is Too short but the CSI_Code not N741", transportDocument.CSI_ReferenceNumberInfo, messageInvalidFormat);

			transportDocument.CSI_Code = TransportDocumentTypeCodes.MAWB;
			transportDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining($"The MAWB number is Too short", transportDocument.CSI_ReferenceNumberInfo, messageInvalidFormat);

			transportDocument.CSI_ReferenceNumber = "123456789012";
			AssertHasMessageErrorContaining($"The MAWB number is Too long", transportDocument.CSI_ReferenceNumberInfo, messageInvalidFormat);

			transportDocument.CSI_ReferenceNumber = "12345X78901";
			AssertHasMessageErrorContaining($"The MAWB number contains one or more non-digit characters", transportDocument.CSI_ReferenceNumberInfo, messageInvalidFormat);

			transportDocument.CSI_ReferenceNumber = "33312345675";
			AssertHasMessageErrorContaining($"The MAWB number Unknown airline", transportDocument.CSI_ReferenceNumberInfo, messageInvalidAirline);

			transportDocument.CSI_ReferenceNumber = "22212345679";
			AssertHasMessageErrorContaining($"The MAWB number has invalid check digit", transportDocument.CSI_ReferenceNumberInfo, messageInvalidCheckDigit);

			transportDocument.CSI_ReferenceNumber = "22212345675";
			foreach (var message in allMessages)
			{
				AssertNoMessageErrorContaining("Valid MAWB number", transportDocument.CSI_ReferenceNumberInfo, message);
			}

			declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
			transportDocument.CSI_ReferenceNumber = "22212345679";
			AssertNoMessageErrorContaining($"NS30065 unactive for EDA", transportDocument.CSI_ReferenceNumberInfo, messageInvalidCheckDigit);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		transportDocument = invoiceHeader.TransportDocuments.AddNew();
	}
	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	TransportDocument transportDocument;
}
