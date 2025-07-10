using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class AsycudaContainerValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckACN_ContainerNumber()
	{
		const string errorMessage = "Duplicate Container Number – Please remove duplicate container.";
		var asycudaManifestHeader = Factory.New<AsycudaManifestHeader>();
		var container1 = asycudaManifestHeader.Containers.AddNew();
		container1.ACN_ContainerNumber = "123";
		var container2 = asycudaManifestHeader.Containers.AddNew();
		container2.ACN_ContainerNumber = "123";
		CombineAssertions(() =>
		{
			AssertHasError("ContainerNumbers is duplicated", container2.ACN_ContainerNumberInfo, errorMessage);

			container2.ACN_ContainerNumber = "333";
			AssertNoErrorContaining("ContainerNumbers is unique", container2.ACN_ContainerNumberInfo, errorMessage);
		});
	}

	public void TestCheckACN_GoodsWeight()
	{
		var asycudaManifestHeader = Factory.New<AsycudaManifestHeader>();
		var container = asycudaManifestHeader.Containers.AddNew();

		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(container.ACN_GoodsWeightInfo);
	}

	public void TestCheckACN_NumberOfPackages()
	{
		const string messageError = "Packages cannot be zero for FCL or LCL Container loads";

		var asycudaManifestHeader = Factory.New<AsycudaManifestHeader>();
		var container = asycudaManifestHeader.Containers.AddNew();

		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyHasValue(container.ACN_NumberOfPackagesInfo, container.ACN_EmptyFullIndicatorInfo,
						new IZType[] { (ZString)EmptyFullIndicatorList.Codes.FullContainerLoad, (ZString)EmptyFullIndicatorList.Codes.LessThanFullContainerLoad },
						true,
						messageError);
	}

	public void TestCheckACN_SetPointTemperatureUnit()
	{
		CombineAssertions(() =>
		{
			ValidateTemperatureUnitHasErrors("A", true);
			ValidateTemperatureUnitHasErrors("C", false);
			ValidateTemperatureUnitHasErrors("F", false);
		});
	}

	public void TestCheckACN_Seal1()
	{
		var asycudaManifestHeader = Factory.New<AsycudaManifestHeader>();
		var container = asycudaManifestHeader.Containers.AddNew();
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(container.ACN_Seal1Info, container.ACN_ContainerNumberInfo);
	}

	void ValidateTemperatureUnitHasErrors(string temperatureUnit, bool shouldHaveErrors)
	{
		var asycudaManifestHeader = Factory.New<AsycudaManifestHeader>();
		var container = asycudaManifestHeader.Containers.AddNew();
		container.ACN_SetPointTemperatureUnit = temperatureUnit;

		if (shouldHaveErrors)
		{
			AssertHasErrors("Enter a valid Temperature UQ.", container.ACN_SetPointTemperatureUnitInfo);
		}
		else
		{
			AssertNoErrors(container.ACN_SetPointTemperatureUnitInfo);
		}
	}
}
