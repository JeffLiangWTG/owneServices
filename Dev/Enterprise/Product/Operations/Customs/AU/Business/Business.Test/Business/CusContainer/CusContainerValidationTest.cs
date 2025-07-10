using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class CusContainerValidationTest : TestCaseWithFactory
	{
		public void TestValidateCO_ContainerNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec.JE_OverrideFreightDefaults = true;
			var container = dec.CusContainers.AddNew();
			var invoiceLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var pivot = invoiceLine.ContainersPivot.AddNew();
			pivot.C2_CO = container.PK;
			container.CO_ContainerNumber = "CRXU1234568";
			Assert(!container.CO_ContainerNumberInfo.HasNotifications());
			dec.JE_JS = shipment.PK;

			var consol1 = (ForwardingConsol)shipment.Consols.AddNew(typeof(ForwardingConsol));
			consol1.JK_RL_NKLoadPort = "SGSIN";
			consol1.JK_RL_NKDischargePort = "AUSYD";

			var consol2 = (ForwardingConsol)shipment.Consols.AddNew(typeof(ForwardingConsol));
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "NZAKL";

			shipment.JS_RL_NKOrigin = "MYJKG";
			shipment.JS_RL_NKDestination = "NZAKL";
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			string message = "This container does not exist in the freight system.";
			container.Validation.ValidateCO_ContainerNumber();
			AssertHasWarning(container.CO_ContainerNumberInfo, message);

			var jobContainer = consol1.Containers.AddNew();
			jobContainer.JC_ContainerNum = "CRXU1234568";
			container.Validation.ValidateCO_ContainerNumber();
			AssertNoWarning(container.CO_ContainerNumberInfo, message);

			jobContainer.JC_JK = consol2.PK;
			consol2.Containers.Load();
			consol1.Containers.Load();
			container.Validation.ValidateCO_ContainerNumber();
			AssertHasWarning(container.CO_ContainerNumberInfo, message);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			container.Validation.ValidateCO_ContainerNumber();
			AssertNoWarning(container.CO_ContainerNumberInfo, message);

			jobContainer.JC_JK = consol1.PK;
			consol2.Containers.Load();
			consol1.Containers.Load();
			container.Validation.ValidateCO_ContainerNumber();
			AssertHasWarning(container.CO_ContainerNumberInfo, message);
		}

		public void TestValidateCO_ContainerNumberWithPRAMessage()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			Factory.Save();
			EDIMessage message = container.Messages.AddNew();
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageSubType = "SSM";
			Assert("Precondition - no errors", !container.CO_ContainerNumberInfo.HasErrors());
			container.CO_ContainerNumber = "0000000000";
			Assert("Should have an error: " + CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyMustCancelToo, container.CO_ContainerNumberInfo.HasError(CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyMustCancelToo));
		}

		public void TestValidateCO_ContainerNumberWithPRARejectMessage()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			Factory.Save();
			EDIMessage message = container.Messages.AddNew();
			message.EM_ReceiveTransmit = "TRX";
			message.EM_MessageSubType = "SSM";
			EDIMessage message2 = container.Messages.AddNew(); // Rejection
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_MessageSubType = "REJ";
			Assert("Precondition - no errors", !container.CO_ContainerNumberInfo.HasErrors());
			container.CO_ContainerNumber = "0000000000";
			Assert("Should have no errors", !container.CO_ContainerNumberInfo.HasErrors());
		}

		public void TestValidateCO_ContainerNumberWithManyPRAMessages()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			Factory.Save();
			EDIMessage message1 = container.Messages.AddNew(); // Original
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageSubType = "SSM";
			EDIMessage message2 = container.Messages.AddNew(); // Rejection
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_MessageSubType = "REJ";
			EDIMessage message3 = container.Messages.AddNew(); // Original
			message3.EM_ReceiveTransmit = "TRX";
			message3.EM_MessageSubType = "SSM";

			Assert("Precondition - no errors", !container.CO_ContainerNumberInfo.HasErrors());
			container.CO_ContainerNumber = "0000000000";
			Assert("Should have an error: " + CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyMustCancelToo, container.CO_ContainerNumberInfo.HasError(CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyMustCancelToo));
		}

		public void TestValidateCO_ContainerNumberWithCancelPRA()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			Factory.Save();
			EDIMessage message1 = container.Messages.AddNew(); // Original
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageSubType = "SSM";
			EDIMessage message2 = container.Messages.AddNew(); // Acceptance
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_MessageSubType = PRAConstants.MessageAcknowledged;
			EDIMessage message3 = container.Messages.AddNew(); // Cancellation
			message3.EM_ReceiveTransmit = "TRX";
			message3.EM_MessageSubType = "SCN";
			EDIMessage message4 = container.Messages.AddNew(); // Acceptance
			message4.EM_ReceiveTransmit = "RCV";
			message4.EM_MessageSubType = PRAConstants.MessageAcknowledged;

			Assert("Precondition - no errors", !container.CO_ContainerNumberInfo.HasErrors());
			container.CO_ContainerNumber = "0000000000";
			Assert("Should have no errors", !container.CO_ContainerNumberInfo.HasErrors());
		}

		public void TestValidateJC_ContainerNumWithCancelPRA()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			Factory.Save();
			EDIMessage message1 = container.Messages.AddNew(); // Original
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageSubType = "SSM";
			EDIMessage message2 = container.Messages.AddNew(); // Acceptance
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_MessageSubType = PRAConstants.MessageAcknowledged;
			EDIMessage message3 = container.Messages.AddNew(); // Cancellation
			message3.EM_ReceiveTransmit = "TRX";
			message3.EM_MessageSubType = "SCN";

			Assert("Precondition - no errors", !container.CO_ContainerNumberInfo.HasErrors());
			container.CO_ContainerNumber = "0000000000";
			Assert("Should have an error: " + CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyToCancellation, container.CO_ContainerNumberInfo.HasError(CommonContainerValidation.CannotChangeContainerWhileWaitingForAReplyToCancellation));
		}

		public void TestValidateJC_ContainerNumAfterOriginalAndAcceptPRA()
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRXU1234568";
			Factory.Save();
			EDIMessage message1 = container.Messages.AddNew(); // Original
			message1.EM_ReceiveTransmit = "TRX";
			message1.EM_MessageSubType = "SSM";
			EDIMessage message2 = container.Messages.AddNew(); // Acceptance
			message2.EM_ReceiveTransmit = "RCV";
			message2.EM_MessageSubType = PRAConstants.MessageAcknowledged;

			Assert("Precondition - no errors", !container.CO_ContainerNumberInfo.HasErrors());
			container.CO_ContainerNumber = "0000000000";
			Assert("Should have an error: " + CommonContainerValidation.CannotChangeContainerWithoutCancellingPRAFirst, container.CO_ContainerNumberInfo.HasError(CommonContainerValidation.CannotChangeContainerWithoutCancellingPRAFirst));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
		}

		JobDeclaration declaration;
	}
}
