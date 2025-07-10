using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class GoodsCatalogMultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestGetAllMessageManagers()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.NotSent;

			var messageSendingObject = new GoodsCatalogMessageSendingObject(goodsCatalog);
			var manager = new GoodsCatalogMultiMessageManagerForTesting(messageSendingObject);
			AssertSame("The TopLevelBizObjToManage should be the goodsCatalog", goodsCatalog, manager.TopLevelBizObjToManage);

			var sendingObject = manager.GetAllMessageManagers_Exposed().Single();
			AssertType<GoodsCatalogMessageManager>("A GoodsCatalogMessageManager should been created for sending object", sendingObject);
		}

		public void TestGetDeferredAmendmentSavingOptions()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.NotSent;

			var messageSendingObject = new GoodsCatalogMessageSendingObject(goodsCatalog);
			var manager = new GoodsCatalogMultiMessageManager(messageSendingObject) as IMessageManager;
			var savingOptions = manager.GetDeferredAmendmentSavingOptions();
			AssertType<CatalogDeferredAmendmentSavingOptions>(savingOptions);
			AssertEquals("SaveWithEntryChanges", true, (savingOptions as CatalogDeferredAmendmentSavingOptions).SaveWithEntryChanges);
		}

		public void TestProcessWhenChangesAreSavedWithoutSending()
		{
			var goodsCatalog = Factory.New<CusGoodsCatalog>();
			goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Accepted;

			var messageSendingObject = new GoodsCatalogMessageSendingObject(goodsCatalog);
			var manager = new GoodsCatalogMultiMessageManager(messageSendingObject) as IMessageManager;
			var savingOptions = manager.GetDeferredAmendmentSavingOptions();

			manager.ProcessWhenChangesAreSavedWithoutSending(savingOptions, null);
			AssertEquals("Message Status not changed", BRMessageStatusList.Codes.Accepted, goodsCatalog.CGC_MessageStatus);

			goodsCatalog.CGC_AuthorityIdentifier = "1";
			manager.ProcessWhenChangesAreSavedWithoutSending(savingOptions, null);
			AssertEquals("Message Status reset", BRMessageStatusList.Codes.NotSent, goodsCatalog.CGC_MessageStatus);
		}
	}

	#region Implementation

	class GoodsCatalogMultiMessageManagerForTesting : GoodsCatalogMultiMessageManager
	{
		public GoodsCatalogMultiMessageManagerForTesting(GoodsCatalogMessageSendingObject messageSendingObjectParent) : base(messageSendingObjectParent)
		{
		}

		public SingleMessageManager[] GetAllMessageManagers_Exposed()
		{
			return GetAllMessageManagers();
		}
	}

	#endregion
}
