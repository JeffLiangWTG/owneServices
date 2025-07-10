using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public partial class IncidentTriageChecklistsItemFilterControl : ZFilterStripControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo0 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			this.RecentItemsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// grid
			//
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem)(null)).IMC_ChecklistNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem)(null)).IMC_ResponseType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem)(null)).IMC_SupportDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem)(null)).IMC_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem)(null)).IMC_IsPublished)));

			zTextBoxColumnStyleInfo0.CaptionResourceString = ZClientEDI.Res.GetData("13cb5e6a-015d-46ef-a6ac-7d6b7239fb66", "Checklist Number");
			zTextBoxColumnStyleInfo0.ColumnName = "IMC_ChecklistNumber";
			zTextBoxColumnStyleInfo0.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("ae3097f3-2dcb-47de-ba8a-994920cdff01", "Checklist Response Type");
			zTextBoxColumnStyleInfo1.ColumnName = "IMC_ResponseType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("23339d2c-8d75-4557-9f32-ee86e1605d98", "Checklist Description");
			zTextBoxColumnStyleInfo2.ColumnName = "IMC_SupportDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("e7612ba2-e232-4ee4-a715-098b633adac4", "Checklist Category");
			zTextBoxColumnStyleInfo3.ColumnName = "IMC_Category";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("2e3e8724-1a4e-45b5-8f22-b8234a6591bc", "Is Checklist Published?");
			zTextBoxColumnStyleInfo4.ColumnName = "IMC_IsPublished";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo0);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);

			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.IncidentTriageChecklistItem);
			//
			// IncidentTriageChecklistItemFilterControl
			//
			this.Name = "IncidentTriageChecklistItemFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			this.RecentItemsPanel.ResumeLayout(false);
			this.RecentItemsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
