using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class IncidentDetails : BasePage
	{
		#region Component Designer generated code

		protected ZArchitecture.Web.GUI.WebControls.ZTextLabel OrderNotFoundLabel;
		protected System.Web.UI.HtmlControls.HtmlGenericControl P1;
		protected ZArchitecture.Web.GUI.WebControls.ZTextLabel Zdatetimelabel1;
		protected ZArchitecture.Web.GUI.WebControls.ZTextLabel NotesLabel;
		protected ZArchitecture.Web.GUI.WebControls.ZNotesControl notes;
		protected System.Web.UI.HtmlControls.HtmlGenericControl Contents;

		#endregion

		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			return Factory.Load<SupportIncident>(GetGuidFromParameter("Ref"));
		}

		protected SupportIncident CurrentIncident
		{
			get { return DataSource as SupportIncident; }
		}

		#endregion

		#region GUI Setup

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (CurrentIncident != null)
			{
				ResolutionComment.Visible = CurrentIncident.ShouldShowResolutionComment;
				ResolutionCommentLabel.Visible = CurrentIncident.ShouldShowResolutionComment;
			}
			else
			{
				IncidentContents.Visible = false;
				NotFoundError.Visible = true;
				IncidentNotFoundLabel.Text = "Incident was not found in the database or you don't have rights to view it.";
			}

			AdjustControlsForLiteViewMode();
		}

		void AdjustControlsForLiteViewMode()
		{
			if (IsInLiteViewMode)
			{
				Breadcrumb.Visible = false;
			}
		}

		#endregion
	}
}
