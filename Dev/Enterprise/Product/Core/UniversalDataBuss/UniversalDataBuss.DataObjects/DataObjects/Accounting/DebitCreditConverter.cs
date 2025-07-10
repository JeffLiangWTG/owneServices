using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	public class DebitCreditConverter : EnumConverter<DebitCredit>
	{
		protected override ZString[] GetCodes()
		{
			return new ZString[]
			{
				Constants.DebitCredit.Debit,
				Constants.DebitCredit.Credit
			};
		}

		protected override DebitCredit[] GetEnumValues()
		{
			return new DebitCredit[]
			{
				DebitCredit.Debit,
				DebitCredit.Credit
			};
		}
	}
}
