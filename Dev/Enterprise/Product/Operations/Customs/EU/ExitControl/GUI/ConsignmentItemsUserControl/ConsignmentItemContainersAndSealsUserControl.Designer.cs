using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ConsignmentItemContainersAndSealsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ContainersAndSealsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ContainersAndSealsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainersAndSealsGrid)).BeginInit();
			this.ContainersAndSealsGrid.SuspendLayout();
			this.ContainersAndSealsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitConsignmentPivotCollection<CusExitConsignmentPivot>);
			// 
			// ContainersAndSealsGrid
			// 
			this.ContainersAndSealsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersAndSealsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitConsignmentPivot)(null)).Container.CXN_ContainerNumber)));
			this.ContainersAndSealsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Container+CXN_ContainerNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.ContainersAndSealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersAndSealsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersAndSealsGrid.GridId = "B28630A4-E6C7-4123-A66E-5A0302298184";
			this.ContainersAndSealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersAndSealsGrid.LayoutKey = "ContainersAndSealsGrid";
			this.ContainersAndSealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ContainersAndSealsGrid.Name = "ContainersAndSealsGrid";
			this.ContainersAndSealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 141, true);
			this.ContainersAndSealsGrid.TabIndex = 0;
			// 
			// ContainersAndSealsGroupBox
			// 
			this.ContainersAndSealsGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("3A018A76-5B3B-49CB-B8DD-6FB74452D7B2", "Containers and Seals");
			this.ContainersAndSealsGroupBox.Controls.Add(this.ContainersAndSealsGrid);
			this.ContainersAndSealsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersAndSealsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersAndSealsGroupBox.Name = "ContainersAndSealsGroupBox";
			this.ContainersAndSealsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 160, true);
			this.ContainersAndSealsGroupBox.TabIndex = 0;
			this.ContainersAndSealsGroupBox.TabStop = false;
			// 
			// ConsignmentItemContainersAndSealsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainersAndSealsGroupBox);
			this.Name = "ConsignmentItemContainersAndSealsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ContainersAndSealsGrid)).EndInit();
			this.ContainersAndSealsGrid.ResumeLayout(false);
			this.ContainersAndSealsGrid.PerformLayout();
			this.ContainersAndSealsGroupBox.ResumeLayout(false);
			this.ContainersAndSealsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid ContainersAndSealsGrid;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox ContainersAndSealsGroupBox;
	}
}

