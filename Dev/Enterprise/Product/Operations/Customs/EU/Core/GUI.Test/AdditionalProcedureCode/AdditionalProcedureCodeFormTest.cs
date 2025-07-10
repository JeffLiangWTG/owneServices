using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(AdditionalProcedureCodeForm))]
	class AdditionalProcedureCodeFormTest : ZFormBasherTest
	{
		public void TestValidate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dec = Factory.New<JobDeclaration>();
				dec.JE_ApplicationCode = "CDS";
				var invoice = dec.Invoices.AddNew();
				var line = invoice.InvoiceLines.AddNew();
				var additionalProcedueCode = line.AdditionalProcedureCodes.AddNew();
				using (var form = new AdditionalProcedureCodeFormForTest(line))
				{
					var e = new CancelEventArgs(false);
					additionalProcedueCode.CY_Code = "zzzz";
					form.OnClosing(e);
					AssertEquals("No validation errors, should not be cancelled", false, e.Cancel);

					e = new CancelEventArgs(false);
					for (var i = 0; i < line.MaxNumberOfAdditionalProcedureCode; i++)
					{
						line.AdditionalProcedureCodes.AddNew();
					}
					form.OnClosing(e);
					AssertEquals("Validation errors exists, should be cancelled", true, e.Cancel);
				}
			}
		}

		public void TestCancel()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.InvoiceLines.AddNew();
			using (var form = new AdditionalProcedureCodeFormForTest(line))
			{
				var initialItem = line.AdditionalProcedureCodes.AddNew();
				initialItem.CY_Code = "zzz";
				form.Show();
				var newItem = line.AdditionalProcedureCodes.AddNew();
				newItem.CY_Code = "yyy";
				AssertEquals("Should have 2 while editing", 2, line.AdditionalProcedureCodes.Count);

				form.DialogResult = DialogResult.Cancel;
				form.Close();
				AssertEquals("Should have 1 again after cancel", 1, line.AdditionalProcedureCodes.Count);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.InvoiceLines.AddNew();
			return new AdditionalProcedureCodeForm(line);
		}

		protected class AdditionalProcedureCodeFormForTest : AdditionalProcedureCodeForm
		{
			public AdditionalProcedureCodeFormForTest(JobComInvoiceLine invoiceLine)
				: base(invoiceLine)
			{
			}

			public new void OnClosing(CancelEventArgs e)
			{
				base.OnClosing(e);
			}
		}
	}
}
