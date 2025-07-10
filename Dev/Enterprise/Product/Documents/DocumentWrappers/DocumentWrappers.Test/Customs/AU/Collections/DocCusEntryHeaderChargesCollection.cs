using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCusEntryHeaderChargesCollection))]
	sealed class DocCusEntryHeaderChargesCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCusEntryHeaderChargesCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusEntryHeaderCharge = Factory.New<CusEntryHeaderCharges>();
			return DocCusEntryHeaderCharges.New(cusEntryHeaderCharge, Factory);
		}

		protected override DocCusEntryHeaderChargesCollection GetCollectionToTest() => new DocCusEntryHeaderChargesCollection(Factory);
	}
}
