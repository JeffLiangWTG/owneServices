using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public abstract class GEIEDIInterchangeCreatorTest : EDIInterchangeCreatorForEInvoicingBatchBaseTest
	{
		protected abstract EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(Func<IAccEInvoiceBatchToGEIConverter> converter = null, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null, AccEInvoicingBatch batchWithErrors = null);

		protected abstract string CountryCode { get; }

		protected abstract AccEInvoicingBatch GetBatchWithReadyStatus();

		protected virtual string ExpectedServicePoint => "GLB_ELEC_INVOICING";
		protected virtual string ExpectedCommunicationTransport => EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;

		public override void TestGetEInvoicingServicePoint()
		{
			var processor = GetInterchangeProcessor();
			AssertEquals(ExpectedServicePoint, processor.EInvoicingServicePoint);
		}

		public override void TestGetCommunicationMode()
		{
			var processor = GetInterchangeProcessor();

			AssertType(typeof(NonPersistentEDICommunicationMode), processor.CommunicationsMode);
			AssertEquals(ExpectedServicePoint, processor.CommunicationsMode.EK_Destination);
			AssertEquals(EDICommunicationsModeFileFormatList.Codes.XML, processor.CommunicationsMode.EK_FileFormat);
			AssertEquals(ExpectedCommunicationTransport, processor.CommunicationsMode.EK_CommunicationsTransport);
		}

		public override void TestSavesWithExceptionHandling()
		{
			GetBatchWithReadyStatus();
			var logger = new TestServiceLogger();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = GetInterchangeProcessor(converter);
				processor.AdditionalActionDuringSave_ForTestOnly += (_, factories) => throw new Exception("This is a generic exception that should be rethrown by exception reporter if it is used");
				var ex = AssertExceptionThrown<Exception>(() => processor.Process(logger));
				AssertEquals("This is a generic exception that should be rethrown by exception reporter if it is used", ex.Message);
			}
		}

		public void TestOriginalExceptionIsLogged_WhenBatchProcessingFails()
		{
			var batch = GetBatchWithReadyStatus();
			var parentTableCode = batch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().First().AIP_ParentTableCode;
			var logger = new TestServiceLogger();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var processor = new MockEDIInterchangeCreatorForExceptionHandling(GlbCompany.CurrentCompany, parentTableCode);
				processor.AdditionalActionDuringSave_ForTestOnly += (e, factories) => throw new Exception("This is a generic exception that should be rethrown by exception reporter if it is used");

				var ex = AssertExceptionThrown<ApplicationException>(() => processor.Process(logger));
				AssertEquals("The 2nd exception (thrown from PerformIfBatchProcessingFails()) should be thrown from Process()", "Boom! Exception thrown from PerformIfBatchProcessingFails()", ex.Message);

				var logMessages = logger.ToString();
				AssertContains("The details of 1st exception (thrown from SaveChanges()) should be logged", "This is a generic exception that should be rethrown by exception reporter if it is used", logMessages);
			}
		}

		public override void TestProcessSingleBatch()
		{
			var batch = GetBatchWithReadyStatus();

			AssertEquals(EInvoicingBatchState.Ready, batch.AIB_Status);
			var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			var logger = new TestServiceLogger();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = GetInterchangeProcessor(converter);
				processor.Process(logger);
			}

			AssertEquals(EInvoicingBatchState.Sent, batch.AIB_Status);
			ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("expect 1 interchange created", 1, ediInterchanges.Length);
			EInvoicingTestHelper.AssertGEIEInvoicingEDIInterchange(ediInterchanges[0], expectedMessageType: EInvoiceAPICommandList.Codes.GenerateInvoiceRequest, expectedTo: ExpectedServicePoint);

			ediMessages = ediInterchanges.First().LoadMessages();
			AssertEquals("expect 1 messages created", 1, ediMessages.Length);
			EInvoicingTestHelper.AssertGEIEInvoicingEDIMessage(ediMessages[0], GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, expectedMessageType: EInvoiceAPICommandList.Codes.GenerateInvoiceRequest);
			AssertContains(string.Format("Universal Transaction batch [{0}] of company [{1}] (organization proxy [{2}]) has been successfully queued for delivery.", batch.AIB_BatchNumber, GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.OrgProxy.OH_Code), logger.ToString());
		}

		public override void TestProcessMultipleBatches()
		{
			var batch1 = GetBatchWithReadyStatus();
			var batch2 = GetBatchWithReadyStatus();

			var logger = new TestServiceLogger();
			AssertEquals(EInvoicingBatchState.Ready, batch1.AIB_Status);
			AssertEquals(EInvoicingBatchState.Ready, batch2.AIB_Status);
			var ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = GetInterchangeProcessor(converter);
				processor.Process(logger);
			}

			AssertEquals(EInvoicingBatchState.Sent, batch1.AIB_Status);
			AssertEquals(EInvoicingBatchState.Sent, batch2.AIB_Status);
			ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("expect 2 interchanges created", 2, ediInterchanges.Length);
			var interchange1 = ediInterchanges[0];
			var interchange2 = ediInterchanges[1];
			ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("expect 2 messages created", 2, ediMessages.Length);
			var message1 = ediMessages.First(x => x.EM_EI == interchange1.PK);
			var message2 = ediMessages.First(x => x.EM_EI == interchange2.PK);

			AssertContains(string.Format("Universal Transaction batch [{0}] of company [{1}] (organization proxy [{2}]) has been successfully queued for delivery.", batch1.AIB_BatchNumber, GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.OrgProxy.OH_Code), logger.ToString());
			AssertContains(string.Format("Universal Transaction batch [{0}] of company [{1}] (organization proxy [{2}]) has been successfully queued for delivery.", batch2.AIB_BatchNumber, GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.OrgProxy.OH_Code), logger.ToString());
			EInvoicingTestHelper.AssertGEIEInvoicingEDIMessage(message1, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, expectedMessageType: EInvoiceAPICommandList.Codes.GenerateInvoiceRequest);
			EInvoicingTestHelper.AssertGEIEInvoicingEDIMessage(message2, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, expectedMessageType: EInvoiceAPICommandList.Codes.GenerateInvoiceRequest);
		}

		public override void TestAllowSendingEInvoicingBatchWithError_RegistryOff()
		{
			var batch = GetBatchWithReadyStatus();

			AssertEquals(EInvoicingBatchState.Ready, batch.AIB_Status);
			var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			var notificationGroupPK = Helper.CreateNotificationGroup("Test User 2", "company2user@abc.com");
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var logger = new TestServiceLogger();
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter(addValidationErrorForBatches: new[] { batch });
				var processor = GetInterchangeProcessor(converter);
				processor.Process(logger);

				//Asserting Batch and Pivot Status
				AssertEquals("Batch Status", EInvoicingBatchState.Discarded, batch.AIB_Status);
				AssertEquals("Pivot Status", EInvoicingPivotState.BatchedWithError, batch.TransactionPivots[0].AIP_Status);
				AssertContains("Error Description", "Forced Validation Error generated for testing while creating Global Electronic Invoicing", batch.TransactionPivots[0].AIP_ErrorDescription);

				//Asserting EDI Messgage and Interchange
				ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
				AssertEquals("expect 1 interchange created with failed status", 1, ediInterchanges.Length);
				EInvoicingTestHelper.AssertGEIEInvoicingEDIInterchange(ediInterchanges[0], "FAL", expectedTo: ExpectedServicePoint);

				ediMessages = ediInterchanges.First().LoadMessages();
				AssertEquals("expect 1 messages created with failed status", 1, ediMessages.Length);
				EInvoicingTestHelper.AssertGEIEInvoicingEDIMessage(ediMessages[0], GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, expectedStatus: "FAL", expectedMessageType: EInvoiceAPICommandList.Codes.GenerateInvoiceRequest);
				Assert("Notes added to EDI Message", ediMessages[0].Notes.HasNotes);
				AssertContains("Notes should contains error details", "Forced Validation Error generated for testing while creating Global Electronic Invoicing", ediMessages[0].Notes.GetAllNotes().ToArray<StmNote>().First().ST_NoteDataAsText);
				AssertContains(string.Format("Debug|Universal Transaction batch [{0}] of company [{1}] (organization proxy [{2}]) has been successfully saved as an EDI message.", batch.AIB_BatchNumber, batch.Company.GC_Code, batch.Company.OrgProxy.OH_Code), logger.ToString());

				//Asserting Notification Email
				AssertEquals("Notificaiton Email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("Email body contains error description", "Forced Validation Error generated for testing while creating Global Electronic Invoicing", Env.OutgoingMailManager.EmailsCreated[0].Body);
				AssertContains("Attempting to send an email notification containing error details.", logger.ToString());
				AssertContains("E-Reporting Email Notification task completed.", logger.ToString());
			}
		}

		public override void TestAllowSendingEInvoicingBatchWithError_RegistryOn()
		{
			var notificationGroupPK = Helper.CreateNotificationGroup("Test User 2", "company2user@abc.com");
			Factory.Save();

			var batch = GetBatchWithReadyStatus();

			AssertEquals(EInvoicingBatchState.Ready, batch.AIB_Status);
			var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
			var ediMessages = Factory.Load<XmlEDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var logger = new TestServiceLogger();
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter(addValidationErrorForBatches: new[] { batch });
				var processor = GetInterchangeProcessor(converter);
				processor.Process(logger);

				AssertEquals(EInvoicingBatchState.Sent, batch.AIB_Status);
				AssertEquals(EInvoicingPivotState.Sent, batch.TransactionPivots[0].AIP_Status);

				ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
				AssertEquals("expect 1 interchange created", 1, ediInterchanges.Length);
				var interchange = ediInterchanges[0];
				EInvoicingTestHelper.AssertGEIEInvoicingEDIInterchange(ediInterchanges[0], "HQU", expectedTo: ExpectedServicePoint);

				ediMessages = ediInterchanges.First().LoadMessages();
				AssertEquals("expect 1 messages created", 1, ediMessages.Length);
				EInvoicingTestHelper.AssertGEIEInvoicingEDIMessage(ediMessages[0], GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
				AssertEquals("No Notes added to EDI Message", (processor as IEDIIntechangeCreatorForTest)?.AddErrorToEDIMessageNotesIfAny ?? false, ediMessages[0].Notes.HasNotes);
				AssertContains(string.Format("Debug|Universal Transaction batch [{0}] of company [{1}] (organization proxy [{2}]) has been successfully queued for delivery.", batch.AIB_BatchNumber, GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.OrgProxy.OH_Code), logger.ToString());

				//Asserting Notification Email
				AssertEquals("No Notificaiton Email is sent", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public override void TestProcessSingleBatchFailed()
		{
			var notificationGroupPK = Helper.CreateNotificationGroup("Test User 2", "company2user@abc.com");
			Factory.Save();

			var batch = GetBatchWithReadyStatus();

			AssertEquals(EInvoicingBatchState.Ready, batch.AIB_Status);
			var ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			#region Failed due to unhandled exception

			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var logger = new TestServiceLogger();
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = GetInterchangeProcessor(converter, ExceptionTypes.UnhandledExceptionDuringEDIMessageCreation);
				processor.Process(logger);

				//Asserting Batch and Pivot Status
				AssertEquals("Batch Status", EInvoicingBatchState.Ready, batch.AIB_Status);
				AssertEquals("Pivot Status", EInvoicingPivotState.Batched, batch.TransactionPivots[0].AIP_Status);
				AssertContains("Error Description", string.Empty, batch.TransactionPivots[0].AIP_ErrorDescription);

				//Asserting EDI Messgage and Interchange
				ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("No EDI Interchange is created due to unhandled exception", 0, ediInterchanges.Length);

				ediMessages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("No EDI Message is created due to unhandled exception", 0, ediMessages.Length);

				//Asserting Notification Email
				AssertEquals("Notificaiton Email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("Email body contains error description", new DivideByZeroException().Message, Env.OutgoingMailManager.EmailsCreated[0].Body);
				AssertContains("Attempting to send an email notification containing error details.", logger.ToString());
				AssertContains("E-Reporting Email Notification task completed.", logger.ToString());
			}

			#endregion

			#region Failed due to Validation error and Notification error generated from EHubDelivery Process 

			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var logger = new TestServiceLogger();
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter(addValidationErrorForBatches: new[] { batch });
				var processor = GetInterchangeProcessor(converter, ExceptionTypes.NotifcationErrorDuringEDIMessageCreation);
				processor.Process(logger);

				//Asserting Batch and Pivot Status
				AssertEquals("Batch Status", EInvoicingBatchState.Discarded, batch.AIB_Status);
				AssertEquals("Pivot Status", EInvoicingPivotState.BatchedWithError, batch.TransactionPivots[0].AIP_Status);
				AssertContains("Error Description", "Forced Validation Error generated for testing while creating Global Electronic Invoicing", batch.TransactionPivots[0].AIP_ErrorDescription);

				//Asserting EDI Messgage and Interchange
				ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("No EDI Interchange is created due to unhandled exception", 0, ediInterchanges.Length);

				ediMessages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("No EDI Message is created due to unhandled exception", 0, ediMessages.Length);

				//Asserting Notification Email
				AssertEquals("Notificaiton Email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("Email body contains error description", "Forced Validation Error generated for testing while creating Global Electronic Invoicing", Env.OutgoingMailManager.EmailsCreated[0].Body);
				AssertContains("Email body contains error description", "Forced Error generated for testing while creating EDI Message", Env.OutgoingMailManager.EmailsCreated[0].Body);
				AssertContains("Attempting to send an email notification containing error details.", logger.ToString());
				AssertContains("E-Reporting Email Notification task completed.", logger.ToString());
			}

			#endregion
		}

		public void TestProcessSingleBatchFailed_AllowSendingEInvoicingBatchWithErrorRegistryOn()
		{
			var notificationGroupPK = Helper.CreateNotificationGroup("Test User 2", "company2user@abc.com");
			Factory.Save();

			var batch = GetBatchWithReadyStatus();

			AssertEquals(EInvoicingBatchState.Ready, batch.AIB_Status);
			var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
			var ediMessages = Factory.Load<XmlEDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			#region Failed due to unhandled exception

			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var logger = new TestServiceLogger();
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = GetInterchangeProcessor(converter, ExceptionTypes.UnhandledExceptionDuringEDIMessageCreation);
				processor.Process(logger);

				var newFactory = new BusinessObjectFactory();
				var reloadedBatch = newFactory.Load<AccEInvoicingBatch>(batch.PK);
				//Asserting Batch and Pivot Status
				AssertEquals("Batch Status", EInvoicingBatchState.Ready, reloadedBatch.AIB_Status);
				AssertEquals("Pivot Status", EInvoicingPivotState.Batched, reloadedBatch.TransactionPivots[0].AIP_Status);
				AssertContains("Error Description", string.Empty, reloadedBatch.TransactionPivots[0].AIP_ErrorDescription);

				//Asserting EDI Messgage and Interchange
				ediInterchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
				AssertEquals("No EDI Interchange is created due to unhandled exception", 0, ediInterchanges.Length);

				ediMessages = newFactory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("No EDI Message is created due to unhandled exception", 0, ediMessages.Length);

				//Asserting Notification Email
				AssertEquals("Notificaiton Email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("Email body contains error description", new DivideByZeroException().Message, Env.OutgoingMailManager.EmailsCreated[0].Body);
				AssertContains("Attempting to send an email notification containing error details.", logger.ToString());
				AssertContains("E-Reporting Email Notification task completed.", logger.ToString());
			}

			#endregion

			#region Failed due to Validation error and Notification error generated from EHubDelivery Process 

			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				var logger = new TestServiceLogger();
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter(addValidationErrorForBatches: new[] { batch });
				var processor = GetInterchangeProcessor(converter, ExceptionTypes.NotifcationErrorDuringEDIMessageCreation);
				processor.Process(logger);

				var newFactory = new BusinessObjectFactory();
				var reloadedBatch = newFactory.Load<AccEInvoicingBatch>(batch.PK);

				//Asserting Batch and Pivot Status
				AssertEquals("Batch Status", EInvoicingBatchState.Ready, reloadedBatch.AIB_Status);
				AssertEquals("Pivot Status", EInvoicingPivotState.BatchedWithError, reloadedBatch.TransactionPivots[0].AIP_Status);
				AssertContains("Error Description", string.Empty, reloadedBatch.TransactionPivots[0].AIP_ErrorDescription);

				//Asserting EDI Messgage and Interchange
				ediInterchanges = newFactory.Load<IXmlEDIInterchange>(new ZQuery());
				AssertEquals("No EDI Interchange is created due to unhandled exception", 0, ediInterchanges.Length);

				ediMessages = newFactory.Load<XmlEDIMessage>(new ZQuery());
				AssertEquals("No EDI Message is created due to unhandled exception", 0, ediMessages.Length);

				//Asserting Notification Email
				AssertEquals("Notificaiton Email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertContains("Email body contains error description", "Forced Validation Error generated for testing while creating Global Electronic Invoicing", Env.OutgoingMailManager.EmailsCreated[0].Body);
				AssertContains("Email body contains error description", "Forced Error generated for testing while creating EDI Message", Env.OutgoingMailManager.EmailsCreated[0].Body);
				AssertContains("Attempting to send an email notification containing error details.", logger.ToString());
				AssertContains("E-Reporting Email Notification task completed.", logger.ToString());
			}

			#endregion
		}

		public override void TestProcessPartOfBatchesThrowException()
		{
			var notificationGroupPK = Helper.CreateNotificationGroup("Test User 2", "company2user@abc.com");
			Factory.Save();

			var batch1 = GetBatchWithReadyStatus();
			var batch2 = GetBatchWithReadyStatus();
			var batch3 = GetBatchWithReadyStatus();

			var logger = new TestServiceLogger();
			AssertEquals(EInvoicingBatchState.Ready, batch1.AIB_Status);
			AssertEquals(EInvoicingBatchState.Ready, batch2.AIB_Status);
			AssertEquals(EInvoicingBatchState.Ready, batch3.AIB_Status);

			var ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter(addValidationErrorForBatches: new[] { batch2 });
				var processor = GetInterchangeProcessor(converter, ExceptionTypes.NotifcationErrorDuringEDIMessageCreation, batch3);
				processor.Process(logger);

				//Batch 1
				AssertEquals(EInvoicingBatchState.Sent, batch1.AIB_Status);
				AssertEquals(EInvoicingPivotState.Sent, batch1.TransactionPivots[0].AIP_Status);

				//Batch 2
				AssertEquals(EInvoicingBatchState.Discarded, batch2.AIB_Status);
				AssertEquals(EInvoicingPivotState.BatchedWithError, batch2.TransactionPivots[0].AIP_Status);

				//Batch 3
				AssertEquals(EInvoicingBatchState.Ready, batch3.AIB_Status);
				AssertEquals(EInvoicingPivotState.Batched, batch3.TransactionPivots[0].AIP_Status);

				AssertEquals("2 error notification email should be sent for batch 2, batch 3", 2, Env.OutgoingMailManager.EmailsCreated.Count);

				ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals(2, ediInterchanges.Length);
				ediMessages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals(2, ediMessages.Length);
			}

			var batch4 = GetBatchWithReadyStatus();

			AssertEquals(EInvoicingBatchState.Ready, batch4.AIB_Status);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = GetInterchangeProcessor(converter, ExceptionTypes.UnhandledExceptionDuringEDIMessageCreation, batch4);
				processor.Process(logger);

				//Batch 4
				AssertEquals(EInvoicingBatchState.Ready, batch4.AIB_Status);
				AssertEquals(EInvoicingPivotState.Batched, batch4.TransactionPivots[0].AIP_Status);

				AssertEquals("1 error notification email should be sent for batch 4", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("batch 1, batch 2 and batch 3", 3, ediInterchanges.Length);
				ediMessages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("batch 1, batch 2 and batch 3", 3, ediMessages.Length);
			}
		}

		public void TestProcessPartOfBatchesThrowException_AllowSendingEInvoicingBatchWithErrorRegistryOn()
		{
			var notificationGroupPK = Helper.CreateNotificationGroup("Test User 2", "company2user@abc.com");
			Factory.Save();

			var batch1 = GetBatchWithReadyStatus();
			var batch2 = GetBatchWithReadyStatus();
			var batch3 = GetBatchWithReadyStatus();

			AssertEquals(EInvoicingBatchState.Ready, batch1.AIB_Status);
			AssertEquals(EInvoicingBatchState.Ready, batch2.AIB_Status);
			AssertEquals(EInvoicingBatchState.Ready, batch3.AIB_Status);

			var ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var logger = new TestServiceLogger();

				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter(addValidationErrorForBatches: new[] { batch2 });
				var processor = GetInterchangeProcessor(converter, ExceptionTypes.NotifcationErrorDuringEDIMessageCreation, batch3);
				processor.Process(logger);

				//Batch 1

				AssertEquals(EInvoicingBatchState.Sent, batch1.AIB_Status);
				AssertEquals(EInvoicingPivotState.Sent, batch1.TransactionPivots[0].AIP_Status);

				//Batch 2
				AssertEquals(EInvoicingBatchState.Sent, batch2.AIB_Status);
				AssertEquals(EInvoicingPivotState.Sent, batch2.TransactionPivots[0].AIP_Status);

				//Batch 3
				AssertEquals(EInvoicingBatchState.Ready, batch3.AIB_Status);
				AssertEquals(EInvoicingPivotState.Batched, batch3.TransactionPivots[0].AIP_Status);

				AssertEquals("1 error notification email should be sent for batch 3", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals(2, ediInterchanges.Length);
				ediMessages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals(2, ediMessages.Length);
			}

			var batch4 = GetBatchWithReadyStatus();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var logger = new TestServiceLogger();

				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
				var processor = GetInterchangeProcessor(converter, ExceptionTypes.UnhandledExceptionDuringEDIMessageCreation, batch4);
				processor.Process(logger);

				//Batch 4
				AssertEquals(EInvoicingBatchState.Ready, batch4.AIB_Status);
				AssertEquals(EInvoicingPivotState.Batched, batch4.TransactionPivots[0].AIP_Status);

				AssertEquals("1 error notification email should be sent for batch 4", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals("batch 1, batch 2 and batch 3", 3, ediInterchanges.Length);
				ediMessages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals("batch 1, batch 2 and batch 3", 3, ediMessages.Length);
			}
		}

		public void TestNullGEIMessage_WhenAllSendingWithErrorOff_AndNoValidationMessage()
			=> TestNullGEIMessage(allowSendingBatchWithError: false, includeValidationMessage: false);

		public void TestNullGEIMessage_WhenAllSendingWithErrorOn_AndNoValidationMessage()
			=> TestNullGEIMessage(allowSendingBatchWithError: true, includeValidationMessage: false);

		public void TestNullGEIMessage_WhenAllSendingWithErrorOff_AndValidationMessage()
			=> TestNullGEIMessage(allowSendingBatchWithError: false, includeValidationMessage: true);

		public void TestNullGEIMessage_WhenAllSendingWithErrorOn_AndValidationMessage()
			=> TestNullGEIMessage(allowSendingBatchWithError: true, includeValidationMessage: true);

		public void TestNullGEIMessage_WhenAllSendingWithErrorOn_AndNoValidationMessage_AndThrownException()
			=> TestNullGEIMessage(allowSendingBatchWithError: true, includeValidationMessage: false, hasException: true);

		public void TestNullGEIMessage_WhenAllSendingWithErrorOff_AndNoValidationMessage_AndThrownException()
			=> TestNullGEIMessage(allowSendingBatchWithError: false, includeValidationMessage: false, hasException: true);

		public void TestNullGEIMessage_WhenAllSendingWithErrorOn_AndValidationMessage_AndThrownException()
			=> TestNullGEIMessage(allowSendingBatchWithError: true, includeValidationMessage: true, hasException: true);

		public void TestNullGEIMessage_WhenAllSendingWithErrorOff_AndValidationMessage_AndThrownException()
			=> TestNullGEIMessage(allowSendingBatchWithError: false, includeValidationMessage: true, hasException: true);

		void TestNullGEIMessage(bool allowSendingBatchWithError, bool includeValidationMessage, bool hasException = false)
		{
			var notificationGroupPK = Helper.CreateNotificationGroup("Test User 2", "company2user@abc.com");
			Factory.Save();

			var batch = GetBatchWithReadyStatus();

			AssertEquals("Precondition: batch is ready", EInvoicingBatchState.Ready, batch.AIB_Status);
			var ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("Precondition: no EDI Interchanges", 0, ediInterchanges.Length);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("Precondition: no EDI Messages", 0, ediMessages.Length);

			var logger = new TestServiceLogger();

			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, allowSendingBatchWithError))
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertEquals("Precondition: No Emails", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				var batchArrayForValidation = includeValidationMessage ? new[] { batch } : null;
				Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter(addValidationErrorForBatches: batchArrayForValidation, null, returnNullEInvoiceForBatches: new[] { batch }, hasException);
				var processor = GetInterchangeProcessor(converter);
				processor.Process(logger);
			}

			//Asserting Batch and Pivot Status
			AssertEquals("Batch Status", EInvoicingBatchState.Discarded, batch.AIB_Status);
			AssertEquals("Pivot Status", EInvoicingPivotState.BatchedWithError, batch.TransactionPivots[0].AIP_Status);
			var expectedValidationMessage = includeValidationMessage
											? "Forced Validation Error generated for testing while creating Global Electronic Invoicing"
											: "E-Invoice could not be created. No further information is available.";
			if (!hasException)
			{
				AssertContains("Error Description from GEI Creation OR generic", expectedValidationMessage, batch.TransactionPivots[0].AIP_ErrorDescription);
			}
			else
			{
				AssertEquals("Error Description from GEI Creation for unexpected exception",
					"CargoWise encountered an unexpected error when attempting to create the electronic invoice file. Reset the Status to Queued for this invoice may resolve the issue. If not, please cancel this invoice and re-enter.",
					batch.TransactionPivots[0].AIP_ErrorDescription);
				AssertContains("Exception on any error of GEI message builder.", logger.ToString());
			}

			//Asserting EDI Messgage and Interchange
			ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("No EDI Interchange is created when GEI creator returns null", 0, ediInterchanges.Length);

			ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals("No EDI Message is created when GEI creator returns nul", 0, ediMessages.Length);

			//Asserting Notification Email
			if (allowSendingBatchWithError)
			{
				AssertEquals("Notification Email should not be sent when AllowSendingEInvoicingBatchWithError is enabled and no GEI was created", hasException ? 1 : 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
			else
			{
				AssertEquals("Notification Email should be sent when AllowSendingEInvoicingBatchWithError is disabled and no GEI was created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				if (!hasException)
				{
					AssertContains("Email body contains error description", expectedValidationMessage, Env.OutgoingMailManager.EmailsCreated[0].Body);
				}
				AssertContains("Attempting to send an email notification containing error details.", logger.ToString());
				AssertContains("E-Reporting Email Notification task completed.", logger.ToString());
			}
		}

		#region Billing

		[TestDate(2018, 10, 20)]
		public override void TestCreateBillingTransaction()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var batch = GetBatchWithReadyStatusForSending(out string number);
				if (batch == null)
				{
					Assert("no billing transaction for test", true);
				}
				else
				{
					var pivot = batch.TransactionPivots[0];
					Factory.Save();

					var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
					var ediMessages = Factory.Load<EDIMessage>(new ZQuery());

					AssertEquals("Precondition", EInvoicingBatchState.Ready, batch.AIB_Status);
					AssertEquals(0, ediInterchanges.Length);
					AssertEquals(0, ediMessages.Length);
					AssertEquals(0, CountBillingLog(pivot));
					AssertNoBillingTransaction(Factory);

					var logger = new TestServiceLogger();
					Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
					var processor = GetInterchangeProcessor(converter);
					processor.Process(logger);

					AssertEquals("Batch should be sent", EInvoicingBatchState.Sent, batch.AIB_Status);
					ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
					AssertEquals("Expect 1 interchange created", 1, ediInterchanges.Length);
					ediMessages = ediInterchanges.First().LoadMessages();
					AssertEquals("Expect 1 messages created", 1, ediMessages.Length);
					AssertContains(string.Format("Universal Transaction batch [{0}] of company [{1}] (organization proxy [{2}]) has been successfully queued for delivery.", batch.AIB_BatchNumber, GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.OrgProxy.OH_Code), logger.ToString());
					AssertEquals("Expect no billing log created", 0, CountBillingLog(pivot));
					AssertNoBillingTransaction(Factory);

					// Resend
					batch = TestObjectCreator.CreateEInvoicingBatch(200, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
					pivot.AIP_AIB = batch.PK;
					pivot.AIP_Status = EInvoicingPivotState.Batched;
					Factory.Save();

					logger = new TestServiceLogger();
					converter = () => new MockAccEInvoiceBatchToGEIConverter();
					processor = GetInterchangeProcessor(converter);
					processor.Process(logger);

					AssertEquals("Batch should be sent", EInvoicingBatchState.Sent, batch.AIB_Status);
					ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
					AssertEquals("Second interchange created", 2, ediInterchanges.Length);
					AssertEquals("Second messages created", 1, ediInterchanges[0].LoadMessages().Length);
					AssertEquals(1, ediInterchanges[1].LoadMessages().Length);
					AssertEquals("NO new billing log created", 0, CountBillingLog(pivot));
					AssertNoBillingTransaction(Factory);
				}
			}
		}

		[TestDate(2018, 10, 20)]
		public virtual void TestCreateBillingTransactionWithFailure()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var batch = GetBatchWithReadyStatusForSending(out string number);
				if (batch == null)
				{
					Assert("no billing transaction for test", true);
				}
				else
				{
					var pivot = batch.TransactionPivots[0];
					Factory.Save();

					var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
					var ediMessages = Factory.Load<EDIMessage>(new ZQuery());

					AssertEquals("Precondition", EInvoicingBatchState.Ready, batch.AIB_Status);
					AssertEquals(0, ediInterchanges.Length);
					AssertEquals(0, ediMessages.Length);
					AssertNoBillingTransaction(Factory);
					AssertEquals(0, CountBillingLog(pivot));

					var logger = new TestServiceLogger();
					Func<IAccEInvoiceBatchToGEIConverter> converter = () => new MockAccEInvoiceBatchToGEIConverter();
					var processor = GetInterchangeProcessor(converter, ExceptionTypes.NotifcationErrorDuringEDIMessageCreation);
					processor.Process(logger);

					AssertNotEquals("Batch should NOT be sent", EInvoicingBatchState.Sent, batch.AIB_Status);
					ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
					AssertEquals("Expect NO interchange created", 0, ediInterchanges.Length);
					AssertEquals("Expect NO messages created", 0, ediMessages.Length);
					AssertEquals("Expect NO billing log created", 0, CountBillingLog(pivot));
					AssertNoBillingTransaction(Factory);

					// Resend
					batch = TestObjectCreator.CreateEInvoicingBatch(200, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
					pivot.AIP_AIB = batch.PK;
					pivot.AIP_Status = EInvoicingPivotState.Batched;
					Factory.Save();

					logger = new TestServiceLogger();
					converter = () => new MockAccEInvoiceBatchToGEIConverter();
					processor = GetInterchangeProcessor(converter);
					processor.Process(logger);

					AssertEquals("Batch should be sent", EInvoicingBatchState.Sent, batch.AIB_Status);
					ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
					AssertEquals("Expect 1 interchange created", 1, ediInterchanges.Length);
					ediMessages = ediInterchanges.First().LoadMessages();
					AssertEquals("Expect 1 messages created", 1, ediMessages.Length);
					AssertContains(string.Format("Universal Transaction batch [{0}] of company [{1}] (organization proxy [{2}]) has been successfully queued for delivery.", batch.AIB_BatchNumber, GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.OrgProxy.OH_Code), logger.ToString());
					AssertEquals("Expect no billing log created", 0, CountBillingLog(pivot));
					AssertNoBillingTransaction(Factory);
				}
			}
		}

		protected virtual AccEInvoicingBatch GetBatchWithReadyStatusForSending(out string number)
		{
			number = string.Empty;
			return null;
		}

		protected void AssertNoBillingTransaction(BusinessObjectFactory factory)
		{
			var stmUsageDataCollection = new BillingManager().GetTransactions(factory, 10);
			AssertEquals("Expect no billing transactions created including integrated compliance billing transactions.", 0, stmUsageDataCollection.Count());
		}

		protected int CountBillingLog(AccEInvoicingTransactionPivot pivot)
		{
			return pivot.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.Events.EditedARecord.Code &&
										x.SL_Reference == "Billing transaction created.").Count();
		}

		#endregion
	}

	sealed public class MockAccEInvoiceBatchToGEIConverter : IAccEInvoiceBatchToGEIConverter
	{
		public MockAccEInvoiceBatchToGEIConverter(
			AccEInvoicingBatch[] addValidationErrorForBatches = null,
			AccEInvoicingBatch[] addValidationWarningForBatches = null,
			AccEInvoicingBatch[] returnNullEInvoiceForBatches = null,
			bool hasException = false)
		{
			ErroneousForBatches = addValidationErrorForBatches ?? Array.Empty<AccEInvoicingBatch>();
			WarningForBatches = addValidationWarningForBatches ?? Array.Empty<AccEInvoicingBatch>();
			NullEInvoiceForBatches = returnNullEInvoiceForBatches ?? Array.Empty<AccEInvoicingBatch>();
			ThrowException = hasException;
		}

		AccEInvoicingBatch[] ErroneousForBatches { get; }
		AccEInvoicingBatch[] WarningForBatches { get; }
		AccEInvoicingBatch[] NullEInvoiceForBatches { get; }
		bool ThrowException { get; }

		(GlobalElectronicInvoicing EInvoice, ZString ValidationErrors, ZString ValidationWarnings) IAccEInvoiceBatchToGEIConverter.Convert(AccEInvoicingBatch batch)
		{
			var eInvoice = EInvoicingTestHelper.GetEInvoice();
			if (NullEInvoiceForBatches.Any(x => x.PK == batch.PK))
			{
				eInvoice = null;
				if (ThrowException)
				{
					throw new NotImplementedException("Exception on any error of GEI message builder.");
				}
			}
			var validationErrors = ZString.Empty;
			var validationWarnings = ZString.Empty;
			if (ErroneousForBatches.Any(x => x.PK == batch.PK))
			{
				validationErrors = "Forced Validation Error generated for testing while creating Global Electronic Invoicing";
			}
			if (WarningForBatches.Any(x => x.PK == batch.PK))
			{
				validationWarnings = "Forced Validation Warning generated for testing while creating Global Electronic Invoicing";
			}
			return (eInvoice, validationErrors, validationWarnings);
		}

		public void Dispose()
		{
		}
	}

	public interface IEDIIntechangeCreatorForTest
	{
		bool AddErrorToEDIMessageNotesIfAny { get; }
	}

	public enum ExceptionTypes
	{
		NotifcationErrorDuringEDIMessageCreation,
		UnhandledExceptionDuringEDIMessageCreation
	}

	class MockEDIInterchangeCreatorForExceptionHandling : EDIInterchangeCreatorForEInvoicingBatchBase
	{
		public MockEDIInterchangeCreatorForExceptionHandling(GlbCompany company, string parentTableCode)
			: base(company)
		{
			this.ParentTableCodeValue = parentTableCode;
		}

		protected override void CreateEDIMessageAndInterchangeCore(TransactionBatchProcessContext batchProcessContext, INotifications notifications)
		{
		}

		protected override ZString GetEInvoicingServicePoint() => "TEST_SERVICEPOINT";

		protected override IEDICommunicationsMode GetDefaultCommunicationsMode()
			=> new NonPersistentEDICommunicationMode
			{
				EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML,
				EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService,
				EK_Destination = EInvoicingServicePoint,
				EK_MessagePurpose = "Some Purpose",
			};

		protected override string ParentTableCode => ParentTableCodeValue;

		readonly string ParentTableCodeValue;

		protected override void PerformIfBatchProcessingFails(TransactionBatchProcessContext batchProcessContext, Exception ex)
		{
			throw new ApplicationException("Boom! Exception thrown from PerformIfBatchProcessingFails()");
		}
	}
}
