using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class InterchangeSenderProxyUserControl : RegistryZUserControl
	{
		ZGroupBox interchangeProxyGroupBox;
		Enterprise.ZArchitecture.ZGrid ParentGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.ParentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.interchangeProxyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).BeginInit();
			this.ParentGrid.SuspendLayout();
			this.interchangeProxyGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.InterchangeSenderProxyUserCollection);
			// 
			// ParentGrid
			// 
			this.ParentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ParentGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.InterchangeSenderProxyUser)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.InterchangeSenderProxyUser)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.InterchangeSenderProxyUser)(null)).DescriptionValue)));
			this.ParentGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "DescriptionValue";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.ParentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ParentGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.ParentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ParentGrid.GridId = "89db08b3-5143-4cfe-9e00-e0d758b0ed74";
			this.ParentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ParentGrid.LayoutKey = "zGrid1";
			this.ParentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ParentGrid.Name = "ParentGrid";
			this.ParentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 301, true);
			this.ParentGrid.TabIndex = 0;
			// 
			// interchangeProxyGroupBox
			// 
			this.interchangeProxyGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4a4eff97-8ed0-4ded-806e-ab1014616223", "Interchange Proxy Users");
			this.interchangeProxyGroupBox.Controls.Add(this.ParentGrid);
			this.interchangeProxyGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.interchangeProxyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.interchangeProxyGroupBox.Name = "interchangeProxyGroupBox";
			this.interchangeProxyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 320, true);
			this.interchangeProxyGroupBox.TabIndex = 1;
			this.interchangeProxyGroupBox.TabStop = false;
			// 
			// InterchangeSenderProxyUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.interchangeProxyGroupBox);
			this.Name = "InterchangeSenderProxyUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 320, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ParentGrid)).EndInit();
			this.ParentGrid.ResumeLayout(false);
			this.ParentGrid.PerformLayout();
			this.interchangeProxyGroupBox.ResumeLayout(false);
			this.interchangeProxyGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
