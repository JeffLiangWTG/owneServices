using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public class IT5DecimalsWeightFormatter
{
	public IT5DecimalsWeightFormatter(ZDecimal value)
	{
		decimalValue = value;
	}

	readonly ZDecimal decimalValue;

	public ZString GetFormattedValue(int decimalDigits = 5)
	{
		return decimalValue == ZDecimal.Zero
			? ZString.Empty
			: ((ZString)decimalValue.Round(decimalDigits).ToString("G29"));
	}
}
