using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class CodeSelectionCollectionControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.codeSelectionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.codeSelectionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CodeSelectionCollection);
			// 
			// codeSelectionGrid
			// 
			this.codeSelectionGrid.AllowNavigation = false;
			this.codeSelectionGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.codeSelectionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeSelection)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeSelection)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeSelection)(null)).Codes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeSelection)(null)).Description)));
			this.codeSelectionGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Codes";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CodeSelectionCollectionControl|b8e661d0-e0da-4aca-9d02-a7e67a5a61a8", "Code");
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CodeSelectionCollectionControl|07209d1b-f6f3-4647-84ab-e74f5a8bc322", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.codeSelectionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.codeSelectionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.codeSelectionGrid.GridId = "55bcf35d-842f-4bcc-8e3a-26fcfcda7bcb";
			this.codeSelectionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.codeSelectionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.codeSelectionGrid.LayoutKey = "codeSelectionGrid";
			this.codeSelectionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.codeSelectionGrid.Name = "codeSelectionGrid";
			this.codeSelectionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 266, true);
			this.codeSelectionGrid.TabIndex = 0;
			// 
			// CodeSelectionCollectionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.codeSelectionGrid);
			this.Name = "CodeSelectionCollectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 266, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.codeSelectionGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid codeSelectionGrid;
	}
}
