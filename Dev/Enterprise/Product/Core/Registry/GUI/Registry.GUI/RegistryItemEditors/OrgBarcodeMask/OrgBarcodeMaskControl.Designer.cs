using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
namespace Enterprise.Registry.GUI
{
	public partial class OrgBarcodeMaskControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.orgBarcodeMaskGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.orgBarcodeMaskGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.OrgBarcodeMaskCollection);
			// 
			// orgBarcodeMaskGrid
			// 
			this.orgBarcodeMaskGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.orgBarcodeMaskGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.OrgBarcodeMask)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.OrgBarcodeMask)(null)).Org)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.OrgBarcodeMask)(null)).Priority)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.OrgBarcodeMask)(null)).Mask)));
			this.orgBarcodeMaskGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Org";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("D4E8CBFA-0F94-4BCD-B9F9-E309FCA42CA2", "Depot");
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "Priority";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Res.GetData("96EDAC2E-2FAC-47E7-BD69-E3F99D31CBBD", "Priority");
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.ColumnName = "Mask";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("DF8D144C-5443-4C1D-8FC8-4A819DF4B403", "Mask");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.orgBarcodeMaskGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.orgBarcodeMaskGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.orgBarcodeMaskGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.orgBarcodeMaskGrid.CopySelectedRowsAllowed = true;
			this.orgBarcodeMaskGrid.GridId = "91a4df1c-24d7-4213-a834-dddfea4a9713";
			this.orgBarcodeMaskGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.orgBarcodeMaskGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.orgBarcodeMaskGrid.LayoutKey = "orgBarcodeMaskGrid";
			this.orgBarcodeMaskGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.orgBarcodeMaskGrid.Name = "orgBarcodeMaskGrid";
			this.orgBarcodeMaskGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			this.orgBarcodeMaskGrid.TabIndex = 0;
			// 
			// OrgBarcodeMaskControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.orgBarcodeMaskGrid);
			this.Name = "OrgBarCodeMaskControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 376, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.orgBarcodeMaskGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
