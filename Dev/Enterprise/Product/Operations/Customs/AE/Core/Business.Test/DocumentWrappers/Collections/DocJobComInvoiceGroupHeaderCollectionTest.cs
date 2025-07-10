using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DocJobComInvoiceGroupHeaderCollection))]
sealed class DocJobComInvoiceGroupHeaderCollectionTest : DocBaseJobComInvoiceGroupHeaderCollectionTest<DocJobComInvoiceGroupHeaderCollection>
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
