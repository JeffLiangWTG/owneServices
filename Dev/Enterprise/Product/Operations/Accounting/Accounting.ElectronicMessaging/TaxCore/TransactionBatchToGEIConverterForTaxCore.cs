using System;
using CargoWise.Common;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public class TransactionBatchToGEIConverterForTaxCore : TransactionBatchToGEIConverter
	{
		public TransactionBatchToGEIConverterForTaxCore(ITaxCoreCountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
		{
			Argument.NotNull(countryEInvoicingObjectFactory, nameof(countryEInvoicingObjectFactory));
			CountryEInvoicingObjectFactory = countryEInvoicingObjectFactory;
		}

		protected override void PerformBeforeConvert(AccEInvoicingBatch batch)
		{
			base.PerformBeforeConvert(batch);
			Batch = batch;
		}

		protected override IGlobalElectronicInvoiceBuilder GetGEIBuilder(string batchNumber)
		{
			Argument.NotNull(Batch, nameof(Batch));
			if (Batch.TransactionPivots.Count != 1)
			{
				var exception = new NotSupportedException(FormattableString.Invariant($"The batch has {Batch.TransactionPivots.Count} transactions. There must be only one transaction per batch to generate the Electronic Invoice."));
				ErrorReporter.ReportOnce("TransactionBatchToGEIConverterForTaxCore_NotOneTransaction", EventProcessorHelper.GetBatchInfo(Batch), exception);
				throw exception;
			}

			var factory = Batch.Factory;
			var transaction = factory.Load<TransactionHeader>(Batch.TransactionPivots[0].AIP_ParentID);
			return new GlobalElectronicInvoiceBuilderForTaxCore(batchNumber, UniversalBatch, transaction, CountryEInvoicingObjectFactory);
		}

		AccEInvoicingBatch Batch { get; set; }

		readonly ITaxCoreCountryEInvoicingObjectFactory CountryEInvoicingObjectFactory;
	}
}
