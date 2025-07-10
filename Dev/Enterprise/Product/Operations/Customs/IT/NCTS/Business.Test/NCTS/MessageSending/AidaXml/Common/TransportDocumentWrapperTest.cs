using System;
using CargoWise.Customs.IT.MessageContracts;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml.Testing;

sealed class TransportDocumentWrapperTest : TransportDocumentBase
{
	public override void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new TransportDocumentWrapper(null));
		AssertNoExceptionThrown("when argument is not null", () => new TransportDocumentWrapper(additionalDocument));
	}

	public override void TestReferenceNumber()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(ITransportDocument.ReferenceNumber), wrapper.ReferenceNumber);

		additionalDocument.CSI_ReferenceNumber = "123";
		wrapper = CreateWrapper();
		AssertEquals(nameof(ITransportDocument.ReferenceNumber), "123", wrapper.ReferenceNumber);
	}

	public override void TestDocumentType()
	{
		var wrapper = CreateWrapper();
		AssertNullOrEmpty(nameof(ITransportDocument.DocumentType), wrapper.DocumentType);

		additionalDocument.CSI_Code = "ABC";
		wrapper = CreateWrapper();
		AssertEquals(nameof(ITransportDocument.DocumentType), "ABC", wrapper.DocumentType);
	}

	protected override void SetUp()
	{
		base.SetUp();
		additionalDocument = Factory.New<NctsBillAdditionalDocument>();
	}

	protected override ITransportDocument CreateWrapper() => new TransportDocumentWrapper(additionalDocument);

	NctsBillAdditionalDocument additionalDocument;
}
