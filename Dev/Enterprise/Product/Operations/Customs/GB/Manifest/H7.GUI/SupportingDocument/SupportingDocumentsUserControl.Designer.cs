using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI
{
	partial class SupportingDocumentsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyle1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyle2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SupportingDocumentsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SupportingDocumentsFieldsControl = new H7SupportingDocumentsFieldsControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			this.SupportingDocumentsPanel.SuspendLayout();
			this.SupportingDocumentsFieldsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, "SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.SupportingDocumentsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zDropEditColumnStyle1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyle1.ColumnName = "CSI_Actions";
			zDropEditColumnStyle2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyle2.ColumnName = "CSI_Availability";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_SubType";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.ColumnName = "CSI_DateOfExpiry";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zTextBoxColumnStyleInfo4.ColumnName = "CSI_ReferenceNumber2";
			this.SupportingDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyle1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyle2);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGrid.GridId = "3265ef25-4173-437d-858f-6029a81236c2";
			this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupportingDocumentsGrid.LayoutKey = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 150, true);
			this.SupportingDocumentsGrid.TabIndex = 0;
			// 
			// SupportingDocumentsPanel
			//
			this.SupportingDocumentsPanel.Controls.Add(this.SupportingDocumentsFieldsControl);
			this.SupportingDocumentsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SupportingDocumentsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 370, true);
			this.SupportingDocumentsPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 181);
			this.SupportingDocumentsPanel.Name = "SupportingDocumentsPanel";
			this.SupportingDocumentsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 221);
			this.SupportingDocumentsPanel.TabIndex = 2;
			// 
			// SupportingDocumentsFieldsControl
			//
			this.SupportingDocumentsFieldsControl.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.H7.Business.AsycudaBill);
			SupportingDocumentsFieldsControl.Dock = DockStyle.Fill;

			// 
			// SupportingDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SupportingDocumentsGrid);
			this.Controls.Add(this.SupportingDocumentsPanel);
			this.Name = "SupportingDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			this.SupportingDocumentsPanel.ResumeLayout(false);
			this.SupportingDocumentsPanel.PerformLayout();
			this.SupportingDocumentsFieldsControl.ResumeLayout(false);
			this.SupportingDocumentsFieldsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel SupportingDocumentsPanel;
		public Enterprise.ZArchitecture.ZGrid SupportingDocumentsGrid;
		private H7SupportingDocumentsFieldsControl SupportingDocumentsFieldsControl;
	}
}

