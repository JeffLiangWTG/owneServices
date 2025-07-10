using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module.Testing
{
	class CustomsOfficeFilterGUIProviderTest : TestCaseWithFactory
	{
		public void TestHandlesCustomsOffice()
		{
			using (ZFilterStrip strip = new ZFilterStrip())
			{
				ZBindingSource source = new ZBindingSource(strip, typeof(CustomsOfficeFilter));

				Control[] controls = CustomsOfficeFilterGUIProvider.GetCustomsOfficeFilterControls(strip, source);
				AssertEquals("Should provide some controls", true, controls.Length > 0);

				foreach (Control control in controls)
				{
					control.Dispose();
				}
			}
		}
	}
}
