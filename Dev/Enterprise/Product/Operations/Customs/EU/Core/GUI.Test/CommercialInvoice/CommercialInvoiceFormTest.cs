using System;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.CommercialInvoice.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		[RequiresSTA]
		public void TestGetNewInvoiceLineUserControl()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			using (var form = new CommercialInvoiceFormForTest(invoiceHeader))
			{
				AssertPackagesPivotTabPageVisibility(form, invoiceHeader, typeof(EUInvoiceLineUserControl), "ABC", false);
				AssertPackagesPivotTabPageVisibility(form, invoiceHeader, typeof(EUImportInvoiceLineUserControl), MessageTypeList.Codes.Import, false);
				AssertPackagesPivotTabPageVisibility(form, invoiceHeader, typeof(EUExportInvoiceLineUserControl), MessageTypeList.Codes.Export, false);
			}

			void AssertPackagesPivotTabPageVisibility(CommercialInvoiceFormForTest form, JobComInvoiceHeader header, Type formType, string messageType, bool expectedValue)
			{
				header.JZ_MessageType = messageType;
				using (var control = form.GetNewInvoiceLineUserControlExposed())
				{
					AssertType(formType, control);
					AssertEquals("Packages tab hidden when loaded on standalone invoice form", expectedValue, ((EUInvoiceLineUserControl)control).PackagesPivotTabPage.TabVisible);
				}
			}
		}

		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());

		sealed class CommercialInvoiceFormForTest : CommercialInvoiceForm
		{
			public CommercialInvoiceFormForTest(JobComInvoiceHeader invoiceHeader)
				: base(invoiceHeader)
			{
			}

			public Customs.GUI.BaseInvoiceLineUserControl GetNewInvoiceLineUserControlExposed() => GetNewInvoiceLineUserControl();
		}
	}
}
