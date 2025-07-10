using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.GUI
{
	partial class RequestedDocumentsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RequestedDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RequestedDocumentsGrid)).BeginInit();
			this.RequestedDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(IRequestedDocumentsProvider);
			// 
			// RequestedDocumentsGrid
			// 
			this.RequestedDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RequestedDocumentsGrid, "RequestedDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.IRequestedDocumentsProvider)(null)).RequestedDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.RequestedDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.IRequestedDocumentsProvider)(null)).RequestedDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.RequestedDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.IRequestedDocumentsProvider)(null)).RequestedDocuments)).SyncRoot)).RequestInformation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.Business.RequestedDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.IRequestedDocumentsProvider)(null)).RequestedDocuments)).SyncRoot)).CSI_DateOfIssue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.Business.RequestedDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.IRequestedDocumentsProvider)(null)).RequestedDocuments)).SyncRoot)).CSI_DateOfExpiry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.RequestedDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.IRequestedDocumentsProvider)(null)).RequestedDocuments)).SyncRoot)).CSI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.RequestedDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.IRequestedDocumentsProvider)(null)).RequestedDocuments)).SyncRoot)).StatusDescription)));
			this.RequestedDocumentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "RequestInformation";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo2.ColumnName = "CSI_DateOfExpiry";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Status";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.RequestedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RequestedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RequestedDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.RequestedDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.RequestedDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RequestedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RequestedDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RequestedDocumentsGrid.GridId = "2caa0e69-1347-4951-93ff-9d39a29ad45c";
			this.RequestedDocumentsGrid.Name = "RequestedDocumentsGrid";
			// 
			// RequestedDocumentsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RequestedDocumentsGrid);
			this.Name = "RequestedDocumentsUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RequestedDocumentsGrid)).EndInit();
			this.RequestedDocumentsGrid.ResumeLayout(false);
			this.RequestedDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected ZArchitecture.ZGrid RequestedDocumentsGrid;
	}
}
