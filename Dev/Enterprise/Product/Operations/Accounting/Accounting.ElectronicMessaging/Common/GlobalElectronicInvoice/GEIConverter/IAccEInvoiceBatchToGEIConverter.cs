using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public interface IAccEInvoiceBatchToGEIConverter : IDisposable
	{
		/// <summary>
		/// Creates a GEI EInvoices object, with optional validation errors and warnings. Based on the supplied batch.
		/// </summary>
		/// <param name="batch"></param>
		/// <remarks>
		/// A null EInvoice may be returned. This will not create EDI Interchange or Message objects. A validation error should also be returned.
		/// </remarks>
		(GlobalElectronicInvoicing EInvoice, ZString ValidationErrors, ZString ValidationWarnings) Convert(AccEInvoicingBatch batch);
	}
}
