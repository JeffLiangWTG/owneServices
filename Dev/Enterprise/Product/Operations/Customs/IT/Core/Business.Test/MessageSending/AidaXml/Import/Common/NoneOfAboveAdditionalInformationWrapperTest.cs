using CargoWise.Customs.IT.MessageContracts;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class NoneOfAboveAdditionalInformationWrapperTest : TestCase
{
	public void TestCode()
	{
		var noneOfAboveAdditionalInformationWrapper = GetNewAdditionalInformation();
		AssertNull($"{nameof(IAdditionalInformation.Code)} must be null", noneOfAboveAdditionalInformationWrapper.Code);
	}

	public void TestDescription()
	{
		var noneOfAboveAdditionalInformationWrapper = GetNewAdditionalInformation();
		AssertEquals(nameof(IAdditionalInformation.Description), "nessuna delle precedenti", noneOfAboveAdditionalInformationWrapper.Description);
	}

	IAdditionalInformation GetNewAdditionalInformation() => new NoneOfAboveAdditionalInformationWrapper();
}
