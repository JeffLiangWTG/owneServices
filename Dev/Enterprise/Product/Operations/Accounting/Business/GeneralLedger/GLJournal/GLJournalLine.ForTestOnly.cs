#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals
{
	public partial class GLJournalLine
	{
		public DebitCreditDataEntry OSAmount_ForTestOnly
		{
			get { return OSAmount; }
		}

		public DebitCreditDataEntry LocalAmount_ForTestOnly
		{
			get { return LocalAmount; }
		}

		public static ZString CR_ForTestOnly => CR;

		public static ZString DR_ForTestOnly => DR;

		public ZDecimal RoundAmountToCurrencyDecimals_ForTestOnly(ZDecimal amount, ZDecimal exchangeRate)
		{
			return RoundAmountToCurrencyDecimals(amount, exchangeRate);
		}

		public ZDecimal RoundAmountToLocalDecimals_ForTestOnly(ZDecimal value)
		{
			return RoundAmountToLocalDecimals(value);
		}

		public ZDecimal RoundAmountToLocalDecimals_ForTestOnly(ZDecimal amount, ZDecimal exchangeRate)
		{
			return RoundAmountToLocalDecimals(amount, exchangeRate);
		}
	}
}

#endif
