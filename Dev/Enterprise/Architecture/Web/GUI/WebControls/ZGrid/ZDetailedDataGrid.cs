using System;
using System.Collections;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Datagrid with edit control underneath
	/// </summary>
	public class ZDetailedDataGrid : WebControl, IContainResources, ISelfBindingWebControl, INamingContainer, IExternallyFiredPostBackHandler
	{
		public ZDetailedDataGrid() : base("DIV")
		{
			this.CssClass = CssConstants.DetailedDataGrid;
		}

		#region Child Controls

		public ZDataGrid DataGrid
		{
			get
			{
				EnsureChildControls();
				return (ZDataGrid)Controls[0];
			}
		}

		protected Panel MainPanel
		{
			get
			{
				EnsureChildControls();
				return Controls[1] as Panel;
			}
		}

		protected Panel EditPanel
		{
			get
			{
				EnsureChildControls();
				return MainPanel.Controls[0] as Panel;
			}
		}

		protected Button OKButton
		{
			get
			{
				EnsureChildControls();
				return (Button)MainPanel.Controls[1];
			}
		}

		protected Button CancelButton
		{
			get
			{
				EnsureChildControls();
				return (Button)MainPanel.Controls[3];
			}
		}

		protected Label NothingToShowLabel
		{
			get
			{
				EnsureChildControls();
				return (Label)Controls[2];
			}
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			ZDataGrid fDataGrid = new ZDataGrid();
			fDataGrid.ExtendedEditMode = true;
			fDataGrid.BindTo = BindTo;
			fDataGrid.AllowAdd = true;
			fDataGrid.AllowDelete = true;
			fDataGrid.AllowEdit = true;
			fDataGrid.CssClass = CssConstants.DetailsTable;
			fDataGrid.ItemStyle.CssClass = CssConstants.DetailsCell;
			fDataGrid.HeaderStyle.CssClass = CssConstants.DetailsHeader;
			fDataGrid.SelectedItemStyle.CssClass = CssConstants.DetailsSelection;

			fDataGrid.AfterItemCommand += new DataGridCommandEventHandler(DataGrid_AfterItemCommand);
			fDataGrid.AfterDeleteCommand += new DataGridCommandEventHandler(DataGrid_AfterDeleteCommand);
			fDataGrid.AfterCancelCommand += new DataGridCommandEventHandler(DataGrid_AfterCancelCommand);
			Controls.Add(fDataGrid);

			Panel mainPanel = new Panel();
			Controls.Add(mainPanel);

			mainPanel.Controls.Add(new Panel());
			Button oKButton = new Button();
			oKButton.Text = Res.GetString("7f559b5d-794f-4613-b9fe-71a959f90a8b", "Save");
			oKButton.CommandName = "OK";
			oKButton.Command += new CommandEventHandler(OKButton_Command);
			mainPanel.Controls.Add(oKButton);

			mainPanel.Controls.Add(new LiteralControl("&nbsp;"));

			Button cancelButton = new Button();
			cancelButton.Text = Res.GetString("0d65774b-65ec-47e5-9c3e-42a64329b4be", "Cancel");
			cancelButton.CommandName = "Cancel";
			cancelButton.Command += new CommandEventHandler(CancelButton_Command);
			mainPanel.Controls.Add(cancelButton);

			Label nothingToShowLabel = new Label();
			nothingToShowLabel.CssClass = CssConstants.ErrorMessage;
			//NothingToShowLabel.Text = "No row is selected. Please add, edit or view a row.";
			Controls.Add(nothingToShowLabel);
		}

		#endregion ChildControls

		#region Properties

		[DefaultValue(false), Category("Appearance"), Description("Details are Readonly")]
		public bool ReadOnly
		{
			get { return fReadOnly; }
			set { fReadOnly = value; }
		}
		bool fReadOnly;

		#endregion

		#region EditPanel

		[DefaultValue(""), Category("Appearance"), Description("Edit panel user control path for row editing")]
		public string EditPanelUserControlPath
		{
			get {  return ViewState["EditPanelUserControlPath"] as string; }
			set
			{
				ViewState["EditPanelUserControlPath"] = value;
				if (!string.IsNullOrEmpty(EditPanelUserControlPath))
				{
					EditPanel.Controls.Add(Page.LoadControl(EditPanelUserControlPath));
				}
			}
		}
		#endregion

		#region Overrides

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			EnsureChildControls();
			OKButton.Visible = !ReadOnly;
			CancelButton.Visible = !ReadOnly;
		}

		protected override void Render(HtmlTextWriter writer)
		{
			EnsureChildControls();
			base.Render(writer);
		}
		#endregion

		#region IContainResources Members

		public ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = new ZWebResourceCollection();
				result.Add(GridEditPanelResource);
				return result;
			}
		}

		protected ZWebResource GridEditPanelResource
		{
			get
			{
				if (fGridEditPanelResource == null)
				{
					fGridEditPanelResource = new ZWebResource(typeof(ZDataGrid), "GridEditPanel.ascx", this.Page as ZPage, "Enterprise.ZArchitecture.Web.GUI.WebControls.ZGrid");
				}
				return fGridEditPanelResource;
			}
		}
		ZWebResource fGridEditPanelResource;

		#endregion

		#region ISelfBindingWebControl Members

		public object DataSource
		{
			get
			{
				return dataSource;
			}
		}

		object dataSource;

		public bool IsBindable(object dataSource)
		{
			return true;
		}

		public void Bind(object dataSource)
		{
			this.dataSource = dataSource;
			DataGrid.Bind(dataSource);
			if (!Page.IsPostBack)
			{
				SetGridSelectedIndex(0);
				BindGridDetails(true);
			}
		}

		void BindGridDetails(bool ignoreChanges)
		{
			if (DetailsDataSource != null)
			{
				new ZWebControlBinder(DetailsDataSource).Bind(EditPanel.Controls, ignoreChanges);
			}
			UpdateControlsVisibility();
		}

		void UpdateControlsVisibility()
		{
			if (Collection != null && Collection.Count > 0)
			{
				MainPanel.Visible = true;
				NothingToShowLabel.Visible = false;
			}
			else
			{
				MainPanel.Visible = false;
				NothingToShowLabel.Visible = true;
				NothingToShowLabel.Text = (ReadOnly) ? Res.GetString("7d2e6b34-861d-4d74-a2e9-8985bdd76a5d", "No data") : Res.GetString("8b495e7b-42f7-40f3-9776-b2b70cd6ecad", "No row is selected. Please add, edit or view a row.");
			}
		}

		public void UnBind()
		{
			// TODO:  Add ZDetailedDataGrid.UnBind implementation
		}

		IBusinessObjectCollection Collection
		{
			get { return DataGrid.DataSource as IBusinessObjectCollection; }
		}

		BusinessObject DetailsDataSource
		{
			get { return (Collection != null && DataGrid.SelectedIndex > -1) ? (BusinessObject)((IList)Collection)[DataGrid.SelectedIndex] : null; }
		}

		#endregion

		#region IBindTo Members

		public string BindTo
		{
			get { return ViewState["BindTo"] as string; }
			set
			{
				ViewState["BindTo"] = value;
				DataGrid.BindTo = value;
			}
		}

		#endregion

		#region Commands

		void SetGridSelectedIndex(int value)
		{
			int idx = -1;

			if (Collection != null && Collection.Count > 0 && value > -1 && value < Collection.Count)
			{
				idx = value;
			}
			DataGrid.SelectedIndex = idx;
		}

		void OKButton_Command(object sender, CommandEventArgs e)
		{
			BindGridDetails(false);

			DataGrid.Bind(((ZPage)Page).DataSource);
			DataGrid.RowAdded = false;
			OnAfterDetailsChanged(e);
		}

		void CancelButton_Command(object sender, CommandEventArgs e)
		{
			if (DataGrid.RemoveRowIfAdded(DataGrid.SelectedIndex))
			{
				SetGridSelectedIndex(0);
			}

			BindGridDetails(true);
			DataGrid.Bind(((ZPage)Page).DataSource);
		}

		void DataGrid_AfterItemCommand(object source, DataGridCommandEventArgs e)
		{
			if (e.CommandName == ZDataGrid.EditPanelCommand)
			{
				if (e.Item.ItemIndex != DataGrid.SelectedIndex)
				{
					SaveDiscardChanges(true, source);
				}
				SetGridSelectedIndex(e.Item.ItemIndex);
				BindGridDetails(true);
			}
			else if (e.CommandName == ZDataGrid.InsertCommandName)
			{
				SaveDiscardChanges(true, source);
				SetGridSelectedIndex(Collection.Count - 1);
				BindGridDetails(true);
			}
		}

		void DataGrid_AfterDeleteCommand(object source, DataGridCommandEventArgs e)
		{
			if (e.Item.ItemIndex != DataGrid.SelectedIndex)
			{
				SaveDiscardChanges(true, source);
			}
			SetGridSelectedIndex(0);
			BindGridDetails(true);
			UpdateControlsVisibility();
		}

		void DataGrid_AfterCancelCommand(object source, DataGridCommandEventArgs e)
		{
			SetGridSelectedIndex(0);
			BindGridDetails(true);
			UpdateControlsVisibility();
		}

		void OnAfterDetailsChanged(CommandEventArgs e)
		{
			if (AfterDetailsChanged != null)
			{
				AfterDetailsChanged(this, e);
			}
		}
		public event CommandEventHandler AfterDetailsChanged;

		#endregion

		#region SaveDiscardChanges

		public void SaveChanges(object sender)
		{
			SaveDiscardChanges(true, sender);
		}

		public void DiscardChanges(object sender)
		{
			SaveDiscardChanges(false, sender);
		}
		protected void SaveDiscardChanges(bool save, object sender)
		{
			if (save)
			{
				OKButton_Command(sender, new CommandEventArgs("SaveChanges", null));
			}
			else
			{
				CancelButton_Command(sender, new CommandEventArgs("DiscardChanges", null));
			}
		}

		#endregion

		#region IExternallyFiredPostBackHandler Members

		public void HandleExternallyFiredPostBack(Control target, EventArgs e)
		{
			if (OnExternallyFiredPostback != null)
			{
				OnExternallyFiredPostback(target, EventArgs.Empty);
			}
		}
		public event EventHandler OnExternallyFiredPostback;

		#endregion
	}
}