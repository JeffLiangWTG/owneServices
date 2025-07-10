using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Containers
{
	[TestedType(typeof(DocConfirmDivotCollection))]
	sealed class DocConfirmDivotCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocConfirmDivotCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonConfirmDivot divot = Factory.New<CommonConfirmDivot>();
			return DocConfirmDivot.New(divot, Factory);
		}

		protected override DocConfirmDivotCollection GetCollectionToTest()
		{
			CommonPickupDeliveryConfirm confirm = Factory.New<CommonPickupDeliveryConfirm>();
			return new DocConfirmDivotCollection(confirm);
		}
	}
}
