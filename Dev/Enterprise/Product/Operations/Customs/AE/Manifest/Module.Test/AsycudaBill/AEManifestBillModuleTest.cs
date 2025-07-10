using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Module.Testing;

[TestedType(typeof(AEManifestBillModule))]
sealed class AEManifestBillModuleTest : ASYCUDA.Module.Testing.ASYCUDAManifestBillModuleAbstractTest
{
	public override void TestExceptionsFilter()
	{
		Assert("Not available on Manifest Bill module", true);
	}

	public override void TestMilestonesFilter()
	{
		Assert("Not available on Manifest Bill module", true);
	}

	public override void TestAutoAddedMilestoneDateFilter()
	{
		Assert("Not available on Manifest Bill module", true);
	}

	public override void TestAutoAddedTaskStatusFilter()
	{
		Assert("Not available on Manifest Bill module", true);
	}

	public override void TestTasksFilter()
	{
		Assert("Not available on Manifest Bill module", true);
	}

	public override void TestTriggersFilter()
	{
		Assert("Not available on Manifest Bill module", true);
	}

	protected override string CountryCode => Core.Constants.CountryCodes.UnitedArabEmirates;

	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.ASYCUDA.ManifestBill;
}
