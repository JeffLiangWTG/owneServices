namespace Enterprise.Customs.JP.Manifest.GUI
{
	partial class JPAsycudaContainerUserControl
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
			this.AdditionalSealsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalSealsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.containerDataSplitContainer)).BeginInit();
			this.containerDataSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalSealsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).BeginInit();
			this.AdditionalSealsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// containerDataSplitContainer
			// 
			this.containerDataSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1168, 245, true);
			this.containerDataSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(122);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader);
			// 
			// AdditionalSealsGroupBox
			// 
			this.AdditionalSealsGroupBox.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("d9af6b8b-1436-43ae-9fe1-d4dfe301f602", "Additional Seals");
			this.AdditionalSealsGroupBox.Controls.Add(this.AdditionalSealsGrid);
			this.AdditionalSealsGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.AdditionalSealsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1007, 0, true);
			this.AdditionalSealsGroupBox.Name = "AdditionalSealsGroupBox";
			this.AdditionalSealsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 245, true);
			this.AdditionalSealsGroupBox.TabIndex = 3;
			this.AdditionalSealsGroupBox.TabStop = false;
			// 
			// AdditionalSealsGrid
			// 
			this.AdditionalSealsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalSealsGrid, "Containers.AdditionalSeals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Manifest.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).AdditionalSeals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.CusSeal)(((System.Collections.IList)(((Enterprise.Customs.JP.Manifest.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).AdditionalSeals)).SyncRoot)).BK_SealNumber)));
			this.AdditionalSealsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "BK_SealNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.AdditionalSealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalSealsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalSealsGrid.GridId = "B1601D57-BAB4-4DA7-B771-646D16ED9075";
			this.AdditionalSealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalSealsGrid.LayoutKey = "AdditionalSealsGrid";
			this.AdditionalSealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.AdditionalSealsGrid.Name = "AdditionalSealsGrid";
			this.AdditionalSealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(158, 230, true);
			this.AdditionalSealsGrid.TabIndex = 0;
			// 
			// JPAsycudaContainerUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AdditionalSealsGroupBox);
			this.Name = "JPAsycudaContainerUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1168, 245, true);
			this.Controls.SetChildIndex(this.AdditionalSealsGroupBox, 0);
			this.Controls.SetChildIndex(this.containerDataSplitContainer, 0);
			((System.ComponentModel.ISupportInitialize)(this.containerDataSplitContainer)).EndInit();
			this.containerDataSplitContainer.ResumeLayout(false);
			this.containerDataSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalSealsGroupBox.ResumeLayout(false);
			this.AdditionalSealsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).EndInit();
			this.AdditionalSealsGrid.ResumeLayout(false);
			this.AdditionalSealsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox AdditionalSealsGroupBox;
		private ZArchitecture.ZGrid AdditionalSealsGrid;
	}
}
