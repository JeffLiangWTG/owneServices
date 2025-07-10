using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Germany;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Hungary;
using Enterprise.Accounting.ElectronicMessaging.KoreaSouth;
using Enterprise.Accounting.ElectronicMessaging.Testing.Germany;
using Enterprise.Accounting.ElectronicMessaging.Turkey;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalEventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing
{
	[TestedType(typeof(AccEInvoicingBatchDataContextManager))]
	public class AccEInvoicingBatchDataContextManagerTest : DataContextManagerTestCase<AccEInvoicingBatchDataContextManager, AccEInvoicingBatch>
	{
		#region Italy

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2012()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Italy,
				MessageSubType = "rispostaSdIRiceviFile"
			};

			var govermentAllocatedNumberContextType = new ContextType();
			govermentAllocatedNumberContextType.Type = "GovernmentAllocatedNumber";

			var eHubAllocatedNumberContextType = new ContextType();
			eHubAllocatedNumberContextType.Type = "eHubAllocatedNumber";

			var companyCodeContextType = new ContextType();
			companyCodeContextType.Type = "CompanyCode";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = govermentAllocatedNumberContextType, Value = "111222333" });
			eventDataObject.ContextCollection.Add(new Context() { Type = eHubAllocatedNumberContextType, Value = "444555666" });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}.
Linked Event to {invoice.HumanReadableName}.");
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertEquals(1, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2011()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Italy,
				MessageSubType = "rispostaSdIRiceviFile"
			};

			var govermentAllocatedNumberContextType = new ContextType();
			govermentAllocatedNumberContextType.Type = "GovernmentAllocatedNumber";

			var eHubAllocatedNumberContextType = new ContextType();
			eHubAllocatedNumberContextType.Type = "eHubAllocatedNumber";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = govermentAllocatedNumberContextType, Value = "111222333" });
			eventDataObject.ContextCollection.Add(new Context() { Type = eHubAllocatedNumberContextType, Value = "444555666" });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}.
Linked Event to {invoice.HumanReadableName}.");
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertEquals(1, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		public void TestGetDataContextKeyMatchingQuery_ForRejection()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			//eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.EventType = AutoEvents.InterchangeRejectedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Italy,
				Reason = "This is the rejection reason from eHub."
			};

			var companyCodeContextType = new ContextType();
			companyCodeContextType.Type = "CompanyCode";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}.
Linked Event to {invoice.HumanReadableName}.");
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			AssertEquals(1, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		#endregion

		#region Taiwan

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2012_Taiwan()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var complianceDocument = objectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice.Lines[0], objectCreator.Debtor);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Taiwan,
			};

			var companyCodeContextType = new ContextType();
			companyCodeContextType.Type = "CompanyCode";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}.
Linked Event to {complianceDocument.HumanReadableName}.");
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertEquals(1, complianceDocument.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2011_Taiwan()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var complianceDocument = objectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice.Lines[0], objectCreator.Debtor);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Taiwan,
			};

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}.
Linked Event to {complianceDocument.HumanReadableName}.");
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertEquals(1, complianceDocument.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		public void TestGetDataContextKeyMatchingQuery_ForRejection_Taiwan()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var complianceDocument = objectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice.Lines[0], objectCreator.Debtor);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.EventType = AutoEvents.InterchangeRejectedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Taiwan,
				Reason = "This is the rejection reason from eHub."
			};

			var companyCodeContextType = new ContextType();
			companyCodeContextType.Type = "CompanyCode";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult1 = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}");
			var expectedImportResult2 = FormattableString.Invariant($@"Linked Event to {complianceDocument.HumanReadableName}");
			AssertContains(expectedImportResult1, importResults.Single().ToString());
			AssertContains(expectedImportResult2, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			AssertEquals(1, complianceDocument.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
		}

		public void TestGetDataContextKeyMatchingQuery_ForInterchangeSent_Taiwan()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var complianceDocument = objectCreator.CreateComplianceDocumentHeaderWithLine(LedgerTypes.AccountsReceivable, "test", "AA001", "TXE", "desc", arInvoice.Lines[0], objectCreator.Debtor);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			objectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.EventType = AutoEvents.InterchangeSentCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Taiwan,
				Department = "TPE"
			};

			var companyCodeContextType = new ContextType();
			companyCodeContextType.Type = "CompanyCode";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult1 = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}");
			var expectedImportResult2 = FormattableString.Invariant($@"Linked Event to {complianceDocument.HumanReadableName}");
			AssertContains(expectedImportResult1, importResults.Single().ToString());
			AssertContains(expectedImportResult2, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeSentCode).Count());
			AssertEquals(1, complianceDocument.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeSentCode).Count());
		}

		#endregion

		#region KoreaSouth

		public void TestGetDataContextKeyMatchingQuery_ForMultipleInvoice_KoreaSouth()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);

			var arInvoice1 = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot1 = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice1, Core.Constants.EInvoicingPivotState.Sent);

			var arInvoice2 = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "002", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var pivot2 = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice2, Core.Constants.EInvoicingPivotState.Sent);

			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.KoreaSouth,
				MessageSubType = KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest
			};

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType() { Type = "CompanyCode" }, Value = GlbCompany.CurrentCompany.GC_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType() { Type = EInvoicingKoreaSouthConstants.DataContext.KoreaStatusCode }, Value = EInvoicingKoreaSouthConstants.StatusCodes.SubmitSuccess });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult1 = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}");
			var expectedImportResult2 = FormattableString.Invariant($@"Linked Event to {arInvoice1.HumanReadableName}");
			var expectedImportResult3 = FormattableString.Invariant($@"Linked Event to {arInvoice2.HumanReadableName}");
			AssertContains(expectedImportResult1, importResults.Single().ToString());
			AssertContains(expectedImportResult2, importResults.Single().ToString());
			AssertContains(expectedImportResult3, importResults.Single().ToString());
			var expectedlogs1 = batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			var expectedlogs2 = arInvoice1.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			var expectedlogs3 = arInvoice2.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode);
			AssertEquals("Should be linked to the same EDIMessage", true, expectedlogs1.Single().RelatedEDIMessage.Message.PK == expectedlogs2.Single().RelatedEDIMessage.Message.PK);
			AssertEquals("Should be linked to the same EDIMessage", true, expectedlogs2.Single().RelatedEDIMessage.Message.PK == expectedlogs3.Single().RelatedEDIMessage.Message.PK);
		}

		#endregion

		#region Fiji

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2011_Fiji()
		{
			AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_Fiji(UniversalXmlInfo.Namespace_2011_11, new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext());
		}

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2012_Fiji()
		{
			AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_Fiji(UniversalXmlInfo.Namespace_2012_11, new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext());
		}

		public void TestGetDataContextKeyMatchingQuery_ForRejection_Fiji()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.EventType = AutoEvents.InterchangeRejectedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Fiji,
				Reason = "This is the rejection reason from eHub."
			};

			var companyCodeContextType = new ContextType();
			companyCodeContextType.Type = "CompanyCode";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult1 = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}");
			var expectedImportResult2 = FormattableString.Invariant($@"Linked Event to {arInvoice.HumanReadableName}");
			AssertContains(expectedImportResult1, importResults.Single().ToString());
			AssertContains(expectedImportResult2, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			AssertEquals(1, arInvoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			var authorisation = Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, arInvoice.PK));
			AssertNull("Authorisation record should not be created", authorisation);
		}

		void AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_Fiji(string universalNamespace, IDataContextDataObject universalContext)
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
			AssertEquals("Pre-condition: no eDocs", 0, arInvoice.DocManagerInfo.AllEDocs.Count);

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = universalContext;
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Fiji,
				MessageSubType = "Fiji Invoice Response",
			};

			eventDataObject.ContextCollection = new List<Context>();

			if (universalNamespace == UniversalXmlInfo.Namespace_2012_11)
			{
				var companyCodeContextType = new ContextType() { Type = "CompanyCode" };
				eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });
			}
			const string minimumJson = @"{'RequestedBy':'','SignedBy':'','DT':'2019-07-16T15:26:00Z','IC':'0','InvoiceCounterExtension':'','IN':'12345678798','VerificationUrl':'','TotalCounter':0,'TransactionTypeCounter':0,'TotalAmount':0,'ID':'','S':'', 'TaxItems':[]}";
			eventDataObject.ContextCollection.Add(new Context() { Type = "ResponseMessage", Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(minimumJson.Replace("'", "\""))) });

			var message = GetQueuedUniversalEventMessage(eventDataObject, universalNamespace);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}.
Linked Event to {arInvoice.HumanReadableName}.");
			AssertEquals("Expected import results for " + universalNamespace, expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertEquals(1, arInvoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
			var authorisation = Factory.BOFactory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, arInvoice.PK));
			AssertNotNull("Authorisation record should be created", authorisation);
			AssertEquals("One eDoc added", 1, arInvoice.DocManagerInfo.AllEDocs.Count);
			AssertContains("Fiji Invoice Response_", arInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertContains(".json", arInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Response should be stored in eDoc", minimumJson.Replace("'", "\""), arInvoice.DocManagerInfo.AllEDocs[0].ImageData.ToUTF8());
		}

		#endregion

		#region Samoa

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2011_Samoa()
		{
			AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_Samoa(UniversalXmlInfo.Namespace_2011_11, new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext());
		}

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2012_Samoa()
		{
			AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_Samoa(UniversalXmlInfo.Namespace_2012_11, new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext());
		}

		public void TestGetDataContextKeyMatchingQuery_ForRejection_Samoa()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.EventType = AutoEvents.InterchangeRejectedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.WesternSamoa,
				Reason = "This is the rejection reason from eHub."
			};

			var companyCodeContextType = new ContextType();
			companyCodeContextType.Type = "CompanyCode";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult1 = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}");
			var expectedImportResult2 = FormattableString.Invariant($@"Linked Event to {arInvoice.HumanReadableName}");
			AssertContains(expectedImportResult1, importResults.Single().ToString());
			AssertContains(expectedImportResult2, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			AssertEquals(1, arInvoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			var authorisation = Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, arInvoice.PK));
			AssertNull("Authorization record should not be created", authorisation);
		}

		void AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_Samoa(string universalNamespace, IDataContextDataObject universalContext)
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
			AssertEquals("Pre-condition: no eDocs", 0, arInvoice.DocManagerInfo.AllEDocs.Count);

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = universalContext;
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.WesternSamoa,
				MessageSubType = "Samoa Invoice Response",
			};

			eventDataObject.ContextCollection = new List<Context>();

			if (universalNamespace == UniversalXmlInfo.Namespace_2012_11)
			{
				var companyCodeContextType = new ContextType() { Type = "CompanyCode" };
				eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });
			}
			const string minimumJson = @"{'RequestedBy':'','SignedBy':'','DT':'2019-07-16T15:26:00Z','IC':'0','InvoiceCounterExtension':'','IN':'12345678798','VerificationUrl':'','TotalCounter':0,'TransactionTypeCounter':0,'TotalAmount':0,'ID':'','S':'', 'TaxItems':[]}";
			eventDataObject.ContextCollection.Add(new Context() { Type = "ResponseMessage", Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(minimumJson.Replace("'", "\""))) });

			var message = GetQueuedUniversalEventMessage(eventDataObject, universalNamespace);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}.
Linked Event to {arInvoice.HumanReadableName}.");
			AssertEquals("Expected import results for " + universalNamespace, expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertEquals(1, arInvoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
			var authorisation = Factory.BOFactory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, arInvoice.PK));
			AssertNotNull("Authorization record should be created", authorisation);
			AssertEquals("One eDoc added", 1, arInvoice.DocManagerInfo.AllEDocs.Count);
			AssertContains("Samoa Invoice Response_", arInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertContains(".json", arInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Response should be stored in eDoc", minimumJson.Replace("'", "\""), arInvoice.DocManagerInfo.AllEDocs[0].ImageData.ToUTF8());
		}

		#endregion

		#region Germany

		internal AccInvoiceBatchContextGermanyTestContent CreateContext_Germany(
			IDataContextDataObject universalContext,
			bool setMessageTypeSuccessful,
			bool saveForTesting)
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
			AssertNotNull("CreateEInvoicingTransactionPivot has failed.", pivot);
			AssertEquals("Pre-condition: no eDocs", 0, arInvoice.DocManagerInfo.AllEDocs.Count);

			if (saveForTesting)
			{
				Factory.SaveForTesting();
			}

			var result = new AccInvoiceBatchContextGermanyTestContent
			{
				ObjectCreator = objectCreator,
				ArInvoice = arInvoice,
				Batch = batch,
				Pivot = pivot,
				EventDataObject = new UniversalEventDataObject(),
			};

			result.EventDataObject.DataContext = universalContext;
			result.EventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, $"{batch.AIB_BatchNumber}");
			if (setMessageTypeSuccessful)
			{
				result.EventDataObject.SetAcknowledgedEventType(Core.Constants.CountryCodes.Germany, "GEN");
			}
			else
			{
				result.EventDataObject.SetRejectedEventType(Core.Constants.CountryCodes.Germany, "GEN");
			}

			result.EventDataObject
				.AddToContextCollection(EInvoicingEventMessageDEProcessor.ContextTypeCode.InvoiceDocument, Convert.ToBase64String(Encoding.UTF8.GetBytes(AccInvoiceBatchContextGermanyTestContent.InvoiceDocumentContentDummy)))
				.AddToContextCollection(EInvoicingEventMessageDEProcessor.ContextTypeCode.TransactionID, "my dummy transaction id")
				.AddToContextCollection("CompanyCode", GlbCompany.CurrentCompany.GC_Code);

			result.ServiceTaskLog = new ServiceTaskLogForTesting();
			result.Manager = new UniversalMessageProcessingManager(result.ServiceTaskLog);

			return result;
		}

		internal void VerifyThatEDocWasAttachedCorrectly_Germany(AccInvoiceBatchContextGermanyTestContent context)
		{
			AssertEquals("One eDoc added", 1, context.ArInvoice.DocManagerInfo.AllEDocs.Count);
			AssertContains("GEN_", context.ArInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertContains(".xml", context.ArInvoice.DocManagerInfo.AllEDocs[0].FileName);

			var actualDocumentContent = context.ArInvoice.DocManagerInfo.AllEDocs[0].ImageData.ToUTF8();
			AssertEquals("Response should be stored in eDoc", AccInvoiceBatchContextGermanyTestContent.InvoiceDocumentContentDummy, actualDocumentContent);
		}

		internal void VerifyThatNoAuthorizationHasBeenDone_Germany(AccInvoiceBatchContextGermanyTestContent context)
		{
			var authorisation = Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(
				new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId,
					context.ArInvoice.PK));
			AssertNull("Authorisation record should not be created", authorisation);
		}

		internal void VerifyProcessLogsWithoutProcessErrors_Germany(
			AccInvoiceBatchContextGermanyTestContent context,
			string universalNamespace,
			IEnumerable<IImportResult> importResults,
			string acknowledgmentCode)
		{
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {context.Batch.HumanReadableName}.
Linked Event to {context.ArInvoice.HumanReadableName}.");

			var importResultMessages = importResults.Single().ToString();
			AssertEquals("Expected import results for " + universalNamespace, expectedImportResult, importResultMessages);
			AssertEquals(1, context.Batch.Logs.Find(l => l.SL_SE_NKEvent == acknowledgmentCode).Count());
			AssertEquals(1, context.ArInvoice.Logs.Find(l => l.SL_SE_NKEvent == acknowledgmentCode).Count());

			var serviceTaskLogMessages = context.ServiceTaskLog.ToString();
			AssertContains("ServiceTask logs shall be equal to the import log", expectedImportResult, serviceTaskLogMessages);
		}
		internal void VerifyProcessLogsDoContainLinkEventMessagesAndEventCode_Germany(
				AccInvoiceBatchContextGermanyTestContent context,
				string universalNamespace,
				IEnumerable<IImportResult> importResults,
				string acknowledgmentCode)
		{
			var expectedImportResult1 = FormattableString.Invariant($@"Linked Event to {context.Batch.HumanReadableName}");
			var expectedImportResult2 = FormattableString.Invariant($@"Linked Event to {context.ArInvoice.HumanReadableName}");
			AssertContains(expectedImportResult1, importResults.Single().ToString());
			AssertContains(expectedImportResult2, importResults.Single().ToString());
			AssertEquals(1, context.Batch.Logs.Find(l => l.SL_SE_NKEvent == acknowledgmentCode).Count());
			AssertEquals(1, context.ArInvoice.Logs.Find(l => l.SL_SE_NKEvent == acknowledgmentCode).Count());
		}

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2011_Germany()
		{
			AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_Germany(
				UniversalXmlInfo.Namespace_2011_11,
				new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext());
		}

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2012_Germany()
		{
			AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_Germany(
				UniversalXmlInfo.Namespace_2012_11,
				new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext());
		}

		void AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_Germany(string universalNamespace, IDataContextDataObject universalContext)
		{
			using (var context = CreateContext_Germany(universalContext, true, false))
			{
				context.EventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);

				if (universalNamespace == UniversalXmlInfo.Namespace_2012_11)
				{
					context.EventDataObject.AddToContextCollection("CompanyCode", GlbCompany.CurrentCompany.GC_Code);
				}

				var message = GetQueuedUniversalEventMessage(context.EventDataObject, universalNamespace);
				var importResults = context.Manager.Process(message).ImportResults;

				VerifyProcessLogsWithoutProcessErrors_Germany(context, universalNamespace, importResults, AutoEvents.InterchangeAcknowledgedCode);
				VerifyThatNoAuthorizationHasBeenDone_Germany(context);
				VerifyThatEDocWasAttachedCorrectly_Germany(context);
			}
		}

		public void TestGetDataContextKeyMatchingQuery_ForRejection_Germany()
		{
			using (var context = CreateContext_Germany(new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext(), false, false))
			{
				var message = GetQueuedUniversalEventMessage(context.EventDataObject, UniversalXmlInfo.Namespace_2012_11);
				var importResults = context.Manager.Process(message).ImportResults;

				VerifyProcessLogsDoContainLinkEventMessagesAndEventCode_Germany(context, UniversalXmlInfo.Namespace_2012_11, importResults, AutoEvents.InterchangeRejectedCode);
				VerifyThatNoAuthorizationHasBeenDone_Germany(context);
				VerifyThatEDocWasAttachedCorrectly_Germany(context);
			}
		}

		public void TestResponseDeserializingErrorsAreNotReportedAsDeveloperError_Germany()
		{
			using (var context = CreateContext_Germany(new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext(), true, false))
			{
				context.EventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, $"{context.Batch.AIB_BatchNumber}");

				var message = GetQueuedUniversalEventMessage(context.EventDataObject, UniversalXmlInfo.Namespace_2012_11);
				var importResults = context.Manager.Process(message).ImportResults;
				AssertNull("No Developer Errors should be reported", ErrorReporter.LastExceptionReported);

				VerifyProcessLogsWithoutProcessErrors_Germany(context, UniversalXmlInfo.Namespace_2012_11, importResults, AutoEvents.InterchangeAcknowledgedCode);
				VerifyThatNoAuthorizationHasBeenDone_Germany(context);
				VerifyThatEDocWasAttachedCorrectly_Germany(context);
			}
		}

		#endregion Germany

		#region India

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2011_India()
		{
			AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_India(UniversalXmlInfo.Namespace_2011_11, new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext());
		}

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2012_India()
		{
			AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_India(UniversalXmlInfo.Namespace_2012_11, new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext());
		}

		public void TestGetDataContextKeyMatchingQuery_ForRejection_India()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.EventType = AutoEvents.InterchangeRejectedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.India,
				Reason = "This is the rejection reason from eHub."
			};

			var companyCodeContextType = new ContextType();
			companyCodeContextType.Type = "CompanyCode";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult1 = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}");
			var expectedImportResult2 = FormattableString.Invariant($@"Linked Event to {arInvoice.HumanReadableName}");
			AssertContains(expectedImportResult1, importResults.Single().ToString());
			AssertContains(expectedImportResult2, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			AssertEquals(1, arInvoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			var authorisation = Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, arInvoice.PK));
			AssertNull("Authorisation record should not be created", authorisation);
		}

		public void TestResponseDeserializingErrorsAreNotReportedAsDeveloperError_India()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.India,
				MessageSubType = "GEN",
			};

			eventDataObject.ContextCollection = new List<Context>();

			var companyCodeContextType = new ContextType() { Type = "CompanyCode" };
			eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });

			const string invalidJson = @"{
'Status':'0'
}";
			eventDataObject.ContextCollection.Add(new Context() { Type = "ResponseMessage", Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(invalidJson.Replace("'", "\""))) });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var jsonReadingErrors = @"Required properties are missing from object: AckNo, AckDt, Irn, SignedInvoice, SignedQRCode. Path '', line 1, position 1.
Required property 'AckNo' not found in JSON. Path '', line 3, position 1.
Required property 'AckDt' not found in JSON. Path '', line 3, position 1.
Required property 'Irn' not found in JSON. Path '', line 3, position 1.
Required property 'SignedInvoice' not found in JSON. Path '', line 3, position 1.
Required property 'SignedQRCode' not found in JSON. Path '', line 3, position 1.
Reading the following India E-Invoice Response has failed due to invalid JSON:
{
""Status"":""0""
}
";
			var expectedImportResult = $@"Linked Event to {batch.HumanReadableName}.
Error - {jsonReadingErrors}";
			AssertEquals("Expected import results", expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertEquals(1, arInvoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			Assert(expectedImportResult.Equals(serviceTaskLog.ToString(), StringComparison.OrdinalIgnoreCase));

			AssertNull("No Developer Errors should be reported", ErrorReporter.LastExceptionReported);
			var authorisation = Factory.BOFactory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, arInvoice.PK));
			AssertNull("Authorisation record should not be created", authorisation);
			AssertEquals("One eDoc added", 1, arInvoice.DocManagerInfo.AllEDocs.Count);
			AssertContains("GEN_", arInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertContains(".json", arInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Response should be stored in eDoc", invalidJson.Replace("'", "\""), arInvoice.DocManagerInfo.AllEDocs[0].ImageData.ToUTF8());
		}

		void AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_India(string universalNamespace, IDataContextDataObject universalContext)
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
			AssertEquals("Pre-condition: no eDocs", 0, arInvoice.DocManagerInfo.AllEDocs.Count);

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = universalContext;
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.India,
				MessageSubType = "GEN",
			};

			eventDataObject.ContextCollection = new List<Context>();

			if (universalNamespace == UniversalXmlInfo.Namespace_2012_11)
			{
				var companyCodeContextType = new ContextType() { Type = "CompanyCode" };
				eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });
			}
			const string minimumJson = @"{
'AckNo':17100000054,
'AckDt':'2019-12-25 12:03:00',
'Irn':'d056a57cc7cfcc6c9230aa0014e439259d0dc57cfa4eea2326253011fd53ea7e',
'SignedInvoice':'eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNyc2Etc2hhMjU2Iiwia2lkIjoiRTc4MDhFNkZGMDNFMTMyODUzMzBCMDQxQjNFMEEzQUVDNDc4MTMyMCIsInR5cCI6IkpXVCIsIng1dCI6IjU0Q09iX0EtRXloVE1MQkJzLUNqcnNSNEV5QSJ9.eyJkYXRhIjoie1wiQWNrTm9cIjoxNzEwMDAwMDA1NCxcIkFja0R0XCI6XCIyMDE5LTEyLTI1IDEyOjAzOjAwXCIsXCJUYXhTY2hcIjpudWxsLFwiVmVyc2lvblwiOlwiMS4wXCIsXCJJcm5cIjpcImQwNTZhNTdjYzdjZmNjNmM5MjMwYWEwMDE0ZTQzOTI1OWQwZGM1N2NmYTRlZWEyMzI2MjUzMDExZmQ1M2VhN2VcIixcIlRyYW5EdGxzXCI6e1wiQ2F0Z1wiOlwiQjJCXCIsXCJSZWdSZXZcIjpcIlJHXCIsXCJUeXBcIjpcIlJFIFwiLFwiRWNtVHJuXCI6XCJOXCIsXCJFY21Hc3RpblwiOlwiXCJ9LFwiRG9jRHRsc1wiOntcIlR5cFwiOlwiSU5WXCIsXCJOb1wiOlwiZG9jL3Rlc3QxXCIsXCJEdFwiOlwiMjAxOS0xMS0yOFwiLFwiT3JnSW52Tm9cIjpcIjEyMzMyMTEyMzMyMTEyMzJcIn0sXCJTZWxsZXJEdGxzXCI6e1wiR3N0aW5cIjpcIjI5QUFHUEI4Njc4TDFaMVwiLFwiVHJkTm1cIjpcIlRyYWRlIE5hbWUxXCIsXCJCbm9cIjpcIjFcIixcIkJubVwiOlwiXCIsXCJGbG5vXCI6XCJcIixcIkxvY1wiOlwiTG9jYXRpb25cIixcIkRzdFwiOlwiXCIsXCJQaW5cIjo1NjAwNDMsXCJTdGNkXCI6MjksXCJQaFwiOm51bGwsXCJFbVwiOlwiYWJjQHh5ei5jb21cIn0sXCJCdXllckR0bHNcIjp7XCJHc3RpblwiOlwiMjlBQUdQQjg2NzhMMVoxXCIsXCJUcmRObVwiOlwiVHJhZGUgTmFtZTFcIixcIkJub1wiOlwiMVwiLFwiQm5tXCI6XCJcIixcIkZsbm9cIjpcIlwiLFwiTG9jXCI6XCJMb2NhdGlvblwiLFwiRHN0XCI6XCJcIixcIlBpblwiOjU2MDA0MyxcIlN0Y2RcIjoyOSxcIlBoXCI6bnVsbCxcIkVtXCI6XCJhYmNAeHl6LmNvbVwifSxcIkRpc3BEdGxzXCI6e1wiR3N0aW5cIjpcIjI5QUFHUEI4Njc4TDFaMVwiLFwiVHJkTm1cIjpcIm5hbWUgXCIsXCJCbm9cIjpcIjEyM1wiLFwiQm5tXCI6XCJcIixcIkZsbm9cIjpcIjJcIixcIkxvY1wiOlwibG9jYXRpb25cIixcIkRzdFwiOlwiXCIsXCJQaW5cIjo1NjAwNDMsXCJTdGNkXCI6MjksXCJQaFwiOm51bGwsXCJFbVwiOlwiYWJDQFhZWi5DT01cIn0sXCJTaGlwRHRsc1wiOntcIkdzdGluXCI6XCIyOUFBR1BCODY3OEwxWjFcIixcIlRyZE5tXCI6XCJuYW1lIFwiLFwiQm5vXCI6XCIxMjNcIixcIkJubVwiOlwiXCIsXCJGbG5vXCI6XCIyXCIsXCJMb2NcIjpcImxvY2F0aW9uXCIsXCJEc3RcIjpcIlwiLFwiUGluXCI6NTYwMDQzLFwiU3RjZFwiOjI5LFwiUGhcIjpudWxsLFwiRW1cIjpcImFiQ0BYWVouQ09NXCJ9LFwiSXRlbUxpc3RcIjpbe1wiUHJkTm1cIjpcIldoZWF0XCIsXCJQcmREZXNjXCI6XCJXaGVhdCBkZXNjXCIsXCJIc25DZFwiOlwiMTAwMVwiLFwiQmFyY2RlXCI6bnVsbCxcIlF0eVwiOjEsXCJGcmVlUXR5XCI6bnVsbCxcIlVuaXRcIjpcIktHU1wiLFwiVW5pdFByaWNlXCI6MCxcIlRvdEFtdFwiOjAsXCJEaXNjb3VudFwiOjAsXCJPdGhDaHJnXCI6MCxcIkFzc0FtdFwiOjAsXCJDZ3N0UnRcIjozLFwiU2dzdFJ0XCI6MyxcIklnc3RSdFwiOjAsXCJDZXNSdFwiOjAsXCJDZXNOb25BZFZhbFwiOjAsXCJTdGF0ZUNlc1wiOjM2LFwiVG90SXRlbVZhbFwiOjIzNDMyfV0sXCJWYWxEdGxzXCI6e1wiQXNzVmFsXCI6MTAsXCJDZ3N0VmFsXCI6MixcIlNnc3RWYWxcIjoyLFwiSWdzdFZhbFwiOjAsXCJDZXNWYWxcIjowLFwiU3RDZXNWYWxcIjowLFwiQ2VzTm9uQWRWYWxcIjowLFwiRGlzY1wiOjAsXCJPdGhDaHJnXCI6MCxcIlRvdEludlZhbFwiOjB9LFwiRXhwRHRsc1wiOntcIkV4cENhdFwiOlwiU0VaXCIsXCJXdGhQYXlcIjpcIk5cIixcIlNoaXBCTm9cIjpcIlwiLFwiU2hpcEJEdFwiOlwiMjAxOS0xMS0yOFwiLFwiUG9ydFwiOlwiXCIsXCJJbnZGb3JDdXJcIjoyMzQ0NSxcIkZvckN1clwiOlwiQkRUXCIsXCJDbnRDb2RlXCI6XCJCRFwifSxcIlBheUR0bHNcIjp7XCJOYW1cIjpcIlwiLFwiTW9kZVwiOlwiQ0FTSFwiLFwiRmluSW5zQnJcIjpcIlwiLFwiUGF5VGVybVwiOlwiMTAwMVwiLFwiUGF5SW5zdHJcIjpcIlwiLFwiQ3JUcm5cIjpcIlwiLFwiRGlyRHJcIjpcIlwiLFwiQ3JEYXlcIjoyLFwiQmFsQW10XCI6MixcIlBheUR1ZUR0XCI6XCIyMDE5LTExLTI4XCIsXCJBY2N0RGV0XCI6XCIxMFwifSxcIlJlZkR0bHNcIjp7XCJJbnZSbWtcIjpcIjBcIixcIkludlN0RHRcIjpcIjIwMTktMTEtMjhcIixcIkludkVuZER0XCI6XCIyMDE5LTExLTI4XCIsXCJQcmVjSW52Tm9cIjpcIlwiLFwiUHJlY0ludkR0XCI6XCIyMDE5LTExLTI4XCIsXCJJbnZSZWZOb1wiOlwiMFwiLFwiUmVjQWR2UmVmXCI6XCJcIixcIlRlbmRSZWZcIjpcIlwiLFwiQ29udHJSZWZcIjpcIjEwXCIsXCJFeHRSZWZcIjpcIjJcIixcIlByb2pSZWZcIjpcIlwiLFwiUE9SZWZcIjpcIlwifX0iLCJpc3MiOiJOSUMifQ.II0jejF0wOrY0CPwIb6oQsSBYilvFSNHZqQJHYOsFt7txrpAvLlCEo4CDNUhPZGWZ6IeERuoY2ScXKNIsCIMQR_9dc0E_fmsaN5XUscxBV3nqPNIkusFcmzjxu59sT8MzcaFqDWfqHunkA-klW40M7oVy6HtL5eI0zbmNoZpdl2yB1Fxfx5AF_vcDv9vWV2u6i39XQ0KdTSbkTAVStdXHix2hE0TlHtHB9Tqt7i9umNPJc_Uf5Kpn4qHiOPvFT2uxhw1N7bIU00cc-PDIqzlLk2VnLb2Zko7wSgYAMrf5G1hn5Op4lbyQe9EwxOPcvJPjJBJsoOmFTbxd1IRgewm7g',
'SignedQRCode':'eyJhbGciOiJodHRwOi8vd3d3LnczLm9yZy8yMDAxLzA0L3htbGRzaWctbW9yZSNyc2Etc2hhMjU2Iiwia2lkIjoiRTc4MDhFNkZGMDNFMTMyODUzMzBCMDQxQjNFMEEzQUVDNDc4MTMyMCIsInR5cCI6IkpXVCIsIng1dCI6IjU0Q09iX0EtRXloVE1MQkJzLUNqcnNSNEV5QSJ9.eyJkYXRhIjoie1wiU2VsbGVyR3N0aW5cIjpcIjI5QUFHUEI4Njc4TDFaMVwiLFwiQnV5ZXJHc3RpblwiOlwiMjlBQUdQQjg2NzhMMVoxXCIsXCJEb2NOb1wiOlwiZG9jL3Rlc3QxXCIsXCJEb2NUeXBcIjpcIklOVlwiLFwiRG9jRHRcIjpcIjIwMTktMTEtMjhcIixcIlRvdEludlZhbFwiOjAsXCJJdGVtQ250XCI6MSxcIk1haW5Ic25Db2RlXCI6XCIxMDAxXCIsXCJJcm5cIjpcImQwNTZhNTdjYzdjZmNjNmM5MjMwYWEwMDE0ZTQzOTI1OWQwZGM1N2NmYTRlZWEyMzI2MjUzMDExZmQ1M2VhN2VcIn0iLCJpc3MiOiJOSUMifQ.UbnWZAI_lq9s9JK8_MovcmjpMPIJnv2-4qGkiXegggQbry_0fJxjwaR8fwCjK_-HVeMujnw8C7F3ITrxKIg5RsdDMzAvLrCNE0K3QkNHGczbvyxhJ02VRFl1wrQoArxSW1RiyGssLSbH_4tdQ6vke0nSUajXRXvXX2yUeI01CyLsbf2FDNIq46MrDmka5RPNGp0dCx4uX2gkmsCx0Yb0CKjqFI176sTbxOQDHQtRCAfJ0I6dwwggVkfnC3p2F-x6GhEls_7GVsFg1Z6k4A_aB89c3N6CaWfe8DOQEJTjWaPggMUa2-1hDE1ODTBcIlLcOwB-8jT971hZWgtBubUzFg',
'Status':'ACT'
}";
			eventDataObject.ContextCollection.Add(new Context() { Type = "ResponseMessage", Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(minimumJson.Replace("'", "\""))) });

			var message = GetQueuedUniversalEventMessage(eventDataObject, universalNamespace);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}.
Linked Event to {arInvoice.HumanReadableName}.");
			AssertEquals("Expected import results for " + universalNamespace, expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertEquals(1, arInvoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
			var authorisation = Factory.BOFactory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, arInvoice.PK));
			AssertNotNull("Authorisation record should be created", authorisation);
			AssertEquals("One eDoc added", 1, arInvoice.DocManagerInfo.AllEDocs.Count);
			AssertContains("GEN_", arInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertContains(".json", arInvoice.DocManagerInfo.AllEDocs[0].FileName);
			AssertEquals("Response should be stored in eDoc", minimumJson.Replace("'", "\""), arInvoice.DocManagerInfo.AllEDocs[0].ImageData.ToUTF8());
		}

		#endregion

		#region Hungary

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2011_Hungary()
		{
			AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_Hungary(UniversalXmlInfo.Namespace_2011_11, new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext());
		}

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2012_Hungary()
		{
			AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_Hungary(UniversalXmlInfo.Namespace_2012_11, new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext());
		}

		void AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_Hungary(string universalNamespace, IDataContextDataObject universalContext)
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var huCompany = objectCreator.CreateCompanyAndBranch("HUBUD");
			Factory.SaveForTesting();
			var defaultBranchPK = Env.CurrentBranchPK;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, huCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
				Factory.SaveForTesting();
				AssertEquals("Pre-condition: no eDocs", 0, arInvoice.DocManagerInfo.AllEDocs.Count);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, defaultBranchPK, Env.CurrentDepartmentPK))
				{
					var eventDataObject = new UniversalEventDataObject();
					eventDataObject.DataContext = universalContext;
					eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
					eventDataObject.DataContext.SetCompanyAndDataProviderDetails(huCompany);
					eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
					eventDataObject.EventTime = ZDateTimeOffset.Now;
					eventDataObject.EventParameters = new EventParameters
					{
						MessageType = Core.Constants.CountryCodes.Hungary,
						MessageSubType = HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest,
					};

					eventDataObject.ContextCollection = new List<Context>();

					if (universalNamespace == UniversalXmlInfo.Namespace_2012_11)
					{
						eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType() { Type = "CompanyCode" }, Value = huCompany.GC_Code });
					}
					eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType() { Type = "TransactionID" }, Value = "2Y14LJK5L01AKOZU" });
					string sampleResponse = @"<?xml version='1.0' encoding='UTF-8' standalone='yes'?>
<QueryTransactionStatusResponse xmlns='http://schemas.nav.gov.hu/OSA/2.0/api' xmlns:ns2='http://schemas.nav.gov.hu/OSA/2.0/data'>
   <header>
      <requestId>WTCQ00000001</requestId>
      <timestamp>2020-04-13T04:01:00.000Z</timestamp>
      <requestVersion>2.0</requestVersion>
      <headerVersion>1.0</headerVersion>
   </header>
   <result>
      <funcCode>OK</funcCode>
   </result>
   <software>
      <softwareId>WISGLOCW1012345678</softwareId>
      <softwareName>CargoWise</softwareName>
      <softwareOperation>ONLINE_SERVICE</softwareOperation>
      <softwareMainVersion>20.01.122</softwareMainVersion>
      <softwareDevName>Wisetech Global</softwareDevName>
      <softwareDevContact>Product.Accounting@wisetechglobal.com</softwareDevContact>
      <softwareDevCountryCode>AU</softwareDevCountryCode>
      <softwareDevTaxNumber>41065894724</softwareDevTaxNumber>
   </software>
   <processingResults>
      <processingResult>
         <index>1</index>
         <invoiceStatus>ABORTED</invoiceStatus>
         <businessValidationMessages>
            <validationResultCode>ERROR</validationResultCode>
            <validationErrorCode>SUPPLIER_TAX_NUMBER_MISMATCH</validationErrorCode>
            <message>Az eladó adószáma nem azonos az API XML-ben megadott, authentikált adószámmal.</message>
            <pointer>
               <tag>InvoiceData/invoiceMain//invoice/invoiceHead/supplierInfo/supplierTaxNumber/taxpayerId</tag>
            </pointer>
         </businessValidationMessages>
         <compressedContentIndicator>false</compressedContentIndicator>
      </processingResult>
      <originalRequestVersion>2.0</originalRequestVersion>
   </processingResults>
</QueryTransactionStatusResponse>
}".Replace("'", "\"");
					eventDataObject.ContextCollection.Add(new Context() { Type = "ResponseMessage", Value = Convert.ToBase64String(MessageEncoding.UTF8WithoutBOM.GetBytes(sampleResponse)) });

					var message = GetQueuedUniversalEventMessage(eventDataObject, universalNamespace);
					var serviceTaskLog = new ServiceTaskLogForTesting();
					var manager = new UniversalMessageProcessingManager(serviceTaskLog);
					var importResults = manager.Process(message).ImportResults;

					var expectedImportResult = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}.
Linked Event to {arInvoice.HumanReadableName}.");
					AssertEquals("Expected import results for " + universalNamespace, expectedImportResult, importResults.Single().ToString());
					AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
					AssertEquals(1, arInvoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
					AssertContains("", "".Trim(), serviceTaskLog.ToString());

					var authorisation = Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, arInvoice.PK));
					AssertNull("Authorisation record is never created for Hungary", authorisation);
					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					AssertEquals("Pivot Status should be Success", "SUC", pivot.AIP_Status);
					AssertNullOrEmpty("Pivot should contain no error details", pivot.AIP_ErrorDescription);

					AssertEquals("Batch should record TransactionID", "2Y14LJK5L01AKOZU", batch.AIB_GovernmentAllocatedNumber);

					AssertEquals("One eDoc added", 1, arInvoice.DocManagerInfo.AllEDocs.Count);
					AssertContains("GEN_", arInvoice.DocManagerInfo.AllEDocs[0].FileName);
					AssertContains(".xml", arInvoice.DocManagerInfo.AllEDocs[0].FileName);
					AssertEquals("Response should be stored in eDoc", sampleResponse, arInvoice.DocManagerInfo.AllEDocs[0].ImageData.ToUTF8());
				}
			}
		}

		public void TestGetDataContextKeyMatchingQuery_ForRejection_Hungary_WithTransactionID()
			=> TestGetDataContextKeyMatchingQuery_ForRejection_Hungary("2ZHJIFWVOO8GAP2X");

		public void TestGetDataContextKeyMatchingQuery_ForRejection_Hungary_WithoutTransactionID()
			=> TestGetDataContextKeyMatchingQuery_ForRejection_Hungary("");

		public void TestGetDataContextKeyMatchingQuery_ForRejection_Hungary(string responseTransactionID)
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var huCompany = objectCreator.CreateCompanyAndBranch("HUBUD");
			Factory.SaveForTesting();
			var defaultBranchPK = Env.CurrentBranchPK;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, huCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.EUR, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, huCompany);
				var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
				Factory.SaveForTesting();
				AssertEquals("Pre-condition: no eDocs", 0, arInvoice.DocManagerInfo.AllEDocs.Count);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, defaultBranchPK, Env.CurrentDepartmentPK))
				{
					var eventDataObject = new UniversalEventDataObject();
					eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
					eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
					eventDataObject.EventType = AutoEvents.InterchangeRejectedCode;
					eventDataObject.EventTime = ZDateTimeOffset.Now;
					eventDataObject.EventParameters = new EventParameters
					{
						MessageType = Core.Constants.CountryCodes.Hungary,
						MessageSubType = HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest,
						Reason = "An error reason as supplied from xHub. This is very long. " + new string('x', 300),
					};

					eventDataObject.ContextCollection = new List<Context>();
					eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType() { Type = "CompanyCode" }, Value = huCompany.GC_Code });
					eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType() { Type = "ResponseMessage" }, Value = Convert.ToBase64String(MessageEncoding.UTF8WithoutBOM.GetBytes("<someXml></someXml>")) });
					eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType() { Type = "TransactionID" }, Value = responseTransactionID });

					var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
					var serviceTaskLog = new ServiceTaskLogForTesting();
					var manager = new UniversalMessageProcessingManager(serviceTaskLog);
					var importResults = manager.Process(message).ImportResults;

					var expectedImportResult1 = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}");
					var expectedImportResult2 = FormattableString.Invariant($@"Linked Event to {arInvoice.HumanReadableName}");
					AssertContains(expectedImportResult1, importResults.Single().ToString());
					AssertContains(expectedImportResult2, importResults.Single().ToString());
					AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
					AssertEquals(1, arInvoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());

					var authorisation = Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, arInvoice.PK));
					AssertNull("Authorisation record is never created for Hungary", authorisation);

					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					AssertEquals("Pivot Status should be Failed", "FAL", pivot.AIP_Status);
					AssertContains("Pivot should contain error details", "An error reason as supplied from xHub.", pivot.AIP_ErrorDescription);

					AssertEquals("Batch should record TransactionID, even if already present", responseTransactionID, batch.AIB_GovernmentAllocatedNumber);

					AssertEquals("One eDoc added", 1, arInvoice.DocManagerInfo.AllEDocs.Count);
					AssertContains("GEN_", arInvoice.DocManagerInfo.AllEDocs[0].FileName);
					AssertContains(".xml", arInvoice.DocManagerInfo.AllEDocs[0].FileName);
					AssertEquals("Response should be stored in eDoc", "<someXml></someXml>", arInvoice.DocManagerInfo.AllEDocs[0].ImageData.ToUTF8());
				}
			}
		}

		#endregion

		#region Turkey

		public void TestGetDataContextKeyMatchingQuery_ReceivePDFCopyForMessageSucceed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (var eventDataObject = new UniversalEventDataObject())
			using (var memoryStream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes("test")))
			using (AccountingConfigurationRegistry.Instance.ThirdPartyEInvoiceDocType.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "MSC"))
			{
				var objectCreator = new TestObjectCreator(Factory.BOFactory);
				var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.EUR, 1m, 100m, 0, 100m, 0m);
				var batch = objectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Delivered, EInvoicingPivotActionType.DocumentAction);

				eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
				eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
				eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
				eventDataObject.EventTime = ZDateTimeOffset.Now;
				eventDataObject.EventParameters = new EventParameters
				{
					MessageType = CountryCodes.Turkey,
					MessageSubType = "Document"
				};

				eventDataObject.ContextCollection = new List<Context>();
				eventDataObject.ContextCollection.Add(new Context() { Type = "ResponseMessage", Value = Convert.ToBase64String(Encoding.UTF8.GetBytes("DocumentActionResponseMessage")) });
				eventDataObject.AttachedDocumentCollection = new List<AttachedDocument>();
				var expectedImportResult = @"Warning - Could not link Attached Documents to AccEInvoicingBatch. Does not have eDocs.
Linked Event to AccEInvoicingBatch.
Successfully Added eDoc: TestMiscellaneousFile.pdf.
Adding eDoc with a document type of MSC and name of TestMiscellaneousFile.pdf.
Linked Event to Accounts Receivable Invoice.";
				var attachedDocument = new AttachedDocument()
				{
					Type = new DocumentType() { Code = "MSC", Description = "Miscellaneous Document" },
					FileName = "TestMiscellaneousFile.pdf",
					IsPublished = true,
					ImageData = memoryStream
				};
				eventDataObject.AttachedDocumentCollection.Add(attachedDocument);

				var message = GetQueuedUniversalEventMessage(Factory, UniversalXmlInfo.Namespace_2011_11);

				Factory.SaveForTesting();

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				using (Factory.BOFactory.AddDisposableService())
				{
					var importResults = manager.Process(message, eventDataObject);
					var attemptedImports = importResults.ImportResults;

					Factory.SaveForTesting();

					AssertEquals(expectedImportResult, attemptedImports.Single().ToString());
				}
				AssertEquals("No attached PDF File", true, invoice.DocManagerInfo.AllEDocs.ToList<StorageFile>().Any(x => x.SC_FileNameWithExtension.Contains("TestMiscellaneousFile")));
				AssertEquals(1, invoice.DocManagerInfo.AllEDocs.Count);
				var eDoc = invoice.DocManagerInfo.AllEDocs[0];
				AssertEquals("MSC", eDoc.DocType);
				AssertEquals("Miscellaneous Document", eDoc.Description);
				AssertEquals("TestMiscellaneousFile.pdf", eDoc.FileName);
				AssertEquals(true, eDoc.IsPublished);
				AssertEquals("test", Encoding.UTF8.GetString(eDoc.ImageData, 0, eDoc.ImageData.Length));
				AssertEquals(EInvoicingPivotState.Succeed, pivot.AIP_Status);
			}
		}

		public void TestGetDataContextKeyMatchingQuery_ReceivePDFCopyForAttachedDocumentCollectionFailed()
		{
			AssertAttachedDocumentCollectionFailed(100, "00001000");
			AssertAttachedDocumentCollectionFailed(200, "00001001", true);
		}

		public void AssertAttachedDocumentCollectionFailed(ZInt batchNumber, string invoiceNumber, bool hasAttachedDocumentCollection = true)
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), invoiceNumber, objectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			var batch = objectCreator.CreateEInvoicingBatch(batchNumber, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Delivered, EInvoicingPivotActionType.DocumentAction);

			using (var eventDataObject = new UniversalEventDataObject())
			{
				eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
				eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
				eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
				eventDataObject.EventTime = ZDateTimeOffset.Now;
				eventDataObject.EventParameters = new EventParameters
				{
					MessageType = CountryCodes.Turkey,
					MessageSubType = "Document"
				};

				if (hasAttachedDocumentCollection)
				{
					eventDataObject.AttachedDocumentCollection = new List<AttachedDocument>();
				}
				var expectedImportResult = $@"Linked Event to AccEInvoicingBatch.
Error - Response Message does not have attached document. Response Message Context for invoice batch {batchNumber} in Eagle Datamation International.";

				var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
				Factory.SaveForTesting();
				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);
				var importResults = manager.Process(message).ImportResults;
				AssertEquals(expectedImportResult, importResults.Single().ToString());

				var found = invoice.DocManagerInfo.AllEDocs.ToList<StorageFile>().Any(x => x.SC_FileNameWithExtension.Contains("TestMiscellaneousFile"));

				AssertEquals("EInvoicingEventMessageTRProcessor_AttachmentNotFound", ErrorReporter.LastKeyReported);
				AssertEquals("Event has attached PDF File", false, found);
				AssertEquals(EInvoicingPivotState.Failed, pivot.AIP_Status);
				ErrorReporter.Clear();
			}
		}

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2012Turkey()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.TRY, 1m, 100m, 0, 100m, 0m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Turkey,
				MessageSubType = "sendInvoice"
			};

			var govermentAllocatedNumberContextType = new ContextType();
			govermentAllocatedNumberContextType.Type = "GovernmentAllocatedNumber";

			var companyCodeContextType = new ContextType();
			companyCodeContextType.Type = "CompanyCode";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = govermentAllocatedNumberContextType, Value = "111222333" });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}.
Linked Event to {invoice.HumanReadableName}.");
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.InterchangeAcknowledgedCode).Count());
			AssertEquals(1, invoice.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.InterchangeAcknowledgedCode).Count());
		}

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2011Turkey()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.TRY, 1m, 100m, 0, 100m, 0m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Turkey,
				MessageSubType = "sendInvoice"
			};

			var govermentAllocatedNumberContextType = new ContextType();
			govermentAllocatedNumberContextType.Type = "GovernmentAllocatedNumber";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = govermentAllocatedNumberContextType, Value = "111222333" });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}.
Linked Event to {invoice.HumanReadableName}.");
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.InterchangeAcknowledgedCode).Count());
			AssertEquals(1, invoice.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.InterchangeAcknowledgedCode).Count());
		}

		public void TestGetDataContextKeyMatchingQuery_ForRejectionTurkey()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, Core.Constants.EInvoicingPivotState.Sent);
			var errorMessage = "This is the rejection reason from xHub.";
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.EventType = AutoEvents.InterchangeRejectedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Turkey,
				Reason = errorMessage
			};

			var companyCodeContextType = new ContextType();
			companyCodeContextType.Type = "CompanyCode";

			var errorMessageContextContextType = new ContextType();
			errorMessageContextContextType.Type = "ErrorMessage";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });
			eventDataObject.ContextCollection.Add(new Context() { Type = errorMessageContextContextType, Value = errorMessage });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult1 = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}");
			var expectedImportResult2 = FormattableString.Invariant($@"Linked Event to {invoice.HumanReadableName}");
			AssertContains(expectedImportResult1, importResults.Single().ToString());
			AssertContains(expectedImportResult2, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			AssertEquals(1, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
		}

		public void TestReceiveStatusUpdate()
		{
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());

			var invoiceForSuccess = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", TestObjectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			var batchForStatusSuccess = TestObjectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var statusPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batchForStatusSuccess, invoiceForSuccess, EInvoicingPivotState.Sent, EInvoicingPivotActionType.StatusCheck);
			var batchForSubmitSuccess = TestObjectCreator.CreateEInvoicingBatch(2, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var submitPivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batchForSubmitSuccess, invoiceForSuccess, EInvoicingPivotState.Sent, EInvoicingPivotActionType.Submit, false);

			var invoiceForFail = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001001", TestObjectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			var batchForStatusFail = TestObjectCreator.CreateEInvoicingBatch(3, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var statusPivotForFail = TestObjectCreator.CreateEInvoicingTransactionPivot(batchForStatusFail, invoiceForFail, EInvoicingPivotState.Sent, EInvoicingPivotActionType.StatusCheck);
			var batchForSubmitFail = TestObjectCreator.CreateEInvoicingBatch(4, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var submitPivotForFail = TestObjectCreator.CreateEInvoicingTransactionPivot(batchForSubmitFail, invoiceForFail, EInvoicingPivotState.Sent, EInvoicingPivotActionType.Submit, false);

			var invoiceWithEmptyContextCollection = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001002", TestObjectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			var batchForinvoiceWithEmptyContextCollection = TestObjectCreator.CreateEInvoicingBatch(5, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var statusPivotForinvoiceWithEmptyContextCollection = TestObjectCreator.CreateEInvoicingTransactionPivot(batchForinvoiceWithEmptyContextCollection, invoiceWithEmptyContextCollection, EInvoicingPivotState.Sent, EInvoicingPivotActionType.StatusCheck);
			var submitBatchForinvoiceWithEmptyContextCollection = TestObjectCreator.CreateEInvoicingBatch(6, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var submitPivotForinvoiceWithEmptyContextCollection = TestObjectCreator.CreateEInvoicingTransactionPivot(submitBatchForinvoiceWithEmptyContextCollection, invoiceWithEmptyContextCollection, EInvoicingPivotState.Sent, EInvoicingPivotActionType.Submit, false);

			var invoiceForNotFinalStatus = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001003", TestObjectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			var batchForNotFinalStatus = TestObjectCreator.CreateEInvoicingBatch(7, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var statusPivotForNotFinalStatus = TestObjectCreator.CreateEInvoicingTransactionPivot(batchForNotFinalStatus, invoiceForNotFinalStatus, EInvoicingPivotState.Sent, EInvoicingPivotActionType.StatusCheck);
			statusPivotForNotFinalStatus.AIP_LastResponseReceivedUtc = DateTime.UtcNow.AddSeconds(-1);
			var batchForSubmitNotFinalStatus = TestObjectCreator.CreateEInvoicingBatch(8, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var submitPivotForNotFinalStatus = TestObjectCreator.CreateEInvoicingTransactionPivot(batchForSubmitNotFinalStatus, invoiceForNotFinalStatus, EInvoicingPivotState.Delivered, EInvoicingPivotActionType.Submit, false);
			TestObjectCreator.Factory.Save();

			using (var eventDataObject = CreateUniversalEventForTest(batchForStatusSuccess, "1000"))
			{
				var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
				var trProcessor = new EInvoicingEventMessageTRProcessor(logger, message, eventDataObject, batchForStatusSuccess);
				trProcessor.Process();
				AssertEquals(EInvoicingPivotState.Succeed, statusPivot.AIP_Status);
				AssertEquals(EInvoicingPivotState.Succeed, submitPivot.AIP_Status);
			}

			using (var eventDataObjectForFail = CreateUniversalEventForTest(batchForStatusFail, "10"))
			{
				var message = GetQueuedUniversalEventMessage(eventDataObjectForFail, UniversalXmlInfo.Namespace_2011_11);
				var trProcessorForStatuspivot = new EInvoicingEventMessageTRProcessor(logger, message, eventDataObjectForFail, batchForStatusFail);
				trProcessorForStatuspivot.Process();
				AssertEquals(EInvoicingPivotState.Succeed, statusPivotForFail.AIP_Status);
				AssertEquals(EInvoicingPivotState.Failed, submitPivotForFail.AIP_Status);
			}

			using (var eventDataObjectWithEmptyContextCollection = CreateUniversalEventForTest(batchForinvoiceWithEmptyContextCollection, "10"))
			{
				eventDataObjectWithEmptyContextCollection.ContextCollection.Clear();
				var message = GetQueuedUniversalEventMessage(eventDataObjectWithEmptyContextCollection, UniversalXmlInfo.Namespace_2011_11);
				var trProcessor = new EInvoicingEventMessageTRProcessor(logger, message, eventDataObjectWithEmptyContextCollection, batchForinvoiceWithEmptyContextCollection);
				AssertNoExceptionThrown(() => trProcessor.Process());
				AssertEquals(EInvoicingPivotState.Sent, statusPivotForinvoiceWithEmptyContextCollection.AIP_Status);
				AssertEquals(EInvoicingPivotState.Sent, submitPivotForinvoiceWithEmptyContextCollection.AIP_Status);
			}

			using (var eventDataObjectForNotFinalStatus = CreateUniversalEventForTest(batchForNotFinalStatus, "1100"))
			{
				var message = GetQueuedUniversalEventMessage(eventDataObjectForNotFinalStatus, UniversalXmlInfo.Namespace_2011_11);
				var trProcessor = new EInvoicingEventMessageTRProcessor(logger, message, eventDataObjectForNotFinalStatus, batchForNotFinalStatus);
				var responseTimeBeforeServiceTask = statusPivotForNotFinalStatus.AIP_LastResponseReceivedUtc;
				trProcessor.Process();
				AssertEquals(EInvoicingPivotState.Sent, statusPivotForNotFinalStatus.AIP_Status);
				AssertEquals(EInvoicingPivotState.Delivered, submitPivotForNotFinalStatus.AIP_Status);
				AssertNotEquals(responseTimeBeforeServiceTask, statusPivotForNotFinalStatus.AIP_LastResponseReceivedUtc);
			}
		}

		UniversalEventDataObject CreateUniversalEventForTest(AccEInvoicingBatch batch, string uyumsoftStatusCode)
		{
			var eventDataObject = new UniversalEventDataObject();

			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = CountryCodes.Turkey
			};

			var statusCode = new ContextType();
			statusCode.Type = "StatusCode";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = statusCode, Value = uyumsoftStatusCode });

			return eventDataObject;
		}

		#endregion

		#region SaudiArabia

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2011_SaudiArabia()
		{
			AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_SaudiArabia(UniversalXmlInfo.Namespace_2011_11, new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext());
		}

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_nameSpace2012_SaudiArabia()
		{
			AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_SaudiArabia(UniversalXmlInfo.Namespace_2012_11, new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext());
		}

		public void TestGetDataContextKeyMatchingQuery_ForRejection_SaudiArabia()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.EventType = AutoEvents.InterchangeRejectedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.SaudiArabia,
				Reason = "This is the rejection reason from eHub."
			};

			var companyCodeContextType = new ContextType();
			companyCodeContextType.Type = "CompanyCode";

			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult1 = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}");
			var expectedImportResult2 = FormattableString.Invariant($@"Linked Event to {arInvoice.HumanReadableName}");
			AssertContains(expectedImportResult1, importResults.Single().ToString());
			AssertContains(expectedImportResult2, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			AssertEquals(1, arInvoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			var authorisation = Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, arInvoice.PK));
			AssertNull("Authorisation record should not be created", authorisation);
		}

		void AssertGetDataContextKeyMatchingQuery_ForAcknowledgement_SaudiArabia(string universalNamespace, IDataContextDataObject universalContext)
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var arInvoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", objectCreator.TWD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			var batch = objectCreator.CreateEInvoicingBatch(1, Core.Constants.EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Sent);
			AssertEquals("Pre-condition: no eDocs", 0, arInvoice.DocManagerInfo.AllEDocs.Count);

			using (var eventDataObject = new UniversalEventDataObject())
			using (var memoryStream = (SubStreamableStream)new MemoryStream(Encoding.ASCII.GetBytes("test")))
			{
				eventDataObject.DataContext = universalContext;
				eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
				eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
				eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
				eventDataObject.EventTime = ZDateTimeOffset.Now;
				eventDataObject.EventParameters = new EventParameters
				{
					MessageType = CountryCodes.SaudiArabia,
					MessageSubType = "GEN"
				};
				eventDataObject.ContextCollection = new List<Context>();
				eventDataObject.ContextCollection.Add(new Context { Type = new ContextType { Type = "EINV_PivotStatus" }, Value = "SUC" });
				eventDataObject.ContextCollection.Add(new Context { Type = new ContextType { Type = "EINV_GovtAllocatedRefNumber" }, Value = "72cf24e4-366e-11eb-adc1-0242ac120002" });
				eventDataObject.ContextCollection.Add(new Context { Type = new ContextType { Type = "EINV_AuthorisationData" }, Value = "PFRlc3QgWE1MPg==" });
				eventDataObject.ContextCollection.Add(new Context { Type = new ContextType { Type = "EINV_Counter" }, Value = "1" });
				eventDataObject.ContextCollection.Add(new Context { Type = new ContextType { Type = "EINV_Number" }, Value = "1" });
				eventDataObject.ContextCollection.Add(new Context { Type = new ContextType { Type = "EINV_DateTime" }, Value = "2022-08-27T17:50:00" });
				eventDataObject.ContextCollection.Add(new Context { Type = new ContextType { Type = "EINV_IDType" }, Value = "TYP" });
				eventDataObject.ContextCollection.Add(new Context { Type = new ContextType { Type = "EINV_VerificationURL" }, Value = "TestVerificationURL" });
				if (universalNamespace == UniversalXmlInfo.Namespace_2012_11)
				{
					var companyCodeContextType = new ContextType() { Type = "CompanyCode" };
					eventDataObject.ContextCollection.Add(new Context() { Type = companyCodeContextType, Value = GlbCompany.CurrentCompany.GC_Code });
				}

				eventDataObject.AttachedDocumentCollection = new List<AttachedDocument>();
				var expectedImportResult = @"Warning - Could not link Attached Documents to AccEInvoicingBatch. Does not have eDocs.
Linked Event to AccEInvoicingBatch.
Successfully Added eDoc: TestMiscellaneousFile.pdf.
Adding eDoc with a document type of MSC and name of TestMiscellaneousFile.pdf.
Linked Event to Accounts Receivable Invoice.";
				var attachedDocument = new AttachedDocument()
				{
					Type = new DocumentType() { Code = "MSC", Description = "Miscellaneous Document" },
					FileName = "TestMiscellaneousFile.pdf",
					IsPublished = true,
					ImageData = memoryStream
				};
				eventDataObject.AttachedDocumentCollection.Add(attachedDocument);

				var message = GetQueuedUniversalEventMessage(Factory, universalNamespace);

				Factory.SaveForTesting();

				var serviceTaskLog = new ServiceTaskLogForTesting();
				var manager = new UniversalMessageProcessingManager(serviceTaskLog);

				using (Factory.BOFactory.AddDisposableService())
				{
					var importResults = manager.Process(message, eventDataObject);
					var attemptedImports = importResults.ImportResults;
					Factory.SaveForTesting();
					AssertEquals(expectedImportResult, attemptedImports.Single().ToString());
				}

				AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
				AssertEquals(1, arInvoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
				AssertContains("", "".Trim(), serviceTaskLog.ToString());
				var authorisation = Factory.BOFactory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, arInvoice.PK));
				AssertNotNull("Authorisation record should be created", authorisation);
				AssertEquals("KSA", authorisation.AHF_RecordType.ToString());
				AssertEquals("1", authorisation.AHF_Counter.ToString());
				AssertEquals("1", authorisation.AHF_Number.ToString());
				AssertEquals("TYP", authorisation.AHF_IDType.ToString());
				AssertEquals("TestVerificationURL", authorisation.AHF_VerificationUrl.ToString());
				AssertEquals("One eDoc added", 1, arInvoice.DocManagerInfo.AllEDocs.Count);
				var eDoc = arInvoice.DocManagerInfo.AllEDocs[0];
				AssertEquals("MSC", eDoc.DocType);
				AssertEquals("Miscellaneous Document", eDoc.Description);
				AssertEquals("TestMiscellaneousFile.pdf", eDoc.FileName);
				AssertEquals(true, eDoc.IsPublished);
				AssertEquals("test", Encoding.UTF8.GetString(eDoc.ImageData, 0, eDoc.ImageData.Length));
				AssertEquals(EInvoicingPivotState.Succeed, pivot.AIP_Status);
			}
		}

		#endregion

		#region CountrySupportedByGlobalEInvoicing

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_Namespace_2011_11_ForCountrySupportedByGlobalEInvoicing()
		{
			AssertForAcknowledgement_ForCountrySupportedByGlobalEInvoicing(UniversalXmlInfo.Namespace_2011_11);
		}

		public void TestGetDataContextKeyMatchingQuery_ForAcknowledgement_Namespace_2012_11_ForCountrySupportedByGlobalEInvoicing()
		{
			AssertForAcknowledgement_ForCountrySupportedByGlobalEInvoicing(UniversalXmlInfo.Namespace_2012_11);
		}

		void AssertForAcknowledgement_ForCountrySupportedByGlobalEInvoicing(string nameSpace)
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			var batch = objectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var universalEventDataObject = GetEventDataObject_ForAcknowledgement_byCountryAndMessageSubType(CountryCodes.Mexico, "GEN", batch, nameSpace);
			var message = GetQueuedUniversalEventMessage(universalEventDataObject, nameSpace);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}.
Linked Event to {invoice.HumanReadableName}.");
			AssertEquals(expectedImportResult, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.InterchangeAcknowledgedCode).Count());
			AssertEquals(1, invoice.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.InterchangeAcknowledgedCode).Count());
		}

		public void TestGetDataContextKeyMatchingQuery_ForRejection_ForCountrySupportedByGlobalEInvoicing()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			var batch = objectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var universalEventDataObject = GetEventDataObject_ForRejection();

			var message = GetQueuedUniversalEventMessage(universalEventDataObject, UniversalXmlInfo.Namespace_2012_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult1 = FormattableString.Invariant($@"Linked Event to {batch.HumanReadableName}");
			var expectedImportResult2 = FormattableString.Invariant($@"Linked Event to {invoice.HumanReadableName}");
			AssertContains(expectedImportResult1, importResults.Single().ToString());
			AssertContains(expectedImportResult2, importResults.Single().ToString());
			AssertEquals(1, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());
			AssertEquals(1, invoice.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeRejectedCode).Count());

			UniversalEventDataObject GetEventDataObject_ForRejection()
			{
				var eventDataObject = new UniversalEventDataObject();
				eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
				eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
				eventDataObject.EventType = AutoEvents.InterchangeRejectedCode;
				eventDataObject.EventTime = ZDateTimeOffset.Now;
				eventDataObject.EventParameters = new EventParameters
				{
					MessageType = CountryCodes.Mexico,
					Reason = "This is the rejection reason."
				};
				eventDataObject.ContextCollection = new List<Context>();
				eventDataObject.ContextCollection.Add(new Context() { Type = new ContextType { Type = "CompanyCode" }, Value = GlbCompany.CurrentCompany.GC_Code });

				return eventDataObject;
			}
		}

		public void TestOnLogParentFoundFromEDIMessage_UnsupportedEventTypeOrMessageSubType_ForCountrySupportedByGlobalEInvoicing()
		{
			var objectCreator = new TestObjectCreator(Factory.BOFactory);
			var invoice = objectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "00001000", objectCreator.EUR, 1m, 100m, 0, 100m, 0m);
			var batch = objectCreator.CreateEInvoicingBatch(1, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
			var pivot = objectCreator.CreateEInvoicingTransactionPivot(batch, invoice, EInvoicingPivotState.Sent);
			Factory.SaveForTesting();

			var expectedMessageType = "XXX";
			var countryObjectFactoryMock = new Mock<ICountryEInvoicingObjectFactory>();
			var isEventMessageProcessSupportedSetup = countryObjectFactoryMock.Setup(x => x.GetGlobalXUEFunctionalityProvider().IsBatchEventMessageProcessSupported(AutoEvents.InterchangeAcknowledgedCode, expectedMessageType));
			GlobalEInvoicingObjectFactory.TestCountryFactory = countryObjectFactoryMock.Object;

			var universalEventDataObject = GetEventDataObject_ForAcknowledgement_byCountryAndMessageSubType(GlobalEInvoicingObjectFactory.TestCountryCode, expectedMessageType, batch, UniversalXmlInfo.Namespace_2011_11);
			var message = GetQueuedUniversalEventMessage(universalEventDataObject);
			var manager = new AccEInvoicingBatchDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			isEventMessageProcessSupportedSetup.Returns(false);
			manager.OnLogParentFoundFromEDIMessage(logger, universalEventDataObject, message, batch);

			AssertEquals("Error - Cannot process message for unsupported Event Type 'IAK' and Message Sub Type 'XXX' for country/region 'Netlandia'.", logger.ToString());
			AssertEquals("AccEInvoicingBatchDataContextManager.messageProcessorCheck", ErrorReporter.LastKeyReported);
			AssertEquals("Error Message", true, ErrorReporter.LastMessageReported.Contains("Cannot process message for unsupported Event Type 'IAK' and Message Sub Type 'XXX' for country/region 'Netlandia'."));
			ErrorReporter.Clear();

			logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			isEventMessageProcessSupportedSetup.Returns(true);
			manager.OnLogParentFoundFromEDIMessage(logger, universalEventDataObject, message, batch);
			AssertEquals("", logger.ToString());
			AssertEquals("", ErrorReporter.LastKeyReported);
		}

		UniversalEventDataObject GetEventDataObject_ForAcknowledgement_byCountryAndMessageSubType(ZString countryCode, ZString messageSubType, AccEInvoicingBatch batch, string nameSpace)
		{
			var eventDataObject = new UniversalEventDataObject();
			if (nameSpace == UniversalXmlInfo.Namespace_2011_11)
			{
				eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			}
			else
			{
				eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2012_11.DataContext();
			}
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = countryCode,
				MessageSubType = messageSubType,
			};
			eventDataObject.ContextCollection = new List<Context>();
			eventDataObject.ContextCollection.Add(new Context { Type = new ContextType { Type = "CompanyCode" }, Value = GlbCompany.CurrentCompany.GC_Code });
			eventDataObject.ContextCollection.Add(new Context { Type = new ContextType { Type = "EINV_GovtAllocatedRefNumber" }, Value = "72cf24e4-366e-11eb-adc1-0242ac120002" });
			eventDataObject.ContextCollection.Add(new Context { Type = new ContextType { Type = "EINV_MX_RemainingStamps" }, Value = "0" });

			return eventDataObject;
		}

		#endregion

		public void TestGetDataContextKeyMatchingQuery_InvalidCountryCode_ReleaseMode()
		{
			PrepareTestDataForGetDataContextKeyMatchingQuery(out AccEInvoicingBatch batch, out UniversalEventDataObject eventDataObject);
			eventDataObject.EventParameters.Reason = AccEInvoicingBatchDataContextManager.TestInReleaseModeReason_ForTestOnly;

			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;

			AssertEquals("Warning - No Module found a Business Entity to link this Universal Event to.", importResults.Single().ToString());
			AssertEquals(0, batch.Logs.Find(l => l.SL_SE_NKEvent == Events.InterchangeAcknowledgedCode).Count());
			AssertContains("Invalid country code in Release mode should be logged", "Event parameter contains unsupported country/region 'AU'", serviceTaskLog.ToString());
			Assert("Invalid country code in Release mode should NOT be error reported", string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
		}

		public void TestGetDataContextKeyMatchingQuery_InvalidCountryCode_DebugMode()
		{
			PrepareTestDataForGetDataContextKeyMatchingQuery(out AccEInvoicingBatch batch, out UniversalEventDataObject eventDataObject);

			var message = GetQueuedUniversalEventMessage(eventDataObject);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			var ex = AssertExceptionThrown<InvalidOperationException>(() => manager.Process(message));
			AssertEquals("AccEInvoicingBatchDataContextManager.GetDataContextKeyMatchingQuery : Event parameter contains unsupported country/region 'AU'", ex.Message);

			AssertContains("The error should be logged", "Event parameter contains unsupported country/region 'AU'", serviceTaskLog.ToString());
			Assert("No error report in debug mode", string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
		}

		void PrepareTestDataForGetDataContextKeyMatchingQuery(out AccEInvoicingBatch batch, out UniversalEventDataObject eventDataObject)
		{
			batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = GlbCompany.CurrentCompany.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_Status = "SNT";
			Factory.SaveForTesting();

			eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = DataContextFactory.New();
			eventDataObject.DataContext.AddDataTarget(DataContextType.AccEInvoicingBatch, string.Format("{0}", batch.AIB_BatchNumber));
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.EventType = AutoEvents.InterchangeAcknowledgedCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;
			eventDataObject.EventParameters = new EventParameters
			{
				MessageType = Core.Constants.CountryCodes.Australia,
				MessageSubType = "rispostaSdIRiceviFile",
			};
		}

		public void TestOnLogParentFoundFromEDIMessageWithInvalidCountryCode()
		{
			var eventXmlText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>1</Key>
              <Type>AccEInvoicingBatch</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>IAK</EventType>
        <EventParameters>
           <MessageType>AU</MessageType>
           <MessageSubType>rispostaSdIRiceviFile</MessageSubType>
        </EventParameters>
        <ContextCollection>
           <Context>
            <Type>CompanyCode</Type>
            <Value>EDI</Value>
          </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";

			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			batch.AIB_GC = GlbCompany.CurrentCompany.PK;
			batch.AIB_BatchNumber = 1;
			batch.AIB_Status = "SNT";
			Factory.SaveForTesting();

			var xmlEvent = new XmlEventDeserializer().Parse(eventXmlText);
			var manager = new AccEInvoicingBatchDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			manager.OnLogParentFoundFromEDIMessage(logger, xmlEvent, null, batch);
			AssertEquals("Error - Cannot process message for unsupported country/region 'AU'", logger.ToString());
			AssertEquals("AccEInvoicingBatchDataContextManager.messageProcessorCheck", ErrorReporter.LastKeyReported);
			AssertEquals("Error Message", true, ErrorReporter.LastMessageReported.Contains("Cannot process message for unsupported country/region 'AU'"));
			ErrorReporter.Instance.Clear();
		}

		public void TestOnLogParentFoundFromEDIMessageWithInvalidObject()
		{
			var eventXmlText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>1</Key>
              <Type>AccEInvoicingBatch</Type>
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
        </ContextCollection>
      </Event>
</UniversalEvent>";

			var xmlEvent = new XmlEventDeserializer().Parse(eventXmlText);
			var manager = new AccEInvoicingBatchDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			manager.OnLogParentFoundFromEDIMessage(logger, xmlEvent, null, Factory.NewWithValidTestData<AccTaxRate>());
			AssertEquals("AccEInvoicingBatchDataContextManager.OnLogParentFoundFromEDIMessage", ErrorReporter.LastKeyReported);
			AssertEquals("Expect DeveloperNotificationException", "Unable to process - unsupported type detected, Parent BO type is Enterprise.MasterFiles.Business.AccTaxRate", ErrorReporter.LastMessageReported);
			AssertContains("Error - Unable to process - unsupported type detected, Parent BO type is", logger.ToString());
			ErrorReporter.Instance.Clear();
		}

		public void TestOnLogParentFoundFromEDIMessageWithUnknownEventType()
		{
			var eventXmlText = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
      <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Key>1</Key>
              <Type>AccEInvoicingBatch</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2018-01-09T09:30:10</EventTime>
        <EventType>XYZ</EventType>
        <EventParameters>
           <MessageType>IT</MessageType>
           <MessageSubType>rispostaSdIRiceviFile</MessageSubType>
        </EventParameters>
        <ContextCollection>
           <Context>
            <Type>CompanyCode</Type>
            <Value>EDI</Value>
          </Context>
        </ContextCollection>
      </Event>
</UniversalEvent>";

			var xmlEvent = new XmlEventDeserializer().Parse(eventXmlText);
			var manager = new AccEInvoicingBatchDataContextManager();
			var logger = new XmlSessionTracker(new ServiceTaskLogForTesting());
			manager.OnLogParentFoundFromEDIMessage(logger, xmlEvent, null, Factory.NewWithValidTestData<AccEInvoicingBatch>());
			AssertEquals("AccEInvoicingBatchDataContextManager.OnLogParentFoundFromEDIMessage", ErrorReporter.LastKeyReported);
			AssertEquals("Expect DeveloperNotificationException", "Cannot process message for unknown event type 'XYZ'", ErrorReporter.LastMessageReported);
			AssertContains("Error - Cannot process message for unknown event type 'XYZ'", logger.ToString());
			ErrorReporter.Instance.Clear();
		}

		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("No Job Number support", true);
		}

		protected override AccEInvoicingBatch GetNewBusinessObjectForTesting()
		{
			AccEInvoicingBatch result = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			Factory.SaveForTesting();
			return result;
		}

		internal TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(new BusinessObjectFactory()));
		TestObjectCreator testObjectCreator;
	}
}
