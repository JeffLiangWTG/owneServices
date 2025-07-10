using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocARInvoiceTaxMessageCollection))]
	sealed class DocARInvoiceTaxMessageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocARInvoiceTaxMessageCollection>
	{
		protected override DocARInvoiceTaxMessageCollection GetCollectionToTest()
		{
			return new DocARInvoiceTaxMessageCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			AccInvMsg message = Factory.New<AccInvMsg>();
			return DocARInvoiceTaxMessage.New(message, Factory);
		}
	}
}
