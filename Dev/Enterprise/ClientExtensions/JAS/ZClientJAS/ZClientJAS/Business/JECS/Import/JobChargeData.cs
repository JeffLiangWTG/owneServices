
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public interface IJobChargeData
	{
		ZString ChargeCode { get; }
		ZString ChargeDescription { get; }
		ZString Currency { get; }
		ZDecimal ChargeAmount { get; }
		bool IsCollect { get; }
	}

	public struct JobChargeData : IJobChargeData
	{
		public JobChargeData(ZString chargeCode, ZString chargeDescription, ZString currency, ZDecimal chargeAmount)
		{
			fChargeCode = chargeCode;
			fChargeDescription = chargeDescription;
			fCurrency = currency;
			fChargeAmount = chargeAmount;
		}

		public ZString ChargeCode
		{
			get { return fChargeCode; }
		}

		public ZString ChargeDescription
		{
			get { return fChargeDescription; }
		}

		public ZString Currency
		{
			get { return fCurrency; }
		}

		public ZDecimal ChargeAmount
		{
			get { return fChargeAmount; }
		}

		public bool IsCollect
		{
			get { return true; }
		}

		readonly ZString fChargeCode;
		readonly ZString fChargeDescription;
		readonly ZString fCurrency;
		readonly ZDecimal fChargeAmount;
	}
}
