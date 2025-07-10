using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Business.Testing
{
	[TestedType(typeof(DocJobComInvoiceGroupHeaderCollection))]
	sealed class DocJobComInvoiceGroupHeaderCollectionTests : DocBaseJobComInvoiceGroupHeaderCollectionTest<DocJobComInvoiceGroupHeaderCollection>
	{
		protected override DocJobComInvoiceGroupHeaderCollection GetCollectionToTest() => new DocJobComInvoiceGroupHeaderCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var jobComInvoiceGroupHeader = Factory.New<JobComInvoiceGroupHeader>();
			return DocJobComInvoiceGroupHeader.New(jobComInvoiceGroupHeader, Factory);
		}
	}
}
