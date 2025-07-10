using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Enterprise.Registry.GUI
{
	public partial class CustomsReferenceNumberTypesRegistryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.customsReferenceNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.customsReferenceNumbersGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CustomsReferenceNumberTypeCollection);
			// 
			// customsReferenceNumbersGrid
			// 
			this.customsReferenceNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.customsReferenceNumbersGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CustomsReferenceNumberType)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CustomsReferenceNumberType)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CustomsReferenceNumberType)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CustomsReferenceNumberType)(null)).IsUnique)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CustomsReferenceNumberType)(null)).IsAutomation)));
			this.customsReferenceNumbersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CustomsReferenceNumberTypesRegistryControl|fb209ba6-2f58-4499-b475-b80b616578c2", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CustomsReferenceNumberTypesRegistryControl|c372df90-63d8-4231-aa85-076a8dfb186b", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CustomsReferenceNumberTypesRegistryControl|33b1f8c6-8c15-4851-9cb7-f4fed2048489", "Is Unique");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsUnique";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CustomsReferenceNumberTypesRegistryControl|52CBC710-2A58-4131-97BB-716E1D1DE635", "Edit via UXML only");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsAutomation";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.customsReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.customsReferenceNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.customsReferenceNumbersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.customsReferenceNumbersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.customsReferenceNumbersGrid.GridId = "ec328df8-af57-4427-acf6-b02aa9cf2098";
			this.customsReferenceNumbersGrid.CopySelectedRowsAllowed = true;
			this.customsReferenceNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customsReferenceNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.customsReferenceNumbersGrid.LayoutKey = "customsReferenceNumbersGrid";
			this.customsReferenceNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customsReferenceNumbersGrid.Name = "customsReferenceNumbersGrid";
			this.customsReferenceNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 277, true);
			this.customsReferenceNumbersGrid.TabIndex = 0;
			// 
			// CustomsReferenceNumberTypesRegistryControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.customsReferenceNumbersGrid);
			this.Name = "CustomsReferenceNumberTypesRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 277, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.customsReferenceNumbersGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid customsReferenceNumbersGrid;
	}
}
