using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(EUH7PackDetailsUserControl))]
	public class EUH7PackDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (var form = new ZForm(header))
			using (var control = new EUH7PackDetailsUserControl())
			{
				control.SetDataBinding(header, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var detailsLayout = control.FindSingle<DynamicLayoutPanel>("DynamicPackDetailsPanel");
				Assert("Details panel should be visible", detailsLayout.Visible);
			}
		}
	}
}
