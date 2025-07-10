using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class WebSecuritySearchControl : ZSearchControl
	{
		#region Controls

		public ZGrid ContactSecurityGrid { get; } = new ZGrid();
		public ZButton ApplyProfileButton { get; } = new ZButton();
		ZTextLabel ContactSecurityGridCaption { get; } = new ZTextLabel();
		ZTextLabel ContactMultiSelectionCaption { get; } = new ZTextLabel();
		UpdatePanel ContactSecurityGridCaptionUpdatePanel { get; } = new UpdatePanel();
		UpdatePanel ContactSecurityGridUpdatePanel { get; } = new UpdatePanel();

		#endregion Controls

		#region Grid

		void ContactSecurityGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.SelectedItem)
			{
				var lastCell = e.Item.Cells.OfType<TableCell>().Last();
				var checkBox = lastCell.Controls[0] as CheckBox;
				if (checkBox != null)
				{
					if (checkBox.Checked)
					{
						e.Item.CssClass = WebSecurityCssConstants.WsGridSelected;
					}

					checkBox.Attributes["onclick"] = "javascript:WsHighlightRow(this);";
				}
			}
		}

		#endregion Grid

		#region Operations

		protected override void Find()
		{
			ResetSecuritySearchControl();
			base.Find();
		}

		public void ResetSecuritySearchControl()
		{
			Wrapper.SelectContact(null);
			ContactSecurityGrid.DataSource = null;
			ContactSecurityGrid.DataBind();
			ContactSecurityGridCaption.Text = "Security Item";
		}

		public void Rebind() => ContactSecurityGrid.Rebind();

		#endregion Operations

		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if (Env.CurrentUser == null)
			{
				Page.AppInstance.SetupSession(null, EventArgs.Empty, false);
			}

			FilterControl.Attributes["class"] += " WsFilterControl";
			ShowCollapsedMessage = false;
			ResultsGridDiv.Controls.Remove(SearchResultsDataGrid);

			ContactSecurityGrid.ID = "WsContactSecurityGrid";
			ContactSecurityGrid.ClientIDMode = ClientIDMode.Static;
			ContactSecurityGrid.Page = this.Page;
			if (ContactSecurityGrid.ColumnProvider == null || ContactSecurityGrid.ColumnProvider.IsEmpty)
			{
				ContactSecurityGrid.ColumnProvider = new WebSecurityGridColumnProviders.ContactGridColumnProvider();
			}
			ContactSecurityGrid.ShowExportToExcelButton = false;
			ContactSecurityGrid.ShowCustomizeColumnsButton = false;
			ContactSecurityGrid.ShowMenu = false;
			ContactSecurityGrid.AllowEdit = true;
			ContactSecurityGrid.AutoGenerateColumns = false;
			ContactSecurityGrid.DisableCollapsing = true;
			ContactSecurityGrid.AllowAdd = false;
			ContactSecurityGrid.AllowDelete = false;
			ContactSecurityGrid.DataKeyField = "PK";
			ContactSecurityGrid.HideButtonsToMenu = true;
			ContactSecurityGrid.ShouldShowControl = false;
			ContactSecurityGrid.CssClass = WebSecurityCssConstants.WsContactSecurityGrid;
			ContactSecurityGrid.ItemStyle.CssClass = WebSecurityCssConstants.WsGridItem;
			ContactSecurityGrid.HeaderStyle.CssClass = WebSecurityCssConstants.WsGridHeader;
			ContactSecurityGrid.BindTo = "ContactSecurityRights";
			ContactSecurityGrid.ItemDataBound += ContactSecurityGrid_ItemDataBound;

			ContactSecurityGridCaption.CssClass = CssConstants.SectionTitle;
			ContactSecurityGridCaption.Text = "Security Item";
			ContactSecurityGridCaption.ID = "ContactSecurityGridCaption";

			ApplyProfileButton.ID = "ApplyProfileButton";
			ApplyProfileButton.Text = "Update Security Profile";

			ContactSecurityGridCaptionUpdatePanel.ID = "ContactSecurityGridCaptionUpdatePanel";
			ContactSecurityGridCaptionUpdatePanel.ContentTemplateContainer.Controls.Add(ContactSecurityGridCaption);
			ContactSecurityGridUpdatePanel.ID = "ContactSecurityGridUpdatePanel";
			ContactSecurityGridUpdatePanel.ClientIDMode = ClientIDMode.Static;
			ContactSecurityGridUpdatePanel.ContentTemplateContainer.Controls.Add(ContactSecurityGrid);

			ContactMultiSelectionCaption.ID = "WsContactMultiSelectionCaption";
			ContactMultiSelectionCaption.ClientIDMode = ClientIDMode.Static;
			ContactMultiSelectionCaption.Style.Add("display", "none");

			var subTable = new Table();
			subTable.CellPadding = 0;
			subTable.CellSpacing = 0;
			subTable.BorderStyle = BorderStyle.None;
			subTable.CssClass = WebSecurityCssConstants.WsContactSubTabRight;
			var subTableRow = new TableRow();
			subTable.Rows.Add(subTableRow);

			var captionCell = new TableCell();
			subTableRow.Cells.Add(captionCell);
			captionCell.Controls.Add(ContactSecurityGridCaptionUpdatePanel);
			var profileButtonCell = new TableCell();
			profileButtonCell.HorizontalAlign = HorizontalAlign.Right;
			profileButtonCell.VerticalAlign = VerticalAlign.Top;
			subTableRow.Cells.Add(profileButtonCell);
			profileButtonCell.Controls.Add(ApplyProfileButton);

			var mainTable = new Table();
			mainTable.CssClass = WebSecurityCssConstants.WsContactMainTab;
			var mainTableRow = new TableRow();
			mainTable.Rows.Add(mainTableRow);

			var searchResultsCell = new TableCell();
			mainTableRow.Cells.Add(searchResultsCell);
			searchResultsCell.CssClass += WebSecurityCssConstants.WsVerticalTop;
			searchResultsCell.Controls.Add(SearchResultsDataGrid);
			SearchResultsDataGrid.Container.Attributes["class"] += (" " + WebSecurityCssConstants.WsContactDiv);

			var blankCell = new TableCell();
			mainTableRow.Cells.Add(blankCell);

			var contactSecurityCell = new TableCell();
			mainTableRow.Cells.Add(contactSecurityCell);
			contactSecurityCell.CssClass += WebSecurityCssConstants.WsContactSecurityCell;
			contactSecurityCell.Controls.Add(subTable);
			contactSecurityCell.Controls.Add(ContactSecurityGridUpdatePanel);
			contactSecurityCell.Controls.Add(ContactMultiSelectionCaption);
			ResultsGridDiv.Controls.Add(mainTable);
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			var pks = SearchResultsDataGrid.GetSelectedPKs();
			var selectedCount = pks.Length;

			if (selectedCount == 0)
			{
				ContactSecurityGrid.Style["display"] = "none";
				ContactMultiSelectionCaption.Style["display"] = "none";
			}
			else if (selectedCount == 1)
			{
				var contact = (OrgContact)((OrgContactCollection)SearchResultsDataGrid.DataSource).FindByPK(pks.First());
				Wrapper.SelectContact(contact);
				Rebind();

				ContactSecurityGrid.Style["display"] = "block";
				ContactMultiSelectionCaption.Style["display"] = "none";
			}
			else if (selectedCount > 1)
			{
				ContactSecurityGrid.Style["display"] = "none";
				ContactMultiSelectionCaption.Style["display"] = "block";
				ContactMultiSelectionCaption.Text = FormattableString.Invariant($"{selectedCount} contacts selected.");
			}
		}

		//do not use cache, it's slow
		protected override ZGuid[] LoadCachedPKs() => null;

		protected override void SaveCachedPKs(ZGuid[] array)
		{
		}

		#endregion Overrides

		WebSecurityDataSource Wrapper => (WebSecurityDataSource)FilterBusinessObject;
	}
}
