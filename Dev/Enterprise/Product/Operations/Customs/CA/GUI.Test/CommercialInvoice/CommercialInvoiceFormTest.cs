using System;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using NUnit.Framework;

#if !WINZOR
namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		public void TestFormMinSize()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			using (var form = new CommercialInvoiceForm(invoice))
			{
				AssertEquals(1200, form.MinimumSize.Width);
				AssertEquals(718, form.MinimumSize.Height);
			}
		}

		public void TestGetNewInvoiceLineUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			using (var form = new CommercialInvoiceFormForTesting(invoice))
			{
				invoice.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("Export Validation", typeof(CAExportInvoiceLineUserControl), form.GetNewInvoiceLineUserControlType());
				invoice.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("Import Validation", typeof(CAImportInvoiceLineUserControl), form.GetNewInvoiceLineUserControlType());
				invoice.JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Misc;
				AssertEquals("Misc Validation", typeof(CAInvoiceLineUserControl), form.GetNewInvoiceLineUserControlType());
			}
		}

		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());

		sealed class CommercialInvoiceFormForTesting : CommercialInvoiceForm
		{
			public CommercialInvoiceFormForTesting(JobComInvoiceHeader header)
				: base(header)
			{
			}

			internal Type GetNewInvoiceLineUserControlType()
			{
				Type result;
				using (var control = GetNewInvoiceLineUserControl())
				{
					result = control.GetType();
				}
				return result;
			}

			internal BaseInvoiceLineUserControl GetNewInvoiceLineUserControl_Exposed()
			{
				return GetNewInvoiceLineUserControl();
			}
		}
	}
}
#endif
