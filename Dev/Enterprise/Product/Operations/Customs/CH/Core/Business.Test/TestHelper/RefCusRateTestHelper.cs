using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

internal class RefCusRateTestHelper
{
	internal RefCusRateTestHelper(BusinessObjectFactory factory)
	{
		Factory = factory;
	}
	BusinessObjectFactory Factory { get; }

	const string Datagrouping = Core.Constants.CountryCodes.Switzerland;
	const string OtherDatagrouping = Core.Constants.CountryCodes.Germany;

	internal const string ValidFeeRateCode = "280";
	internal const string InvalidFeeRateCode = "800";

	internal void CreateFeeRateCodes()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var rateType = helper.CreateNewOrGetExistingRateType(Datagrouping, RateTypes.AdditionalFees);
		helper.CreateCusRateCode(Factory, ValidFeeRateCode, rateType.PK, description: nameof(ValidFeeRateCode));
		helper.CreateCusRateCode(Factory, FeeRateCodes.CustomsReliefControlTax, rateType.PK, description: nameof(FeeRateCodes.CustomsReliefControlTax));
		var otherDatagroupingRateType = helper.CreateNewOrGetExistingRateType(OtherDatagrouping, RateTypes.AdditionalFees);
		helper.CreateCusRateCode(Factory, InvalidFeeRateCode, otherDatagroupingRateType.PK);
		Factory.Save();
	}
}
