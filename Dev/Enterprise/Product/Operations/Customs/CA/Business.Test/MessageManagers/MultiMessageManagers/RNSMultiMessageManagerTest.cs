using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class RNSMultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestGetAllMessageManagers()
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

			var wrapper = RNSParentLoadListWrapper.Load(loadList);
			var manager = new RNSMultiMessageManagerForTesting(wrapper);

			AssertSame("The TopLevelBizObjToManage should be the wrapper", wrapper, manager.TopLevelBizObjToManage);

			var singleMessageManagers = manager.GetAllMessageManagers_Exposed();
			var rnsMessageManager1 = singleMessageManagers.First(m => m.BusinessObject == shipment1);
			var rnsMessageManager2 = singleMessageManagers.First(m => m.BusinessObject == shipment2);

			AssertType<RNSMessageManager>("A RNSMessageManager should been created for shipment1", rnsMessageManager1);
			AssertType<RNSMessageManager>("A RNSMessageManager should been created for shipment2", rnsMessageManager2);
		}
	}
}
