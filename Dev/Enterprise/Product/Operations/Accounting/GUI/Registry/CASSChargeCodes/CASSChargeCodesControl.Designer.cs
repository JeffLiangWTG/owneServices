namespace Enterprise.Accounting.Registry.GUI
{
	partial class CASSChargeCodesControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CASSComponentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CASSComponentsGrid)).BeginInit();
			this.CASSComponentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.CASSChargeCodeCollection);
			// 
			// CASSComponentsGrid
			// 
			this.CASSComponentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CASSComponentsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.CASSChargeCode)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.CASSChargeCode)(null)).CASSType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.CASSChargeCode)(null)).CASSComponentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.CASSChargeCode)(null)).CASSComponentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Registry.Business.CASSChargeCode)(null)).ChargeCodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.CASSChargeCode)(null)).Lookups.ChargeCodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.CASSChargeCode)(null)).ChargeDescription)));
			this.CASSComponentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f4a52637-6ff9-46b5-b5b1-294b0b4f95ce", "CASS Type");
			zDropEditColumnStyleInfo1.ColumnName = "CASSType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6678db58-4fe9-4bc8-b4da-b6217f770065", "CASS Component");
			zDropEditColumnStyleInfo2.ColumnName = "CASSComponentCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8c3f464a-6d50-49f1-a60b-6eb26a5e2bd1", "CASS Description");
			zTextBoxColumnStyleInfo1.ColumnName = "CASSComponentDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);			
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2866a045-0043-484c-8abd-1d727680a8b1", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ChargeCodePK";
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.AccChargeCodeForRegistry;
			zGuidFindBoxColumnStyleInfo1.PopupCaption = "Select a Charge Code";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("885daf62-00b1-4a41-9e1c-42031d36685a", "Charge Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ChargeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.CASSComponentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CASSComponentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CASSComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CASSComponentsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CASSComponentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CASSComponentsGrid.CopySelectedRowsAllowed = true;
			this.CASSComponentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CASSComponentsGrid.GridId = "0edae90a-b9ef-49eb-b555-0aca9df670b3";
			this.CASSComponentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CASSComponentsGrid.LayoutKey = "CASSComponentsGrid";
			this.CASSComponentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CASSComponentsGrid.Name = "CASSComponentsGrid";
			this.CASSComponentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 189, true);
			this.CASSComponentsGrid.TabIndex = 0;
			// 
			// CASSChargeCodesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CASSComponentsGrid);
			this.Name = "CASSChargeCodesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 189, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CASSComponentsGrid)).EndInit();
			this.CASSComponentsGrid.ResumeLayout(false);
			this.CASSComponentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid CASSComponentsGrid;
	}
}
