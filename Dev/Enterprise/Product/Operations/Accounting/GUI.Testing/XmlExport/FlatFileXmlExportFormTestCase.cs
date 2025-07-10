using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.XmlExport.Testing
{
	[TestedType(typeof(FlatFileXmlExportForm))]
	public class FlatFileXmlExportFormTestCase : ZFormBasherTest
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new FlatFileXmlExportForm(new XmlExportGUIWrapper(Factory));
		}

		public void TestCorrectControlsAreDisabled()
		{
			using (FlatFileXmlExportForm form = new FlatFileXmlExportForm(new XmlExportGUIWrapper(Factory)))
			{
				Assert(!form.ARInvoiceCheckBox.Enabled);
				Assert(!form.APInvoiceCheckBox.Enabled);
				Assert(!form.ARCreditNoteCheckBox.Enabled);
				Assert(!form.ARAdjustmentNoteCheckBox.Enabled);

				Assert(!form.APCreditNoteCheckBox.Enabled);
				Assert(!form.APAdjustmentNoteCheckBox.Enabled);
				Assert(!form.OrganisationModuleButtonGrid.Enabled);
				Assert(!form.NewExportBatchGroupBox.Enabled);

				Assert(!form.WipPostingCheckBox.Enabled);
				Assert(!form.WipReversalCheckBox.Enabled);
				Assert(!form.AccrualPostingCheckBox.Enabled);
				Assert(!form.AccrualReversingCheckBox.Enabled);

				Assert(!form.ExcludeARJobRelatedCheckBox.Enabled);
				Assert(!form.ExcludeARNonJobRelatedCheckBox.Enabled);
				Assert(!form.ExcludeAPJobRelatedCheckBox.Enabled);
				Assert(!form.ExcludeAPNonJobRelatedCheckBox.Enabled);

				Assert(!form.FromZDateEdit.Enabled);
				Assert(!form.ToZDateEdit.Enabled);
				Assert(!form.DatesGroupBox.Enabled);

				Assert(!form.TransactionTypesGroupBox.Enabled);
				Assert(!form.ExcludeTransactionsGroupBox.Enabled);
				Assert(!form.DepartmentModuleButtonGrid.Enabled);
				Assert(!form.BranchModuleButtonGrid.Enabled);

				Assert(!form.JobModuleButtonGrid.Enabled);
				Assert(!form.PeriodFromPeriodEdit.Enabled);
				Assert(!form.PeriodToPeriodEdit.Enabled);

				Assert(form.ExportBatchNumberCalcEdit.Enabled);
			}
		}
	}
}
