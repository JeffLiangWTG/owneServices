using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocJobComInvoiceLineCollection))]
	sealed class DocJobComInvoiceLineCollectionTests : DocBaseJobComInvoiceLineCollectionTest<DocJobComInvoiceLineCollection>
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
