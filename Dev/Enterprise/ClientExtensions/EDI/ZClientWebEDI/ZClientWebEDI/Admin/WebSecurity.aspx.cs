using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class WebSecurity : BasePage
	{
		#region Overrides

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			Response.Cache.SetCacheability(HttpCacheability.NoCache);
			Response.Cache.SetNoStore();
		}

		protected override void OnPreInit(EventArgs e)
		{
			base.OnPreInit(e);
			SetupSearchControl(); // dynamic controls must be created by OnPreInit()
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (IsInLiteViewMode)
			{
				Breadcrumb.Visible = false;
			}
		}

		protected override void OnPreBind()
		{
			base.OnPreBind();
			SetupZGrids();
		}

		protected override void OnLoadComplete(EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(Session[PageSwitchingSessionKey] as string))
			{
				Session[PageSwitchingSessionKey] = "";
				SwitchPageCore();
			}
			base.OnLoadComplete(e);
		}

		#endregion Overrides

		#region DataSource

		//the SearchControl accesses Page.Factory before GetNewDataSource()
		//in order to use only one signle factory instance, we have to save the factory to the session.
		protected override BusinessObjectFactory GetNewFactory()
		{
			if (!IsPostBack || Session[FactorySessionKey] == null) // !IsPostBack means the very first page access
			{
				var factory = base.GetNewFactory();
				Session[FactorySessionKey] = factory;
				return factory;
			}
			else
			{
				return (BusinessObjectFactory)Session[FactorySessionKey];
			}
		}

		const string FactorySessionKey = "5bc10b3e-848c-4c3c-95cf-7505ed5d32b9";
		const string PageSwitchingSessionKey = "b290aa77-d105-4815-9ca9-c1fbadb4fa77";
		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		protected override BusinessObject GetNewDataSource() => new WebSecurityDataSource(Factory, SiteUser.LoggedInOrganisation.PK);

		WebSecurityDataSource Wrapper => (WebSecurityDataSource)DataSource;

		#endregion DataSource

		#region DataGrids

		void SetupZGrids()
		{
			SecurityGrid.ColumnProvider = new WebSecurityGridColumnProviders.SecurityGridColumnProvider();
			SecurityGrid.ShowExportToExcelButton = false;
			SecurityGrid.ShowCustomizeColumnsButton = false;
			SecurityGrid.ShowMenu = false;

			BulkUpdateSecurityGrid.ColumnProvider = new WebSecurityGridColumnProviders.BulkUpdateSecurityGridColumnProvider();
			BulkUpdateSecurityGrid.ShowExportToExcelButton = false;
			BulkUpdateSecurityGrid.ShowCustomizeColumnsButton = false;
			BulkUpdateSecurityGrid.ShowMenu = false;
		}

		protected override string GetControlKeyIdentifier(Control control)
		{
			if (control == SecurityGrid)
			{
				return "2cf95d87-471a-408a-ad93-e7c929889400";
			}
			else if (control == searchControl?.SearchResultsDataGrid)
			{
				return "7216b957-2f3a-4ed7-89ff-ed05d6a3b71c";
			}
			else if (control == searchControl?.ContactSecurityGrid)
			{
				return "8aa5d73d-d1f6-4c5c-b1bb-4447777964a9";
			}
			else if (control == BulkUpdateSecurityGrid)
			{
				return "a4cc4a29-ee35-44ba-802b-081e52751ba0";
			}

			throw new ArgumentOutOfRangeException(control.ToString());
		}

		#region Security Grid

		protected void SecurityGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
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

		#endregion Security Grid

		#region BulkUpdateSecurityGrid

		protected void BulkUpdateSecurityGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.SelectedItem)
			{
				var cells = e.Item.Cells.OfType<TableCell>().Reverse().Take(2);
				var checkBox = cells.Last().Controls[0] as CheckBox;
				if (checkBox != null)
				{
					if (checkBox.Checked)
					{
						e.Item.CssClass = WebSecurityCssConstants.WsGridSelected;
					}
					checkBox.Attributes["onclick"] = "javascript:WsClickBulkChkBox(this);";
				}
			}
		}

		#endregion BulkUpdateSecurityGrid

		#endregion DataGrids

		#region Search Control setup

		public ISearchControl SearchControl { get { return searchControl; } }

		WebSecuritySearchControl searchControl;

		void SetupSearchControl()
		{
			searchControl = new WebSecuritySearchControl();
			searchControl.Page = this;
			searchControl.ModuleID = WebModuleIDs.CargoWiseEDIWebSecurityContacts;
			searchControl.SearchResultsDataGrid.AllowMultiLineSelection = true;
			searchControl.SearchResultsDataGrid.DisableCollapsing = true;
			searchControl.ShowCollapsedMessage = false;
			searchControl.SearchResultsDataGrid.ShowCollapsedMessage = false;
			searchControl.IsNewButtonVisible = false;
			searchControl.IsViewButtonVisible = false;
			searchControl.IsExportToExcelButtonVisible = false;
			searchControl.ApplyProfileButton.Click += (_, x_) =>
			{
				BulkUpdateDiv.CssClass = WebSecurityCssConstants.WsModalOn;
				Wrapper.InitBulkUpdate(Factory.GetDatabaseCount(typeof(OrgContact), searchControl.FilterBusinessObject.Filter), searchControl.SearchResultsDataGrid.GetSelectedPKs().Length);
				BulkUpdateSecurityGrid.Rebind();
				BulkUpdateProfile.DataBind();
				UpdateModeRadioButton.Controls.Clear();
				UpdateModeRadioButton.Bind(Wrapper);
			};
			searchControl.IsCustomiseColumnsButtonVisible = false;
			searchControl.PageSize = 500;
			searchControl.SearchResultsDataGrid.ShowMenu = true;
			searchControl.SearchResultsDataGrid.ItemStyle.Wrap = true;
			searchControl.SearchResultsDataGrid.ShowExportToExcelButton = false;
			searchControl.SearchResultsDataGrid.ShowCustomizeColumnsButton = false;
			searchControl.SearchResultsDataGrid.ItemDataBound += SearchResultsDataGrid_ItemDataBound;
			searchControl.SearchResultsDataGrid.CssClass = WebSecurityCssConstants.WsContactGrid;
			searchControl.SearchResultsDataGrid.HeaderStyle.CssClass = WebSecurityCssConstants.WsGridHeader;
			searchControl.SearchResultsDataGrid.ItemStyle.CssClass = WebSecurityCssConstants.WsGridItem;
			searchControl.SearchResultsDataGrid.AlternatingItemStyle.CssClass = WebSecurityCssConstants.WsGridItem;
			searchControl.SearchResultsDataGrid.SelectedItemStyle.CssClass = WebSecurityCssConstants.WsContactGridItemChecked;
			searchControl.SearchResultsDataGrid.Caption = "Contact";
			searchControl.IsResultsRelatedOperation = true;
			SearchControlHolder.Controls.AddAt(0, searchControl);
		}

		void SearchResultsDataGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.SelectedItem)
			{
				e.Item.Attributes.Add("onmouseover", "this.style.cursor='pointer'");
				var chkBox = e.Item.Cells[0].Controls[0] as CheckBox;

				if (chkBox != null)
				{
					chkBox.Attributes["onclick"] += "WsClickContactChkBox(this);";

					foreach (var cell in e.Item.Cells.OfType<TableCell>().Skip(1))
					{
						cell.Attributes["onclick"] = FormattableString.Invariant($"javascript:WsContactCellClick(this);");
					}
				}
			}
			else if (e.Item.ItemType == ListItemType.Header)
			{
				var selectAll = e.Item.Cells[0].Controls[0] as CheckBox;
				if (selectAll != null)
				{
					selectAll.Attributes["onclick"] = "javascript:WsSelectAllContacts(this);"; // SelectAllCheckboxes(SelectionCheckBox.js) selects ALL checkboxes on the same PAGE.
				}
			}
		}

		#endregion Search Control setup

		#region Buttons

		protected void BulkUpdateButton_Click(object sender, EventArgs e)
		{
			BulkUpdateDiv.CssClass = WebSecurityCssConstants.WsModalOn;
		}

		protected void ResetButton_Click(object sender, EventArgs e) => Response.Redirect(Request.RawUrl);

		protected void SaveButton_Click(object sender, EventArgs e)
		{
			BulkUpdateSecurityGrid.ReadOnly = true;

			try
			{
				Wrapper.Factory.Save();
			}
			catch (ZSaveConcurrencyException)
			{
				ErrorConfirmationDiv.CssClass = WebSecurityCssConstants.WsModalOn;
				return;
			}

			Wrapper.RefreshAfterSave();
			SecurityGrid.Rebind();
			BulkUpdateSecurityGrid.ReadOnly = false;
		}

		protected void BulkUpdateCancel_Click(object sender, EventArgs e)
		{
			BulkUpdateDiv.CssClass = WebSecurityCssConstants.WsModalOff;
			Wrapper.CancelBulkUpdate();
			BulkUpdateSecurityGrid.Rebind();
			BulkUpdateProfile.DataBind();
		}

		protected void BulkUpdateOK_Click(object sender, EventArgs e)
		{
			BulkUpdateDiv.CssClass = WebSecurityCssConstants.WsModalOff;
			if (Wrapper.IsUpdateSelectedContacts)
			{
				Wrapper.BulkUpdateSelected((OrgContactCollection)searchControl.SearchResultsDataGrid.DataSource, searchControl.SearchResultsDataGrid.GetSelectedPKs());
			}
			else
			{
				Wrapper.BulkUpdateAll(searchControl.FilterBusinessObject.Filter);
			}
			SaveButton_Click(null, null);
			searchControl.Rebind();
		}

		protected void SecurityProfileFilter_SelectedIndexChanged(object sender, EventArgs e) => SecurityGrid.Rebind();

		protected void BulkUpdateProfile_SelectedIndexChanged(object sender, EventArgs e) => BulkUpdateSecurityGrid.Rebind();

		#region Tab Pages

		protected void SwitchPage_Click(object sender, EventArgs e) => SwitchPage();

		void SwitchToDefaultsPage()
		{
			TabSecurityDefaults.Visible = true;
			TabContacts.Visible = false;
			searchControl.ResetSecuritySearchControl();
			ButtonContacts.Enabled = true;
			ButtonSecurityDefaults.Enabled = false;
		}

		void SwitchToContactsPage()
		{
			TabSecurityDefaults.Visible = false;
			TabContacts.Visible = true;
			searchControl.ResetSecuritySearchControl();
			ButtonContacts.Enabled = false;
			ButtonSecurityDefaults.Enabled = true;
		}

		void SwitchPage()
		{
			if (Wrapper.HasChanges)
			{
				ConfirmationDiv.CssClass = WebSecurityCssConstants.WsModalOn;
			}
			else
			{
				SwitchPageCore();
			}
		}

		void SwitchPageCore()
		{
			if (TabSecurityDefaults.Visible)
			{
				SwitchToContactsPage();
			}
			else
			{
				SwitchToDefaultsPage();
			}
		}

		protected void ConfirmationYes_Click(object sender, EventArgs e)
		{
			SaveButton_Click(null, null);
			SwitchPageCore();
			ConfirmationDiv.CssClass = WebSecurityCssConstants.WsModalOff;
		}

		protected void ConfirmationCancel_Click(object sender, EventArgs e)
		{
			ConfirmationDiv.CssClass = WebSecurityCssConstants.WsModalOff;
		}

		protected void ConfirmationNo_Click(object sender, EventArgs e)
		{
			Session[PageSwitchingSessionKey] = TabContacts.Visible ? "1" : "";
			ResetButton_Click(null, null);
		}

		#endregion Tab Pages

		#endregion Buttons
	}
}
