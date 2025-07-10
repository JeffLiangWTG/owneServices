using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestMessageSendingNotificationCollector))]
	sealed class ManifestMessageSendingNotificationCollectorTest : TestCaseWithFactory
	{
		public void TestShouldIncludeNotificationsFromInfo_NVC01()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Voyage = "JPTYO";
			header.AMA_MasterBill = "12345678906";
			header.Bills.AddNew();
			var masterBill = header.MasterBill;
			var sendingObjectsParent = new ManifestMessageSendingObjectParent(header);

			var collector = new ManifestMessageSendingNotificationCollector(sendingObjectsParent, sendingObjectsParent.SelectedSendingObjects.Cast<ManifestMessageSendingObject>().Select(x => x.Bill));

			CombineAssertions(() =>
			{
				foreach (ZPropertyInfo info in header.ZPropertyInfoHash)
				{
					switch (info.Name)
					{
						case nameof(AsycudaManifestHeader.AMA_MasterBill):
						case nameof(AsycudaManifestHeader.AMA_CustomsOffice):
						case nameof(AsycudaManifestHeader.AMA_GS_NKCustomsAgent):
						case nameof(AsycudaManifestHeader.AMA_CustomsAgentCredentialPK):
							Assert($"{info.Name} should be included.", collector.ShouldIncludeNotificationsForHeader_NVC01(info));
							break;
						default:
							Assert($"{info.Name} should not be included.", !collector.ShouldIncludeNotificationsForHeader_NVC01(info));
							break;
					}
				}

				foreach (ZPropertyInfo info in masterBill.ZPropertyInfoHash)
				{
					switch (info.Name)
					{
						case nameof(AsycudaBill.ABL_GoodsLocation):
							Assert($"{info.Name} should be included.", collector.ShouldIncludeNotificationsForBill_NVC01(info));
							break;
						default:
							Assert($"{info.Name} should not be included.", !collector.ShouldIncludeNotificationsForBill_NVC01(info));
							break;
					}
				}
			});
		}

		public void TestOnlySelectedBillsAreCollected()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Voyage = "JPTYO";
			header.AMA_MasterBill = "12345678906";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "123";
			header.MasterBill.ABL_GoodsLocation = "QQ";

			var sendingObjectsParent = new ManifestMessageSendingObjectParent(header);
			var sendingObjects = sendingObjectsParent.SendingObjectsCollection;
			AssertEquals(2, sendingObjects.Count);

			sendingObjects[0].ShouldSend = true;
			sendingObjects[1].ShouldSend = false;
			var messageErrors = new ManifestMessageSendingNotificationCollector(sendingObjectsParent, sendingObjectsParent.SelectedSendingObjects.Cast<ManifestMessageSendingObject>().Select(x => x.Bill), true).GetMessageErrors().ToMessageListString();
			AssertNotContains("HAWB: You have not entered a HAWB.", messageErrors);

			sendingObjects[0].ShouldSend = false;
			sendingObjects[1].ShouldSend = true;
			messageErrors = new ManifestMessageSendingNotificationCollector(sendingObjectsParent, sendingObjectsParent.SelectedSendingObjects.Cast<ManifestMessageSendingObject>().Select(x => x.Bill), true).GetMessageErrors().ToMessageListString();
			AssertContains("HAWB: You have not entered a HAWB.", messageErrors);
			AssertContains("Into Bonded Warehouse: The code you have selected is not in the list.", messageErrors);
		}
	}
}
