using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Containers
{
	[TestedType(typeof(DocPickupDeliveryConfirmCollection))]
	sealed class DocPickupDeliveryConfirmCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocPickupDeliveryConfirmCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonPickupDeliveryConfirm confirm = Factory.New<CommonPickupDeliveryConfirm>();
			return DocPickupDeliveryConfirm.New(confirm, Factory);
		}

		protected override DocPickupDeliveryConfirmCollection GetCollectionToTest()
		{
			return new DocPickupDeliveryConfirmCollection(Factory);
		}
	}
}
