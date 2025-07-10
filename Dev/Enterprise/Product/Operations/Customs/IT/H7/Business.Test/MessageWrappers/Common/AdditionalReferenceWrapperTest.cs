using System;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(AdditionalReferenceWrapper))]
sealed class AdditionalReferenceWrapperTest : DataProviderTestCase<AdditionalReferenceWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new AdditionalReferenceWrapper(null));
	}

	public void TestReferenceType()
	{
		AssertEquals(nameof(AdditionalReferenceWrapper.ReferenceType), "00300", Provider.ReferenceType);
	}

	public void TestReferenceNumber()
	{
		AssertEquals(nameof(AdditionalReferenceWrapper.ReferenceNumber), "123456", Provider.ReferenceNumber);
	}

	protected override AdditionalReferenceWrapper GetProvider()
	{
		var additionalReference = Factory.New<AdditionalInfo>();
		additionalReference.CSI_Code = "00300";
		additionalReference.CSI_ReferenceNumber = "123456";

		return new AdditionalReferenceWrapper(additionalReference);
	}
}
