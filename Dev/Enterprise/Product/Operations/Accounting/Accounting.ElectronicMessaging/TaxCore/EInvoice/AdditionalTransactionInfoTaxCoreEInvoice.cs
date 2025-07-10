using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public interface IAdditionalTransactionInfoForTaxCoreEInvoice
	{
		ZString OriginalTransactionGovtReferenceNumber { get; set; }

		ZDateTime? OriginalTransactionCreationDate { get; set; }
	}

	public class AdditionalTransactionInfoTaxCoreEInvoice : IAdditionalTransactionInfoForTaxCoreEInvoice
	{
		public ZString OriginalTransactionGovtReferenceNumber { get; set; } = null;

		public ZDateTime? OriginalTransactionCreationDate { get; set; }
	}
}
