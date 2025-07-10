using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	class EDIWorkItemFormCustomisationSettingsProvider : WorkItemFormCustomisationSettingsProvider
	{
		protected override Enterprise.MasterFiles.Business.FormCustomisableElementCollection GetDisplayFields()
		{
			var result = base.GetDisplayFields();

			result.Add((NoResString)"Update Note Work Item", EDIControlNames.UpdateNoteWorkItem, false, (NoResString)ControlNames.StatePanel, ControlNames.DetailsTabName, TabPlacement.Placements.TopMiddle, 8);

			return result;
		}

		public static class EDIControlNames
		{
			public const string UpdateNoteWorkItem = "updateNoteWorkItemGuidFindBox";
		}
	}
}

