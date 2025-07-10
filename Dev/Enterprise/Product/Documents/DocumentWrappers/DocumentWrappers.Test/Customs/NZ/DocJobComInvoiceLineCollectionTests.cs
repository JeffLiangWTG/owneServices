using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	[TestedType(typeof(DocJobComInvoiceLineCollection))]
	sealed class DocJobComInvoiceLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocJobComInvoiceLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			return DocJobComInvoiceLine.New(jobComInvoiceLine, Factory);
		}

		protected override DocJobComInvoiceLineCollection GetCollectionToTest()
		{
			return new DocJobComInvoiceLineCollection(Factory);
		}
	}
}
