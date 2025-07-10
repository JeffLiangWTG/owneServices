using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(AUExportTariffBulkChangeForm))]
	sealed class AUExportTariffBulkChangeFormTest : ZFormBasherTest
	{
		public void TestGridId()
		{
			using (var form = (AUExportTariffBulkChangeForm)GetFormToBashCore())
			{
				var grid = (ZGrid)form.Controls.Find("OldTariffsZGrid", true).First();
				AssertEquals("GridLayoutRtMJ/QZn7uvvF+lupXbJ6g==", grid.GridId);
			}
		}

		protected override Form GetFormToBashCore() => new AUExportTariffBulkChangeForm(new AUExportTariffBulkChange(Factory));

		protected override bool AllowSaveOnFormForTestHasChanges => false;
	}
}
