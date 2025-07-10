using System;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class AdditionalInformationWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => new AdditionalInformationWrapper(null));
	}

	public void TestCode()
	{
		var additionalInformation = GetNewAdditionalInformation();
		AssertEquals(nameof(IAdditionalInformation.Code), "", additionalInformation.Code);

		additionalInfo.CSI_Code = "00300";
		additionalInformation = GetNewAdditionalInformation();
		AssertEquals(nameof(IAdditionalInformation.Code), "00300", additionalInformation.Code);
	}

	public void TestDescription()
	{
		var additionalInformation = GetNewAdditionalInformation();
		AssertEquals(nameof(IAdditionalInformation.Description), "", additionalInformation.Description);

		additionalInfo.CSI_Description = "SPEDITORE - IDENTITÀ TRA DICHIARANTE E SPEDITORE";
		additionalInformation = GetNewAdditionalInformation();
		AssertEquals(nameof(IAdditionalInformation.Description), "SPEDITORE - IDENTITÀ TRA DICHIARANTE E SPEDITORE", additionalInformation.Description);
	}

	protected override void SetUp()
	{
		base.SetUp();

		additionalInfo = Factory.New<AdditionalInfo>();
	}

	AdditionalInfo additionalInfo;

	IAdditionalInformation GetNewAdditionalInformation() => new AdditionalInformationWrapper(additionalInfo);
}
