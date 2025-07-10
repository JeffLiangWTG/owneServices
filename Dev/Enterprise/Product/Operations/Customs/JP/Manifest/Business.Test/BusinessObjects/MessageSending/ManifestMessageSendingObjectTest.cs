using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using NUnit.Framework;
using static Enterprise.Customs.JP.Common.JPMessageActionList;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestMessageSendingObject))]
	sealed class ManifestMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValue_Action_HDF01()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			header.AMA_TransportMode = TransportTypeList.Codes.Air;

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			bill2.ABL_BillStatus = JPCustomsStatusList.Codes.REG;
			var bill3 = header.Bills.AddNew();
			bill3.ABL_BillStatus = JPCustomsStatusList.Codes.CAN;
			var bill4 = header.Bills.AddNew();
			bill4.ABL_BillStatus = JPCustomsStatusList.Codes.Deleted;
			var bill5 = header.Bills.AddNew();
			bill5.ABL_BillStatus = JPCustomsStatusList.Codes.AWR;
			var bill6 = header.Bills.AddNew();
			bill6.ABL_BillStatus = JPCustomsStatusList.Codes.AWC;
			var bill7 = header.Bills.AddNew();
			bill7.ABL_BillStatus = JPCustomsStatusList.Codes.AWD;

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HDF01 }))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObjects = sendingObjectParent.SendingObjectsCollection;

				AssertEquals("", sendingObjects[0].Action);
				AssertEquals("", sendingObjects[1].Action);
				AssertEquals("", sendingObjects[2].Action);
				AssertEquals("", sendingObjects[3].Action);
				AssertEquals(JPMessageActionList.Codes.X, sendingObjects[4].Action);
				AssertEquals(JPMessageActionList.Codes.X, sendingObjects[5].Action);
				AssertEquals(JPMessageActionList.Codes.X, sendingObjects[6].Action);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var parent =  new ManifestMessageSendingObjectParent(header);
			var sendingObject = parent.SendingObjectsCollection.First();
			return sendingObject;
		}

		public void TestDefaultMessageType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			ManifestMessageSendingObjectParent sendingObjectParent;
			ManifestMessageSendingObject sendingObject;

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HCH01 }))
			{
				sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				sendingObject = sendingObjectParent.SendingObjectsCollection[0];

				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				AssertEquals(JPProcedureCodeList.Codes.HCH01, sendingObject.MessageType);
			}

			header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HDF01 }))
			{
				sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				sendingObject = sendingObjectParent.SendingObjectsCollection[0];
				AssertEquals(JPProcedureCodeList.Codes.HDF01, sendingObject.MessageType);
			}

			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01 }))
			{
				sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				sendingObject = sendingObjectParent.SendingObjectsCollection[0];
				AssertEquals(JPProcedureCodeList.Codes.NVC01, sendingObject.MessageType);
			}
		}

		public void TestDefaultActionWhenNVC01()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01 }))
			{
				var emptyStatusBill = header.Bills.AddNew();
				var regBill = header.Bills.AddNew();
				var amdBill = header.Bills.AddNew();
				var delBill = header.Bills.AddNew();
				var awgBill = header.Bills.AddNew();
				var awaBill = header.Bills.AddNew();
				var awdBill = header.Bills.AddNew();
				regBill.ABL_BillStatus = JPCustomsStatusList.Codes.REG;
				amdBill.ABL_BillStatus = JPCustomsStatusList.Codes.AMD;
				delBill.ABL_BillStatus = JPCustomsStatusList.Codes.DEL;
				awgBill.ABL_BillStatus = JPCustomsStatusList.Codes.AWR;
				awaBill.ABL_BillStatus = JPCustomsStatusList.Codes.AWA;
				awdBill.ABL_BillStatus = JPCustomsStatusList.Codes.AWD;

				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObjects = sendingObjectParent.SendingObjectsCollection;
				CombineAssertions(() =>
				{
					AssertEquals("Bill with empty customs status should have 9 as its default action value.", NVC01MessageActionList.Codes.Nine, sendingObjects[0].Action);
					AssertEquals("Bill with REG customs status should have 5 as its default action value.", NVC01MessageActionList.Codes.Five, sendingObjects[1].Action);
					AssertEquals("Bill with AMD customs status should have 5 as its default action value.", NVC01MessageActionList.Codes.Five, sendingObjects[2].Action);
					AssertEquals("Bill with DEL customs status should have an empty string as its default action value.", ZString.Empty, sendingObjects[3].Action);
					AssertEquals("Bill with AWR customs status should have 9 as its default action value.", NVC01MessageActionList.Codes.Nine, sendingObjects[4].Action);
					AssertEquals("Bill with AWA customs status should have 9 as its default action value.", NVC01MessageActionList.Codes.Nine, sendingObjects[5].Action);
					AssertEquals("Bill with AWD customs status should have 9 as its default action value.", NVC01MessageActionList.Codes.Nine, sendingObjects[6].Action);
				});
			}
		}

		public void TestDefaultShouldSend()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var sendingObject = new ManifestMessageSendingObjectParent(header).SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
			AssertEquals(true, sendingObject.ShouldSend);

			bill.ABL_BillStatus = JPCustomsStatusList.Codes.Mismatch;
			sendingObject = new ManifestMessageSendingObjectParent(header).SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
			AssertEquals(false, sendingObject.ShouldSend);
		}

		public void TestAction()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var sendingObject = new ManifestMessageSendingObjectParent(header).SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
			var actionInfo = sendingObject.ActionInfo;
			sendingObject.MessageType = JPProcedureCodeList.Codes.HCH01;
			AssertEquals(1, actionInfo.MaxLength);
			Assert(actionInfo.ReadOnly);

			sendingObject.MessageType = JPProcedureCodeList.Codes.HDF01;
			Assert(!actionInfo.ReadOnly);
			AssertEquals("Action", DataBoundResourceStrings.GetDataForProperty(actionInfo).Caption);
		}

		public void TestActionDescription()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var sendingObject = new ManifestMessageSendingObjectParent(header).SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
			sendingObject.Action = HDF01MessageActionList.Codes.D;
			AssertEquals(HDF01MessageActionList.Descriptions.D, sendingObject.ActionDescription);
			AssertEquals("Action Description", DataBoundResourceStrings.GetDataForProperty(sendingObject.ActionDescriptionInfo).Caption);
		}
	}
}
