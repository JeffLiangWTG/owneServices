namespace Enterprise.Customs.EU.GUI
{
	partial class CustomsCusContainersWithTrackingAndAdditionalSealUserControl
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
			this.containersUserControl1.SuspendLayout();
			this.BaseAllPanel.SuspendLayout();
			this.BaseContainerPanel.SuspendLayout();
			this.ContainersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).BeginInit();
			this.CusContainersBoundGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalSealsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).BeginInit();
			this.AdditionalSealsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BaseContainerPanel
			// 
			this.BaseContainerPanel.Controls.Add(this.AdditionalSealsGroupBox);
			this.BaseContainerPanel.Controls.SetChildIndex(this.AdditionalSealsGroupBox, 0);
			this.BaseContainerPanel.Controls.SetChildIndex(this.ContainersGroupBox, 0);
			// 
			// ContainersGroupBox
			// 
			this.ContainersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 140, true);
			// 
			// CusContainersBoundGrid
			// 
			// 
			// 
			// 
			this.CusContainersBoundGrid.InnerGrid.AllowNavigation = false;
			this.CusContainersBoundGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CusContainersBoundGrid.InnerGrid.CaptionVisible = false;
			this.CusContainersBoundGrid.InnerGrid.GridId = "GridLayoutV6/wYdjdj5Cz4Nc7hGpbKA==";
			this.CusContainersBoundGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CusContainersBoundGrid.InnerGrid.LayoutKey = "CusContainersBoundGrid";
			this.CusContainersBoundGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CusContainersBoundGrid.InnerGrid.Name = "Grid";
			this.CusContainersBoundGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 83, true);
			this.CusContainersBoundGrid.InnerGrid.TabIndex = 0;
			this.CusContainersBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(666, 121, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// AdditionalSealsGroupBox
			// 
			this.AdditionalSealsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9de58f98-7210-4506-9887-096ca102369f", "Additional Seals");
			this.AdditionalSealsGroupBox.Controls.Add(this.AdditionalSealsGrid);
			this.AdditionalSealsGroupBox.Dock = System.Windows.Forms.DockStyle.Right;
			this.AdditionalSealsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(672, 0, true);
			this.AdditionalSealsGroupBox.Name = "AdditionalSealsGroupBox";
			this.AdditionalSealsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 140, true);
			this.AdditionalSealsGroupBox.TabIndex = 1;
			this.AdditionalSealsGroupBox.TabStop = false;
			// 
			// AdditionalSealsGrid
			// 
			this.AdditionalSealsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalSealsGrid, "CusContainers.AdditionalSeals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CusContainers)).SyncRoot)).AdditionalSeals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusSeal)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CusContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CusContainers)).SyncRoot)).AdditionalSeals)).SyncRoot)).BK_SealNumber)));
			this.AdditionalSealsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "BK_SealNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.AdditionalSealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalSealsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalSealsGrid.GridId = "530381fe-4e15-4ac7-959d-8c5ff76abc58";
			this.AdditionalSealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalSealsGrid.LayoutKey = "listBox1";
			this.AdditionalSealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AdditionalSealsGrid.Name = "AdditionalSealsGrid";
			this.AdditionalSealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 121, true);
			this.AdditionalSealsGrid.TabIndex = 0;
			// 
			// CustomsCusContainersWithTrackingAndAdditionalSealUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "CustomsCusContainersWithTrackingAndAdditionalSealUserControl";
			this.containersUserControl1.ResumeLayout(true);
			this.containersUserControl1.PerformLayout();
			this.BaseAllPanel.ResumeLayout(false);
			this.BaseAllPanel.PerformLayout();
			this.BaseContainerPanel.ResumeLayout(false);
			this.BaseContainerPanel.PerformLayout();
			this.ContainersGroupBox.ResumeLayout(false);
			this.ContainersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusContainersBoundGrid.InnerGrid)).EndInit();
			this.CusContainersBoundGrid.ResumeLayout(true);
			this.CusContainersBoundGrid.PerformLayout();
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

		Enterprise.ZArchitecture.GUI.ZGroupBox AdditionalSealsGroupBox;
		Enterprise.ZArchitecture.ZGrid AdditionalSealsGrid;
	}
}
