using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsSADLineSpecialMentionInfoAdditionalInformationWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When NctsDepartureCargoDesc parameter is null", () => GetWrapper(null));
		AssertNoExceptionThrown(() => GetWrapper(goodsItem));
	}

	public void TestEmptyAdditionalInformation()
	{
		var emptyGoodsItem = Factory.New<NctsDepartureCargoDesc>();
		wrapper = new NctsSADLineSpecialMentionInfoAdditionalInformationWrapper(emptyGoodsItem);
		CombineAssertions("Check GoodsItem without AdditionalInformation", () =>
		{
			AssertEquals(nameof(wrapper.AdditionalInformation), ZString.Empty, wrapper.AdditionalInformation);
			AssertEquals(nameof(wrapper.AdditionalInformationCoded), ZString.Empty, wrapper.AdditionalInformationCoded);
			AssertEquals(nameof(wrapper.IsExportFromCE), null, wrapper.IsExportFromCE);
			AssertEquals(nameof(wrapper.ExportCountry), ZString.Empty, wrapper.ExportCountry);
		});
	}

	public void TestAdditionalInformation()
	{
		AssertEquals(nameof(wrapper.AdditionalInformation), additionalInfo.CSI_Description, wrapper.AdditionalInformation);
	}

	public void TestAdditionalInformationCoded()
	{
		AssertEquals(nameof(wrapper.AdditionalInformationCoded), additionalInfo.CSI_Code, wrapper.AdditionalInformationCoded);
	}

	public void TestIsExportFromCE()
	{
		AssertEquals(nameof(wrapper.IsExportFromCE), additionalInfo.CSI_NctsExportFromEC, wrapper.IsExportFromCE);
	}

	public void TestExportCountry()
	{
		AssertEquals(nameof(wrapper.ExportCountry), additionalInfo.CSI_RN_NKCountryCode, wrapper.ExportCountry);
	}

	NctsSADLineSpecialMentionInfoAdditionalInformationWrapper GetWrapper(NctsDepartureCargoDesc departureCargoDesc) => new NctsSADLineSpecialMentionInfoAdditionalInformationWrapper(departureCargoDesc);

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.NewDepartureNctsHeaderPhase5();
		goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		additionalInfo = goodsItem.AdditionalInfos.AddNew();
		additionalInfo.CSI_Description = "Description";
		additionalInfo.CSI_Code = "Code";
		additionalInfo.CSI_NctsExportFromEC = true;
		additionalInfo.CSI_RN_NKCountryCode = "IT";
		wrapper = new NctsSADLineSpecialMentionInfoAdditionalInformationWrapper(goodsItem);
	}

	NctsDepartureCargoDesc goodsItem;
	NctsAdditionalInfo additionalInfo;
	NctsSADLineSpecialMentionInfoAdditionalInformationWrapper wrapper;
}
