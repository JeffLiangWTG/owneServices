using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class DraftTransactionStatusReasonCodeControl
	{


		#region Component Designer generated code

		private ZGrid DraftTransactionStatusReasonCodeGrid;

		private void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new ZCheckBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new ZCheckBoxColumnStyleInfo();
			this.DraftTransactionStatusReasonCodeGrid = new ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DraftTransactionStatusReasonCodeGrid)).BeginInit();
			this.DraftTransactionStatusReasonCodeGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(DraftTransactionStatusReasonCodeCollection);
			// 
			// DraftTransactionStatusReasonCodeGrid
			// 
			this.DraftTransactionStatusReasonCodeGrid.AllowNavigation = false;
			this.DraftTransactionStatusReasonCodeGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.DraftTransactionStatusReasonCodeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((DraftTransactionStatusReasonCode)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DraftTransactionStatusReasonCode)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((DraftTransactionStatusReasonCode)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DraftTransactionStatusReasonCode)(null)).ANL)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DraftTransactionStatusReasonCode)(null)).DFT)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DraftTransactionStatusReasonCode)(null)).DSC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DraftTransactionStatusReasonCode)(null)).DIS)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DraftTransactionStatusReasonCode)(null)).AFP)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DraftTransactionStatusReasonCode)(null)).AWA)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((DraftTransactionStatusReasonCode)(null)).PRS)));
			this.DraftTransactionStatusReasonCodeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DraftTransactionStatusReasonCodeControl|574a620a-3ea8-4a86-8f28-f6550397e63d", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DraftTransactionStatusReasonCodeControl|9eef0f1c-334f-43d5-ad71-36e3cdd8b57a", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DraftTransactionStatusReasonCodeControl|ae613585-c15e-4ddd-8ca8-1f365cd8a74f", "ANL");
			zCheckBoxColumnStyleInfo1.ColumnName = "ANL";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DraftTransactionStatusReasonCodeControl|df114785-55fe-45c0-96ea-e51074b98a71", "DFT");
			zCheckBoxColumnStyleInfo2.ColumnName = "DFT";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DraftTransactionStatusReasonCodeControl|cc4bc189-c4bf-4bff-8056-8b713b6af73f", "DSC");
			zCheckBoxColumnStyleInfo3.ColumnName = "DSC";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DraftTransactionStatusReasonCodeControl|bf1dd174-57f4-4067-adc8-b096aaa32813", "DIS");
			zCheckBoxColumnStyleInfo4.ColumnName = "DIS";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DraftTransactionStatusReasonCodeControl|b5536403-569f-4b17-a1e7-773e82c5a286", "AFP");
			zCheckBoxColumnStyleInfo5.ColumnName = "AFP";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DraftTransactionStatusReasonCodeControl|a25a6612-179e-4010-b6bb-958d5716c861", "AWA");
			zCheckBoxColumnStyleInfo6.ColumnName = "AWA";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DraftTransactionStatusReasonCodeControl|a0055f82-f845-4270-a4a4-0b61d2bb2997", "PRS");
			zCheckBoxColumnStyleInfo7.ColumnName = "PRS";
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DraftTransactionStatusReasonCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DraftTransactionStatusReasonCodeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DraftTransactionStatusReasonCodeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DraftTransactionStatusReasonCodeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.DraftTransactionStatusReasonCodeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.DraftTransactionStatusReasonCodeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.DraftTransactionStatusReasonCodeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.DraftTransactionStatusReasonCodeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.DraftTransactionStatusReasonCodeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.DraftTransactionStatusReasonCodeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DraftTransactionStatusReasonCodeGrid.GridId = "0021C1B1-20D3-498D-A42A-849430F5B296";
			this.DraftTransactionStatusReasonCodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DraftTransactionStatusReasonCodeGrid.LayoutKey = "DraftTransactionStatusReasonCodeGrid";
			this.DraftTransactionStatusReasonCodeGrid.LimitedColumns = null;
			this.DraftTransactionStatusReasonCodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DraftTransactionStatusReasonCodeGrid.Name = "DraftTransactionStatusReasonCodeGrid";
			this.DraftTransactionStatusReasonCodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 152, true);
			this.DraftTransactionStatusReasonCodeGrid.TabIndex = 0;
			// 
			// DraftTransactionStatusReasonCodeControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DraftTransactionStatusReasonCodeGrid);
			this.Name = "DraftTransactionStatusReasonCodeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(532, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DraftTransactionStatusReasonCodeGrid)).EndInit();
			this.DraftTransactionStatusReasonCodeGrid.ResumeLayout(false);
			this.DraftTransactionStatusReasonCodeGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
