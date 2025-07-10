using CargoWise.EntityFramework;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed partial class ZModuleButtonGridNonPersistentTestForm
	{
		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			var zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			var zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			this.CalcEdit = new ZCalcEdit();
			this.TextBox = new ZTextBox();
			this.SaveUserControl = new ZPostingButtonsUserControl();
			this.Grid = new ZDummyNonPersistentModuleButtonGrid();
			this.Grid.GridId = "iwer897423yui";
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = new System.Drawing.Point(0, 285);
			this.MainStatusBar.Name = "MainStatusBar";
			this.MainStatusBar.Size = new System.Drawing.Size(560, 23);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = 272;
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = 273;
			// 
			// CalcEdit
			// 
			this.CalcEdit.BindTo = "Z0_Number";
			this.CalcEdit.Decimals = 2;
			this.CalcEdit.Location = new System.Drawing.Point(40, 224);
			this.CalcEdit.Name = "CalcEdit";
			this.CalcEdit.TabIndex = 1;
			this.CalcEdit.Text = "ZCALCEDIT1";
			this.CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TextBox
			// 
			this.TextBox.BindTo = "Z0_Description";
			this.TextBox.Location = new System.Drawing.Point(40, 256);
			this.TextBox.Name = "TextBox";
			this.TextBox.TabIndex = 2;
			this.TextBox.Text = "ZTEXTBOX1";
			// 
			// SaveUserControl
			// 
			this.SaveUserControl.Location = new System.Drawing.Point(264, 248);
			this.SaveUserControl.Name = "SaveUserControl";
			this.SaveUserControl.Size = new System.Drawing.Size(240, 23);
			this.SaveUserControl.TabIndex = 3;
			// 
			// Grid
			// 
			this.Grid.AlwaysRequiresSaveBeforeEdit = false;
			this.Grid.BindToFindBoxList = "FilteredCollection";
			this.Grid.BindToGridList = "Collection";
			this.Grid.Controls.Add(this.Grid.InnerGrid);
			// 
			// Grid.InnerGrid
			// 
			this.Grid.InnerGrid.AllowNavigation = false;
			this.Grid.InnerGrid.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right);
			this.Grid.InnerGrid.BindTo = "Collection";
			this.Grid.InnerGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Description";
			zTextBoxColumnStyleInfo1.ColumnName = "Z0_Description";
			this.Grid.InnerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

			zCalcEditColumnStyleInfo1.Caption = "Number";
			zCalcEditColumnStyleInfo1.ColumnName = "Z0_Number";
			this.Grid.InnerGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);

			this.Grid.InnerGrid.EnableToolTips = false;
			this.Grid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.InnerGrid.LayoutKey = "Grid";
			this.Grid.InnerGrid.Location = new System.Drawing.Point(3, 2);
			this.Grid.InnerGrid.Name = "Grid";
			this.Grid.InnerGrid.Size = new System.Drawing.Size(374, 146);
			this.Grid.InnerGrid.TabIndex = 4;
			this.Grid.Location = new System.Drawing.Point(96, 16);
			this.Grid.ModuleID = Modules.Testing.DummyModuleIDs.Dummy;
			this.Grid.Name = "Grid";
			this.Grid.ShowAttachButton = true;
			this.Grid.ShowDetachButton = true;
			this.Grid.ShowEditButton = true;
			this.Grid.ShowNewButton = true;
			this.Grid.Size = new System.Drawing.Size(384, 184);
			this.Grid.TabIndex = 0;
			// 
			// ZModuleButtonGridTestForm
			// 

			this.ClientSize = new System.Drawing.Size(560, 308);
			this.Controls.Add(this.Grid);
			this.Controls.Add(this.SaveUserControl);
			this.Controls.Add(this.TextBox);
			this.Controls.Add(this.CalcEdit);
			this.Name = "ZModuleButtonGridTestForm";
			this.Text = "TestForm";
			this.Controls.SetChildIndex(this.CalcEdit, 0);
			this.Controls.SetChildIndex(this.TextBox, 0);
			this.Controls.SetChildIndex(this.SaveUserControl, 0);
			this.Controls.SetChildIndex(this.Grid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid.InnerGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
