using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSAddInfoCusContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestZG_UnitCode()
		{
			var container = Factory.New<EMCSCusContainer>();
			container.AddInfoValidation.ValidateZG_UnitCode();
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("Mandatory", container.ZG_UnitCodeInfo, MandatoryValidation.YouHaveNotEntered);
				container.ZG_UnitCode = "X";
				AssertNoMessageErrorContaining("Value Entered", container.ZG_UnitCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageError("Invalid Value", container.ZG_UnitCodeInfo, ListValidation.InvalidCodeMessageError);
				container.ZG_UnitCode = EMCSTransportUnitCodeList.Codes.Container;
				AssertNoMessageError("Valid Value", container.ZG_UnitCodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}
	}
}
