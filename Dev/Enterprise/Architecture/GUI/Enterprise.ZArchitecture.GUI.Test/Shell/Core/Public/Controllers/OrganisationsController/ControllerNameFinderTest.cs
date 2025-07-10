using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ControllerNameFinderTest : TestCaseWithFactory
	{
		public void TestGetControllerName()
		{
			var header = Factory.New<IOrgHeader>();
			var finder = new ControllerNameFinder();
			AssertEquals("Organisation", finder.GetControllerNameForType(header.GetType()));
		}
	}
}
