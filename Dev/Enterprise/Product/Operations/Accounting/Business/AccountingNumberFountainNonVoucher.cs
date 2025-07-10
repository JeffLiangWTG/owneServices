using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business
{
	public class AccountingNumberFountainNonVoucher : AccountingNumberFountainWrapper
	{
		public AccountingNumberFountainNonVoucher(AccountingNumberFountainPooler numberFountainPooler)
			: base(numberFountainPooler, NumberFountainType.None)
		{
		}

		// Do not expose the other GetNext(ZDateTime) 
		protected override string GetNext(IDbConnected factory, ZDateTime postDate)
		{
			return numberFountainPooler.GetTodaysPeriodFountain().GetNextFormatted(factory);
		}

		public string GetNext(IDbConnected factory)
		{
			return numberFountainPooler.GetTodaysPeriodFountain().GetNextFormatted(factory);
		}
	}
}