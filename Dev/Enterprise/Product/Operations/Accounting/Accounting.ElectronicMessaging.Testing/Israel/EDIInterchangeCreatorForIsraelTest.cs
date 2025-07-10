using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Israel.Testing
{
	public class EDIInterchangeCreatorForIsraelTest : GEIEDIInterchangeCreatorTest
	{
		ICountryEInvoicingObjectFactory GetTestCountryFactory() => new IsraelEInvoicingObjectFactory();

		public override void TestIsBillingSupported()
		{
			var processor = new InterchangeProcessorWithIsBillingSupportedExposed();
			Assert("Billing transactions are created for Israel.", processor.GetIsBillingSupported());
		}

		public override void TestProcessPartOfBatchesFailed()
		{
			Assert("Each batch contains only one invoice, so partial failure of a batch is not possible for a Israel company", true);
		}

		#region Implementation

		protected override string CountryCode => CountryCodes.Israel;

		protected override string ExpectedServicePoint => "XHUB_IL_EINVOICING";

		public override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(GlbCompany company) => GetInterchangeProcessor();

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(
			Func<IAccEInvoiceBatchToGEIConverter> converter = null,
			ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null,
			AccEInvoicingBatch batchWithErrors = null)
		{
			var interchangeCreator = new MockEDIInterchangeCreatorForIsrael(GlbCompany.CurrentCompany, GetTestCountryFactory(), converter, forceThisErrorWhileCreatingEDIInterchange);
			interchangeCreator.BatchForWhichErrorWillBeForced = batchWithErrors;
			return interchangeCreator;
		}

		protected override AccEInvoicingBatch GetBatchWithReadyStatus() => GetBatchWithReadyStatusForSending(out string number);

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

		class InterchangeProcessorWithIsBillingSupportedExposed : EDIInterchangeCreatorForIsrael
		{
			public InterchangeProcessorWithIsBillingSupportedExposed()
				: base(GlbCompany.CurrentCompany, null)
			{ }

			public bool GetIsBillingSupported() => IsBillingSupported;
		}

		#endregion
	}

	public class MockEDIInterchangeCreatorForIsrael : EDIInterchangeCreatorForIsrael, IEDIIntechangeCreatorForTest
	{
		public MockEDIInterchangeCreatorForIsrael(GlbCompany company, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory, Func<IAccEInvoiceBatchToGEIConverter> converter = null, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null)
				: base(company, countryEInvoicingObjectFactory)
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

		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => Converter == null ? new TransactionBatchToGEIConverterForIsrael(CountryFactory) : Converter();

		bool IEDIIntechangeCreatorForTest.AddErrorToEDIMessageNotesIfAny => AddErrorToEDIMessageNotesIfAny;
	}
}
