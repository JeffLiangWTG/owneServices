using System;
using CargoWise.Customs.IT.MessageContracts.NCTS.Departure;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class SupportingDocumentWrapperTest : SupportingDocumentBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new SupportingDocumentWrapper(null));
		AssertNoExceptionThrown("when argument is not null", () => new SupportingDocumentWrapper(supportingDocument));
	}

	public override void TestDocumentType()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(ISupportingDocument.DocumentType), wrapper.DocumentType);

		supportingDocument.CSI_Code = "ABC";
		wrapper = CreateWrapper();
		AssertEquals(nameof(ISupportingDocument.DocumentType), "ABC", wrapper.DocumentType);
	}

	public override void TestReferenceNumber()
	{
		var wrapper = CreateWrapper();
		AssertEquals("-", wrapper.ReferenceNumber);

		wrapper = CreateWrapper("2023", "", "");
		AssertEquals(nameof(ISupportingDocument.ReferenceNumber), "2023", wrapper.ReferenceNumber);
		wrapper = CreateWrapper("", "IT", "");
		AssertEquals(nameof(ISupportingDocument.ReferenceNumber), "IT", wrapper.ReferenceNumber);
		wrapper = CreateWrapper("", "", "REF1");
		AssertEquals(nameof(ISupportingDocument.ReferenceNumber), "REF1", wrapper.ReferenceNumber);

		wrapper = CreateWrapper("", "IT", "REF1");
		AssertEquals(nameof(ISupportingDocument.ReferenceNumber), "IT-REF1", wrapper.ReferenceNumber);

		wrapper = CreateWrapper("2023", "IT", "REF1");
		AssertEquals(nameof(ISupportingDocument.ReferenceNumber), "2023-IT-REF1", wrapper.ReferenceNumber);

		wrapper = CreateWrapper("2023", "IT", "");
		AssertEquals(nameof(ISupportingDocument.ReferenceNumber), "2023-IT", wrapper.ReferenceNumber);

		wrapper = CreateWrapper("2023", "", "REF1");
		AssertEquals(nameof(ISupportingDocument.ReferenceNumber), "2023-REF1", wrapper.ReferenceNumber);
	}

	public override void TestItemNumber()
	{
		var wrapper = CreateWrapper();
		AssertEquals(nameof(ISupportingDocument.ItemNumber), 0, wrapper.ItemNumber);

		supportingDocument.CSI_ItemNumber = 3;
		wrapper = CreateWrapper();
		AssertEquals(nameof(ISupportingDocument.ItemNumber), 3, wrapper.ItemNumber);
	}

	public override void TestComplementOfInformation()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(ISupportingDocument.ComplementOfInformation), wrapper.ComplementOfInformation);

		supportingDocument.CSI_ReferenceNumber2 = "111";
		wrapper = CreateWrapper();
		AssertEquals(nameof(ISupportingDocument.ComplementOfInformation), "111", wrapper.ComplementOfInformation);
	}

	protected override void SetUp()
	{
		base.SetUp();
		supportingDocument = Factory.New<NctsSupportingDocument>();
	}

	protected override ISupportingDocument CreateWrapper() => new SupportingDocumentWrapper(supportingDocument);

	ISupportingDocument CreateWrapper(string yearOfIssue, string countryCode, string referenceNumber)
	{
		var supportingDocument = Factory.New<NctsSupportingDocument>();
		supportingDocument.CSI_YearOfIssue = yearOfIssue;
		supportingDocument.CSI_RN_NKCountryCode = countryCode;
		supportingDocument.CSI_ReferenceNumber = referenceNumber;
		return new SupportingDocumentWrapper(supportingDocument);
	}

	NctsSupportingDocument supportingDocument;
}
