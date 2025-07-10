using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(DHRInvoiceLineWrapper))]
	sealed class DHRInvoiceLineWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invoiceLine = new ImportDHRInvoiceLine();
			return new DHRInvoiceLineWrapper(invoiceLine, Factory);
		}
	}
}
