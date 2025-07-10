using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalEventDataObject = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.DataTransfer.Universal.Testing
{
	[TestedType(typeof(CollectionBatchDataContextManager))]
	public class CollectionBatchDataContextManagerTest : DataContextManagerTestCase<CollectionBatchDataContextManager, AccCollectionBatch>
	{
		protected override void TestBusinessObjectImplementsIJobNumberCore()
		{
			Assert("CollectionBatch doesn't have any JobNumber", true);
		}

		public void TestDataImportEvent_SpecificOrderNumber()
		{
			SetupCollectionBatch();

			var eventDataObject = SetupUniversalEvent(Batch.ACB_BatchNumber, "2019-07-17", Order1.ACO_OrderNumber, "100.00");

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {Batch.HumanReadableName}.
Linked Event to {Order1.HumanReadableName}.
Order {Order1.ACO_OrderNumber} Deposited Date is updated");
			AssertEquals(expectedImportResult, importResults.Single().ToString());

			AssertEquals(1, Batch.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).Count());
			AssertEquals(1, Order1.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).Count());
			AssertEquals(new ZDate(2019, 7, 17), Order1.ACO_DepositedDate);
			AssertEquals(ZDate.Empty, Order2.ACO_DepositedDate);
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		public void TestDataImportEvent_AllOrderNumber()
		{
			SetupCollectionBatch();

			var eventDataObject = SetupUniversalEvent(Batch.ACB_BatchNumber, "2019-07-17");

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			var expectedImportResult = FormattableString.Invariant($@"Linked Event to {Batch.HumanReadableName}.
Linked Event to {Order1.HumanReadableName}.
Order {Order1.ACO_OrderNumber} Deposited Date is updated
Linked Event to {Order2.HumanReadableName}.
Order {Order2.ACO_OrderNumber} Deposited Date is updated");
			AssertEquals(expectedImportResult, importResults.Single().ToString());

			AssertEquals(1, Batch.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).Count());
			AssertEquals(1, Order1.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).Count());
			AssertEquals(1, Order2.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).Count());
			AssertEquals(new ZDate(2019, 7, 17), Order1.ACO_DepositedDate);
			AssertEquals(new ZDate(2019, 7, 17), Order2.ACO_DepositedDate);
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		public void TestGetDataContextKeyMatchingQuery_InvalidBatchNumber()
		{
			SetupCollectionBatch();

			var eventDataObject = SetupUniversalEvent("test123", "2019-07-17", Order1.ACO_OrderNumber, "100.00");

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			AssertEquals("Warning - No Module found a Business Entity to link this Universal Event to.", importResults.Single().ToString());

			AssertEquals(0, Batch.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).Count());
			AssertEquals(0, Order1.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).Count());
			AssertEquals(ZDate.Empty, Order1.ACO_DepositedDate);
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		public void TestGetDataContextKeyMatchingQuery_InvalidOrderNumber()
		{
			SetupCollectionBatch();

			var eventDataObject = SetupUniversalEvent(Batch.ACB_BatchNumber, "2019-07-17", "test123", "100.00");

			var message = GetQueuedUniversalEventMessage(eventDataObject, UniversalXmlInfo.Namespace_2011_11);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			var importResults = manager.Process(message).ImportResults;
			AssertEquals("Linked Event to AccCollectionBatch.", importResults.Single().ToString());

			AssertEquals(1, Batch.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).Count());
			AssertEquals(0, Order1.Logs.Find(l => l.SL_SE_NKEvent == Events.DataImportCode).Count());
			AssertEquals(ZDate.Empty, Order1.ACO_DepositedDate);
			AssertEquals(ZDate.Empty, Order2.ACO_DepositedDate);
			AssertContains("", "".Trim(), serviceTaskLog.ToString());
		}

		void SetupCollectionBatch()
		{
			var batch = GetNewBusinessObjectForTesting();
			var orders = batch.CollectionOrders.OrderBy(x => x.ACO_OrderNumber);
			var order1 = orders.First();
			var order2 = orders.Last();

			this.Batch = Factory.Load<AccCollectionBatch>(batch.PK);
			Order1 = Factory.Load<AccCollectionOrder>(order1.PK);
			Order2 = Factory.Load<AccCollectionOrder>(order2.PK);

			AssertEquals(ZDate.Empty, Order1.ACO_DepositedDate);
			AssertEquals(ZDate.Empty, Order2.ACO_DepositedDate);
		}

		UniversalEventDataObject SetupUniversalEvent(string batchNumber, string depositedDate, string orderNumber = null, string receiptAmount = null)
		{
			var eventDataObject = new UniversalEventDataObject();
			eventDataObject.DataContext = new UniversalDataBuss.DataObjects.Universal._2011_11.DataContext();
			eventDataObject.DataContext.AddDataTarget(DataContextType.CollectionBatch, string.Format("{0}", batchNumber));
			eventDataObject.DataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			eventDataObject.EventType = AutoEvents.DataImportCode;
			eventDataObject.EventTime = ZDateTimeOffset.Now;

			eventDataObject.ContextCollection = new List<Context>();
			if (!string.IsNullOrEmpty(orderNumber))
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = AccCollectionOrderSchema.Constants.TableName, Value = orderNumber });
			}
			if (!string.IsNullOrEmpty(receiptAmount))
			{
				eventDataObject.ContextCollection.Add(new Context() { Type = "ReceiptAmount", Value = receiptAmount });
			}
			eventDataObject.ContextCollection.Add(new Context() { Type = "DepositedDate", Value = depositedDate });

			return eventDataObject;
		}

		AccCollectionBatch Batch;
		AccCollectionOrder Order1;
		AccCollectionOrder Order2;

		#region Implementation

		protected override AccCollectionBatch GetNewBusinessObjectForTesting()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(newFactory);

			InvoicingBase invoice1 = newFactory.NewWithValidTestData<ARInvoice>();
			InvoicingBase invoice2 = newFactory.NewWithValidTestData<ARInvoice>();
			var bank = newFactory.NewWithValidTestData<AccBankAccount>();

			var batch = newFactory.New<AccCollectionBatch>();
			batch.ACB_TotalAmount = 300m;
			batch.ACB_AB = bank.PK;
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.ACB_BatchNumber = "0001000";

			var order1 = newFactory.New<AccCollectionOrder>();
			order1.ACO_ACB = batch.PK;
			order1.ACO_CollectionDate = ZDateTime.Today.Date;
			order1.ACO_OH_Debtor = creator.AALSHI.PK;
			order1.ACO_OrderNumber = "000001";
			order1.IncludeInBatch = true;
			order1.ACO_Amount = 100m;

			var orderline1 = newFactory.New<AccCollectionOrderLine>();
			orderline1.AOL_ACO = order1.PK;
			orderline1.AOL_AH = invoice1.PK;
			orderline1.AOL_IsCancelled = false;
			orderline1.IncludeInOrder = true;

			var order2 = newFactory.New<AccCollectionOrder>();
			order2.ACO_ACB = batch.PK;
			order2.ACO_CollectionDate = ZDateTime.Today.Date;
			order2.ACO_OH_Debtor = creator.AALSHI.PK;
			order2.ACO_OrderNumber = "000002";
			order2.IncludeInBatch = true;
			order2.ACO_Amount = 200m;

			var orderline2 = newFactory.New<AccCollectionOrderLine>();
			orderline2.AOL_ACO = order1.PK;
			orderline2.AOL_AH = invoice2.PK;
			orderline2.AOL_IsCancelled = false;
			orderline2.IncludeInOrder = true;
			newFactory.Save();

			return batch;
		}

		#endregion
	}
}
