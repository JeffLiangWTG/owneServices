using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	internal partial class ShipmentAndBillingDetailsForm
	{


		#region Windows Form Designer generated code

		protected new void InitializeComponent()
		{
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DetailsGrid = new ZDisplayGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).BeginInit();
			this.DetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 185, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 22, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ShipmentAndBillingDetails);
			// 
			// DetailsGrid
			// 
			this.DetailsGrid.AllowNavigation = false;
			this.DetailsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DetailsGrid, "DetailsRows");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZBool)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).ShowRelatedCharges)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).RelatedJobNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).RelatedJobOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).RelatedJobDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).PickUpAgentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).PickUpAgentName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).DeliveryAgentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).DeliveryAgentName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).PreviousSendingAgentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).PreviousSendingAgentName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).DebtorCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).DebtorName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).DebtorAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).TargetJobNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((ShipmentAndBillingDetailsRow)(((System.Collections.IList)(((ShipmentAndBillingDetails)(null)).DetailsRows)).SyncRoot)).TargetJobRoute)));
			this.DetailsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "ShowRelatedCharges";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|1ce82004-f412-45e1-9a62-8edc6fb70503", "Shipment ID");
			zTextBoxColumnStyleInfo1.ColumnName = "RelatedJobNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|036b6ed7-5389-4b75-8af5-49ca908fdcfd", "Origin");
			zTextBoxColumnStyleInfo2.ColumnName = "RelatedJobOrigin";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|6104cfc6-36ae-40e3-842e-d01419ea2cb0", "Destination");
			zTextBoxColumnStyleInfo3.ColumnName = "RelatedJobDestination";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|526dcee9-bd3d-4312-bc9a-da3323ac8698", "Pick Up Agent");
			zTextBoxColumnStyleInfo4.ColumnName = "PickUpAgentCode";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|ae0c405e-57f4-4bab-aab1-488058d8bf05", "Pick Up Agent Name");
			zTextBoxColumnStyleInfo5.ColumnName = "PickUpAgentName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|8f9d1a94-8c0a-4907-9370-05a1a570b8a1", "Delivery Agent");
			zTextBoxColumnStyleInfo6.ColumnName = "DeliveryAgentCode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|87581860-4162-49d8-8e25-584241fb9eb3", "Delivery Agent Code");
			zTextBoxColumnStyleInfo7.ColumnName = "DeliveryAgentName";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|4a6cf923-7486-4e04-9f2c-6c7202b8799e", "Previous Sending Agent");
			zTextBoxColumnStyleInfo8.ColumnName = "PreviousSendingAgentCode";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|c2ee2f14-4da4-4da3-90d1-1fb16cb3f2cc", "Previous Sending Agent Name");
			zTextBoxColumnStyleInfo9.ColumnName = "PreviousSendingAgentName";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|65eefcb8-dd58-4eaa-a49c-56e4223a6ea4", "Debtor");
			zTextBoxColumnStyleInfo10.ColumnName = "DebtorCode";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|b45df8d1-0f81-45eb-ad8c-2ed20ba09af9", "Debtor Name");
			zTextBoxColumnStyleInfo11.ColumnName = "DebtorName";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|1547a702-4ff4-4133-bc9f-b1a063ce17ae", "Debtor Address");
			zTextBoxColumnStyleInfo12.ColumnName = "DebtorAddress";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|183d9bcd-00bc-4568-9ba1-3d91497ed771", "Invoice Target Job");
			zTextBoxColumnStyleInfo13.ColumnName = "TargetJobNum";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|9fa807ba-e8a0-4453-8cc4-1b91d27b2bc3", "Invoice Target's Route");
			zTextBoxColumnStyleInfo14.ColumnName = "TargetJobRoute";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.DetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.DetailsGrid.GridId = "f9d7b05f-890c-4f7c-8884-8950ddc7c13b";
			this.DetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DetailsGrid.LayoutKey = "DetailsGrid";
			this.DetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.DetailsGrid.Name = "DetailsGrid";
			this.DetailsGrid.ShouldSetErrorsOnTabPage = false;
			this.DetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 146, true);
			this.DetailsGrid.TabIndex = 1;
			// 
			// ShipmentAndBillingDetailsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ShipmentAndBillingDetailsForm|43cef3f0-0e8e-4ceb-a840-7f7b93d0be5f", "Shipment and Billing Details");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 206, true);
			this.Controls.Add(this.DetailsGrid);
			this.DataSourceAssemblyName = "Enterprise.Accounting.GUI";
			this.DataSourceType = typeof(ShipmentAndBillingDetails);
			this.DataSourceTypeName = "Enterprise.Accounting.GUI.JobInvoicing.ShipmentAndBillingDetails";
			this.Name = "ShipmentAndBillingDetailsForm";
			this.Controls.SetChildIndex(this.DetailsGrid, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DetailsGrid)).EndInit();
			this.DetailsGrid.ResumeLayout(false);
			this.DetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZDisplayGrid DetailsGrid;

		#endregion

	}
}