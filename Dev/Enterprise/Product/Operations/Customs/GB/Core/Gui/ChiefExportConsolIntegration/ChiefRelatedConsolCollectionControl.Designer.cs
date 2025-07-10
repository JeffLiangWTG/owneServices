namespace Enterprise.Customs.GB.GUI.ChiefExportConsolIntegration
{
	partial class ChiefRelatedConsolCollectionControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.zModuleButtonGrid1 = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.CustomsExportConsolIntegrationWrapper);
			// 
			// zModuleButtonGrid1
			// 
			this.zModuleButtonGrid1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zModuleButtonGrid1, "RelatedConsols");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.CustomsExportConsolIntegrationWrapper)(null)).RelatedConsols)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration.CustomsExportConsolIntegrationWrapper)(null)).UnRelatedConsolsToAdd)));
			this.zModuleButtonGrid1.BindToFindBoxList = "UnRelatedConsolsToAdd";
			this.zModuleButtonGrid1.CaptionResourceString = null;
			zTextBoxColumnStyleInfo1.Caption = "Consol ID";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.CaptionResourceString = null; 
			zTextBoxColumnStyleInfo1.ColumnName = "JK_UniqueConsignRef";
			zTextBoxColumnStyleInfo2.Caption = "MAWB";
			zTextBoxColumnStyleInfo2.CaptionResourceString = null; 
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "JK_MasterBillNum";
			zTextBoxColumnStyleInfo3.Caption = "Type";
			zTextBoxColumnStyleInfo3.CaptionResourceString = null; 
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ColumnName = "JK_AgentType";
			zTextBoxColumnStyleInfo4.Caption = "Agent Reference";
			zTextBoxColumnStyleInfo4.CaptionResourceString = null; 
			zTextBoxColumnStyleInfo4.ColumnName = "JK_AgentsReference";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Caption = "Load";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.CaptionResourceString = null; 
			zTextBoxColumnStyleInfo5.ColumnName = "JK_RL_NKLoadPort";
			zTextBoxColumnStyleInfo6.Caption = "Discharge";
			zTextBoxColumnStyleInfo6.CaptionResourceString = null; 
			zTextBoxColumnStyleInfo6.ColumnName = "JK_RL_NKDischargePort";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Shipments";
			zCalcEditColumnStyleInfo1.CaptionResourceString = null;
			zCalcEditColumnStyleInfo1.IsReadOnly = true; 
			zCalcEditColumnStyleInfo1.ColumnName = "ShipmentCount";
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.zModuleButtonGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zModuleButtonGrid1.DetachMessage = null;
			this.zModuleButtonGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// 
			// 
			this.zModuleButtonGrid1.InnerGrid.AllowNavigation = false;
			this.zModuleButtonGrid1.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zModuleButtonGrid1.InnerGrid.CaptionVisible = false;
			this.zModuleButtonGrid1.InnerGrid.CopySelectedRowsAllowed = true;
			this.zModuleButtonGrid1.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zModuleButtonGrid1.InnerGrid.LayoutKey = "Grid";
			this.zModuleButtonGrid1.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.zModuleButtonGrid1.InnerGrid.Name = "Grid";
			this.zModuleButtonGrid1.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 154, true);
			this.zModuleButtonGrid1.InnerGrid.TabIndex = 0;
			this.zModuleButtonGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zModuleButtonGrid1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.JobConsol;
			this.zModuleButtonGrid1.Name = "zModuleButtonGrid1";
			this.zModuleButtonGrid1.NameOfAGridElement = Enterprise.Customs.GB.GUI.Res.GetData("9B0F933D-C347-4B88-985F-56F5CD0C08D9", "UCR");
			this.zModuleButtonGrid1.ReadOnly = false;
			this.zModuleButtonGrid1.ShowNewButton = false;
			this.zModuleButtonGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 192, true);
			this.zModuleButtonGrid1.TabIndex = 4;
			// 
			// ChiefRelatedConsolCollectionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zModuleButtonGrid1);
			this.Name = "ChiefRelatedConsolCollectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 192, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZModuleButtonGrid zModuleButtonGrid1;
	}
}
