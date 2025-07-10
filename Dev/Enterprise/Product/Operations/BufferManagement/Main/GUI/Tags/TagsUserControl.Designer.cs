namespace Enterprise.BufferManagement.GUI
{
	partial class TagsUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.TagLinkGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.tagLinkGrid = new Enterprise.BufferManagement.GUI.TagGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TagLinkGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.tagLinkGrid)).BeginInit();
			this.tagLinkGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.TagLinkCollection);
			// 
			// TagLinkGroupBox
			// 
			this.TagLinkGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("d76748bd-f870-4ca5-a3aa-36ff69b99219", "Tags");
			this.TagLinkGroupBox.Controls.Add(this.tagLinkGrid);
			this.TagLinkGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TagLinkGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TagLinkGroupBox.Name = "TagLinkGroupBox";
			this.TagLinkGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(374, 289, true);
			this.TagLinkGroupBox.TabIndex = 0;
			this.TagLinkGroupBox.TabStop = false;
			// 
			// tagLinkGrid
			// 
			this.tagLinkGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.tagLinkGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.Business.TagLink)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.TagLink)(null)).TagDefinitionPk)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.TagLink)(null)).TGL_TGM_Magnitude)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.TagLink)(null)).TGL_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.TagLink)(null)).TGL_Magnitude)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.BufferManagement.Business.TagLink)(null)).TGL_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.TagLink)(null)).TGL_SystemCreateUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.TagLink)(null)).EffectiveNudge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.TagLink)(null)).TGL_Sequence)));
			this.tagLinkGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "TagDefinitionPk";
			zGuidDropEditColumnStyleInfo1.MaxLengthOverride = 3;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo2.ColumnName = "TGL_TGM_Magnitude";
			zGuidDropEditColumnStyleInfo2.MaxLengthOverride = 3;
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
			this.tagLinkGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.tagLinkGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.tagLinkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.tagLinkGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.tagLinkGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.tagLinkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.tagLinkGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.tagLinkGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.tagLinkGrid.CopySelectedRowsAllowed = true;
			this.tagLinkGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tagLinkGrid.GridId = "8cc34b60-edc2-42a6-a860-335089e30175";
			this.tagLinkGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.tagLinkGrid.LayoutKey = "tagLinkGrid";
			this.tagLinkGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.tagLinkGrid.Name = "tagLinkGrid";
			this.tagLinkGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 270, true);
			this.tagLinkGrid.TabIndex = 0;
			// 
			// TagsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TagLinkGroupBox);
			this.Name = "TagsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(374, 289, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TagLinkGroupBox.ResumeLayout(false);
			this.TagLinkGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.tagLinkGrid)).EndInit();
			this.tagLinkGrid.ResumeLayout(false);
			this.tagLinkGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox TagLinkGroupBox;
		private TagGrid tagLinkGrid;

	}
}
