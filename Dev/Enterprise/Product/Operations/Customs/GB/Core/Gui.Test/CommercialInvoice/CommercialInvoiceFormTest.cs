using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		public void TestInvoiceLineUserControl()
		{
			var invHeader = Factory.New<JobComInvoiceHeader>();
			invHeader.JZ_MessageType = MessageTypeList.Codes.Import;
			using (var form = new CommercialInvoiceFormForTest(invHeader))
			{
				using (var control = form.InvoiceLineUserControlExposed)
				{
					AssertType<ImportInvoiceLineUserControl>(control);
				}
			}
		}

		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());

		sealed class CommercialInvoiceFormForTest : CommercialInvoiceForm
		{
			public Customs.GUI.BaseInvoiceLineUserControl InvoiceLineUserControlExposed => base.GetNewInvoiceLineUserControl();

			public CommercialInvoiceFormForTest(JobComInvoiceHeader invoiceHeader)
				: base(invoiceHeader)
			{
			}
		}
	}
}
