using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public class SpecificRateFromSnapshotProvider : IImportSpecificRate
	{
		public SpecificRateFromSnapshotProvider(DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate specificRate)
		{
			this.specificRate = Argument.NotNull(specificRate, nameof(specificRate));
		}

		readonly DEMonthlyClosingEntryLineSnapshotAssessmentSpecificRate specificRate;

		public string Type => specificRate.Type;

		public decimal Value => specificRate.Value;
	}
}
