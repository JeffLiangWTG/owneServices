using System;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Italy.Testing
{
	public class EInvoicingEventMessageITProcessorTest : TestCaseWithFactory
	{
		public void TestRicevFilMessageProcessing()
		{
			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>rispostaSdIRiceviFile</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);

			processor.Process();

			AssertEquals("Postcondition", 0, logger.Logs.Count());
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
		}

		[TestDate(2018, 01, 09, 08, 30, 0)]
		public void TestRicevutaConsegnaMessageProcessing()
		{
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			pivot.AIP_ErrorDescription = "Some random error";
			batch.AIB_GovernmentAllocatedNumber = "444555666";
			batch.AIB_EHubAllocatedNumber = "111222333";
			Factory.Save();

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>ricevutaConsegna</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
			<Context>
				<Type>ResponseMessage</Type>
				<Value>{base64EncodedResponseMessage}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", "Some random error", pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);

			processor.Process();

			AssertEquals("Postcondition", 0, logger.Logs.Count());
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Delivered, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 1, invoice.DocManagerInfo.AllEDocs.Count);
			AssertEquals("PostCondition", "ricevutaConsegna_20180109083000.xml", invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("PostCondition", Core.Constants.ReferenceTypes.Accounting, invoice.DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("PostCondition", new ZBlob(Encoding.UTF8.GetBytes(testResponseMessage)), invoice.DocManagerInfo.AllEDocs[0].ImageData);
		}

		[TestDate(2018, 01, 09, 08, 30, 0)]
		public void TestNotificaMancataConsegnaMessageProcessing()
		{
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			batch.AIB_GovernmentAllocatedNumber = "444555666";
			batch.AIB_EHubAllocatedNumber = "111222333";
			Factory.Save();

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>notificaMancataConsegna</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
			<Context>
				<Type>ResponseMessage</Type>
				<Value>{base64EncodedResponseMessage}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);

			processor.Process();

			AssertEquals("Postcondition", 0, logger.Logs.Count());
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 1, invoice.DocManagerInfo.AllEDocs.Count);
			AssertEquals("PostCondition", "notificaMancataConsegna_20180109083000.xml", invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("PostCondition", Core.Constants.ReferenceTypes.Accounting, invoice.DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("PostCondition", new ZBlob(Encoding.UTF8.GetBytes(testResponseMessage)), invoice.DocManagerInfo.AllEDocs[0].ImageData);
		}

		[TestDate(2018, 01, 09, 08, 30, 0)]
		public void TestNotificaScartoMessageProcessing()
		{
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			batch.AIB_GovernmentAllocatedNumber = "444555666";
			batch.AIB_EHubAllocatedNumber = "111222333";
			Factory.Save();

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>notificaScarto</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
			<Context>
				<Type>ResponseMessage</Type>
				<Value>{base64EncodedResponseMessage}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);

			processor.Process();

			AssertEquals("Postcondition", 1, logger.Logs.Count());
			AssertEquals("Postcondition", emailSendingUnsuccessfulMessage, logger.Logs.ElementAt(0).Message);
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Failed, pivot.AIP_Status);
			AssertEquals("Postcondition", "Scarto", pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 1, invoice.DocManagerInfo.AllEDocs.Count);
			AssertEquals("PostCondition", "notificaScarto_20180109083000.xml", invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("PostCondition", Core.Constants.ReferenceTypes.Accounting, invoice.DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("PostCondition", new ZBlob(Encoding.UTF8.GetBytes(testResponseMessage)), invoice.DocManagerInfo.AllEDocs[0].ImageData);
		}

		[TestDate(2018, 01, 09, 08, 30, 0)]
		public void TestNotificaEsitoMessageProcessing()
		{
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			pivot.AIP_ErrorDescription = "Some random error";
			batch.AIB_GovernmentAllocatedNumber = "444555666";
			batch.AIB_EHubAllocatedNumber = "111222333";
			Factory.Save();

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>notificaEsito</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
			<Context>
				<Type>ResponseMessage</Type>
				<Value>{base64EncodedResponseMessage}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", "Some random error", pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);

			processor.Process();

			AssertEquals("Postcondition", 0, logger.Logs.Count());
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Succeed, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 1, invoice.DocManagerInfo.AllEDocs.Count);
			AssertEquals("PostCondition", "notificaEsito_20180109083000.xml", invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("PostCondition", Core.Constants.ReferenceTypes.Accounting, invoice.DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("PostCondition", new ZBlob(Encoding.UTF8.GetBytes(testResponseMessage)), invoice.DocManagerInfo.AllEDocs[0].ImageData);
		}

		[TestDate(2018, 01, 09, 08, 30, 0)]
		public void TestNotificaDecorrenzaTerminiMessageProcessing()
		{
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			batch.AIB_GovernmentAllocatedNumber = "444555666";
			batch.AIB_EHubAllocatedNumber = "111222333";
			Factory.Save();

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>notificaDecorrenzaTermini</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
			<Context>
				<Type>ResponseMessage</Type>
				<Value>{base64EncodedResponseMessage}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);

			processor.Process();

			AssertEquals("Postcondition", 1, logger.Logs.Count());
			AssertEquals("Postcondition", emailSendingUnsuccessfulMessage, logger.Logs.ElementAt(0).Message);
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Failed, pivot.AIP_Status);
			AssertEquals("Postcondition", "Decorrenza Termini", pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 1, invoice.DocManagerInfo.AllEDocs.Count);
			AssertEquals("PostCondition", "notificaDecorrenzaTermini_20180109083000.xml", invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("PostCondition", Core.Constants.ReferenceTypes.Accounting, invoice.DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("PostCondition", new ZBlob(Encoding.UTF8.GetBytes(testResponseMessage)), invoice.DocManagerInfo.AllEDocs[0].ImageData);
		}

		[TestDate(2018, 01, 09, 08, 30, 0)]
		public void TestAttestazioneTranmissioneFatturaMessageProcessing()
		{
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			batch.AIB_GovernmentAllocatedNumber = "444555666";
			batch.AIB_EHubAllocatedNumber = "111222333";
			Factory.Save();

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>attestazioneTranmissioneFattura</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
			<Context>
				<Type>ResponseMessage</Type>
				<Value>{base64EncodedResponseMessage}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);

			processor.Process();

			AssertEquals("Postcondition", 1, logger.Logs.Count());
			AssertEquals("Postcondition", emailSendingUnsuccessfulMessage, logger.Logs.ElementAt(0).Message);
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Failed, pivot.AIP_Status);
			AssertEquals("Postcondition", "Attentazione Trasmissione Fattura", pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 1, invoice.DocManagerInfo.AllEDocs.Count);
			AssertEquals("PostCondition", "attestazioneTranmissioneFattura_20180109083000.xml", invoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("PostCondition", Core.Constants.ReferenceTypes.Accounting, invoice.DocManagerInfo.AllEDocs[0].DocType);
			AssertEquals("PostCondition", new ZBlob(Encoding.UTF8.GetBytes(testResponseMessage)), invoice.DocManagerInfo.AllEDocs[0].ImageData);
		}

		public void TestIRJEventMessageProcessing()
		{
			var expectedErrorDescription = "Failed to send message to government";

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IRJ</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType/>
			<Reason>Failed to send message to government</Reason>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);

			processor.Process();

			AssertEquals("Postcondition", 0, logger.Logs.Count());
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Failed, pivot.AIP_Status);
			AssertEquals("Postcondition", expectedErrorDescription, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
		}

		public void TestEInvoiceBatchHasNoTransactionPivots()
		{
			batch.AIB_Status = Core.Constants.EInvoicingBatchState.Sent;
			pivot.Delete();
			Factory.Save();

			AssertEquals("Precondition", 0, batch.TransactionPivots.Count);
			AssertEquals("Precodnition", Core.Constants.EInvoicingBatchState.Sent, batch.AIB_Status);

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>rispostaSdIRiceviFile</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
			AssertEquals("Precondition", ZString.Empty, ErrorReporter.LastKeyReported);

			processor.Process();

			AssertEquals("PostCondition", "EInvoicingEventMessageITProcessor_PivotNotFound", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "No update performed due to transaction pivot not found for invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		public void TestProcessFailedMessageForaSucceededPivot()
		{
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			pivot.AIP_Status = Core.Constants.EInvoicingPivotState.Succeed;
			batch.AIB_GovernmentAllocatedNumber = "444555666";
			batch.AIB_EHubAllocatedNumber = "111222333";
			Factory.Save();

			var testResponseMessage = "This is my test response message";
			var base64EncodedResponseMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(testResponseMessage));

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>notificaScarto</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
			<Context>
				<Type>ResponseMessage</Type>
				<Value>{base64EncodedResponseMessage}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Succeed, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);

			processor.Process();

			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Warning && x.Message == "No update performed due to transaction pivot having 'SUC' status for invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Succeed, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		public void TestMessageSubTypeNotAvailable()
		{
			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType></MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
			<Context>
				<Type>ResponseMessage</Type>
				<Value>{base64EncodedResponseMessage}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
			AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);

			processor.Process();

			AssertEquals("PostCondition", "EInvoicingEventMessageITProcessor_EmptyMessageSubType", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "No update performed due to universal event not containing message sub type for invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		public void TestUnknownMessageType()
		{
			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>UnknownMessageType</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
			<Context>
				<Type>ResponseMessage</Type>
				<Value>{base64EncodedResponseMessage}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
			AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);

			processor.Process();

			AssertEquals("PostCondition", "EInvoicingEventMessageITProcessor_UnknownMessageSubType", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "No update performed due to invalid message sub type [UnknownMessageType] found for invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		public void TestUniversalEventHasNoResponseMessageContext()
		{
			batch.AIB_GovernmentAllocatedNumber = "444555666";
			batch.AIB_EHubAllocatedNumber = "111222333";
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			Factory.Save();

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>ricevutaConsegna</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
			AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);

			processor.Process();

			AssertEquals("", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Warning && x.Message == "Response Message was not attached to eDocs due to context collection not having Response Message Context for invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Delivered, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		public void TestUniversalEventResponseMessageContextValueIsEmpty()
		{
			batch.AIB_GovernmentAllocatedNumber = "444555666";
			batch.AIB_EHubAllocatedNumber = "111222333";
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			Factory.Save();

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>ricevutaConsegna</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
			<Context>
				<Type>ResponseMessage</Type>
				<Value></Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
			AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);

			processor.Process();

			AssertEquals("", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Warning && x.Message == "Response Message was not attached to eDocs due to Response Message Context value was empty for invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Delivered, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		public void TestRelatedTransactionCouldNotBeFound()
		{
			batch.AIB_GovernmentAllocatedNumber = "444555666";
			batch.AIB_EHubAllocatedNumber = "111222333";
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			pivot.AIP_ParentID = new Guid("becbd841-8715-4cf8-80b9-1a2ddc720d8a");
			Factory.Save();

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>ricevutaConsegna</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
			<Context>
				<Type>ResponseMessage</Type>
				<Value>{base64EncodedResponseMessage}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			ErrorReporter.Clear();
			AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);

			processor.Process();

			AssertEquals("PostCondition", "EInvoicingEventMessageITProcessor_TransactionNotFound", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == $"Response Message was not attached to eDocs due to related transaction was not found for pivot in invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", ZDateTime.BrettsBirthday, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			ErrorReporter.Clear();
		}

		public void TestContextCollectionNotAvailable()
		{
			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>rispostaSdIRiceviFile</MessageSubType>
		</EventParameters>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
			AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);

			processor.Process();

			AssertEquals("PostCondition", "EInvoicingEventMessageITProcessor_MissingContextCollection", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "Government Allocated Number and eHub Allocated Number was not updated due to universal event message not having context collection for invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		public void TestGovernmnetAllocatedNumberContextNotAvailable()
		{
			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>rispostaSdIRiceviFile</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
			AssertEquals("PreCondition", ZString.Empty, ErrorReporter.LastKeyReported);

			processor.Process();

			AssertEquals("PostCondition", "EInvoicingEventMessageITProcessor_MissingGovernmentAllocatedNumberContext", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "Government Allocated Number was not updated due to context collection not having Government Allocated Number Context for invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		public void TestGovernmnetAllocatedNumberContextHasNoValue()
		{
			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>rispostaSdIRiceviFile</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value></Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
			AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);

			processor.Process();

			AssertEquals("PostCondition", "EInvoicingEventMessageITProcessor_GovernmentAllocatedNumberContextHasNoValue", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "Government Allocated Number was not updated due to Government Allocated Number Context value was empty for invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", "111222333", batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		public void TestEHubAllocatedNumberContextNotAvailable()
		{
			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>rispostaSdIRiceviFile</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
			AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);

			processor.Process();

			AssertEquals("PostCondition", "EInvoicingEventMessageITProcessor_MissingEHubAllocatedNumberContext", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "eHub Allocated Number was not updated due to context collection not having eHub Allocated Number Context for invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		public void TestEHubAllocatedNumberContextHasNoValue()
		{
			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>rispostaSdIRiceviFile</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value></Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", ZDateTime.Empty, pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Precondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Precondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
			AssertEquals(ZString.Empty, ErrorReporter.LastKeyReported);

			processor.Process();

			AssertEquals("PostCondition", "EInvoicingEventMessageITProcessor_EHubAllocatedNumberContextHasNoValue", ErrorReporter.LastKeyReported);
			AssertEquals(1, logger.Logs.Count(x => x.Type == Enterprise.Integration.LogType.Error && x.Message == "eHub Allocated Number was not updated due to eHub Allocated Number Context value was empty for invoice batch 1 in Eagle Datamation International."));
			AssertEquals("Postcondition", "444555666", batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", new ZDateTime(2018, 1, 9, 9, 30, 10), pivot.AIP_LastResponseReceivedUtc);
			AssertEquals("Postcondition", Core.Constants.EInvoicingPivotState.Sent, pivot.AIP_Status);
			AssertEquals("Postcondition", ZString.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		public void TestEDocsFactoryIsSetupForSavedWithMainFactory()
		{
			pivot.AIP_LastResponseReceivedUtc = ZDateTime.BrettsBirthday;
			pivot.AIP_ErrorDescription = "Some random error";
			batch.AIB_GovernmentAllocatedNumber = "444555666";
			batch.AIB_EHubAllocatedNumber = "111222333";
			Factory.Save();

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = $@"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>ricevutaConsegna</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
			<Context>
				<Type>ResponseMessage</Type>
				<Value>{base64EncodedResponseMessage}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			var docFactory = (BusinessObjectFactory)invoice.DocManagerInfo.MasterFactory;
			Assert(!Factory.ChildFactories.Contains(docFactory));

			processor.Process();

			Assert("eDocs factory should be saved with pivot factory.", Factory.ChildFactories.Contains(docFactory));
		}

		public void TestNoExceptionOnEventForDiscardedBatch()
		{
			batch.AIB_Status = Core.Constants.EInvoicingBatchState.Discarded;
			pivot.Delete();
			Factory.Save();

			AssertEquals("Precondition", 0, batch.TransactionPivots.Count);
			AssertEquals("Precodnition", Core.Constants.EInvoicingBatchState.Discarded, batch.AIB_Status);

			var inMessage = EDIMessageTestFactory.New(Factory);
			inMessage.EM_MessageText = @"
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>AccEInvoicingBatch</Type>
					<Key>1</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2018-01-09T09:30:10</EventTime>
		<EventType>IAK</EventType>
		<EventParameters>
			<MessageType>IT</MessageType>
			<MessageSubType>rispostaSdIRiceviFile</MessageSubType>
		</EventParameters>
		<ContextCollection>
			<Context>
				<Type>CompanyCode</Type>
				<Value>EDI</Value>
			</Context>
			<Context>
				<Type>eHubAllocatedNumber</Type>
				<Value>111222333</Value>
			</Context>
			<Context>
				<Type>GovernmentAllocatedNumber</Type>
				<Value>444555666</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
			var universalEvent = inMessage.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = new EInvoicingEventMessageITProcessor(logger, EDIMessage, universalEvent, batch);

			AssertEquals("Precondition", 0, logger.Logs.Count());
			AssertEquals("Precondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Precondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Precondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
			AssertEquals("Precondition", ZString.Empty, ErrorReporter.LastKeyReported);

			processor.Process();

			AssertEquals("Postcondition", ZString.Empty, ErrorReporter.LastKeyReported);
			AssertEquals(0, logger.Logs.Count());
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_GovernmentAllocatedNumber);
			AssertEquals("Postcondition", ZString.Empty, batch.AIB_EHubAllocatedNumber);
			AssertEquals("Postcondition", 0, invoice.DocManagerInfo.AllEDocs.Count);
			ErrorReporter.Clear();
		}

		EDIMessage EDIMessage
		{
			get
			{
				if (ediMessage == null)
				{
					ediMessage = EDIMessageTestFactory.New(Factory);
				}
				return ediMessage;
			}
		}
		EDIMessage ediMessage;

		protected override void SetUp()
		{
			base.SetUp();
			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			objectCreator = new TestObjectCreator(Factory);

			invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Sent);
			Factory.Save();
		}

		readonly string emailSendingUnsuccessfulMessage = $@"Error: Email Notification was not sent for Eagle Datamation International. Email (Subject: 'E-Reporting error notification for Transaction AR INV 00001000 [EDI]', For Group: {AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.HumanReadableRegistryPath()}) must have at least one recipient, CC or BCC
E-Reporting Email Notification task completed.
";
		static readonly string testResponseMessage = "This is my test response message";
		static readonly string base64EncodedResponseMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(testResponseMessage));
		InvoicingBase invoice;
		AccEInvoicingBatch batch;
		AccEInvoicingTransactionPivot pivot;
		IXmlSessionTracker logger;
		TestObjectCreator objectCreator;
	}
}
