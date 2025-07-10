using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ZeroAmountTaxTypesDescriptionsControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zeroAmountTaxTypesDescriptionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.zeroAmountTaxTypesDescriptionsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.ZeroAmountTaxTypesDescriptionsCollection);
			// 
			// zeroAmountTaxTypesDescriptionsGrid
			// 
			this.zeroAmountTaxTypesDescriptionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zeroAmountTaxTypesDescriptionsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Registry.Business.ZeroAmountTaxTypesDescriptions)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ZeroAmountTaxTypesDescriptions)(null)).TaxType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ZeroAmountTaxTypesDescriptions)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ZeroAmountTaxTypesDescriptions)(null)).EnglishDefaultValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.ZeroAmountTaxTypesDescriptions)(null)).EnglishOverrideValue)));
			this.zeroAmountTaxTypesDescriptionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "TaxType";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zTextBoxColumnStyleInfo3.ColumnName = "EnglishDefaultValue";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ColumnName = "EnglishOverrideValue";
			this.zeroAmountTaxTypesDescriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zeroAmountTaxTypesDescriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zeroAmountTaxTypesDescriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zeroAmountTaxTypesDescriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zeroAmountTaxTypesDescriptionsGrid.CopySelectedRowsAllowed = true;
			this.zeroAmountTaxTypesDescriptionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zeroAmountTaxTypesDescriptionsGrid.GridId = "3df63a55-2cc8-4e3f-87b6-33f64f9e3253";
			this.zeroAmountTaxTypesDescriptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zeroAmountTaxTypesDescriptionsGrid.LayoutKey = "zeroAmountTaxTypesDescriptionsGrid";
			this.zeroAmountTaxTypesDescriptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zeroAmountTaxTypesDescriptionsGrid.Name = "zeroAmountTaxTypesDescriptionsGrid";
			this.zeroAmountTaxTypesDescriptionsGrid.ReadOnly = true;
			this.zeroAmountTaxTypesDescriptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 277, true);
			this.zeroAmountTaxTypesDescriptionsGrid.TabIndex = 0;
			// 
			// ZeroAmountTaxTypesDescriptionsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zeroAmountTaxTypesDescriptionsGrid);
			this.Name = "ZeroAmountTaxTypesDescriptionsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 277, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.zeroAmountTaxTypesDescriptionsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid zeroAmountTaxTypesDescriptionsGrid;
	}
}
