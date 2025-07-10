using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania.Testing
{
	public class RomaniaEDIInterchangeCreatorTest : GEIEDIInterchangeCreatorTest
	{
		ICountryEInvoicingObjectFactory GetTestCountryFactory() => new RomaniaEInvoicingObjectFactory();

		public void TestProcessSingleBatch_WhenPivotStatusIsDelivered()
		{
			var batch = GetBatchWithReadyStatus_WhenPivotStatusIsDelivered();

			AssertEquals(EInvoicingBatchState.Ready, batch.AIB_Status);
			var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			var logger = new TestServiceLogger();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var processor = GetInterchangeProcessor();
				processor.Process(logger);
			}

			AssertEquals(EInvoicingBatchState.Sent, batch.AIB_Status);
			AssertEquals(EInvoicingPivotState.Delivered, batch.TransactionPivots[0].AIP_Status);

			ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("expect 1 interchange created", 1, ediInterchanges.Length);
			EInvoicingTestHelper.AssertGEIEInvoicingEDIInterchange(ediInterchanges[0], expectedMessageType: RomaniaEInvoiceAPICommandList.Codes.QueryInvoiceRequest, expectedTo: ExpectedServicePoint);

			ediMessages = ediInterchanges.First().LoadMessages();
			AssertEquals("expect 1 messages created", 1, ediMessages.Length);
			EInvoicingTestHelper.AssertGEIEInvoicingEDIMessage(ediMessages[0], GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, expectedMessageType: RomaniaEInvoiceAPICommandList.Codes.QueryInvoiceRequest);
			AssertContains(string.Format("Universal Transaction batch [{0}] of company [{1}] (organization proxy [{2}]) has been successfully queued for delivery.", batch.AIB_BatchNumber, GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.OrgProxy.OH_Code), logger.ToString());
		}

		public void TestProcessSingleBatch_WhenThereIsABatchButNoInvoices()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(Business.TestObjectCreator.GetRandomInt(1, 1000), EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), Business.TestObjectCreator.GetRandomString(5), TestObjectCreator.EUR, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);

			batch.TransactionPivots[0].AIP_Status = EInvoicingPivotState.Discarded;
			batch.TransactionPivots[0].AIP_LastResponseReceivedUtc = ZDateTime.Now.AddDays(-1);
			Factory.Save();

			var logger = new TestServiceLogger();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var processor = GetInterchangeProcessor();
				processor.Process(logger);
			}

			var expectedMessage = $"Information|There is no invoice in batch [{batch.AIB_BatchNumber}] that is ready to be sent. Therefore, no EDI message can be created.";
			AssertContains(expectedMessage, logger.ToString());
		}

		public void TestProcessSingleBatch_WhenPivotStatusIsDelivered_AndBatchHasValidationErrors()
		{
			var batch = GetBatchWithReadyStatus_WhenPivotStatusIsDelivered();

			AssertEquals(EInvoicingBatchState.Ready, batch.AIB_Status);
			var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			var logger = new TestServiceLogger();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var converter = () => new MockAccEInvoiceBatchToGEIConverter(addValidationErrorForBatches: new[] { batch });
				var processor = GetInterchangeProcessor(converter);
				processor.Process(logger);
			}

			AssertEquals(EInvoicingBatchState.Discarded, batch.AIB_Status);
			AssertEquals(EInvoicingPivotState.BatchedWithError, batch.TransactionPivots[0].AIP_Status);

			ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("expect 1 interchange created", 1, ediInterchanges.Length);
			EInvoicingTestHelper.AssertGEIEInvoicingEDIInterchange(ediInterchanges[0], EDIInterchange.Status.Failed, expectedMessageType: EInvoiceAPICommandList.Codes.GenerateInvoiceRequest, expectedTo: ExpectedServicePoint);

			ediMessages = ediInterchanges.First().LoadMessages();
			AssertEquals("expect 1 messages created", 1, ediMessages.Length);
			EInvoicingTestHelper.AssertGEIEInvoicingEDIMessage(ediMessages[0], GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, EDIMessage.Status.Failed, expectedMessageType: EInvoiceAPICommandList.Codes.GenerateInvoiceRequest);
			Assert("Notes added to EDI Message", ediMessages[0].Notes.HasNotes);
			AssertContains("Notes should contains error details", "Forced Validation Error generated for testing while creating Global Electronic Invoicing", ediMessages[0].Notes.GetAllNotes().ToArray<StmNote>().First().ST_NoteDataAsText);

			AssertContains("Debug|Attempting to send an email notification containing error details.", logger.ToString());
		}

		public void TestProcessSingleBatch_WhenPivotStatusIsDelivered_AndBatchHasValidationWarnings()
		{
			var batch = GetBatchWithReadyStatus_WhenPivotStatusIsDelivered();

			AssertEquals(EInvoicingBatchState.Ready, batch.AIB_Status);
			var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
			var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			var logger = new TestServiceLogger();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var converter = () => new MockAccEInvoiceBatchToGEIConverter(addValidationWarningForBatches: new[] { batch });
				var processor = GetInterchangeProcessor(converter);
				processor.Process(logger);
			}

			AssertEquals(EInvoicingBatchState.Sent, batch.AIB_Status);
			AssertEquals(EInvoicingPivotState.Delivered, batch.TransactionPivots[0].AIP_Status);

			ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("expect 1 interchange created", 1, ediInterchanges.Length);
			EInvoicingTestHelper.AssertGEIEInvoicingEDIInterchange(ediInterchanges[0], expectedMessageType: EInvoiceAPICommandList.Codes.GenerateInvoiceRequest, expectedTo: ExpectedServicePoint);

			ediMessages = ediInterchanges.First().LoadMessages();
			AssertEquals("expect 1 messages created", 1, ediMessages.Length);
			EInvoicingTestHelper.AssertGEIEInvoicingEDIMessage(ediMessages[0], GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK, expectedMessageType: EInvoiceAPICommandList.Codes.GenerateInvoiceRequest);
			Assert("Notes added to EDI Message", ediMessages[0].Notes.HasNotes);
			AssertContains("Notes should contains warning details", "Forced Validation Warning generated for testing while creating Global Electronic Invoicing", ediMessages[0].Notes.GetAllNotes().ToArray<StmNote>().First().ST_NoteDataAsText);

			AssertContains(string.Format("Universal Transaction batch [{0}] of company [{1}] (organization proxy [{2}]) has been successfully queued for delivery.", batch.AIB_BatchNumber, GlbCompany.CurrentCompany.GC_Code, GlbCompany.CurrentCompany.OrgProxy.OH_Code), logger.ToString());
		}

		public override void TestIsBillingSupported()
		{
			var processor = new InterchangeProcessorWithIsBillingSupportedExposed();
			Assert("Billing transactions are created for Romania.", processor.GetIsBillingSupported());
		}

		public override void TestProcessPartOfBatchesFailed()
		{
			Assert("Each batch contains only one invoice, so partial failure of a batch is not possible for a Romania company", true);
		}

		#region Implementation

		protected override string CountryCode => CountryCodes.Romania;

		protected override string ExpectedServicePoint => "XHUB_RO_EINVOICING";

		public override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(GlbCompany company)
		{
			return GetInterchangeProcessor();
		}

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(
			Func<IAccEInvoiceBatchToGEIConverter> converter = null,
			ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null,
			AccEInvoicingBatch batchWithErrors = null)
		{
			var interchangeCreator = new StubRomaniaEDIInterchangeCreator(GlbCompany.CurrentCompany, GetTestCountryFactory(), converter, forceThisErrorWhileCreatingEDIInterchange);
			interchangeCreator.BatchForWhichErrorWillBeForced = batchWithErrors;
			return interchangeCreator;
		}

		protected override AccEInvoicingBatch GetBatchWithReadyStatus()
		{
			return GetBatchWithReadyStatusForSending(out string number);
		}

		protected AccEInvoicingBatch GetBatchWithReadyStatus_WhenPivotStatusIsDelivered()
		{
			var batch = GetBatchWithReadyStatusForSending(out string number);
			batch.TransactionPivots[0].AIP_Status = EInvoicingPivotState.Delivered;
			batch.TransactionPivots[0].AIP_LastResponseReceivedUtc = ZDateTime.Now.AddDays(-1);
			Factory.Save();

			return batch;
		}

		protected override AccEInvoicingBatch GetBatchWithReadyStatusForSending(out string number)
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(Business.TestObjectCreator.GetRandomInt(1, 1000), EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), Business.TestObjectCreator.GetRandomString(5), TestObjectCreator.EUR, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);
			Factory.Save();

			number = arInvoice.AH_TransactionNum;
			return batch;
		}

		class InterchangeProcessorWithIsBillingSupportedExposed : RomaniaEDIInterchangeCreator
		{
			public InterchangeProcessorWithIsBillingSupportedExposed()
				: base(GlbCompany.CurrentCompany, null, string.Empty)
			{ }

			public bool GetIsBillingSupported() => IsBillingSupported;
		}

		#endregion
	}

	public class StubRomaniaEDIInterchangeCreator : RomaniaEDIInterchangeCreator, IEDIIntechangeCreatorForTest
	{
		public StubRomaniaEDIInterchangeCreator(GlbCompany company, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory, Func<IAccEInvoiceBatchToGEIConverter> converter = null, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null)
				: base(company, countryEInvoicingObjectFactory, string.Empty)
		{
			Converter = converter;
			ForceThisError = forceThisErrorWhileCreatingEDIInterchange;
		}

		Func<IAccEInvoiceBatchToGEIConverter> Converter { get; }

		ExceptionTypes? ForceThisError { get; }

		public AccEInvoicingBatch BatchForWhichErrorWillBeForced { get; set; }

		protected override void CreateEDIMessageAndInterchangeCore(TransactionBatchProcessContext batchProcessContext, INotifications notifications)
		{
			base.CreateEDIMessageAndInterchangeCore(batchProcessContext, notifications);
			EInvoicingTestHelper.ForceErrorWhileCreatingEDIInterchange(batchProcessContext, notifications, ForceThisError, BatchForWhichErrorWillBeForced);
		}

		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => Converter == null ? new RomaniaTransactionBatchToGEIConverter(CountryFactory) : Converter();

		bool IEDIIntechangeCreatorForTest.AddErrorToEDIMessageNotesIfAny => AddErrorToEDIMessageNotesIfAny;
	}
}
