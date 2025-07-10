using CargoWise.Windows.UI;

namespace Enterprise.Registry.GUI
{
	partial class EnableAddEditAndDeleteLogsItemsControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            this.MainPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
            this.EnableAddEditAndDeleteLogsItemsGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EnableAddEditAndDeleteLogsItemsGrid)).BeginInit();
            this.EnableAddEditAndDeleteLogsItemsGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.EnableAddEditAndDeleteLogsItemCollection);
            // 
            // MainPanel
            // 
            this.MainPanel.ColumnCount = 1;
            this.MainPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.MainPanel.Controls.Add(this.EnableAddEditAndDeleteLogsItemsGrid, 0, 0);
            this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MainPanel.Name = "MainPanel";
            this.MainPanel.RowCount = 1;
            this.MainPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 423, true);
            this.MainPanel.TabIndex = 0;
            // 
            // EnableAddEditAndDeleteLogsItemsGrid
            // 
            this.EnableAddEditAndDeleteLogsItemsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.EnableAddEditAndDeleteLogsItemsGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.EnableAddEditAndDeleteLogsItem)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.EnableAddEditAndDeleteLogsItem)(null)).Table)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.EnableAddEditAndDeleteLogsItem)(null)).EnableADDLogs)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.EnableAddEditAndDeleteLogsItem)(null)).EnableEDTLogs)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.EnableAddEditAndDeleteLogsItem)(null)).EnableDELLogs)));
            this.EnableAddEditAndDeleteLogsItemsGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6f7b5e1f-ba6c-4819-9a7e-0094ef8989ef", "Table");
            zTextBoxColumnStyleInfo1.ColumnName = "Table";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.IsSortable = false;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("7dce89a3-8086-4724-8a40-d2bfd2462af6", "Enable ADD Logs");
            zCheckBoxColumnStyleInfo1.ColumnName = "EnableADDLogs";
            zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo1.IsSortable = false;
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("207bc04a-72b5-49c6-9199-81d1ac0fd782", "Enable EDT Logs");
            zCheckBoxColumnStyleInfo2.ColumnName = "EnableEDTLogs";
            zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo2.IsSortable = false;
            zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("12ab3c8d-4d45-4f6c-a7de-d5ec09db9ca9", "Enable DEL Logs");
            zCheckBoxColumnStyleInfo3.ColumnName = "EnableDELLogs";
            zCheckBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCheckBoxColumnStyleInfo3.IsSortable = false;
            zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            this.EnableAddEditAndDeleteLogsItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.EnableAddEditAndDeleteLogsItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.EnableAddEditAndDeleteLogsItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
            this.EnableAddEditAndDeleteLogsItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
            this.EnableAddEditAndDeleteLogsItemsGrid.GridId = "13f272e7-5152-4531-96b5-83f454cad8a2";
            this.EnableAddEditAndDeleteLogsItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.EnableAddEditAndDeleteLogsItemsGrid.LayoutKey = "EnableAddEditAndDeleteLogsItemsGrid";
            this.EnableAddEditAndDeleteLogsItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
            this.EnableAddEditAndDeleteLogsItemsGrid.Name = "EnableAddEditAndDeleteLogsItemsGrid";
            this.EnableAddEditAndDeleteLogsItemsGrid.ReadOnly = true;
            this.EnableAddEditAndDeleteLogsItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 420, true);
            this.EnableAddEditAndDeleteLogsItemsGrid.TabIndex = 0;
            // 
            // EnableAddEditAndDeleteLogsItemsControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.MainPanel);
            this.Name = "EnableAddEditAndDeleteLogsItemsControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 423, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EnableAddEditAndDeleteLogsItemsGrid)).EndInit();
            this.EnableAddEditAndDeleteLogsItemsGrid.ResumeLayout(false);
            this.EnableAddEditAndDeleteLogsItemsGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private KTableLayoutPanel MainPanel;
		internal ZArchitecture.ZGrid EnableAddEditAndDeleteLogsItemsGrid;
	}
}
