using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	public class UCC6TemporaryStoragePackagesControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new UCC6TemporaryStoragePackagesControl())
			{
				var tabControl = control.FindSingle<ZTemplateTabControl>("PackTabControl");
				AssertNotNull(tabControl);

				AssertEquals("Tab Pages count", 1, tabControl.TabCount);

				AssertContainsExactElementsInExactOrder("Tab Pages names", new[]
				{
					"PackDetailsTabPage"
				}, tabControl.TabPages.ToList<ZTabPage>().Select(x => x.Name));
			}
		}

		public void TestPackDetailsLayoutPanel()
		{
			using (var control = new UCC6TemporaryStoragePackagesControl())
			{
				var tabControl = control.FindSingle<DynamicLayoutPanel>("PackDetailsLayoutPanel");
				AssertNotNull(tabControl);
			}
		}
	}
}
