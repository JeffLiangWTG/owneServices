using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.General.Testing
{
	[TestedType(typeof(DocJobComInvoiceHeaderCollection))]
	sealed class DocJobComInvoiceHeaderCollectionTests : DocBaseJobComInvoiceHeaderCollectionTest<DocJobComInvoiceHeaderCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceHeader jobComInvoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			return DocJobComInvoiceHeader.New(jobComInvoiceHeader, Factory);
		}

		protected override DocJobComInvoiceHeaderCollection GetCollectionToTest()
		{
			return new DocJobComInvoiceHeaderCollection(Factory);
		}
	}
}
