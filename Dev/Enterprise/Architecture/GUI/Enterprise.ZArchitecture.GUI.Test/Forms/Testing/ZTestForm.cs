using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[SuppressFormDesignerAnalysis]
	[SuppressFormsLocalizedTest]
	public class ZTestForm : ZForm
	{
		public ZCalcEdit CalcEdit;
		public ZTextBox TextBox;
		public ZGrid Grid;
		public ZTabControl TabControl;
		public Enterprise.Core.Forms.ZPostingButtonsUserControl SaveUserControl;
		public ZTextBox TextBoxViaGrid;
		public ZCalcEdit CalcEditViaGrid;

		public new MenuItem EditMenuItem
		{
			get { return base.EditMenuItem; }
		}

		public ZTestForm()
		{
			InitializeComponent();
		}

		public override string FormCaption
		{
			get { return "ZTestForm"; }
		}

		public ZTestForm(IBusiness entity) : base(entity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, SaveUserControl);
		}

		public int DeleteCoreCallCount;
		protected override void DeleteCore()
		{
			DeleteCoreCallCount++;
			base.DeleteCore();
		}

		public new void Delete()
		{
			base.Delete();
		}

		#region Auto

		protected virtual ZGrid GetNewGrid()
		{
			return new ZGrid();
		}

		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			var zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			this.CalcEdit = new ZCalcEdit.Bare();
			this.TextBox = new ZTextBox.Bare();
			this.Grid = GetNewGrid();
			this.TabControl = new ZTabControl();
			this.SaveUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.TextBoxViaGrid = new ZTextBox.Bare();
			this.CalcEditViaGrid = new ZCalcEdit.Bare();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 287, true);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 23, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(276);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(277);
			// 
			// CalcEdit
			// 
			this.CalcEdit.BindTo = "Z0_Number";
			this.CalcEdit.Decimals = 2;
			this.CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.CalcEdit.Name = "CalcEdit";
			this.CalcEdit.TabIndex = 1;
			this.CalcEdit.Text = "ZCALCEDIT1";
			this.CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TextBox
			// 
			this.TextBox.BindTo = "Z0_Description";
			this.TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.TextBox.Name = "TextBox";
			this.TextBox.TabIndex = 2;
			this.TextBox.Text = "ZTEXTBOX1";
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.Grid.BindTo = "Collection";
			this.Grid.CaptionVisible = false;
			this.Grid.GridId = "9af86211-53ce-4ab1-8245-0c6080c34d93";
			zTextBoxColumnStyleInfo1.ColumnName = "Z0_Description";
			zCalcEditColumnStyleInfo1.ColumnName = "Z0_Number";
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.Grid.EnableToolTips = false;
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "Grid";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 16, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 224, true);
			this.Grid.TabIndex = 3;
			// 
			// TabControl
			// 
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 96, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 72, true);
			this.TabControl.TabIndex = 4;
			// 
			// SaveUserControl
			// 
			this.SaveUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 248, true);
			this.SaveUserControl.Name = "SaveUserControl";
			this.SaveUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.SaveUserControl.TabIndex = 5;
			// 
			// TextBoxViaGrid
			// 
			this.TextBoxViaGrid.BindTo = "Collection.Z0_Description";
			this.TextBoxViaGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 216, true);
			this.TextBoxViaGrid.Name = "TextBoxViaGrid";
			this.TextBoxViaGrid.TabIndex = 7;
			this.TextBoxViaGrid.Text = "ZTEXTBOX1";
			// 
			// CalcEditViaGrid
			// 
			this.CalcEditViaGrid.BindTo = "Collection.Z0_Number";
			this.CalcEditViaGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 184, true);
			this.CalcEditViaGrid.Name = "CalcEditViaGrid";
			this.CalcEditViaGrid.TabIndex = 6;
			this.CalcEditViaGrid.Text = "ZCALCEDIT1";
			this.CalcEditViaGrid.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ZTestForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 310, true);
			this.Controls.Add(this.TextBoxViaGrid);
			this.Controls.Add(this.CalcEditViaGrid);
			this.Controls.Add(this.SaveUserControl);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.Grid);
			this.Controls.Add(this.TextBox);
			this.Controls.Add(this.CalcEdit);
			this.Name = "ZTestForm";
			this.Text = "TestForm";
			this.Controls.SetChildIndex(this.CalcEdit, 0);
			this.Controls.SetChildIndex(this.TextBox, 0);
			this.Controls.SetChildIndex(this.Grid, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			this.Controls.SetChildIndex(this.SaveUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CalcEditViaGrid, 0);
			this.Controls.SetChildIndex(this.TextBoxViaGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		#endregion

		public new MenuItem ActionsMenuItem
		{
			get { return base.ActionsMenuItem; }
		}
	}
}
