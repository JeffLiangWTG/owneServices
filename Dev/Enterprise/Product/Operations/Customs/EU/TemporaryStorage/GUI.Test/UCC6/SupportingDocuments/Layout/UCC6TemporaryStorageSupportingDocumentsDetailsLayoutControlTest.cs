using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	sealed class UCC6TemporaryStorageSupportingDocumentsDetailsLayoutControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new UCC6TemporaryStorageSupportingDocumentsDetailsLayoutControl())
			{
				AssertNotNull(control.FindSingle<DynamicLayoutPanel>("DetailsPanel"));
			}
		}
	}
}
