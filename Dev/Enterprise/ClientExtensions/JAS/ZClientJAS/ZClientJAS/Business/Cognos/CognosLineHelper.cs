using CargoWise.Common;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosLineHelper
	{
		public const int FieldCount = 10;

		public string ConvertToString(ICognosLine line)
		{
			var result = new OCsvLine(new string[FieldCount], new bool[FieldCount]);
			result.FieldValues[0] = line.AccountName;
			result.FieldValues[1] = line.AccountCode;
			result.FieldValues[2] = line.CounterCompany;
			result.FieldValues[3] = line.Mode;
			result.FieldValues[4] = line.Branch;
			result.FieldValues[5] = line.Business;
			result.FieldValues[6] = line.Amount.ToString("#0.00+;#0.00-;0");
			result.FieldValues[7] = line.TransactionCurrency;
			result.FieldValues[8] = (line.TransactionAmount == 0) ? "" : line.TransactionAmount.ToString("#0.00+;#0.00-;0");
			result.FieldValues[9] = line.Geographical;
			return result.ToString();
		}
	}
}
