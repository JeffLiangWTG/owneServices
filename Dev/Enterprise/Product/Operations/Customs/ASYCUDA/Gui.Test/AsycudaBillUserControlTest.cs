using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaBillUserControlTest : TestCaseWithFactory
	{
		public void TestAutoScrollIsSetCorrectly()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();

			using (var form = new ZForm(bill))
			using (var control = new AsycudaBillUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals("AutoScroll", true, control.AutoScroll);
				AssertEquals("control.AutoScrollMinSize.Width", 1163, control.AutoScrollMinSize.Width);
				AssertEquals("control.AutoScrollMinSize.Height", 0, control.AutoScrollMinSize.Height);
			}
		}
	}
}
