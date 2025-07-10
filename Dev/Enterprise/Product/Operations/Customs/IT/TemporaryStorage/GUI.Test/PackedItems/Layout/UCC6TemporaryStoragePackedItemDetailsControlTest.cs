using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

sealed class UCC6TemporaryStoragePackedItemDetailsControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new UCC6TemporaryStoragePackedItemDetailsControl())
		{
			CombineAssertions(() =>
			{
				AssertNotNull(control.FindSingle<ZTextBox>("RegistrationNoTextBox"));
				AssertNotNull(control.FindSingle<ZDateEdit>("ReleaseDateEdit"));
			});
		}
	}
}
