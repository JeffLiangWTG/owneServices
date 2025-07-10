using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public interface ITaxDetailWrapper
	{
		ZString typtax { get; }
		ZString codtax { get; }
		ZDecimal asstax { get; }
		ZDecimal quotax { get; }
		ZLong montanttax { get; }
		ZString statutLiquidation { get; }

		ZString SuppUnitsMethodOfCalculation { get; }

		ZBool LiquidationItemHasSuppUnits { get; }
	}
}
