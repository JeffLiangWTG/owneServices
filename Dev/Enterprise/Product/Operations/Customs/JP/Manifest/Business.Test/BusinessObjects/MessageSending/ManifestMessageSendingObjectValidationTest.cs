using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;
using static Enterprise.Customs.JP.Common.JPMessageActionList;

namespace Enterprise.Customs.JP.Manifest.Business.Testing
{
	[TestedType(typeof(ManifestMessageSendingObjectValidation))]
	sealed class ManifestMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckMessageType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
			var sendingObject = sendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
			AssertNotNull(sendingObject);

			sendingObject.MessageType = "HCH01";
			AssertNoMessageError(sendingObject.MessageTypeInfo, ListValidation.InvalidCodeMessageError);

			sendingObject.MessageType = "00000";
			AssertHasMessageError(sendingObject.MessageTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckAction()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();
			var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
			var sendingObject = sendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;

			var info = sendingObject.ActionInfo;
			ValidationTestHelper.AssertInvalidCodeMessageError(info, ["1"], ["", "C", "D", "X"]);

			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			header.AMA_TransportMode = "SEA";

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01 }))
			{
				sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				sendingObject = sendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
				info = sendingObject.ActionInfo;
				sendingObject.Action = string.Empty;
				sendingObject.Validation.ValidateAction();
				AssertHasError(info, "Please enter a Message Action.");

				sendingObject.Action = NVC01MessageActionList.Codes.Nine;
				AssertNoError(info, "Please enter a Message Action.");
			}
		}

		public void TestCheckAction_HDF01()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HDF;
			var bill = header.Bills.AddNew();

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HDF01 }))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObject = sendingObjectParent.SendingObjectsCollection[0];

				var expectedMessageError = "This bill is currently awaiting a response from NACCS.";
				AssertMessageError(JPCustomsStatusList.Codes.AWR);
				AssertMessageError(JPCustomsStatusList.Codes.AWC);
				AssertMessageError(JPCustomsStatusList.Codes.AWD);

				void AssertMessageError(string billStatus)
				{
					bill.ABL_BillStatus = billStatus;
					sendingObject.Action = JPMessageActionList.Codes.X;
					AssertNoMessageError(sendingObject.ActionInfo, expectedMessageError);

					sendingObject.Action = JPMessageActionList.Codes.C;
					AssertHasMessageError(sendingObject.ActionInfo, expectedMessageError);
				}
			}
		}

		public void TestCheckShouldSend()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			var bill = header.Bills.AddNew();
			ManifestMessageSendingObjectParent sendingObjectParent;
			ManifestMessageSendingObject sendingObject;

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.NVC01 }))
			{
				sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				sendingObject = sendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;

				var info = sendingObject.ShouldSendInfo;
				var expectedErrorMessage = "The Customs Status of selected bill is 'REG' or 'AMD', do you still want to send this message?";

				sendingObject.Action = NVC01MessageActionList.Codes.Nine;
				bill.ABL_BillStatus = JPCustomsStatusList.Codes.REG;
				sendingObject.Validation.ValidateShouldSend();
				AssertHasWarning(info, expectedErrorMessage);

				bill.ABL_BillStatus = JPCustomsStatusList.Codes.AMD;
				sendingObject.Validation.ValidateShouldSend();
				AssertHasWarning(info, expectedErrorMessage);

				bill.ABL_BillStatus = JPCustomsStatusList.Codes.DEL;
				sendingObject.Validation.ValidateShouldSend();
				AssertNoWarning(info, expectedErrorMessage);

				expectedErrorMessage = "The Customs Status of selected bill is 'DEL', do you still want to send this message?";
				sendingObject.Action = NVC01MessageActionList.Codes.Five;
				sendingObject.Validation.ValidateShouldSend();
				AssertHasWarning(info, expectedErrorMessage);

				sendingObject.Action = NVC01MessageActionList.Codes.One;
				sendingObject.Validation.ValidateShouldSend();
				AssertHasWarning(info, expectedErrorMessage);

				bill.ABL_BillStatus = JPCustomsStatusList.Codes.AMD;
				sendingObject.Validation.ValidateShouldSend();
				AssertNoWarning(info, expectedErrorMessage);

				expectedErrorMessage = "The selected bill does not contain Customs Status, do you still you want to send this message?";
				bill.ABL_BillStatus = string.Empty;
				sendingObject.Validation.ValidateShouldSend();
				AssertHasWarning(info, expectedErrorMessage);

				bill.ABL_BillStatus = JPCustomsStatusList.Codes.AMD;
				sendingObject.Validation.ValidateShouldSend();
				AssertNoWarning(info, expectedErrorMessage);

				expectedErrorMessage = "You've already sent a message to NACCS and are currently waiting for a response.";
				sendingObject.Validation.ValidateShouldSend();
				AssertNoMessageError(info, expectedErrorMessage);

				bill.ABL_BillStatus = JPCustomsStatusList.Codes.AWA;
				sendingObject.Validation.ValidateShouldSend();
				AssertHasMessageError(info, expectedErrorMessage);

				bill.ABL_BillStatus = JPCustomsStatusList.Codes.AWD;
				sendingObject.Validation.ValidateShouldSend();
				AssertHasMessageError(info, expectedErrorMessage);

				bill.ABL_BillStatus = JPCustomsStatusList.Codes.AWR;
				sendingObject.Validation.ValidateShouldSend();
				AssertHasMessageError(info, expectedErrorMessage);
			}

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HDE }))
			{
				header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
				header.AMA_MasterBill = "123456";
				header.MasterBill.ABL_BillStatus = JPMasterBillStatusList.Codes.END;
				var expectedErrorMessage = "Master Bill 123456 has already been registered as completed (END – All House Bills Sent).";
				sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				sendingObject = sendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
				sendingObject.ShouldSend = true;

				AssertHasMessageError(sendingObject.ShouldSendInfo, expectedErrorMessage);
			}
		}

		public void TestCheckReason()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;
			var bill = header.Bills.AddNew();

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.CHA }))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObject = sendingObjectParent.SendingObjectsCollection[0];

				sendingObject.ShouldSend = true;
				AssertHasErrorContaining(sendingObject.ReasonInfo, MandatoryValidation.MustBeEntered);

				sendingObject.Reason = ReasonList.Codes.ADD;
				AssertNoErrorContaining(sendingObject.ReasonInfo, MandatoryValidation.MustBeEntered);

				var expectedWarning = "The reason 'MST - Input Error' should only be applied once the bill has been registered. However, this bill has not been registered.";
				AssertNoWarningContaining(sendingObject.ReasonInfo, expectedWarning);
				sendingObject.Reason = ReasonList.Codes.MST;
				AssertHasWarningContaining(sendingObject.ReasonInfo, expectedWarning);

				expectedWarning = "The reason 'ADD - Addition' should only be applied prior to the bill’s registration. However, this bill has already been registered.";
				bill.ABL_BillStatus = JPCustomsStatusList.Codes.AWC;
				AssertNoWarningContaining(sendingObject.ReasonInfo, expectedWarning);
				sendingObject.Reason = ReasonList.Codes.ADD;
				AssertHasWarningContaining(sendingObject.ReasonInfo, expectedWarning);
			}
		}

		public void TestCheckShouldSend_ShouldBeCheckedWhenEndSendMessage()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_Nature = JPJobMessageTypeList.Codes.Import;
			header.AMA_TransportMode = TransportTypeList.Codes.Air;
			var bill = header.Bills.AddNew();

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext()))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObjectCollection = sendingObjectParent.SendingObjectsCollection;
				sendingObjectParent.EndSendMessage = true;
				var sendingObject = sendingObjectCollection.First() as ManifestMessageSendingObject;

				var expectedError = "When the END checkbox is ticked, all the Send? checkboxes where not ACK should be ticked.";
				sendingObject.ShouldSend = false;
				AssertHasMessageError(sendingObject.ShouldSendInfo, expectedError);

				sendingObject.ShouldSend = true;
				AssertNoMessageError(sendingObject.ShouldSendInfo, expectedError);

				bill.ABL_BillStatus = JPCustomsStatusList.Codes.ACK;
				sendingObject.ShouldSend = false;
				AssertNoMessageError(sendingObject.ShouldSendInfo, expectedError);
			}
		}

		public void TestCheckShouldSend_HCH01MaxCount()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;

			for (var i = 0; i < 23; i++)
			{
				header.Bills.AddNew();
			}

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HCH01 } ))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObjectCollection = sendingObjectParent.SendingObjectsCollection;

				var expectedError = "A maximum of 20 HAWBs can be included in the message.";
				for (var i = 0; i < 20; i++)
				{
					var sendingObject = sendingObjectCollection[i];
					sendingObject.Validation.ValidateShouldSend();
					AssertNoMessageError(sendingObject.ShouldSendInfo, expectedError);
				}

				for (var i = 20; i < 23; i++)
				{
					var sendingObject = sendingObjectCollection[i];
					sendingObject.Validation.ValidateShouldSend();
					AssertHasError(sendingObject.ShouldSendInfo, expectedError);
				}

				expectedError = "A maximum of 19 HAWBs can be included in the message when it is marked as END.";
				sendingObjectParent.EndSendMessage = true;
				for (var i = 0; i < 19; i++)
				{
					var sendingObject = sendingObjectCollection[i];
					sendingObject.Validation.ValidateShouldSend();
					AssertNoMessageError(sendingObject.ShouldSendInfo, expectedError);
				}

				for (var i = 19; i < 23; i++)
				{
					var sendingObject = sendingObjectCollection[i];
					sendingObject.Validation.ValidateShouldSend();
					AssertHasError(sendingObject.ShouldSendInfo, expectedError);
				}
			}
		}

		public void TestCheckShouldSend_CHAMaxCount()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = JPManifestTypeCodeList.Codes.HCH;

			for (var i = 0; i < 23; i++)
			{
				header.Bills.AddNew();
			}

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.CHA }))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				var sendingObjectCollection = sendingObjectParent.SendingObjectsCollection;

				var expectedError = "A maximum of 20 HAWBs can be included in the message.";
				for (var i = 0; i < 20; i++)
				{
					var sendingObject = sendingObjectCollection[i];
					sendingObject.Validation.ValidateShouldSend();
					AssertNoMessageError(sendingObject.ShouldSendInfo, expectedError);
				}

				for (var i = 20; i < 23; i++)
				{
					var sendingObject = sendingObjectCollection[i];
					sendingObject.Validation.ValidateShouldSend();
					AssertHasError(sendingObject.ShouldSendInfo, expectedError);
				}
			}
		}
	}
}
