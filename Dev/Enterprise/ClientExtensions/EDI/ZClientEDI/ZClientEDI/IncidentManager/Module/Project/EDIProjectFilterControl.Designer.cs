using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	partial class EDIProjectFilterControl
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
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).CallbackBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).PlannedInstall)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).InstallDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).EstimatedSiteLiveDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).SiteLiveDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).EnterpriseCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.EDIProject)(null)).EnterpriseID)));

			zDateEditColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("0f1272ff-e80f-4e0a-a2ba-c02816964b1f", "Follow Up Date");
			zDateEditColumnStyleInfo2.ColumnName = "CallbackBy";
			zDateEditColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("7ccb8385-878a-4242-b679-e97b6697ded6", "Planned Install");
			zDateEditColumnStyleInfo4.ColumnName = "PlannedInstall";
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("74947121-ed3b-4d2f-b98c-a5f8c124fd0c", "Install Complete");
			zDateEditColumnStyleInfo5.ColumnName = "InstallDate";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo6.CaptionResourceString = ZClientEDI.Res.GetData("39a28904-2365-435a-99dc-a8238e1f59ab", "Planned Go-Live");
			zDateEditColumnStyleInfo6.ColumnName = "EstimatedSiteLiveDate";
			zDateEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo7.CaptionResourceString = ZClientEDI.Res.GetData("21e8ef4c-15a1-41f2-a6be-f4d2958097c3", "Go-Live Complete");
			zDateEditColumnStyleInfo7.ColumnName = "SiteLiveDate";
			zDateEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("af5e2673-ad24-4725-abfd-d08fe51439d8", "Enterprise Code");
			zTextBoxColumnStyleInfo1.ColumnName = "EnterpriseCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo8.CaptionResourceString = ZClientEDI.Res.GetData("9769B9D2-C076-469E-9BE4-4CDCBD97021B", "Agreed Go-Live");
			zDateEditColumnStyleInfo8.ColumnName = "AgreedLiveDate";
			zDateEditColumnStyleInfo8.DateTimeFormat = ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("dc787b1d-353e-4f4f-b9a9-d6447aad07d4", "Enterprise ID");
			zTextBoxColumnStyleInfo2.ColumnName = "EnterpriseID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.EDIProject);
			// 
			// EDIProjectFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "EDIProjectFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
