using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class ComboBoxFilterLayoutRegistryEditorInfoTest : TestCase
	{
		public void TestConstructor()
		{
			ComboBoxFilterLayoutRegistryEditorInfo editorInfo = new ComboBoxFilterLayoutRegistryEditorInfo("TrackingShipments");
			AssertEquals("Module name should be", "TrackingShipments", editorInfo.ModuleName);
		}
	}
}
