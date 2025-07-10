using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class LoadListForwardManifestSupportTest : BaseLoadListForwardManifestSupportTest
	{
		public void TestClassifyForwardedManifests()
		{
			LoadListForwardManifestSupport support = new LoadListForwardManifestSupport(loadList);
			var messages = support.MatchingForwardedManifests;

			Assert("Matching Forward Manifests should contains message1", messages.Any(message => message.PK == message1.PK));
			Assert("Matching Forward Manifests should contains message2", messages.Any(message => message.PK == message2.PK));
			Assert("Matching Forward Manifests should contains message3", messages.Any(message => message.PK == message3.PK));
			Assert("Matching Forward Manifests should contains message4", messages.Any(message => message.PK == message4.PK));
			Assert("Matching Forward Manifests should contains message5", messages.Any(message => message.PK == message5.PK));
			Assert("Matching Forward Manifests should not contains message6", !messages.Any(message => message.PK == message6.PK));

			AssertEquals(1, support.UnlinkedAndNoShipmentMessages.Length);
			Assert("UnlinkedAndNoShipmentMessages should contains message1", support.UnlinkedAndNoShipmentMessages.Any(message => message.PK == message1.PK));

			AssertEquals(1, support.UnlinkedAndExistingShipmentMessages.Length);
			Assert("UnlinkedAndNoShipmentMessages should contains message2", support.UnlinkedAndExistingShipmentMessages.Any(message => message.PK == message2.PK));

			AssertEquals(1, support.LinkedAndAttachedShipmentMessages.Length);
			Assert("UnlinkedAndNoShipmentMessages should contains message3", support.LinkedAndAttachedShipmentMessages.Any(message => message.PK == message3.PK));

			AssertEquals(2, support.LinkedInconsistentMesasges.Length);
			Assert("UnlinkedAndNoShipmentMessages should contains message4", support.LinkedInconsistentMesasges.Any(message => message.PK == message4.PK));
			Assert("UnlinkedAndNoShipmentMessages should contains message5", support.LinkedInconsistentMesasges.Any(message => message.PK == message5.PK));
		}

		public void TestAttachOrCreateShipmentsFromForwardedManifests()
		{
			AssertEquals("Precondition:", 3, loadList.Shipments.Count);

			var support = new LoadListForwardManifestSupport(loadList);
			string resultText = LoadListForwardManifestSupport.AttachOrCreateShipmentsFromForwardedManifests(support);

			AssertEquals("One new CFSShipment was created", 4, loadList.Shipments.Count);

			ZStringBuilder expectedTextBuilder = new ZStringBuilder();

			expectedTextBuilder.Append(string.Format("{0} was created and linked to Forwarded Manifest (CCN: 10207000067891).", loadList.Shipments[loadList.Shipments.Count - 1].HumanReadableName));
			expectedTextBuilder.Append(string.Format("{0} was linked to Forwarded Manifest (CCN: 10207000067892).", shipment2.HumanReadableName, message2.CargoControlNumber));
			expectedTextBuilder.Append(string.Format("{0} had already been linked to Forwarded Manifest (CCN: 10207000067894), \r\n    but it is not a shipment that is attached to this Load List.", shipment4.HumanReadableName));
			expectedTextBuilder.Append(string.Format("{0} had already been linked to Forwarded Manifest (CCN: 10207000067895), \r\n    but the shipment house CCN is 10207000067896", shipment5.HumanReadableName));

			AssertEquals(expectedTextBuilder.ToStringWithNewLineBetweenAppends(), resultText);

			var reloadLoadList = Factory.Load<CFSLoadListConsol>(loadList.PK);

			var shipment = (CFSShipment)reloadLoadList.Shipments.FindByPK(loadList.Shipments[3].PK);
			AssertNotNull("The new shipment was added to the Load List", shipment);

			var message = Factory.Load<ACIHouseBillMessage>(message2.PK);
			AssertEquals("Shipment2 was linked to message2", shipment2.PK, message.EM_LinkUniqueID);
		}

		public void TestAttachOrCreateShipmentsFromForwardedManifestsWithZSaveException()
		{
			AssertEquals("Precondition:", 3, loadList.Shipments.Count);

			var support = new LoadListForwardManifestSupportFotTesting(loadList);
			support.OnNewShipmentCreated += (shipment) =>
			{
				TestConnection.ExecuteNonQuery(string.Format("insert into dbo.JobShipment (JS_PK, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser) values('{0}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", shipment.PK));
			};

			UnitTestUserNotification.Instance.ClearMessages();
			AssertNotNull("Precondition:", UnitTestUserNotification.Instance.LastMessage.WasNone);

			string resultText = LoadListForwardManifestSupport.AttachOrCreateShipmentsFromForwardedManifests(support);

			ZStringBuilder expectedTextBuilder = new ZStringBuilder();

			expectedTextBuilder.Append(string.Format("{0} had already been linked to Forwarded Manifest (CCN: 10207000067894), \r\n    but it is not a shipment that is attached to this Load List.", shipment4.HumanReadableName));
			expectedTextBuilder.Append(string.Format("{0} had already been linked to Forwarded Manifest (CCN: 10207000067895), \r\n    but the shipment house CCN is 10207000067896", shipment5.HumanReadableName));

			AssertEquals(expectedTextBuilder.ToStringWithNewLineBetweenAppends(), resultText);

			AssertEquals("Shipment2 should not be linked to message2", ZGuid.Empty, message2.EM_LinkUniqueID);
			AssertEquals("There should be no CFSShipment to be created", 3, loadList.Shipments.Count);

			AssertNotNull("Exception should be reported", UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		class LoadListForwardManifestSupportFotTesting : LoadListForwardManifestSupport
		{
			public LoadListForwardManifestSupportFotTesting(CFSLoadListConsol loadListConsol)
				: base(loadListConsol)
			{
			}

			public OnNewShipmentCreatedDelegate OnNewShipmentCreated;

			protected override CFSShipment createAndAttachShipmentFromForwardedManifest(ACIHouseBillMessage message)
			{
				CFSShipment shipment = base.createAndAttachShipmentFromForwardedManifest(message);
				OnNewShipmentCreated(shipment);
				return shipment;
			}
		}

		delegate void OnNewShipmentCreatedDelegate(CFSShipment shipment);
	}
}
