using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI
{
	public partial class ReportUserControl : ZEmbeddedModuleControl
	{
		public ReportGrid ReportGrid;

		private Enterprise.ZArchitecture.ZTextBox zTextBox2;

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ReportGrid = new Enterprise.DocumentEngine.GUI.ReportGrid();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.ReportCommandCollection);
			// 
			// ReportGrid
			// 
			this.ReportGrid.AllowNavigation = false;
			this.ReportGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
									| System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReportGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.ReportCommand)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.ReportCommand)(null)).SU_MenuNameMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.ReportCommand)(null)).SU_IsSystemDefined)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.ReportCommand)(null)).SU_IsClientSpecific)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentEngine.ReportCommand)(null)).SU_IsPublished)));
			this.ReportGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportUserControl|b1f67336-8833-4d5b-8f63-908100aeea78", "Report Name");
			zTextBoxColumnStyleInfo1.ColumnName = "SU_MenuNameMultilingual";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			zCheckBoxColumnStyleInfo1.ColumnName = "SU_IsSystemDefined";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportUserControl|969cd685-3165-49f2-970b-94c97adfb6a8", "Client", "Is this a client specific report?");
			zCheckBoxColumnStyleInfo2.ColumnName = "SU_IsClientSpecific";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportUserControl|e1f93058-d5c4-4b4c-8247-4e8af477d3f4", "Published", "Is this report published for all users to see?");
			zCheckBoxColumnStyleInfo3.ColumnName = "SU_IsPublished";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.ReportGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReportGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ReportGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.ReportGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.ReportGrid.GridId = "39bcb9e3-e75e-46d6-bda6-35de600c3dea";
			this.ReportGrid.IsWholeRowSelectedOnClick = true;
			this.ReportGrid.LayoutKey = "zGrid1";
			this.ReportGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportGrid.Name = "ReportGrid";
			this.ReportGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 200, true);
			this.ReportGrid.TabIndex = 0;
			// 
			// zTextBox2
			// 
			this.zTextBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
									| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.zTextBox2, "SU_HintMultilingual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.ReportCommand)(null)).SU_HintMultilingual)));
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportUserControl|724f5085-f908-4d86-94ec-1a47693abdf6", "Description");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zTextBox2, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 239, true);
			this.zTextBox2.Multiline = true;
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 150, true);
			this.zTextBox2.TabIndex = 1;
			// 
			// ReportUserControl
			// 
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zTextBox2);
			this.Controls.Add(this.ReportGrid);
			this.Name = "ReportUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReportGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
