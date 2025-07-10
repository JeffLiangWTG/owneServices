using CargoWise.Types;
using Enterprise.MasterFiles.GUI.Organisation;

namespace Enterprise.CommissionManagement.Module
{
	public class CommissionLinesPreviewPaneGuiState
	{
		public CommissionLinesPreviewPaneGuiState(CommissionLinesPreviewPane commissionLinesPreviewPane)
		{
			this.commissionLinesPreviewPane = commissionLinesPreviewPane;
		}

		readonly CommissionLinesPreviewPane commissionLinesPreviewPane;

		public ZInt SplitterDistance
		{
			get { return OrganisationGuiState.LoadInteger(commissionLinesPreviewPane, Schema.SplitterDistance); }
			set { OrganisationGuiState.SaveInteger(commissionLinesPreviewPane, Schema.SplitterDistance, value); }
		}

		static class Schema
		{
			public const string SplitterDistance = "SplitterDistance";
		}
	}
}
