using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	sealed class AsycudaItemSelectionDialogTest : TestCaseWithFactory
	{
		public void TestSendManifest_SelectedItemTextOnDialog()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_GoodsDescription = "Bill1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_GoodsDescription = "Bill2";
			var bill3 = header.Bills.AddNew();
			bill3.ABL_GoodsDescription = "Bill3";
			var bill4 = header.Bills.AddNew();
			bill4.ABL_GoodsDescription = "Bill4";

			header.AMA_ManifestType = "IAM";
			header.AMA_CustomsOffice = "X";

			var billCountries = header.Bills.OfType<AsycudaBill>().Where(x => x != null).ToArray();

			using (var dlg = new AsycudaItemSelectionDialog(new MessageChooser(header, billCountries, true), "BillsForManifestTest"))
			{
				dlg.SelectOnlyBillNodes_ForTestOnly(bizoPK => bizoPK == bill1.PK);
				AssertEquals("1 Selected", "1 of 4 bill(s) selected.", dlg.SeletedItem);
				dlg.SelectOnlyBillNodes_ForTestOnly(bizoPK => (bizoPK == bill2.PK || bizoPK == bill1.PK));
				AssertEquals("2 Selected", "2 of 4 bill(s) selected.", dlg.SeletedItem);
			}
		}
	}
}
