using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocJobPaymentBasis : DocBaseWrapper
	{
		DocJobPaymentBasis(JobPaymentBasis paymentBasis, BusinessObjectFactory factoryToWrap)
			: base(paymentBasis, factoryToWrap)
		{
		}

		public static DocJobPaymentBasis New(JobPaymentBasis paymentBasis, BusinessObjectFactory factoryToWrap)
		{
			if (paymentBasis == null)
			{
				return null;
			}

			return new DocJobPaymentBasis(paymentBasis, factoryToWrap);
		}

		public ZGuid Id => PaymentBasis.PK;
		public ZString Reference => PaymentBasis.PBS_ChargeableDescription;
		public ZString ChargeCode => PaymentBasis.ChargeCode;
		public ZString ChargeDescription => PaymentBasis.ChargeDescription;
		public ZString Quantity => PaymentBasis.Quantity;
		public ZString QuantityUnit => !Quantity.IsEmpty ? PaymentBasis.PBS_ChargeableUnit : ZString.Empty;
		public ZDecimal Rate => RoundAmount(PaymentBasis.RateValue);
		public ZString RateReference => PaymentBasis.RateReference;
		public ZString RateUnit => PaymentBasis.RateUnit;
		public ZString Adapter => PaymentBasis.PBS_AdapterID;
		public ZString Currency => PaymentBasis.PBS_RX_NKRateCurrency;
		public ZDecimal Amount => RoundAmount(PaymentBasis.Amount);

		JobPaymentBasis PaymentBasis => (JobPaymentBasis)WrappedObject;

		static ZDecimal RoundAmount(ZDecimal original)
		{
			return original.DecimalPlaces <= 2
				? original.Round(2)
				: original.Round(4);
		}
	}
}
