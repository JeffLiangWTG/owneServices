using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public interface IMonthlyClosingEntryLineSnapshot : IMonthlyClosingDecLine
	{
		string CessionManagementFlag { get; }

		string TobaccoRevenueStampNumber { get; }

		string PreferentialCountry { get; }

		string ForeignTradeFlag { get; }

		ILinePreferentialTreatment PreferentialTreatment { get; }

		IAmount InwardMovementAmount { get; }
	}
}
