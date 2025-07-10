using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSTAXLineDutyProvider : ICUSTAXLineDuty
	{
		public CUSTAXLineDutyProvider(GCTAXMBodyGoodsItemCustomsDutiesCustomsDuty duty, decimal taxValue, decimal customsValue)
		{
			this.taxValue = taxValue;
			this.customsValue = customsValue;
			this.duty = CargoWise.Common.Argument.NotNull(duty, nameof(duty));
		}

		readonly decimal taxValue;
		readonly decimal customsValue;
		readonly GCTAXMBodyGoodsItemCustomsDutiesCustomsDuty duty;

		public decimal ChargeAmount => duty.PayableAmountByTypeSpecified ? duty.PayableAmountByType : decimal.Zero;

		public string ChargeType => duty.TypeSpecified ? duty.Type.MapXmlEnumToString() : string.Empty;

		public decimal BaseValue => ChargeType.LeftEmptyIfNull(3) == VatChargeType ? taxValue : customsValue;

		public string MethodOfCalculation => duty.Group;

		public string MethodOfPayment => duty.AdditionalInformation;

		public IReadOnlyCollection<ICUSTAXLineDutyRate> DutyRates => dutyRates ??= duty.CustomsDutyRate?.Select(x => new CUSTAXLineDutyRateProvider(x)).ToArray() ?? Array.Empty<ICUSTAXLineDutyRate>();
		IReadOnlyCollection<ICUSTAXLineDutyRate> dutyRates;

		const string VatChargeType = "B00";
	}
}
