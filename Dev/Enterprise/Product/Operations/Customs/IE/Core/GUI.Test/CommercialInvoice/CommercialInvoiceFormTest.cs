using System;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	public class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		public void TestGetNewInvoiceLineUserControl()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			using (var form = new CommercialInvoiceFormForTest(invoiceHeader))
			{
				AssertPackagesPivotTabPageVisibility(form, invoiceHeader, typeof(EUInvoiceLineUserControl), "ABC", false);
				AssertPackagesPivotTabPageVisibility(form, invoiceHeader, typeof(ImportComInvoiceLineUserControl), MessageTypeList.Codes.Import, false);
				AssertPackagesPivotTabPageVisibility(form, invoiceHeader, typeof(ExportComInvoiceLineUserControl), MessageTypeList.Codes.Export, false);
			}

			void AssertPackagesPivotTabPageVisibility(CommercialInvoiceFormForTest form, JobComInvoiceHeader header, Type formType, string messageType, bool expectedValue)
			{
				header.JZ_MessageType = messageType;
				using (var control = form.GetNewInvoiceLineUserControlExposed())
				{
					AssertType(formType, control);
				}
			}
		}

		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());

		sealed class CommercialInvoiceFormForTest : CommercialInvoiceForm
		{
			public Customs.GUI.BaseInvoiceLineUserControl GetNewInvoiceLineUserControlExposed() => GetNewInvoiceLineUserControl();

			public CommercialInvoiceFormForTest(JobComInvoiceHeader invoiceHeader)
				: base(invoiceHeader)
			{
			}
		}
	}
}
