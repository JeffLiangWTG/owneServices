using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class DeferredPaymentProvider : IDeferredPayment
	{
		public DeferredPaymentProvider(AsycudaManifestHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}

		readonly AsycudaManifestHeader header;

		public string CcQualifier => null;

		public string DeferredPayment => header.AMA_PaymentAccountNumber;
	}
}
