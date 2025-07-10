using System;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(AdditionalInformationWrapper))]
sealed class AdditionalInformationWrapperTest : DataProviderTestCase<AdditionalInformationWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new AdditionalInformationWrapper(null));
	}

	public void TestCode()
	{
		AssertEquals(nameof(AdditionalInformationWrapper.Code), "00300", Provider.Code);
	}

	public void TestDescription()
	{
		AssertEquals(nameof(AdditionalInformationWrapper.Description), "123456", Provider.Description);
	}

	protected override AdditionalInformationWrapper GetProvider()
	{
		var additionalInfo = Factory.New<AdditionalInfo>();
		additionalInfo.CSI_Code = "00300";
		additionalInfo.CSI_Description = "123456";

		return new AdditionalInformationWrapper(additionalInfo);
	}
}
