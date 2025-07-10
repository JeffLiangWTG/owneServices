using CargoWise.Types;

namespace Enterprise.Customs.IT.Registry;

public static class AccountHelper
{
	public static (ZString DeclarantTaxNumber, ZInt WorkstationSequentialNumber) SplitCodeBySeparator(ZString code)
	{
		var indexOfSeparator = code.IndexOf(AccountNumberSeparator);

		if (indexOfSeparator >= 0)
		{
			var declarantTaxNumber = code.Left(indexOfSeparator);
			var workstationNumber = ZInt.ParseSafe(code.SubstringSafe(indexOfSeparator + 1), ZInt.Zero);

			return (declarantTaxNumber, workstationNumber);
		}

		return (code, ZInt.Zero);
	}

	const char AccountNumberSeparator = '-';
}
