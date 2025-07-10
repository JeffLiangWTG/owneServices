using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Core.Forms;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[SuppressFormDesignerAnalysis]
	class ZTestGridForm : ZForm
	{
		public ZCalcEdit CalcEdit;
		public ZTextBox TextBox;
		public ZTabControl TabControl;
		public ZPostingButtonsUserControl SaveUserControl;
		private ZTabPage oTabPage1;
		public ZGrid TabGrid;
		private readonly System.ComponentModel.Container components;
		private TableLayoutPanel layoutPanel;

		public ZTestGridForm()
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return "ZTestForm"; }
		}

		public ZTestGridForm(IBusiness entity)
			: base(entity)
		{
		}

		public bool IsDataVersionLogsMenuItemVisible
		{
			get { return TabGrid.IsDataVersionLogsMenuItemVisible; }
			set { TabGrid.IsDataVersionLogsMenuItemVisible = value; }
		}

		#region Auto

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			var zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			this.CalcEdit = new ZCalcEdit();
			this.TextBox = new ZTextBox();
			this.TabControl = new ZTabControl();
			this.oTabPage1 = new ZTabPage();
			this.TabGrid = new ZGrid();
			this.TabGrid.GridId = "08923409054io45097";
			this.layoutPanel = new TableLayoutPanel();

			this.SaveUserControl = new ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.TabControl.SuspendLayout();
			this.oTabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TabGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = ControlDpiScalingHelper.NewScaledPoint(0, 285);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = ControlDpiScalingHelper.NewScaledSize(560, 23);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(272);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(273);
			// 
			// CalcEdit
			// 
			this.CalcEdit.BindTo = "Z0_Number";
			this.CalcEdit.Decimals = 2;
			this.CalcEdit.Location = ControlDpiScalingHelper.NewScaledPoint(16, 16);
			this.CalcEdit.Name = "CalcEdit";
			this.CalcEdit.TabIndex = 1;
			this.CalcEdit.Text = "ZCALCEDIT1";
			this.CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TextBox
			// 
			this.TextBox.BindTo = "Z0_Description";
			this.TextBox.Location = ControlDpiScalingHelper.NewScaledPoint(16, 48);
			this.TextBox.Name = "TextBox";
			this.TextBox.TabIndex = 2;
			this.TextBox.Text = "ZTEXTBOX1";
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.oTabPage1);
			this.TabControl.Location = ControlDpiScalingHelper.NewScaledPoint(8, 96);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = ControlDpiScalingHelper.NewScaledSize(208, 184);
			this.TabControl.TabIndex = 4;
			// 
			// oTabPage1
			// 
			this.oTabPage1.CheckForNotifications = true;
			this.oTabPage1.Controls.Add(layoutPanel);
			this.oTabPage1.Location = ControlDpiScalingHelper.NewScaledPoint(4, 23);
			this.oTabPage1.Name = "oTabPage1";
			this.oTabPage1.Size = ControlDpiScalingHelper.NewScaledSize(200, 157);
			this.oTabPage1.TabIndex = 0;
			this.oTabPage1.Text = "oTabPage1";
			// 
			// TabGrid
			// 
			this.TabGrid.AllowNavigation = false;
			this.TabGrid.BindTo = "Collection";
			this.TabGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Z0_Description";
			zCalcEditColumnStyleInfo1.ColumnName = "Z0_Number";
			this.TabGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TabGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TabGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabGrid.EnableToolTips = false;
			this.TabGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TabGrid.LayoutKey = "TabGrid";
			this.TabGrid.Location = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.TabGrid.Name = "TabGrid";
			this.TabGrid.Size = ControlDpiScalingHelper.NewScaledSize(200, 157);
			this.TabGrid.TabIndex = 4;
			// 
			// layoutPanel
			// 
			this.layoutPanel.Name = "layoutPanel";
			this.layoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layoutPanel.Controls.Add(this.TabGrid);
			// 
			// SaveUserControl
			// 
			this.SaveUserControl.Location = ControlDpiScalingHelper.NewScaledPoint(264, 248);
			this.SaveUserControl.Name = "SaveUserControl";
			this.SaveUserControl.Size = ControlDpiScalingHelper.NewScaledSize(240, 23);
			this.SaveUserControl.TabIndex = 5;
			// 
			// ZTestGridForm
			// 

			this.ClientSize = ControlDpiScalingHelper.NewScaledSize(560, 308);
			this.Controls.Add(this.SaveUserControl);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.TextBox);
			this.Controls.Add(this.CalcEdit);
			this.Name = "ZTestGridForm";
			this.Text = "TestForm";
			this.Controls.SetChildIndex(this.CalcEdit, 0);
			this.Controls.SetChildIndex(this.TextBox, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			this.Controls.SetChildIndex(this.SaveUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.oTabPage1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TabGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		#endregion
	}
}
