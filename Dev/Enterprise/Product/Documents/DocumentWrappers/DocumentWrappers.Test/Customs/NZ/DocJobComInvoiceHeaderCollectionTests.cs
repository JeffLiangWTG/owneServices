using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	[TestedType(typeof(DocJobComInvoiceHeaderCollection))]
	sealed class DocJobComInvoiceHeaderCollectionTests : DocBaseJobComInvoiceHeaderCollectionTest<DocJobComInvoiceHeaderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jobComInvoiceHeader = Factory.New<JobComInvoiceHeader>();
			return DocJobComInvoiceHeader.New(jobComInvoiceHeader, Factory);
		}

		protected override DocJobComInvoiceHeaderCollection GetCollectionToTest()
		{
			return new DocJobComInvoiceHeaderCollection(Factory);
		}
	}
}
