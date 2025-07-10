using CargoWise.Types;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public interface ISupportAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper
	{
		AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper AdditionaInfo { get; }
	}

	public class AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper
	{
		public virtual ZString BusinessName => string.Empty;

		public virtual ZString LocationName => string.Empty;

		public virtual ZString Address => string.Empty;

		public virtual ZString District => string.Empty;

		public virtual ZString EInvoicePaymentMethod => string.Empty;

		public virtual ZString OriginalTransactionReferenceNumber => string.Empty;
	}
}
