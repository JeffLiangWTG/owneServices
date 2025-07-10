using CargoWise.Types;
using Enterprise.MasterFiles.GUI.Organisation;

namespace Enterprise.CommissionManagement.Module
{
	public class CommissionManagementFilterControlGuiState
	{
		public CommissionManagementFilterControlGuiState(CommissionManagementFilterControl commissionManagementFilterControl)
		{
			this.commissionManagementFilterControl = commissionManagementFilterControl;
		}

		readonly CommissionManagementFilterControl commissionManagementFilterControl;

		public ZInt MainSplitterPosition
		{
			get { return OrganisationGuiState.LoadInteger(commissionManagementFilterControl, Schema.MainSplitterPosition); }
			set { OrganisationGuiState.SaveInteger(commissionManagementFilterControl, Schema.MainSplitterPosition, value); }
		}

		static class Schema
		{
			public const string MainSplitterPosition = "MainSplitterPosition";
		}
	}
}
