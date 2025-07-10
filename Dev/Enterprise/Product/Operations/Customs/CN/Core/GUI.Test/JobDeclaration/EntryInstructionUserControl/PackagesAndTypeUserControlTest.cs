using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(PackagesAndTypeUserControl))]
	class PackagesAndTypeUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExistence()
		{
			using var control = new PackagesAndTypeUserControl();
			TestUtility.AssertControlExistance(control, "PackTypeCodeDropEdit", "CEI_PackageUQ");
			TestUtility.AssertControlExistance(control, "NoOfPacksCalcBox", "CEI_Packages");
		}
	}
}
