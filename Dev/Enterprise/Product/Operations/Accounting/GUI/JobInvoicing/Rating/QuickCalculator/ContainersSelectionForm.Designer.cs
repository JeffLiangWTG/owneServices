namespace Enterprise.Accounting.GUI.JobInvoicing
{
	partial class ContainersSelectionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;
		internal Enterprise.ZArchitecture.ZGrid ContainersGrid;
		Enterprise.ZArchitecture.GUI.ZButton Button_Cancel;
		Enterprise.ZArchitecture.GUI.ZButton Button_Ok;

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
		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.Button_Cancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Button_Ok = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.ContainersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 357, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.JobInvoicing.ContainerSelectionBusinessObjectCollection);
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.ContainersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContainersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.ContainerSelectionBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.JobInvoicing.ContainerSelectionBusinessObject)(null)).IsSelected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ContainerSelectionBusinessObject)(null)).Number)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobInvoicing.ContainerSelectionBusinessObject)(null)).Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobInvoicing.ContainerSelectionBusinessObject)(null)).Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.ContainerSelectionBusinessObject)(null)).Commodity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobInvoicing.ContainerSelectionBusinessObject)(null)).Quantity)));
			this.ContainersGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainersSelectionForm|b985f37d-a928-49a8-8399-15b877d2421e", "Select");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsSelected";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(32);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainersSelectionForm|997a34dd-ac66-4192-b2f9-42baf5863357", "Container #");
			zTextBoxColumnStyleInfo1.ColumnName = "Number";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainersSelectionForm|6be6957e-cba3-433e-a5ab-ee2c8bc86cd2", "Weight");
			zCalcEditColumnStyleInfo1.ColumnName = "Weight";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainersSelectionForm|8a26dbad-8309-4620-9b37-9397d1f73e52", "Volume");
			zCalcEditColumnStyleInfo2.ColumnName = "Volume";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainersSelectionForm|16895f9a-2005-4446-9cb8-1164983470b2", "Commodity");
			zTextBoxColumnStyleInfo2.ColumnName = "Commodity";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainersSelectionForm|20483e04-0659-4c5b-8a31-4494e0de5b04", "Count");
			zCalcEditColumnStyleInfo3.ColumnName = "Quantity";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ContainersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ContainersGrid.GridId = "7fbd7050-ea55-4360-a298-a2b58f9340a8";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "ContainersGrid";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(521, 323, true);
			this.ContainersGrid.TabIndex = 1;
			// 
			// Button_Cancel
			// 
			this.Button_Cancel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainersSelectionForm|a6568858-3ee2-46fc-a85f-261a94a0e64e", "&Cancel");
			this.Button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Button_Cancel.IsCaptionOverridden = false;
			this.Button_Cancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(362, 331, true);
			this.Button_Cancel.Name = "Button_Cancel";
			this.Button_Cancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Button_Cancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 21, true);
			this.Button_Cancel.TabIndex = 2;
			this.Button_Cancel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.Button_Cancel.ToolTipCaption = null;
			// 
			// Button_Ok
			// 
			this.Button_Ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Button_Ok.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainersSelectionForm|1a4d2999-bf2b-48a0-924e-1222216422d2", "&OK");
			this.Button_Ok.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Button_Ok.IsCaptionOverridden = false;
			this.Button_Ok.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(451, 331, true);
			this.Button_Ok.Name = "Button_Ok";
			this.Button_Ok.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Button_Ok.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.Button_Ok.TabIndex = 3;
			this.Button_Ok.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.Button_Ok.ToolTipCaption = null;
			// 
			// ContainersSelectionForm
			// 
			this.AcceptButton = this.Button_Ok;
			this.CancelButton = this.Button_Cancel;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ContainersSelectionForm|565d4c49-77e4-412a-901a-d3ad22335f0c", "Containers");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(531, 380, true);
			this.Controls.Add(this.Button_Ok);
			this.Controls.Add(this.Button_Cancel);
			this.Controls.Add(this.ContainersGrid);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.JobInvoicing.ContainerSelectionBusinessObjectCollection);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ContainersSelectionForm";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Controls.SetChildIndex(this.ContainersGrid, 0);
			this.Controls.SetChildIndex(this.Button_Cancel, 0);
			this.Controls.SetChildIndex(this.Button_Ok, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ContainersGrid.ResumeLayout(false);
			this.ContainersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
