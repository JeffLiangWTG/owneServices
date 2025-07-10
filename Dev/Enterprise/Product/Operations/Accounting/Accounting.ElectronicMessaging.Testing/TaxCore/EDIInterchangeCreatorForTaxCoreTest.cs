using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing
{
	public abstract class EDIInterchangeCreatorForTaxCoreTest : GEIEDIInterchangeCreatorTest
	{
		public override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(GlbCompany company)
		{
			return new MockEDIInterchangeCreatorForTaxCoreEInvoicingBatch(GlbCompany.CurrentCompany, CountryCode, () => new MockAccEInvoiceBatchToGEIConverter());
		}

		protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeProcessor(Func<IAccEInvoiceBatchToGEIConverter> converter = null, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null, AccEInvoicingBatch batchWithErrors = null)
		{
			var interchangeCreator = new MockEDIInterchangeCreatorForTaxCoreEInvoicingBatch(GlbCompany.CurrentCompany, CountryCode, converter, forceThisErrorWhileCreatingEDIInterchange);
			interchangeCreator.BatchForWhichErrorWillBeForced = batchWithErrors;
			return interchangeCreator;
		}

		public override void TestIsBillingSupported()
		{
			var processor = new InterchangeProcessorWithIsBillingSupportedExposed(GlbCompany.CurrentCompany, CountryCode);
			Assert(FormattableString.Invariant($"Billing transactions are created for {CountryCode}."), processor.GetIsBillingSupported());
		}

		public override void TestProcessPartOfBatchesFailed()
		{
			Assert(FormattableString.Invariant($"Each batch contains only one invoice, so partial failure of a batch is not possible for an {CountryCode} company"), true);
		}

		protected override AccEInvoicingBatch GetBatchWithReadyStatus()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(Business.TestObjectCreator.GetRandomInt(1, 1000), Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), Business.TestObjectCreator.GetRandomString(5), TestObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();

			return batch;
		}

		protected override string CountryCode => CountryCodes.Fiji;

		#region Inner Classes

		class InterchangeProcessorWithIsBillingSupportedExposed : EDIInterchangeCreatorForTaxCore
		{
			public InterchangeProcessorWithIsBillingSupportedExposed(GlbCompany company, ZString countryCode)
				: base(company, TaxCoreEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(countryCode))
			{ }

			public bool GetIsBillingSupported() => IsBillingSupported;
		}

		#endregion
	}

	public class MockEDIInterchangeCreatorForTaxCoreEInvoicingBatch : EDIInterchangeCreatorForTaxCore
	{
		public MockEDIInterchangeCreatorForTaxCoreEInvoicingBatch(GlbCompany company, ZString countryCode, Func<IAccEInvoiceBatchToGEIConverter> converter, ExceptionTypes? forceThisErrorWhileCreatingEDIInterchange = null)
			: base(company, TaxCoreEInvoicingObjectFactory.GetICountryEInvoicingObjectFactory(countryCode))
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
}
