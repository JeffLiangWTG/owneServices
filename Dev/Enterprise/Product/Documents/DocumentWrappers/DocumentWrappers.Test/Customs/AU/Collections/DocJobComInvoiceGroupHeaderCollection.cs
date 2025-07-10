using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocJobComInvoiceGroupHeaderCollection))]
	sealed class DocJobComInvoiceGroupHeaderCollectionTests : DocBaseJobComInvoiceGroupHeaderCollectionTest<DocJobComInvoiceGroupHeaderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jobComInvoiceGroupHeader = Factory.New<JobComInvoiceGroupHeader>();
			return DocJobComInvoiceGroupHeader.New(jobComInvoiceGroupHeader, Factory);
		}

		protected override DocJobComInvoiceGroupHeaderCollection GetCollectionToTest()
		{
			return new DocJobComInvoiceGroupHeaderCollection(Factory);
		}
	}
}
