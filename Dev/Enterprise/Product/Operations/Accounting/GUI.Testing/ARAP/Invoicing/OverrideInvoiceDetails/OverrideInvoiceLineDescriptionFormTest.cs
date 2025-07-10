using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideInvoiceLineDescriptionForm))]
	public class OverrideInvoiceLineDescriptionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new OverrideInvoiceLineDescriptionForm(new InvoiceLineOverrideForEditingDescriptionAdaptor(Factory, System.Array.Empty<ZGuid>()));
		}

		public void TestHideColumnsForNonGstRegisteredCompany()
		{
			string[] columnStylesToCheck = new[] { "AL_AT", "AL_OSTaxAmount", "AL_OSGSTAmount", "AL_LocalGSTAmount", "AL_LocalTaxAmount", "AL_LocalTotalAmount", "AL_OverseasTotal" };

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (var form = (OverrideInvoiceLineDescriptionForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				foreach (var columnStyleString in columnStylesToCheck)
				{
					AssertEquals(string.Format("Column style {0} is available", columnStyleString), false, form.InvoicesGridForTest.GetColumnStyle(columnStyleString).IsUnavailable);
				}
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			using (var form = (OverrideInvoiceLineDescriptionForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				foreach (var columnStyleString in columnStylesToCheck)
				{
					AssertNull(string.Format("Column style {0} is not available", columnStyleString), form.InvoicesGridForTest.GetColumnStyle(columnStyleString));
				}
			}
		}
	}
}
