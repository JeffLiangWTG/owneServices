using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	sealed class SupplementaryDeclarantUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			var bill = header.Bills.AddNew();

			using (var form = new ZForm(bill))
			using (var control = new SupplementaryDeclarantUserControl())
			{
				control.SetDataBinding(bill, string.Empty);

				form.Controls.Add(control);
				form.Show();

				EUICS2GUITestHelper.AssertGridLayout(control, "supplementaryDeclarantGrid", "CY_Data", "CY_Code");
			}
		}
	}
}
