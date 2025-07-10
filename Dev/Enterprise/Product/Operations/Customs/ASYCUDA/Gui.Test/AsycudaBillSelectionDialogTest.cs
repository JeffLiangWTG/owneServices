using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	[TestedType(typeof(AsycudaItemSelectionDialog))]
	sealed class AsycudaBillSelectionDialogTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "BILL1";
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL2";
			Factory.Save();
			var result = new AsycudaItemSelectionDialog(new MessageChooser(header, new[] { bill1, bill2 }, true), "Bills");
			((IBusinessObjectState)result.BusinessEntity).ClearHasChangesIncludingChildren();
			return result;
		}
	}
}
