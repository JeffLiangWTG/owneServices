using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	public class UCC6TemporaryStoragePackageDetailsControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new UCC6TemporaryStoragePackageDetailsControl())
			{
				AssertNotNull(control.FindSingle<ZGuidDropEditWithFixedWidth>("containerPKGuidDropEditWithFixedWidth"));
				AssertNotNull(control.FindSingle<ZCalcEdit>("packQtyCalcEdit"));
				AssertNotNull(control.FindSingle<ZDropEditWithFixedWidth>("packUQDropEditWithFixedWidth"));
				AssertNotNull(control.FindSingle<ZTextBox>("marksAndNumbersTextBox"));
			}
		}
	}
}
