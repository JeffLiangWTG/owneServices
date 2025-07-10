using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.TemporaryStorage.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	internal class UCC6TemporaryStorageContainerControlTest : TestCaseWithFactory
	{
		public void TestAdditionalGrid()
		{
			using (var control = new UCC6TemporaryStorageContainerControl())
			{
				var additionalSealsGrid = control.FindSingle<ZGrid>("AdditionalSealsGrid");
				AssertNotNull(additionalSealsGrid);
			}
		}

		public void TestContainerGrid()
		{
			using (var control = new UCC6TemporaryStorageContainerControl())
			{
				var containersGrid = control.FindSingle<ZGrid>("ContainersGrid");
				AssertNotNull(containersGrid);
			}
		}
	}
}
