using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(OuterPackLineCollectionView))]
	sealed class OuterPackLineCollectionViewTest : BusinessObjectCollectionViewTestCase<OuterPackLineCollectionView>
	{
		CommonConsol consol;
		CommonContainer container;
		CommonShipment shipment;

		protected override OuterPackLineCollectionView GetCollectionToTest()
		{
			return new OuterPackLineCollectionView(shipment.OuterPackLines, container);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return shipment.OuterPackLines.AddNew();
		}

		protected override void SetUp()
		{
			consol = Factory.New<CommonConsol>();
			container = consol.Containers.AddNew();
			shipment = consol.Shipments.AddNew();

			base.SetUp();
		}
	}
}
