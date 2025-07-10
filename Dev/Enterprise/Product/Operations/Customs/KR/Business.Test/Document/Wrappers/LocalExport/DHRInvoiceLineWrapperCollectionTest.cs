using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(DHRInvoiceLineWrapperCollection))]
	sealed class DHRInvoiceLineWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DHRInvoiceLineWrapperCollection>
	{
		protected override DHRInvoiceLineWrapperCollection GetCollectionToTest() => new DHRInvoiceLineWrapperCollection(Enumerable.Empty<ImportDHRInvoiceLine>(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var line = new ImportDHRInvoiceLine();

			return new DHRInvoiceLineWrapper(line, Factory);
		}
	}
}
