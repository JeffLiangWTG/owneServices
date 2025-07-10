using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DocJobComInvoiceLineCollection))]
	sealed class DocJobComInvoiceLineCollectionTests : DocBaseJobComInvoiceLineCollectionTest<DocJobComInvoiceLineCollection>
	{
		protected override DocJobComInvoiceLineCollection GetCollectionToTest() => new DocJobComInvoiceLineCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jobComInvoiceLine = Factory.New<JobComInvoiceLine>();
			return DocJobComInvoiceLine.New(jobComInvoiceLine, Factory);
		}
	}
}
