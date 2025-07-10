using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocIDInvoiceTaxInfoCollection))]
	sealed class DocIDInvoiceTaxInfoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocIDInvoiceTaxInfoCollection>
	{
		protected override DocIDInvoiceTaxInfoCollection GetCollectionToTest()
		{
			return new DocIDInvoiceTaxInfoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			IDInvoiceTaxInfo taxInfo = new IDInvoiceTaxInfo();
			return DocIDInvoiceTaxInfo.New(taxInfo, Factory);
		}
	}
}
