using System;
using System.Linq;
using System.Web;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class NotificationRolesSearchControl : ZSearchControl
	{
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			FilterControl.Attributes["class"] += " NrFilterControl";
			SearchResultsDataGrid.Container.Attributes["class"] += (" " + NotificationRolesCssConstants.NrContactDiv);

			BulkUpdateButton.ID = "BulkUpdateButton";
			BulkUpdateButton.Text = "Bulk Apply Roles";
			BulkUpdateButton.Style.Add("float", "right");
			var resultGrid = ResultsGridDiv.Controls.IndexOf(SearchResultsDataGrid);
			ResultsGridDiv.Controls.AddAt(resultGrid, BulkUpdateButton);
		}

		#region Cache //do not use cache, it's slow

		protected override ZGuid[] LoadCachedPKs() => null;

		protected override void SaveCachedPKs(ZGuid[] array)
		{
		}

		#endregion Cache //do not use cache, it's slow

		public ZButton BulkUpdateButton { get; } = new ZButton();

		protected override void OnLoad(EventArgs e)
		{
			//The search control is not working properly with editable columns.
			//We have to unset the flag IsResultsRelatedOperation conditionally to avoid double data binding (base.BindResultsGrid()).
			var shouldSetIsResultsRelatedOperation = false;
			var @params = HttpContext.Current.Request.Params;
			var lastFocusControlID = @params[@params.AllKeys.FirstOrDefault(x => x.EndsWith(LastFocusIDKey, StringComparison.OrdinalIgnoreCase))];
			if (lastFocusControlID != null)
			{
				if (SearchControlButtonIDs.Any(x => lastFocusControlID.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
				{
					IsResultsRelatedOperation = false;
					shouldSetIsResultsRelatedOperation = true;
				}
			}

			base.OnLoad(e);

			if (shouldSetIsResultsRelatedOperation)
			{
				IsResultsRelatedOperation = true;
			}
		}

		const string LastFocusIDKey = "__LASTFOCUSID";
		readonly string[] SearchControlButtonIDs = { "_FindButton", "_ClearButton", "_ResetLayoutButton" };
	}
}
