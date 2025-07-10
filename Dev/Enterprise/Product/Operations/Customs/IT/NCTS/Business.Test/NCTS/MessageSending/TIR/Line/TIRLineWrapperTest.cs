using System;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class TIRLineWrapperTest : NctsSADLineCommonWrapperTest<TIRLineWrapper>
{
	public void TestDeclarationType()
	{
		AssertEquals($"{nameof(lineWrapper.DeclarationType)} must be empty", ZString.Empty, lineWrapper.DeclarationType);
	}

	public void TestDispatchCountryCode()
	{
		AssertEquals($"{nameof(lineWrapper.DispatchCountryCode)} must be empty", ZString.Empty, lineWrapper.DispatchCountryCode);
	}

	protected override TIRLineWrapper GetNewLineWrapper(NctsDepartureCargoDesc goodsItem) => new TIRLineWrapper(goodsItem);

	protected override Type ExpectedSpecialMentionGroup => typeof(NctsSADLineSpecialMentionGroupWrapper);
}
