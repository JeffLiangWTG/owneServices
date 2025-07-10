using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(MessageSendingAction))]
	class MessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestResetEditableChildObject()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00032432";
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			AssertEquals(2, header.Bills.Count);

			var sendingAction = new MessageSendingAction(header, ActionCode.Registering);
			AssertEquals(2, sendingAction.MessageSendingObjects.Count);
			var sendingObject1 = sendingAction.MessageSendingObjects[0];
			var sendingObject2 = sendingAction.MessageSendingObjects[1];
			if (sendingObject2.PK == bill1.PK)
			{
				sendingObject1 = sendingAction.MessageSendingObjects[1];
				sendingObject2 = sendingAction.MessageSendingObjects[0];
			}
			AssertEquals(true, sendingObject1.IsRegisteredEditableChildObject(bill1));
			AssertEquals(true, sendingObject2.IsRegisteredEditableChildObject(bill2));

			sendingAction.ResetEditableChildObject();
			AssertEquals(false, sendingObject1.IsRegisteredEditableChildObject(bill1));
			AssertEquals(false, sendingObject2.IsRegisteredEditableChildObject(bill2));
		}

		public void TestMessageSendingObjects()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00032432";
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "MB2";

			var sendingAction = new MessageSendingAction(header, ActionCode.Registering);
			AssertEquals(2, sendingAction.MessageSendingObjects.Count);
			var sendObject1 = sendingAction.MessageSendingObjects[0];
			var sendObject2 = sendingAction.MessageSendingObjects[1];
			if (sendObject2.PK == bill1.PK)
			{
				sendObject1 = sendingAction.MessageSendingObjects[1];
				sendObject2 = sendingAction.MessageSendingObjects[0];
			}
			AssertEquals(bill1.PK, sendObject1.PK);
			AssertEquals(bill2.PK, sendObject2.PK);
		}

		public void TestUpdateMessageSendingAction()
		{
			var header = Factory.New<JPAFRHeader>();
			var messageSendingAction = new MessageSendingAction(header, ActionCode.Registering);
			AssertEquals(ZDateTime.Empty, messageSendingAction.JPM_ETA);
			AssertEquals(ZString.Empty, messageSendingAction.JPM_MasterBillOfLadingNumber);

			var testDate = ZDateTime.UtcNow;
			header.JPH_MasterBillNumber = "MBOL";
			header.JPH_ETA = testDate;
			messageSendingAction.UpdateMessageSendingAction();
			AssertEquals(testDate, messageSendingAction.JPM_ETA);
			AssertEquals("MBOL", messageSendingAction.JPM_MasterBillOfLadingNumber);
		}

		public void TestGetWarningForBillsToSendThatAreWaitingForResponse()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();

			var action = new MessageSendingAction(header, ActionCode.AmendingUpdate);
			AssertEquals("Precondition", 3, action.MessageSendingObjects.Count);
			var billToSend1 = action.MessageSendingObjects[0];
			billToSend1.JPM_Send = ZBool.True;
			billToSend1.JPM_MessageStatus = ZString.Empty;
			billToSend1.JPM_BillOfLadingNumber = "OTT1BOL123";
			var billToSend2 = action.MessageSendingObjects[1];
			billToSend2.JPM_Send = ZBool.True;
			billToSend2.JPM_MessageStatus = ZString.Empty;
			billToSend2.JPM_BillOfLadingNumber = "BOL456";
			var billToSend3 = action.MessageSendingObjects[2];
			billToSend3.JPM_Send = ZBool.True;
			billToSend3.JPM_MessageStatus = ZString.Empty;
			billToSend3.JPM_BillOfLadingNumber = "OTT4";
			AssertEquals("Precondition", 3, action.ObjectsToSend.Count);
			AssertEquals(ZString.Empty, action.GetWarningForBillsToSendThatAreWaitingForResponse());
			billToSend1.JPM_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillAdd;
			billToSend2.JPM_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillUpdate;
			AssertEquals(ValidationConstants.MessageSending.BillsToSendThatAreWaitingForResponseMessage("OTT1BOL123\r\nBOL456"), action.GetWarningForBillsToSendThatAreWaitingForResponse());
			billToSend2.JPM_MessageStatus = MessageStatusList.Codes.ClearHouseBillUpdate;
			billToSend3.JPM_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillDelete;
			AssertEquals(ValidationConstants.MessageSending.BillsToSendThatAreWaitingForResponseMessage("OTT1BOL123\r\nOTT4"), action.GetWarningForBillsToSendThatAreWaitingForResponse());

			billToSend1.JPM_Send = false;
			AssertEquals(ValidationConstants.MessageSending.BillsToSendThatAreWaitingForResponseMessage("OTT4"), action.GetWarningForBillsToSendThatAreWaitingForResponse());
		}

		public void TestBillsToSend()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "MB2";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "MB3";
			var action = new MessageSendingAction(header, ActionCode.AmendingAdd);
			AssertEquals(3, action.MessageSendingObjects.Count);
			action.MessageSendingObjects[1].JPM_Send = false;
			action.MessageSendingObjects[2].JPM_Send = false;
			AssertEquals(1, action.ObjectsToSend.Count);
			AssertEquals(action.MessageSendingObjects[0].JPM_BillOfLadingNumber, action.ObjectsToSend[0].JPM_BillOfLadingNumber);
			action.MessageSendingObjects[2].JPM_Send = true;
			AssertEquals(2, action.ObjectsToSend.Count);
			AssertEquals(action.MessageSendingObjects[0].JPM_BillOfLadingNumber, action.ObjectsToSend[0].JPM_BillOfLadingNumber);
			AssertEquals(action.MessageSendingObjects[2].JPM_BillOfLadingNumber, action.ObjectsToSend[1].JPM_BillOfLadingNumber);
		}

		public void TestHasATDBeenSent_ReadOnly()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = false;

			var action = new MessageSendingAction(header, ActionCode.AmendingAdd);
			AssertEquals(false, action.HasATDBeenSent);
			AssertEquals(false, action.HasATDBeenSent_ReadOnly);

			header.JPH_IsShippingLineEntry = true;
			action = new MessageSendingAction(header, ActionCode.AmendingAdd);
			AssertEquals(false, action.HasATDBeenSent);
			AssertEquals(true, action.HasATDBeenSent_ReadOnly);

			header.LogDepartureTimeRegistration();
			action = new MessageSendingAction(header, ActionCode.AmendingAdd);
			AssertEquals(true, action.HasATDBeenSent);
			AssertEquals(true, action.HasATDBeenSent_ReadOnly);

			action = new MessageSendingAction(header, ActionCode.ChangeDepartureTimeAfterATD);
			AssertEquals(true, action.HasATDBeenSent);
			AssertEquals(false, action.HasATDBeenSent_ReadOnly);

			action = new MessageSendingAction(header, ActionCode.RegisterDepartureTime);
			AssertEquals(true, action.HasATDBeenSent);
			AssertEquals(false, action.HasATDBeenSent_ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			return new MessageSendingAction(header, ActionCode.Registering);
		}
	}
}
