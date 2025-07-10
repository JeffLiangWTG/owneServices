using System.Drawing;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class TagGridFormForTesting : ZForm, IPostingButtonsProvider
	{
		public TagGridFormForTesting(TagLinkCollection tagLinkCollection)
			: base(tagLinkCollection)
		{
			InitializeComponent();
		}

		readonly System.ComponentModel.IContainer Components;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (Components != null))
			{
				Components.Dispose();
			}
			base.Dispose(disposing);
		}

		new void InitializeComponent()
		{
			var zGuidDropEditColumnStyleInfo1 = new ZGuidDropEditColumnStyleInfo();
			var zGuidDropEditColumnStyleInfo2 = new ZGuidDropEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			var zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			var zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			TagGrid = new TagGrid();
			((System.ComponentModel.ISupportInitialize)(BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(TagGrid)).BeginInit();
			TagGrid.SuspendLayout();
			SuspendLayout();
			// 
			// BindingSource
			// 
			BindingSource.DataSourceType = typeof(TagLinkCollection);
			// 
			// TagLinkGroupBox
			// 
			Controls.Add(TagGrid);
			// 
			// tagGrid
			// 
			TagGrid.AllowNavigation = false;
			BindingSource.SetBindingMember(TagGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TagLink)(null)).TagDefinitionPk);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TagLink)(null)).TGL_TGM_Magnitude);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TagLink)(null)).TGL_Description);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TagLink)(null)).TGL_Magnitude);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TagLink)(null)).TGL_SystemCreateTimeUtc);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TagLink)(null)).TGL_SystemCreateUser);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TagLink)(null)).EffectiveNudge);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((TagLink)(null)).TGL_Sequence);

			TagGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "TagDefinitionPk";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidDropEditColumnStyleInfo2.ColumnName = "TGL_TGM_Magnitude";
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "TGL_Description";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TGL_Magnitude";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "TGL_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.IsVisible = false;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "TGL_SystemCreateUser";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "EffectiveNudge";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "TGL_Sequence";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			TagGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			TagGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			TagGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			TagGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			TagGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			TagGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			TagGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			TagGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			TagGrid.CopySelectedRowsAllowed = true;
			TagGrid.Dock = DockStyle.Fill;
			TagGrid.GridId = "8cc34b60-edc2-42a6-a860-335089e30175";
			TagGrid.HeaderForeColor = SystemColors.ControlText;
			TagGrid.LayoutKey = "tagLinkGrid";
			TagGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			TagGrid.Name = "tagLinkGrid";
			TagGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 270, true);
			TagGrid.TabIndex = 0;

			// Facilitates testing with the save dialog
			fApplyButton = new ZButton();
			Controls.Add(fApplyButton as Control);

			((System.ComponentModel.ISupportInitialize)(BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(TagGrid)).EndInit();
			TagGrid.ResumeLayout(false);
			TagGrid.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		public TagGrid TagGrid;
	}
}
