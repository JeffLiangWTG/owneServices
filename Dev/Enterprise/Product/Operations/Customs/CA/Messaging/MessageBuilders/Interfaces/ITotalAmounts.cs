using CargoWise.Types;

namespace Enterprise.Customs.CA.Messaging
{
	public interface ITotalAmounts
	{
		ZDecimal Deposit { get; }
		ZDecimal TotalCustomsDuty { get; }
		ZDecimal TotalSIMAAssessment { get; }
		ZDecimal TotalExciseTax { get; }
		ZDecimal TotalGST { get; }
		ZDecimal TotalAllDutyAndTaxes { get; }
	}
}
