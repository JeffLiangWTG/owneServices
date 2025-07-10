using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class ArrivalNctsGuaranteeValidationTest : BaseNctsGuaranteeValidationTest
{
	public void TestCheckPW_BondType()
	{
		Guarantee.PW_BondType = ZString.Empty;
		AssertNoNotifications(Guarantee.PW_BondTypeInfo);
	}

		protected override string MovementType => Common.EU.NctsMoveHeaderType.Codes.Arrival;
}
