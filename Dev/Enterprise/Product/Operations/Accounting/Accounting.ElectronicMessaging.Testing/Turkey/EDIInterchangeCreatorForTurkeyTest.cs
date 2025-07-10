using System;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey.Testing
{
	public class EDIInterchangeCreatorForTurkeyTest : GEIEDIInterchangeCreatorTest
	{
		ICountryEInvoicingObjectFactory GetTestCountryFactory() => new TurkeyEInvoicingObjectFactory();
		protected override string ExpectedServicePoint => "XHUB_TR_EINVOICING";

		public override void TestIsBillingSupported()
		{
			var processor = new InterchangeProcessorWithIsBillingSupportedExposed(GlbCompany.CurrentCompany, GetTestCountryFactory());
			Assert("Billing transactions are created for Turkey.", processor.GetIsBillingSupported());
		}

		public override void TestProcessPartOfBatchesFailed()
		{
			Assert("Each batch contains only one invoice, so partial failure of a batch is not possible for an Turkey company", true);
		}

		protected override AccEInvoicingBatch GetBatchWithReadyStatus()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(Business.TestObjectCreator.GetRandomInt(1, 1000), EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), Business.TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);
			Factory.Save();

			return batch;
		}

		#region Implementation

		protected override string CountryCode => CountryCodes.Turkey;

		public override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(GlbCompany company) => new MockEDIInterchangeCreatorForTurkeyEInvoicingBatch(GlbCompany.CurrentCompany, () => new MockAccEInvoiceBatchToGEIConverter(), GetTestCountryFactory());

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(Func<IAccEInvoiceBatchToGEIConverter> converter = null, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null, AccEInvoicingBatch batchWithErrors = null)
		{
			var interchangeCreator = new MockEDIInterchangeCreatorForTurkeyEInvoicingBatch(GlbCompany.CurrentCompany, converter, GetTestCountryFactory(), forceThisErrorWhileCreatingEDIInterchange)
			{
				BatchForWhichErrorWillBeForced = batchWithErrors
			};
			return interchangeCreator;
		}

		#endregion

		#region Inner Classes

		class InterchangeProcessorWithIsBillingSupportedExposed : EDIInterchangeCreatorForTurkey
		{
			public InterchangeProcessorWithIsBillingSupportedExposed(GlbCompany company, ICountryEInvoicingObjectFactory countryFactory)
				: base(company,countryFactory)
			{ }

			public bool GetIsBillingSupported() => IsBillingSupported;
		}

		public class MockEDIInterchangeCreatorForTurkeyEInvoicingBatch : EDIInterchangeCreatorForTurkey
		{
			public MockEDIInterchangeCreatorForTurkeyEInvoicingBatch(GlbCompany company, Func<IAccEInvoiceBatchToGEIConverter> converter, ICountryEInvoicingObjectFactory countryFactory, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null)
				: base(company, countryFactory)
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

			protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => Converter();
		}

		#endregion
	}
}
