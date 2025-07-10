using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
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
