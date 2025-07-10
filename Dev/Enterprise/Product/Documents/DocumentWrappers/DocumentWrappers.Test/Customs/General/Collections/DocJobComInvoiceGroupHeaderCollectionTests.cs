using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.General.Testing
{
	[TestedType(typeof(DocJobComInvoiceGroupHeaderCollection))]
	sealed class DocJobComInvoiceGroupHeaderCollectionTests : DocBaseJobComInvoiceGroupHeaderCollectionTest<DocJobComInvoiceGroupHeaderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceGroupHeader jobComInvoiceGroupHeader = Factory.New<BaseJobComInvoiceGroupHeader>();
			return DocJobComInvoiceGroupHeader.New(jobComInvoiceGroupHeader, Factory);
		}

		protected override DocJobComInvoiceGroupHeaderCollection GetCollectionToTest()
		{
			return new DocJobComInvoiceGroupHeaderCollection(Factory);
		}
	}
}
