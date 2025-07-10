using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.CFS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuilders.Testing
{
	[TestedType(typeof(RNSParentLoadListWrapper))]
	sealed class RNSParentLoadListWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetRNSRequestCollections()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			CFSShipment shipment1 = loadList.Shipments.AddNew();
			var entryNumber1 = shipment1.Numbers.AddNew();
			entryNumber1.CE_EntryNum = "111";
			entryNumber1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			CFSShipment shipment2 = loadList.Shipments.AddNew();
			var entryNumber2 = shipment2.Numbers.AddNew();
			entryNumber2.CE_EntryNum = "222";
			entryNumber2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;

			RNSParentLoadListWrapper wrapper = RNSParentLoadListWrapper.Load(loadList);
			AssertSame("The ParentBusinessObject should be the loadList", loadList, wrapper.ParentBusinessObject);

			var rnsRequestCollections = wrapper.GetRNSRequestCollections();
			var rnsRequest1 = rnsRequestCollections.First(rnsRequest => rnsRequest.TopLevelBusinessObject == shipment1);
			var rnsRequest2 = rnsRequestCollections.First(rnsRequest => rnsRequest.TopLevelBusinessObject == shipment2);

			AssertNotNull("A RNS request should been created for shipment1", rnsRequest1);
			AssertEquals("The CargoControlNumber should be the CCN of shipment1", "111", rnsRequest1.CargoControlNumber);
			AssertNotNull("A RNS request should been created for shipment2", rnsRequest2);
			AssertEquals("The CargoControlNumber should be the CCN of shipment2", "222", rnsRequest2.CargoControlNumber);
		}

		#region Overrides of BusinessObjectBaseTestCase

		protected override BusinessObject GetNewBusinessObject()
		{
			return RNSParentLoadListWrapper.Load(Factory.New<CFSLoadListConsol>());
		}

		#endregion
	}
}
