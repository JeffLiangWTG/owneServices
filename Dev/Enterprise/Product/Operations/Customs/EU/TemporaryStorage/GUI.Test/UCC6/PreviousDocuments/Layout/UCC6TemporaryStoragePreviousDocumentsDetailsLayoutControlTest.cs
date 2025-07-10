using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class UCC6TemporaryStoragePreviousDocumentsDetailsLayoutControlTest : TestCaseWithFactory
	{
		public void TestFieldsLayoutHost()
		{
			using (var control = new UCC6TemporaryStoragePreviousDocumentsDetailsLayoutControl())
			{
				AssertType<DynamicLayoutPanel>(control.FindSingle<DynamicLayoutPanel>("DetailsPanel"));
			}
		}
	}
}
