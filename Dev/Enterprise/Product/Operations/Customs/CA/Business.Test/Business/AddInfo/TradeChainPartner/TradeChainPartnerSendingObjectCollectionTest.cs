using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(TradeChainPartnerSendingObjectCollection))]
	sealed class TradeChainPartnerSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TradeChainPartnerSendingObjectCollection>
	{
		public void TestCollectWithCorrectActions()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var imp = OrgImpAddInfo.Get(org);
			var tcp01 = imp.TradeChainPartners.AddNew();
			var tcp02 = imp.TradeChainPartners.AddNew();
			var tcp03 = imp.TradeChainPartners.AddNew();

			tcp01.CA_CSAStatus = CSAStatusList.Codes.Deleted;
			tcp02.CA_CSAStatus = CSAStatusList.Codes.Deleted;
			tcp03.CA_CSAStatus = CSAStatusList.Codes.Added;

			tcp01.CA_Action = ZString.Empty;
			tcp02.CA_Action = CSAActionTypeList.Codes.ReqAdd;
			tcp03.CA_Action = CSAActionTypeList.Codes.ReqDel;

			var collection = new TradeChainPartnerSendingObjectCollection(imp);
			AssertEquals(2, collection.Count);
			var item01 = collection.Cast<TradeChainPartnerSendingObject>().FirstOrDefault(x => x.ActionType == CSAStatusList.Codes.Added).TradeChainPartner;
			var item02 = collection.Cast<TradeChainPartnerSendingObject>().FirstOrDefault(x => x.ActionType == CSAStatusList.Codes.Deleted).TradeChainPartner;
			AssertEquals(tcp02.CA_CSAStatus, item01.CA_CSAStatus);
			AssertEquals(tcp02.CA_Action, item01.CA_Action);
			AssertEquals(tcp03.CA_CSAStatus, item02.CA_CSAStatus);
			AssertEquals(tcp03.CA_Action, item02.CA_Action);
		}

		protected override void SetUp()
		{
			base.SetUp();
			this.organisation = CreateNewOrgForTest("TESTORG", "Test Organisation", "Test Organisation Address");
			CreateNewOrgForTest("TESTORG01", "Test Organisation 01", "Test Organisation 01 Address 01");
			this.orgImp = OrgImpAddInfo.Get(organisation);
		}

		protected override TradeChainPartnerSendingObjectCollection GetCollectionToTest()
		{
			tCPSendingObjectCollection = new TradeChainPartnerSendingObjectCollection(orgImp);
			return tCPSendingObjectCollection;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var org = CreateNewOrgForTest("RANDOMORG", "Random Org", "Random Org Address");
			var tcp = orgImp.TradeChainPartners.AddNew();
			tcp.CA_Org = org.PK;
			return new TradeChainPartnerSendingObject(tcp, org);
		}

		OrgHeader CreateNewOrgForTest(string orgCode, string orgFullName, string orgAddress)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = orgCode;
			var address = org.Addresses.AddNew();
			address.OA_Address1 = orgAddress;
			return org;
		}

		OrgHeader organisation;
		OrgImpAddInfo orgImp;
		TradeChainPartnerSendingObjectCollection tCPSendingObjectCollection;
	}
}
