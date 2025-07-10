using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

sealed class AsycudaContainerValidationTest : TestCaseWithFactory
{
	public void TestCheckACN_IsShipperOwned()
	{
		const string error = "The entered Container Type does not have an Equipment Size Type Code.";

		var containerType = Factory.New<RefContainer>();
		containerType.RC_Code = "CT1";

		var containerTypeWithSizeType = Factory.New<RefContainer>();
		containerTypeWithSizeType.RC_Code = "CT2";
		containerTypeWithSizeType.RC_ISOEquipmentSizeTypeCode = "ABC";

		Factory.Save();

		var container = Factory.New<AsycudaContainer>();
		container.ACN_RC_ContainerType = containerType.PK;
		var propertyInfo = container.ACN_RC_ContainerTypeInfo;
		AssertHasError(propertyInfo, error);

		container.ACN_RC_ContainerType = containerTypeWithSizeType.PK;
		AssertNoError(propertyInfo, error);
	}
}
