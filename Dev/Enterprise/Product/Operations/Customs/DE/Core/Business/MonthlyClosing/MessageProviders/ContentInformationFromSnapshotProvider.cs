using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.Import;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public sealed class ContentInformationFromSnapshotProvider : IContentInformation
	{
		public ContentInformationFromSnapshotProvider(DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation contentInformation)
		{
			this.contentInformation = Argument.NotNull(contentInformation, nameof(contentInformation));
		}

		readonly DEMonthlyClosingEntryLineSnapshotAssessmentContentInformation contentInformation;

		public string ContentType => contentInformation.Type;

		public decimal DegreePercentage => contentInformation.DegreePercentage;
	}
}
