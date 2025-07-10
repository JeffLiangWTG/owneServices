using CargoWise.Customs.BE.MessageContracts.Interfaces;

namespace Enterprise.Customs.BE.Business;

public class DeferredPaymentProvider : IDeferredPayment
{
	public DeferredPaymentProvider(int sequenceNumber, string deferredPayment)
	{
		SequenceNumber = sequenceNumber;
		DeferredPayment = deferredPayment;
	}

	public int SequenceNumber { get; }

	public string DeferredPayment { get; }

	public string CCQualifier => Constants.Qualifiers.NietVanToepassing;
}
