using CargoWise.Types;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public class ValueAndUnitSelfTotaller
	{
		public ValueAndUnitSelfTotaller(ZDecimal value, ZString unitCode)
		{
			Value = value;
			UnitCode = unitCode;
		}
		public ZDecimal Value;
		public ZString UnitCode;
		public ZBool EncounteredInvalidCode
		{
			get { return encounteredInvalidCode; }
			set
			{
				encounteredInvalidCode = value;
				if (encounteredInvalidCode)
				{
					Value = ZDecimal.Zero;
					UnitCode = ZString.Empty;
				}
			}
		}
		ZBool encounteredInvalidCode;
		public ZString ToString(ZString decimalPlaces)
		{
			int decimals = 0;
			return !EncounteredInvalidCode
				? int.TryParse(decimalPlaces, out decimals) ? Value.ToString(decimals) + " " + UnitCode : Value.ToString() + " " + UnitCode
				: string.Empty;
		}
	}
}
