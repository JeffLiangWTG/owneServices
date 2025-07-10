using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	class CusSupplyChainActorReferenceUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var manifest = Factory.NewWithValidTestData<Business.AsycudaManifestHeader>();

			using (var form = new ZForm(manifest))
			using (var control = new CusSupplyChainActorReferenceUserControl())
			{
				control.SetDataBinding(manifest, string.Empty);

				form.Controls.Add(control);
				form.Show();

				EUICS2GUITestHelper.AssertGridLayout(control, "supplyChainActorReferenceGrid", "CFR_Code", "OwnerOrgPK", "CFR_OA_Owner", "CFR_Reference");
			}
		}
	}
}
