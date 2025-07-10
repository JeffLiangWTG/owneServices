using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(HTSTariffBulkChangeStartForm))]
	sealed class TariffBulkChangeStartFormTest : ZFormBasherTest
	{
		public void TestTariffUpdateUserFileZButtonClick()
		{
			using (var tempFile = TempFile.New())
			{
				var bizo = new CAHTSTariffBulkChange(Factory);
				using (var form = new HTSTariffBulkChangeStartForm(bizo))
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

		public void TestTariffUpdateCustomsFileZButtonClick()
		{
			var bizo = new CAHTSTariffBulkChange(Factory);
			using (var form = new HTSTariffBulkChangeStartForm(bizo))
			{
				form.Show();
				form.TariffUpdateCustomsFileZButton.PerformClick();
				Assert(bizo.IsSaveAllowed);
				Assert(bizo.IsImbeddedConcordance);
			}
		}

		public void TestTariffUpdateCustomsFileZButton()
		{
			var bizo = new CAHTSTariffBulkChange(Factory);
			using (var form = new HTSTariffBulkChangeStartForm(bizo))
			{
				form.Show();
				AssertEquals(true, form.TariffUpdateCustomsFileZButton.Text.StartsWith("Perform HS2022"));
			}
		}

		protected override Form GetFormToBashCore() => new HTSTariffBulkChangeStartForm(new CAHTSTariffBulkChange(Factory));
	}
}
