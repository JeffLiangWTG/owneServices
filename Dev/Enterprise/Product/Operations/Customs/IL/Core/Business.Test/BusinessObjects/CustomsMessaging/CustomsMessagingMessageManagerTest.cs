using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(CustomsMessagingMessageManager))]
	sealed class CustomsMessagingMessageManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessagesLoad()
		{
			AssertEquals("Expected 0 EDI Messages", 0, Manager.Messages.Count);
			CustomsMessagingEDIMessageCollection messages = new CustomsMessagingEDIMessageCollection(shipment);
			var message = CreateMessage("GPM");
			message = CreateMessage("DLO");
			message = CreateMessage("UCI");

			CustomsMessagingMessageManager manager = CustomsMessagingMessageManager.New(shipment);
			Factory.Save();
			AssertEquals("Expected 2 EDI Messages", 2, manager.Messages.Count);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return CustomsMessagingMessageManager.New(Factory.NewWithValidTestData<ForwardingShipment>());
		}

		protected override void SetUp()
		{
			base.SetUp();
			shipment = Factory.New<ForwardingShipment>();
		}

		CustomsMessagingMessageManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = CustomsMessagingMessageManager.New(Factory.NewWithValidTestData<ForwardingShipment>());
				}

				return fManager;
			}
		}
		CustomsMessagingMessageManager fManager;
		
		ForwardingShipment shipment;

		ILEDIMessage CreateMessage(ZString messageType)
		{
			var message = Factory.New<ILEDIMessage>();
			message.EM_MessageText = "MessageText";
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationReference = "AppRef";
			message.EM_LinkTable = shipment.TableName;
			message.EM_LinkUniqueID = shipment.PK;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.ILCustoms;
			return message;
		}

		#endregion
	}
}
