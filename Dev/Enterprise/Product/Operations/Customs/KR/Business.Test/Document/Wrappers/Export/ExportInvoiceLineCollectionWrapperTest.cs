using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExportInvoiceLineCollectionWrapper))]
	sealed class ExportInvoiceLineCollectionWrapperTest : NonPersistentBusinessObjectCollectionTestCase<ExportInvoiceLineCollectionWrapper>
	{
		protected override ExportInvoiceLineCollectionWrapper GetCollectionToTest() => new ExportInvoiceLineCollectionWrapper(Enumerable.Empty<IExportInvoiceLine>(), Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoiceLine = new ExportInvoiceLine();
			return new ExportInvoiceLineWrapper(invoiceLine, Factory);
		}
	}
}
