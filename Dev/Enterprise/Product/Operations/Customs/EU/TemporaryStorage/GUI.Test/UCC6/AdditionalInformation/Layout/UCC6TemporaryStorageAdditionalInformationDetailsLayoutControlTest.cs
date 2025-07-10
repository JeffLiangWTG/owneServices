using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	public class UCC6TemporaryStorageAdditionalInformationDetailsLayoutControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new UCC6TemporaryStorageAdditionalInformationDetailsLayoutControl())
			{
				AssertNotNull(control.FindSingle<DynamicLayoutPanel>("DetailsPanel"));
			}
		}
	}
}
