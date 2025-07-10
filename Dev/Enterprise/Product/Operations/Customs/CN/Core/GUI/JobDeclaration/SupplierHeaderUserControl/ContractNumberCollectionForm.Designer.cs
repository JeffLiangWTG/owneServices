namespace Enterprise.Customs.CN.GUI
{
	public partial class ContractNumberCollectionForm
	{
		#region Component Designer generated code

		Enterprise.ZArchitecture.ZGrid ContractNumbersGrid;
		Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		Enterprise.ZArchitecture.GUI.ZButton OKButton;
		System.ComponentModel.Container components = null;

		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 =
				new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ContractNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContractNumbersGrid)).BeginInit();
			this.ContractNumbersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 234, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType =
				typeof(Enterprise.Customs.CN.Business.JobComInvoiceHeaderContractCollection);
			// 
			// ContractNumbersGrid
			// 
			this.ContractNumbersGrid.AllowNavigation = false;
			this.ContractNumbersGrid.AllowSorting = false;
			this.ContractNumbersGrid.Anchor =
				((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top |
				                                        System.Windows.Forms.AnchorStyles.Bottom)
				                                       | System.Windows.Forms.AnchorStyles.Left)
				                                      | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContractNumbersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(
				((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobComInvoiceHeaderContract)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(
				((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.JobComInvoiceHeaderContract)(null))
					.J2_ReferenceNumber)));
			this.ContractNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "J2_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.ContractNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContractNumbersGrid.GridId = "3EF6825F-98F0-43E6-875F-10E6F50E56C7";
			this.ContractNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContractNumbersGrid.LayoutKey = "ContractNumbersGrid";
			this.ContractNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContractNumbersGrid.Name = "ContractNumbersGrid";
			this.ContractNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 206, true);
			this.ContractNumbersGrid.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom |
			                                                                System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString =
				Enterprise.Customs.CN.GUI.Res.GetData("B9E3DF23-54A6-4743-BA09-3EA66CE78863", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 211, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.OnCloseButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom |
			                                                             System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString =
				Enterprise.Customs.CN.GUI.Res.GetData("BAAA88E6-3BA3-4AB1-AEE9-D91DB6269ECE", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 211, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OnOKButton_Click);
			// 
			// ContractNumberCollectionForm
			// 
			this.AcceptButton = this.OKButton;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString =
				Enterprise.Customs.CN.GUI.Res.GetData("B37325D1-A8E0-4332-802D-1478AB303CDD", "Contract Numbers");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 257, true);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ContractNumbersGrid);
			this.DataSourceAssemblyName = "Enterprise.Customs.CN.Business";
			this.DataSourceType = typeof(Enterprise.Customs.CN.Business.JobComInvoiceHeaderContractCollection);
			this.DataSourceTypeName = "Enterprise.Customs.CN.Business.JobComInvoiceHeaderContractCollection";
			this.Name = "ContractNumberCollectionForm";
			this.Controls.SetChildIndex(this.ContractNumbersGrid, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContractNumbersGrid)).EndInit();
			this.ContractNumbersGrid.ResumeLayout(false);
			this.ContractNumbersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
