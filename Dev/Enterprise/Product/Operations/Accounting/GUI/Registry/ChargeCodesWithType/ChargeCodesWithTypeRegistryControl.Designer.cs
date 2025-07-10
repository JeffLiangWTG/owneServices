using Enterprise.Accounting.Registry.Business;
namespace Enterprise.Accounting.Registry.GUI
{
	partial class ChargeCodesWithTypeRegistryControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ChargeCodesWithTypeGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodesWithTypeGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ChargeCodeWithTypeCollection);
			// 
			// ChargeCodesWithTypeGrid
			// 
			this.ChargeCodesWithTypeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ChargeCodesWithTypeGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ChargeCodeWithType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeCodeWithType)(null)).PartyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ChargeCodeWithType)(null)).UseDefaultProfitShareChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ChargeCodeWithType)(null)).ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeCodeWithType)(null)).ChargeCodeDescription)));
			this.ChargeCodesWithTypeGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodesWithTypeRegistryControl|d8e81af8-568e-4a25-9924-dd5bca517a8b", "Party Type");
			zTextBoxColumnStyleInfo1.ColumnName = "PartyType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodesWithTypeRegistryControl|c53eed2a-fb08-4d58-ae3e-8196aac4da91", "Use Default");
			zCheckBoxColumnStyleInfo1.ColumnName = "UseDefaultProfitShareChargeCode";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodesWithTypeRegistryControl|6643e86f-5e55-4ee0-ac6f-14946244caa1", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ChargeCode";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodesWithTypeRegistryControl|750a21a6-147f-431a-a554-a255614ea7f3", "Charge Code Description");
			zTextBoxColumnStyleInfo2.ColumnName = "ChargeCodeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ChargeCodesWithTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ChargeCodesWithTypeGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ChargeCodesWithTypeGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargeCodesWithTypeGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ChargeCodesWithTypeGrid.GridId = "80d87418-4208-471b-b8bf-809d5b47cc84";
			this.ChargeCodesWithTypeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChargeCodesWithTypeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeCodesWithTypeGrid.LayoutKey = "ChargeCodesWithTypeGrid";
			this.ChargeCodesWithTypeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargeCodesWithTypeGrid.Name = "ChargeCodesWithTypeGrid";
			this.ChargeCodesWithTypeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 191, true);
			this.ChargeCodesWithTypeGrid.TabIndex = 0;
			// 
			// ChargeCodesWithTypeRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ChargeCodesWithTypeGrid);
			this.Name = "ChargeCodesWithTypeRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 191, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodesWithTypeGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid ChargeCodesWithTypeGrid;

	}
}
