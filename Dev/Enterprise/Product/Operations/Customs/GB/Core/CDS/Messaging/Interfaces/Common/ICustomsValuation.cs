using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface ICustomsValuation
	{
		IEnumerable<IChargeDeduction> ChargeDeductions { get; }
		ZString MethodCode { get; }
	}

	class CustomsValuationWrapper : ICustomsValuation
	{
		CustomsValuationWrapper(IEnumerable<IChargeDeduction> chargeDeductions, ZString methodCode)
		{
			this.chargeDeductions = chargeDeductions;
			this.methodCode = methodCode;
		}

		public static CustomsValuationWrapper New(IEnumerable<IChargeDeduction> chargeDeductions, ZString methodCode)
		{
			return new CustomsValuationWrapper(chargeDeductions, methodCode);
		}

		IEnumerable<IChargeDeduction> ICustomsValuation.ChargeDeductions => chargeDeductions;

		ZString ICustomsValuation.MethodCode => methodCode;

		readonly IEnumerable<IChargeDeduction> chargeDeductions;
		readonly ZString methodCode;
	}
}
