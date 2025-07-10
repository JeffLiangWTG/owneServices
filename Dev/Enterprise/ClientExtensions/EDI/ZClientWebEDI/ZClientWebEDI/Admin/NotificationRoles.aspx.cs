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
	public partial class NotificationRoles : BasePage
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

		const string FactorySessionKey = "5b43ca62-fb26-4172-a41a-91cfdcaa7bc2";

		protected override bool IsPersistDataSourceBetweenPostbacks => true;

		protected override BusinessObject GetNewDataSource() => new NotificationRolesDataSource(Factory, SiteUser.LoggedInOrganisation.PK);

		NotificationRolesDataSource Wrapper => (NotificationRolesDataSource)DataSource;

		#endregion DataSource

		#region DataGrids

		void SetupZGrids()
		{
			BulkUpdateGrid.ColumnProvider = new BulkUpdateAdminGroupGridColumnProvider();
			BulkUpdateGrid.ShowExportToExcelButton = false;
			BulkUpdateGrid.ShowCustomizeColumnsButton = false;
			BulkUpdateGrid.ShowMenu = false;
			BulkUpdateGrid.ShowFooter = false;
		}

		protected override string GetControlKeyIdentifier(Control control)
		{
			if (control == searchControl?.SearchResultsDataGrid)
			{
				return "2445cfd9-3d64-4e36-ba9a-3fbd62f8f02f";
			}
			else if (control == BulkUpdateGrid)
			{
				return "3cda8f41-093d-4f29-bfbd-a0671dccdacd";
			}

			throw new ArgumentOutOfRangeException(control.ToString());
		}

		#endregion DataGrids

		#region Search Control setup

		public ISearchControl SearchControl { get { return searchControl; } }

		NotificationRolesSearchControl searchControl;

		void SetupSearchControl()
		{
			searchControl = new NotificationRolesSearchControl();
			searchControl.Page = this;
			searchControl.ModuleID = WebModuleIDs.CargoWiseEDINotificationRolesContacts;
			searchControl.SearchResultsDataGrid.AllowMultiLineSelection = true;
			searchControl.SearchResultsDataGrid.DisableCollapsing = true;
			searchControl.ShowCollapsedMessage = false;
			searchControl.SearchResultsDataGrid.ShowCollapsedMessage = false;
			searchControl.IsNewButtonVisible = false;
			searchControl.IsViewButtonVisible = false;
			searchControl.IsExportToExcelButtonVisible = false;
			searchControl.BulkUpdateButton.Click += (_, x_) =>
			{
				BulkUpdateDiv.CssClass = NotificationRolesCssConstants.NrModalOn;
				Wrapper.InitBulkUpdate(Factory.GetDatabaseCount(typeof(OrgContact), searchControl.FilterBusinessObject.Filter), searchControl.SearchResultsDataGrid.GetSelectedPKs().Length);
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
			searchControl.SearchResultsDataGrid.CssClass = NotificationRolesCssConstants.NrContactGrid;
			searchControl.SearchResultsDataGrid.HeaderStyle.CssClass = NotificationRolesCssConstants.NrGridHeader;
			searchControl.SearchResultsDataGrid.ItemStyle.CssClass = NotificationRolesCssConstants.NrGridItem;
			searchControl.SearchResultsDataGrid.AlternatingItemStyle.CssClass = NotificationRolesCssConstants.NrGridItem;
			searchControl.SearchResultsDataGrid.SelectedItemStyle.CssClass = NotificationRolesCssConstants.NrContactGridItemChecked;
			searchControl.SearchResultsDataGrid.Caption = "Contact";
			searchControl.IsResultsRelatedOperation = true;
			searchControl.ShowCollapsedMessage = false;
			searchControl.SearchResultsDataGrid.AllowEdit = true;
			searchControl.SearchResultsDataGrid.ReadOnly = false;
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
					foreach (var cell in e.Item.Cells.OfType<TableCell>().Skip(1))
					{
						cell.Attributes["onclick"] = FormattableString.Invariant($"javascript:NrContactCellClick(this);");
					}

					foreach (var cell in e.Item.Cells.OfType<TableCell>().Skip(5))
					{
						if (cell.Controls[0] is CheckBox roleChkBox)
						{
							roleChkBox.Attributes["onclick"] = "event.stopPropagation();";
						}
					}
				}
			}
			else if (e.Item.ItemType == ListItemType.Header)
			{
				var selectAll = e.Item.Cells[0].Controls[0] as CheckBox;
				if (selectAll != null)
				{
					selectAll.Attributes["onclick"] = "javascript:NrSelectAllContacts(this);"; // SelectAllCheckboxes(SelectionCheckBox.js) selects ALL checkboxes on the same PAGE.
				}
			}

			AddToolTips(e);
		}

		void AddToolTips(DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.SelectedItem)
			{
				foreach (var item in e.Item.Cells.OfType<TableCell>().Skip(5).Select((x, i) => new { WebControl = x.Controls[0] as WebControl, Index = i }))
				{
					item.WebControl.ToolTip = ContactsModuleColumnProvider.NotificationGroupList[item.Index].Description;
				}
			}
			if (e.Item.ItemType == ListItemType.Header)
			{
				foreach (var item in e.Item.Cells.OfType<TableCell>().Skip(5).Select((cell, index) => new { Cell = cell, Index = index }))
				{
					item.Cell.ToolTip = ContactsModuleColumnProvider.NotificationGroupList[item.Index].Description;
				}
			}
		}

		readonly NotificationRolesContactsModuleColumnProvider ContactsModuleColumnProvider = new NotificationRolesContactsModuleColumnProvider();

		#endregion Search Control setup

		#region Buttons

		protected void ResetButton_Click(object sender, EventArgs e) => Response.Redirect(Request.RawUrl);

		protected void SaveButton_Click(object sender, EventArgs e) => SaveDataSourceFactory();

		#endregion Buttons

		#region Bulk Update

		protected void BulkUpdateGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.SelectedItem)
			{
				var cells = e.Item.Cells.OfType<TableCell>().Reverse().Take(2);
				var checkBox = cells.Last().Controls[0] as CheckBox;
				if (checkBox != null)
				{
					if (checkBox.Checked)
					{
						e.Item.CssClass = NotificationRolesCssConstants.NrGridSelected;
					}
					checkBox.Attributes["onclick"] = "javascript:NrClickBulkChkBox(this);";
				}
			}
		}

		protected void BulkUpdateButton_Click(object sender, EventArgs e)
		{
			BulkUpdateDiv.CssClass = NotificationRolesCssConstants.NrModalOn;
		}

		protected void BulkUpdateCancel_Click(object sender, EventArgs e)
		{
			BulkUpdateDiv.CssClass = NotificationRolesCssConstants.NrModalOff;
			Wrapper.CancelBulkUpdate();
			BulkUpdateGrid.Rebind();
		}

		protected void BulkUpdateOK_Click(object sender, EventArgs e)
		{
			BulkUpdateDiv.CssClass = NotificationRolesCssConstants.NrModalOff;
			if (Wrapper.BulkUpdateModeCollection.SelectedOrder.ModeCode == BulkUpdateModeCodes.SelectedContacts)
			{
				Wrapper.BulkUpdateSelected((OrgContactCollection)searchControl.SearchResultsDataGrid.DataSource, searchControl.SearchResultsDataGrid.GetSelectedPKs());
			}
			else
			{
				Wrapper.BulkUpdateAll(searchControl.FilterBusinessObject.Filter);
			}

			BulkUpdateGrid.ReadOnly = true;
			SaveButton_Click(null, null);
			searchControl.SearchResultsDataGrid.Rebind();
			BulkUpdateGrid.ReadOnly = false;
		}

		#endregion Bulk Update
	}
}
