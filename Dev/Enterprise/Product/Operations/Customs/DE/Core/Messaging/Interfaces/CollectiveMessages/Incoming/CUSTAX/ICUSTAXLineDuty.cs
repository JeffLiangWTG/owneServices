using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSTAXLineDuty : IInboundProvider
	{
		decimal ChargeAmount { get; }

		string ChargeType { get; }

		decimal BaseValue { get; }

		string MethodOfCalculation { get; }

		string MethodOfPayment { get; }

		IReadOnlyCollection<ICUSTAXLineDutyRate> DutyRates { get; }
	}
}
