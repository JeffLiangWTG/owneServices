using System;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(TransportDocumentWrapper))]
sealed class TransportDocumentWrapperTest : DataProviderTestCase<TransportDocumentWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new TransportDocumentWrapper(null));
	}

	public void TestDocumentType()
	{
		AssertEquals(nameof(TransportDocumentWrapper.DocumentType), "00300", Provider.DocumentType);
	}

	public void TestReferenceNumber()
	{
		AssertEquals(nameof(TransportDocumentWrapper.ReferenceNumber), "123456", Provider.ReferenceNumber);
	}

	protected override TransportDocumentWrapper GetProvider()
	{
		var transportDocument = Factory.New<AdditionalInfo>();
		transportDocument.CSI_Code = "00300";
		transportDocument.CSI_ReferenceNumber = "123456";

		return new TransportDocumentWrapper(transportDocument);
	}
}
