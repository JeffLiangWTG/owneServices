using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DocDeliveryContactCollection))]
	sealed class DocDeliveryContactCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocDeliveryContactCollection>
	{
		protected override DocDeliveryContactCollection GetCollectionToTest() => new DocDeliveryContactCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new DocDeliveryContact(Factory);
	}
}
