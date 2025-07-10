using System;
using System.Windows.Forms;
using Enterprise.Client.Rohlig.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Rohlig.Bellin
{
	[TestedType(typeof(BellinExportForm))]
	public class BellinExportFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new BellinExportForm(new BellinExportGUIWrapper(Factory));
		}

		public void TestFormCaption()
		{
			BellinExportGUIWrapper wrapper = new BellinExportGUIWrapper(Factory);
			using (BellinExportForm form = new BellinExportForm(wrapper))
			{
				AssertEquals("Form Caption should be the same as the GuiWrapper FormCaption", wrapper.FormCaption, form.FormCaption);
			}
		}

		public void TestDatesFromAndToAreNotReadOnlyAfterExport()
		{
			BellinExportGUIWrapper wrapper = new BellinExportGUIWrapper(Factory);
			using (BellinExportForm form = new BellinExportForm(wrapper))
			{
				form.ExportButton_Click(this, EventArgs.Empty);
				Assert("Date From should not be readonly", !wrapper.DateFromInfo.ReadOnly);
				Assert("Date To should not be readonly", !wrapper.DateToInfo.ReadOnly);
			}
		}
	}
}
