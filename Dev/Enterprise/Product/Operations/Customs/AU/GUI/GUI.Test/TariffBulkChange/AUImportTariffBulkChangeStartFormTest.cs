using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(AUImportTariffBulkChangeStartForm))]
	sealed class AUImportTariffBulkChangeStartFormTest : ZFormBasherTest
	{
		public void TestTariffUpdateUserFileZButtonClick()
		{
			using (var tempFile = TempFile.New())
			{
				var bizo = new AUImportTariffBulkChange(Factory);
				using (var form = new AUImportTariffBulkChangeStartForm(bizo))
				{
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					form.TariffUpdateUserFileZButton.PerformClick();
					Assert(bizo.IsSaveAllowed);
					Assert(!bizo.IsImbeddedConcordance);
				}
			}
		}

		public void TestTCOUpdateUserFileZButtonClick()
		{
			using (var tempFile = TempFile.New())
			{
				var bizo = new AUImportTariffBulkChange(Factory);
				using (var form = new AUImportTariffBulkChangeStartForm(bizo))
				{
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					form.TCOUpdateUserFileZButton.PerformClick();
					Assert(bizo.IsSaveAllowed);
					Assert(!bizo.IsImbeddedConcordance);
				}
			}
		}

		protected override Form GetFormToBashCore() => new AUImportTariffBulkChangeStartForm(new AUImportTariffBulkChange(Factory));
	}
}
