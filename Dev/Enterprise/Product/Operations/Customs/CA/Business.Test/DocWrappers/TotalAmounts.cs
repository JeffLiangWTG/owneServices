using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class TotalAmounts : ITotalAmounts
	{
		public ZDecimal Deposit { get; set; }
		public ZDecimal TotalCustomsDuty { get; set; }
		public ZDecimal TotalSIMAAssessment { get; set; }
		public ZDecimal TotalExciseTax { get; set; }
		public ZDecimal TotalGST { get; set; }

		public ZDecimal TotalAllDutyAndTaxes
		{
			get { return TotalCustomsDuty + TotalSIMAAssessment + TotalExciseTax + TotalGST + Deposit; }
		}
	}
}
