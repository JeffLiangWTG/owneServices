using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoad()
		{
			CusHAWB cusHAWB = Factory.NewWithValidTestData<CusHAWB>();

			CusMAWB cusMAWB = Factory.NewWithValidTestData<CusMAWB>();
			cusMAWB.CM_MAWB = "08122222222";
			cusHAWB.CS_CM = cusMAWB.PK;

			CusMAWB unRelatedCusMAWB = Factory.NewWithValidTestData<CusMAWB>();

			Factory.Save();

			BusinessObjectFactory cleanFactory = new BusinessObjectFactory();

			cusHAWB = cleanFactory.Load<CusHAWB>(cusHAWB.PK);
			unRelatedCusMAWB = cleanFactory.Load<CusMAWB>(unRelatedCusMAWB.PK);

			ZQuery cusMAWBFilter = new ZQuery(CusMAWBSchema.PK, cusMAWB.PK);
			cusMAWBFilter.FetchOnlyFromLocalCache = true;

			cusMAWB = cleanFactory.LoadTop1<CusMAWB>(cusMAWBFilter);
			AssertEquals("08122222222", cusMAWB.CM_MAWB);
		}

		public void TestFetchForValidate_Messages()
		{
			var mawb = Factory.New<CusMAWB>();
			mawb.CM_MAWB = "MB1";
			mawb.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			for (var i = 1; i < 7; i++)
			{
				var hawb = mawb.ChildBills.AddNew();
			}
			Factory.Save();

			// need to create the messages in a separate factory to stop the CusHAWB from getting a message status
			var newFactory = new BusinessObjectFactory();
			foreach (CusHAWB hawb in mawb.ChildBills)
			{
				var message = newFactory.New<EDIMessage>();
				message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.CMR;
				message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				message.EM_MessageType = Declaration.Business.CMRMessage.CMRMessageTypes.AIRCR;
				message.EM_LinkUniqueID = hawb.PK;
				message.EM_LinkTable = hawb.TableName;
			}
			newFactory.Save();
			foreach (CusHAWB hawb in mawb.ChildBills)
			{
				hawb.Reload();
				AssertEquals("PreCondition: No message status", "", hawb.CS_MsgStatus);
			}

			newFactory = new BusinessObjectFactory();
			var mawbInNewFactory = newFactory.Load<CusMAWB>(mawb.PK);
			mawbInNewFactory.LoadChildEditableObjects();
			mawbInNewFactory.RunPreSaveValidationWithFetchHints();
			AssertEquals(0, newFactory.GetTableHitCount(EDIMessage.Schema.TableName));
			AssertEquals(6, mawbInNewFactory.ChildBills.Count);
			foreach (CusHAWB hawb in mawbInNewFactory.ChildBills)
			{
				AssertHasMessageErrors(hawb.CS_HAWBInfo);
			}
			AssertEquals(0, newFactory.GetTableHitCount(EDIMessage.Schema.TableName));

			mawbInNewFactory.HasChanges = true;
			mawbInNewFactory.ChildBills[0].HasChanges = true;
			newFactory.Save();
			AssertEquals(0, newFactory.GetTableHitCount(EDIMessage.Schema.TableName));
		}
	}
}
