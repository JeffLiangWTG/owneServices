using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceFormLayoutProvider))]
	internal class CommercialInvoiceFormLayoutProviderTest : CommercialInvoiceFormLayoutProviderAbstractTest<CommercialInvoiceFormLayoutProvider, BaseJobComInvoiceHeader>
	{
		protected override Type ExpectedCommercialInvoiceHeaderDetailsLayoutType => typeof(EU.GUI.CommercialInvoice.InvoiceHeaderDetailsLayout);

		public void TestGetInvoiceHeaderDetailsLayoutType()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var provider = new CommercialInvoiceFormLayoutProvider();

			invoiceHeader.JZ_MessageType = IEJobMessageTypeList.Codes.Export;
			var invoiceLayout = provider.GetInvoiceHeaderDetailsLayout(invoiceHeader);
			AssertType<ExportInvoiceDetailsLayout>("Should return type for Export jobs.", invoiceLayout);

			invoiceHeader.JZ_MessageType = IEJobMessageTypeList.Codes.Import;
			invoiceLayout = provider.GetInvoiceHeaderDetailsLayout(invoiceHeader);
			AssertType<EU.GUI.CommercialInvoice.InvoiceHeaderDetailsLayout>("Should return type for Import jobs.", invoiceLayout);
		}
	}
}
