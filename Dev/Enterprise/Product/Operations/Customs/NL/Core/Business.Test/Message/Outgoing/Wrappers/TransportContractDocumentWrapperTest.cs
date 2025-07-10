using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class TransportContractDocumentWrapperTest : DataProviderTestCase<TransportContractDocumentWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new TransportContractDocumentWrapper(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, wrapper.SequenceNumeric);
	}

	public void TestId()
	{
		additionalInfo.CSI_ReferenceNumber = "TRAREF1";
		AssertEquals("TRAREF1", wrapper.Id);
	}

	public void TestTypeCode()
	{
		additionalInfo.CSI_Code = "TRACOD1";
		AssertEquals("TRACOD1", wrapper.TypeCode);
	}

	protected override void SetUp()
	{
		base.SetUp();
		additionalInfo = Factory.New<Declaration.AdditionalInfo>();
		wrapper = new TransportContractDocumentWrapper(additionalInfo, 1);
	}
	Declaration.AdditionalInfo additionalInfo;
	TransportContractDocumentWrapper wrapper;

	protected override TransportContractDocumentWrapper GetProvider() => wrapper;
}
