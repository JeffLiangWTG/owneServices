using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDIProjectFormCustomisationSettingsProvider : ProcessManagement.Business.ProjectFormCustomisationSettingsProvider
	{
		protected override FormCustomisableElementCollection GetDisplayFields()
		{
			FormCustomisableElementCollection result = base.GetDisplayFields();
			var statePanel = (NoResString)ControlNames.StatePanel;
			result.Add((NoResString)"Follow Up Date/Training Start", EDIControlNames.FollowUpAndTrainingStartDates, false, statePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 8);
			result.Add((NoResString)"Planned Install/Install Complete", EDIControlNames.InstallDates, false, statePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 9);
			result.Add((NoResString)"Planned Go-Live/Go-Live Complete", EDIControlNames.GoLiveDates, false, statePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 10);
			result.Add((NoResString)"Agreed Go-Live", EDIControlNames.AgreedGoLiveDate, false, statePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopLeft, 11);
			return result;
		}

		public static class EDIControlNames
		{
			public const string FollowUpAndTrainingStartDates = "FollowUpDateEdit";
			public const string InstallDates = "PlannedInstallDateEdit";
			public const string GoLiveDates = "PlannedGoLiveDateEdit";
			public const string AgreedGoLiveDate = "AgreedGoLiveDateEdit";
		}
	}
}

