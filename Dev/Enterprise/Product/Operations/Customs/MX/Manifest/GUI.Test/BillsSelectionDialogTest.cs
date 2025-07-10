using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.MX.Manifest.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.GUI.Testing
{
	[TestedType(typeof(BillsSelectionDialog))]
	class BillsSelectionDialogBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "BILL1";

			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillNumber = "BILL2";

			Factory.Save();

			var result = new BillsSelectionDialog(new MXMessageChooser(header, new[] { bill1, bill2 }, MessageSubTypeCodes.Codes.Original), "Bills");
			((IBusinessObjectState)result.BusinessEntity).ClearHasChangesIncludingChildren();

			return result;
		}
	}
}

