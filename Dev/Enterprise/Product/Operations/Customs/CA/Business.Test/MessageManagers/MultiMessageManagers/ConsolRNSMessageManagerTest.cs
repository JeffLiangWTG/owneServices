using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	sealed class ConsolRNSMessageManagerTest : TestCaseWithFactory
	{
		public void TestGetAllMessageManagers()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var entryNumber1 = shipment1.Numbers.AddNew();
			entryNumber1.CE_EntryNum = "111";
			entryNumber1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			var shipment2 = consol.Shipments.AddNew();
			var entryNumber2 = shipment2.Numbers.AddNew();
			entryNumber2.CE_EntryNum = "222";
			entryNumber2.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			Factory.Save();
			var wrapper = RNSParentConsolWrapper.Load(consol);
			var collection = new RNSRequestBOCollection(consol);
			var manager = new ConsolRNSMessageManagerForTesting(wrapper, collection.GetSelectedRequestBOs());

			AssertSame("The TopLevelBizObjToManage should be the wrapper", wrapper, manager.TopLevelBizObjToManage);
			var singleMessageManagers = manager.GetAllMessageManagers_Exposed();
			AssertEquals("There should be 0 message managers", 0, singleMessageManagers.Length);

			collection[0].Selected = true;
			collection[1].Selected = true;
			manager = new ConsolRNSMessageManagerForTesting(wrapper, collection.GetSelectedRequestBOs());
			singleMessageManagers = manager.GetAllMessageManagers_Exposed();
			AssertEquals("There should be 2 message managers", 2, singleMessageManagers.Length);
			var rnsMessageManager1 = singleMessageManagers.First(m => m.BusinessObject == shipment1);
			var rnsMessageManager2 = singleMessageManagers.First(m => m.BusinessObject == shipment2);

			AssertType<RNSMessageManager>("A RNSMessageManager should been created for shipment1", rnsMessageManager1);
			AssertType<RNSMessageManager>("A RNSMessageManager should been created for shipment2", rnsMessageManager2);
		}
	}
}
