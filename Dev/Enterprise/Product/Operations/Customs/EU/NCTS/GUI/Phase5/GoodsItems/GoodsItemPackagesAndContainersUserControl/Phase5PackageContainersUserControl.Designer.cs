namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5PackageContainersUserControl
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
			this.ContainersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SelectedContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectedContainersGrid)).BeginInit();
			this.SelectedContainersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsPackage);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("A8B43D6F-14F9-4051-979F-D9CECCB93535", "Containers");
			this.ContainersGroupBox.Controls.Add(this.SelectedContainersGrid);
			this.ContainersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ContainersGroupBox, false);
			this.ContainersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainersGroupBox.Name = "ContainersGroupBox";
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200, true);
			this.ContainersGroupBox.TabIndex = 0;
			this.ContainersGroupBox.TabStop = false;
			// 
			// SelectedContainersGrid
			// 
			this.SelectedContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SelectedContainersGrid, "ContainersPivotsForBindingOnly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).ContainersPivotsForBindingOnly)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NonPersistentContainerPivotPhase5)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).ContainersPivotsForBindingOnly)).SyncRoot)).ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NonPersistentContainerPivotPhase5)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsPackage)(null)).ContainersPivotsForBindingOnly)).SyncRoot)).ContainerSelected)));
			this.SelectedContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ContainerNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "ContainerSelected";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.SelectedContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SelectedContainersGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.SelectedContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SelectedContainersGrid.GridId = "1eea3a76-d153-47d0-81d8-a88a6d6b1e48";
			this.SelectedContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectedContainersGrid.LayoutKey = "zGrid1";
			this.SelectedContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SelectedContainersGrid.Name = "SelectedContainersGrid";
			this.SelectedContainersGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SelectedContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200, true);
			this.SelectedContainersGrid.TabIndex = 0;
			// 
			// Phase5PackageContainersUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ContainersGroupBox);
			this.Name = "Phase5PackageContainersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ContainersGroupBox.ResumeLayout(false);
			this.ContainersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectedContainersGrid)).EndInit();
			this.SelectedContainersGrid.ResumeLayout(false);
			this.SelectedContainersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected internal ZArchitecture.ZGrid SelectedContainersGrid;
		internal ZArchitecture.GUI.ZGroupBox ContainersGroupBox;
	}
}
