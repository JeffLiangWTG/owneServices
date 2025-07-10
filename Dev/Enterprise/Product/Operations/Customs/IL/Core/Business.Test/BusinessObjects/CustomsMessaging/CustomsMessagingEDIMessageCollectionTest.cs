using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(CustomsMessagingEDIMessageCollection))]
	sealed class CustomsMessagingEDIMessageCollectionTest : ActiveBusinessObjectCollectionTestCase<CustomsMessagingEDIMessageCollection>
	{
		public void TestReadonly()
		{
			var shipment = Shipment;
			var message = shipment.Messages.AddNew();
			message.EM_ApplicationCode = "ILC";
			message.EM_MessageType = "DLO";
			var messageManager = new CustomsMessagingMessageManager(shipment);
			Assert("Should be set to ReadOnly in the constructor", messageManager.Messages.ReadOnly);
			Assert("Children should be ReadOnly as well", messageManager.Messages[0].ReadOnly);
		}

		public void TestCustomsMessagingEDIMessageFKCorrectlySet()
		{
			var shipment = Shipment;
			var message = shipment.Messages.AddNew();
			message.EM_ApplicationCode = "ILC";
			message.EM_MessageType = "DLO";
			var messageManager = new CustomsMessagingMessageManager(shipment);
			AssertContainsExactElementsInAnyOrder(messageManager.Messages.Select(x => x.PK), new[] { message.PK });
		}

		public void TestAddNewCorrectType()
		{
			var collection = GetCollectionToTest();
			var message = collection.AddNew();
			AssertType<ILEDIMessage>(message);
		}

		#region Implementation

		protected override CustomsMessagingEDIMessageCollection GetCollectionToTest() => GetNewMessageCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var message = Factory.New<ILEDIMessage>();
			message.EM_LinkUniqueID = Shipment.PK;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.ILCustoms;
			message.EM_MessageType = ILMessageTypeList.Codes.DLO;
			return message;
		}

		CustomsMessagingEDIMessageCollection GetNewMessageCollection() => new CustomsMessagingEDIMessageCollection(Shipment);

		ForwardingShipment Shipment
		{
			get
			{
				if (this.shipment == null)
				{
					this.shipment = Factory.New<ForwardingShipment>();
				}
				return this.shipment;
			}
		}
		ForwardingShipment shipment;

		#endregion
	}
}
