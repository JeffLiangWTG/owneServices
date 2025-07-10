using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing
{
	public class CusTempStorageRegLineItemDetailsLayoutControlTest : TestCaseWithFactory
	{
		public void TestItemsPanel()
		{
			using var control = new CusTempStorageRegLineItemDetailsLayoutControl();
			AssertType<DynamicLayoutPanel>(control.ItemsPanel);
		}
	}
}
