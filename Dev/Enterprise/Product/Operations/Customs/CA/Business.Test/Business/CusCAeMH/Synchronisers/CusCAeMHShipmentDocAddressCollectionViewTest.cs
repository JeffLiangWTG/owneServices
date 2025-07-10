using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHDocAddressCollectionSynchroniser))]
	sealed class CusCAeMHShipmentDocAddressCollectionViewTest : BusinessObjectCollectionViewTestCase<CusCAeMHDocAddressCollectionSynchroniser.CusCAeMHShipmentDocAddressCollectionView>
	{
		protected override CusCAeMHDocAddressCollectionSynchroniser.CusCAeMHShipmentDocAddressCollectionView GetCollectionToTest()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var result = new CusCAeMHDocAddressCollectionSynchroniser.CusCAeMHShipmentDocAddressCollectionView(shipment.DocAddresses);
			Factory.Save();
			return result;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<JobDocAddress>();
			result.E2_AddressType = DocAddressTypes.Codes.ConsigneeDocumentaryAddress;
			return result;
		}
	}
}
