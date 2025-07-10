using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TransitLineWrapperTest : NctsSADLineCommonWrapperTest<TransitLineWrapper>
{
	public void TestDeclarationType()
	{
		AssertEquals(nameof(lineWrapper.DeclarationType), ZString.Empty, lineWrapper.DeclarationType);

		goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
		AssertEquals(nameof(lineWrapper.DeclarationType), NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure, lineWrapper.DeclarationType);
	}

	public void TestDispatchCountryCode()
	{
		AssertEquals(nameof(lineWrapper.DispatchCountryCode), ZString.Empty, lineWrapper.DispatchCountryCode);

		goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.Germany;
		AssertEquals(nameof(lineWrapper.DispatchCountryCode), Core.Constants.CountryCodes.Germany, lineWrapper.DispatchCountryCode);
	}

	protected override TransitLineWrapper GetNewLineWrapper(NctsDepartureCargoDesc goodsItem) => new TransitLineWrapper(goodsItem);

	protected override Type ExpectedSpecialMentionGroup => typeof(TransitLineSpecialMentionGroupWrapper);
}
