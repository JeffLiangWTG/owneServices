using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public abstract class AccEInvoiceBatchToGEIConverter : IAccEInvoiceBatchToGEIConverter
	{
		public (GlobalElectronicInvoicing EInvoice, ZString ValidationErrors, ZString ValidationWarnings) Convert(AccEInvoicingBatch batch)
		{
			PerformBeforeConvert(batch);
			var builder = GetGEIBuilder(batch.AIB_BatchNumber.ToString());
			var (eInvoice, validationErrors, validationWarnings) = builder.Create();
			return (eInvoice, validationErrors.ToString(), validationWarnings.ToString());
		}

		protected virtual void PerformBeforeConvert(AccEInvoicingBatch batch)
		{
		}

		protected abstract IGlobalElectronicInvoiceBuilder GetGEIBuilder(string batchNumber);

		#region IDisposable Support
		bool disposed; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}