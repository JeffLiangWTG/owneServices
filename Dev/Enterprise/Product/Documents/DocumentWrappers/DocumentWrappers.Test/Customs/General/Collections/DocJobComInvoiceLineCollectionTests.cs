using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.General.Testing
{
	[TestedType(typeof(DocJobComInvoiceLineCollection))]
	sealed class DocJobComInvoiceLineCollectionTests : DocBaseJobComInvoiceLineCollectionTest<DocJobComInvoiceLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BaseJobComInvoiceLine jobComInvoiceLine = Factory.New<BaseJobComInvoiceLine>();
			return DocJobComInvoiceLine.New(jobComInvoiceLine, Factory);
		}

		protected override DocJobComInvoiceLineCollection GetCollectionToTest()
		{
			return new DocJobComInvoiceLineCollection(Factory);
		}
	}
}
