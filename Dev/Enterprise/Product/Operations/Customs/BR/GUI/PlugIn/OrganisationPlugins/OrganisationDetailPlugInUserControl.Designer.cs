using Enterprise.Customs.BR.Business;

namespace Enterprise.Customs.BR.GUI
{
	partial class OrganisationDetailPlugInUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.AdditionalIdentificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalIdentificationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalIdentificationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalIdentificationGrid)).BeginInit();
			this.AdditionalIdentificationGrid.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.OrgHeaderWrapper);
			// 
			// AdditionalIdentificationGroupBox
			// 
			this.AdditionalIdentificationGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("BCB2108F-588A-4582-8D2B-FC5ABB19EAEA", "Additional Identification");
			this.AdditionalIdentificationGroupBox.Controls.Add(this.AdditionalIdentificationGrid);
			this.AdditionalIdentificationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.AdditionalIdentificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalIdentificationGroupBox.Name = "AdditionalIdentificationGroupBox";
			this.AdditionalIdentificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 86, true);
			this.AdditionalIdentificationGroupBox.TabIndex = 0;
			this.AdditionalIdentificationGroupBox.TabStop = false;
			// 
			// AdditionalIdentificationGrid
			// 
			this.AdditionalIdentificationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalIdentificationGrid, "AdditionalIdentification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.OrgHeaderWrapper)(null)).AdditionalIdentification)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AdditionalIdentification)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.OrgHeaderWrapper)(null)).AdditionalIdentification)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.AdditionalIdentification)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.OrgHeaderWrapper)(null)).AdditionalIdentification)).SyncRoot)).CY_Code)));
			this.AdditionalIdentificationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CY_Code";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AdditionalIdentificationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalIdentificationGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AdditionalIdentificationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalIdentificationGrid.GridId = "2edf3128-ebe2-4df0-92fc-218d0943b12a";
			this.AdditionalIdentificationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalIdentificationGrid.LayoutKey = "zGrid1";
			this.AdditionalIdentificationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AdditionalIdentificationGrid.Name = "AdditionalIdentificationGrid";
			this.AdditionalIdentificationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 67, true);
			this.AdditionalIdentificationGrid.TabIndex = 0;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "AddInfo.ZO_BrokerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.OrgHeaderWrapper)(null)).AddInfo.ZO_BrokerCode)));
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 93, true);
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BrokerCodeFindBox.ParentType = null;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
			this.BrokerCodeFindBox.TabIndex = 1;
			// 
			// OrganisationDetailPlugInUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BrokerCodeFindBox);
			this.Controls.Add(this.AdditionalIdentificationGroupBox);
			this.Name = "OrganisationDetailPlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 202, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalIdentificationGroupBox.ResumeLayout(false);
			this.AdditionalIdentificationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalIdentificationGrid)).EndInit();
			this.AdditionalIdentificationGrid.ResumeLayout(false);
			this.AdditionalIdentificationGrid.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox AdditionalIdentificationGroupBox;
		internal ZArchitecture.ZGrid AdditionalIdentificationGrid;
		internal ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
	}
}
