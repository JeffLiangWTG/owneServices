using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using ContainerModes = Enterprise.Core.Constants.ContainerModes;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(JobDeclarationHelper))]
sealed class JobDeclarationHelperTest : TestCaseWithFactory
{
	public void TestIsContainerised()
	{
		var containerisedModes = new[] { ContainerModes.Containerised, ContainerModes.FCL, ContainerModes.LCL };

		foreach (var containerMode in typeof(ContainerModes).GetConstantValues())
		{
			AssertEquals(containerMode, containerMode.In(containerisedModes), JobDeclarationHelper.IsContainerised(containerMode));
		}
	}

	public void TestGetConvertedPackType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_TotalNoOfPacks = 10;
		declaration.JE_TotalNoOfPacksPackType = "BAG";
		declaration.JE_HouseBill = "HB1";
		var packingGroup = declaration.PrimaryHouseBill.PackingGroups[0];
		var package = packingGroup.Packages[0];
		AssertEquals(10, package.CW_PackQty);
		AssertEquals("BG", package.CW_PackType);

		declaration.JE_TotalNoOfPacksPackType = "CS";
		packingGroup = declaration.PrimaryHouseBill.PackingGroups[0];
		package = packingGroup.Packages[0];
		AssertEquals(10, package.CW_PackQty);
		AssertEquals("CS", package.CW_PackType);

		declaration.JE_TotalNoOfPacksPackType = "G";
		packingGroup = declaration.PrimaryHouseBill.PackingGroups[0];
		package = packingGroup.Packages[0];
		AssertEquals(10, package.CW_PackQty);
		AssertEquals("PK", package.CW_PackType);
	}
}
