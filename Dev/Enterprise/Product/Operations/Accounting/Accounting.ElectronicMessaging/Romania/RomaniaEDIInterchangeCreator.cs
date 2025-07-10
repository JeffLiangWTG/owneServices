using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.Export.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania
{
	public class RomaniaEDIInterchangeCreator : GlobalEDIInterchangeCreator
	{
		public RomaniaEDIInterchangeCreator(GlbCompany company, ICountryEInvoicingObjectFactory countryFactory, ZString suffix) : base(company, countryFactory, suffix)
		{
		}

		protected override ZGuid[] LoadEInvoicingPKsToBeSent(AccEInvoicingBatch batch)
		{
			if (batch.TransactionPivots.ToArray<AccEInvoicingTransactionPivot>().Single().AIP_Status == EInvoicingPivotState.Delivered)
			{
				return batch.GetInvoicesWithStatus(EInvoicingPivotState.Delivered).Select(x => x.PK).ToArray();
			}
			else
			{
				return base.LoadEInvoicingPKsToBeSent(batch);
			}
		}

		protected override void PerformAfterCreatingEDIMessageAndInterchangeSuccessfully(TransactionBatchProcessContext batchProcessContext)
		{
			base.PerformAfterCreatingEDIMessageAndInterchangeSuccessfully(batchProcessContext);

			var pivots = batchProcessContext.Batch.TransactionPivots.ToArray<AccEInvoicingTransactionPivot>().Single();

			if ((ZString)pivots.AIP_StatusInfo.OriginalValue == EInvoicingPivotState.Delivered
				&& pivots.AIP_Status == EInvoicingPivotState.Sent)
			{
				pivots.AIP_Status = EInvoicingPivotState.Delivered;
			}
		}

		protected override IAccEInvoiceBatchToGEIConverter BatchToGEIConverter => new RomaniaTransactionBatchToGEIConverter(CountryFactory);
	}

	public class RomaniaTransactionBatchToGEIConverter : GlobalTransactionBatchToGEIConverter
	{
		public RomaniaTransactionBatchToGEIConverter(ICountryEInvoicingObjectFactory countryFactory) : base(countryFactory)
		{
		}

		protected override TransactionBatchExporter GetBatchExporter(BatchExportDataAccess dataAccess)
			=> new RomaniaTransactionBatchExporter(dataAccess, PopulateOptionalXUTFieldsSetting.AllTrue());
	}

	public class RomaniaTransactionBatchExporter : TransactionBatchExporter
	{
		public RomaniaTransactionBatchExporter(BatchExportDataAccess dataAccess, PopulateOptionalXUTFieldsSetting optionalXUTFieldsSetting = null)
			: base(dataAccess, optionalXUTFieldsSetting: optionalXUTFieldsSetting)
		{
		}

		protected override InvoicingBase[] GetInvoicingBasesToExport(AccEInvoicingBatch batch)
		{
			if (batch.TransactionPivots.ToArray<AccEInvoicingTransactionPivot>().Single().AIP_Status == EInvoicingPivotState.Delivered)
			{
				return batch.GetInvoicesWithStatus(EInvoicingPivotState.Delivered);
			}
			else
			{
				return base.GetInvoicingBasesToExport(batch);
			}
		}
	}
}
