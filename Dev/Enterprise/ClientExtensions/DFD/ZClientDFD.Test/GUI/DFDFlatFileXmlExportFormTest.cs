using Enterprise.Accounting.GUI.XmlExport.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DFD.GUI
{
	[TestedType(typeof(DFDFlatFileXmlExportForm))]
	public class DFDFlatFileXmlExportFormTest : FlatFileXmlExportFormTestCase
	{
		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			return new DFDFlatFileXmlExportForm(new DFDFlatFileXmlExportGUIWrapper(Factory));
		}

		public void TestExportBatchNumberCalcEditDisabled()
		{
			using (DFDFlatFileXmlExportForm form = new DFDFlatFileXmlExportForm(new DFDFlatFileXmlExportGUIWrapper(Factory)))
			{
				Assert(!form.InternalExportBatchNumberCalcEdit.Enabled);
				Assert(form.InternalDatesGroupBox.Enabled);
				Assert(form.InternalFromZDateEdit.Enabled);
				Assert(form.InternalToZDateEdit.Enabled);
				Assert(form.InternalNewExportBatchGroupBox.Enabled);
			}
		}
	}
}
