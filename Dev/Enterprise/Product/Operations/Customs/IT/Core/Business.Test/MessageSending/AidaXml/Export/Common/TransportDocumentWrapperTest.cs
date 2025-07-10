using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export.Testing;

sealed class TransportDocumentWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new TransportDocumentWrapper(null));
	}

	public void TestDocumentType()
	{
		var wrapper = GetNewTransportDocumentWrapper();
		AssertEquals(nameof(ITransportDocument.DocumentType), "", wrapper.DocumentType);

		transportDocument.CSI_Code = "00300";
		wrapper = GetNewTransportDocumentWrapper();
		AssertEquals(nameof(ITransportDocument.DocumentType), "00300", wrapper.DocumentType);
	}

	public void TestReferenceNumber()
	{
		var wrapper = GetNewTransportDocumentWrapper();
		AssertEquals(nameof(ITransportDocument.ReferenceNumber), "", wrapper.ReferenceNumber);

		transportDocument.CSI_ReferenceNumber = "123456";
		wrapper = GetNewTransportDocumentWrapper();
		AssertEquals(nameof(ITransportDocument.ReferenceNumber), "123456", wrapper.ReferenceNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		transportDocument = Factory.New<AdditionalInfo>();
	}

	AdditionalInfo transportDocument;

	ITransportDocument GetNewTransportDocumentWrapper() => new TransportDocumentWrapper(transportDocument);
}
