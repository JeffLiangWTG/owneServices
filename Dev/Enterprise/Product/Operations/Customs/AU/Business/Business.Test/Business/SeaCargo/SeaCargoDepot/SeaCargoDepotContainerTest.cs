using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(SeaCargoDepotContainer))]
	sealed class SeaCargoDepotContainerTest : SeaCargoDepotNonPersistantBusineesObjectTestCase
	{
		public void TestIMessageManageableBizObj()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			SeaCargoDepotContainer depotContainer = SeaCargoDepotContainer.Load(container);
			Customs.Business.IMessageManageableBizObj bizObj = depotContainer;

			AssertEquals("MessageManager", typeof(SeaCargoDepotMultiMessageManager), bizObj.GetMessageManagerForAmendmentDetection().GetType());
		}

		public void TestGetAllPossibleCollectionProviders()
		{
			CFSContainer container = Factory.New<CFSContainer>();

			CFSPackLine packLine1 = Factory.New<CFSPackLine>();
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			container.PackLines.Add(packLine1);

			CFSPackLine packLine2 = Factory.New<CFSPackLine>();
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;

			CFSShipment shipment = Factory.New<CFSShipment>();
			packLine2.JL_JS = shipment.PK;
			container.PackLines.Add(packLine2);

			SeaCargoDepotContainer depotContainer = SeaCargoDepotContainer.Load(container);
			ICusUnderbondDependentCollectionParent[] providers = depotContainer.GetAllPossibleCollectionProviders();
			AssertEquals(2, providers.Length);
			AssertEquals(shipment, ((CFSShipmentWrapper)providers[0]).Shipment);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			SeaCargoDepotContainer result = SeaCargoDepotContainer.Load(container);
			return result;
		}

		[ExpectNoExceptions()]
		public void TestDelete()
		{
			SeaCargoDepotContainer sCAContainer = SeaCargoDepotContainer.Load(container);
			sCAContainer.Delete();
		}

		public void TestCorretType()
		{
			var container = Factory.New<CFSContainer>();
			var depotContainer = SeaCargoDepotContainer.Load(container);
			AssertType<CusUnderbondUnionCollection>(depotContainer.AllUnderbonds);
			AssertType<CusUnderbondCollection>(depotContainer.Underbonds);
		}

		CFSContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<CFSContainer>();
			container.JC_OH_CFSClient = SomeForwarder().PK;
		}
	}
}
