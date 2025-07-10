using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	partial class ContainerSpecificUserControl
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
            this.components = new System.ComponentModel.Container();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            this.SealTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
            this.AdditionalSealsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.AdditionalSealsGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SealTabControl.SuspendLayout();
            this.AdditionalSealsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).BeginInit();
            this.AdditionalSealsGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Manifest.Business.AsycudaContainer);
            // 
            // SealTabControl
            // 
            this.SealTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.SealTabControl.Controls.Add(this.AdditionalSealsTabPage);
            this.SealTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SealTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SealTabControl.Name = "SealTabControl";
            this.SealTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 205, true);
            this.SealTabControl.TabIndex = 0;
            // 
            // AdditionalSealsTabPage
            // 
            this.AdditionalSealsTabPage.CaptionResourceString = Enterprise.Customs.IL.Manifest.GUI.Res.GetData("93EA636C-0DD1-49E5-8A61-0A695F449F83", "Additional Seals");
            this.AdditionalSealsTabPage.Controls.Add(this.AdditionalSealsGrid);
            this.AdditionalSealsTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdditionalSealsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.AdditionalSealsTabPage.Name = "AdditionalSealsTabPage";
            this.AdditionalSealsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.AdditionalSealsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 178, true);
            this.AdditionalSealsTabPage.TabIndex = 0;
            this.AdditionalSealsTabPage.UseVisualStyleBackColor = true;
            // 
            // AdditionalSealsGrid
            // 
            this.AdditionalSealsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.AdditionalSealsGrid, "AdditionalSeals");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaContainer)(null)).AdditionalSeals)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.CusSeal)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaContainer)(null)).AdditionalSeals)).SyncRoot)).BK_SealNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.CusSeal)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaContainer)(null)).AdditionalSeals)).SyncRoot)).BK_SealType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.CusSeal)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaContainer)(null)).AdditionalSeals)).SyncRoot)).BK_UnloadingState)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.CusSeal)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaContainer)(null)).AdditionalSeals)).SyncRoot)).BK_SealingPartyType)));
            this.AdditionalSealsGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "BK_SealNumber";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zDropEditColumnStyleInfo1.ColumnName = "BK_SealType";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zDropEditColumnStyleInfo2.ColumnName = "BK_UnloadingState";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 2;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zDropEditColumnStyleInfo3.ColumnName = "BK_SealingPartyType";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.AdditionalSealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.AdditionalSealsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.AdditionalSealsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.AdditionalSealsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.AdditionalSealsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdditionalSealsGrid.GridId = "e09dbc77-7c4e-4dc0-b8dc-500e771fbe24";
            this.AdditionalSealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.AdditionalSealsGrid.LayoutKey = "AdditionalSealsGrid";
            this.AdditionalSealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.AdditionalSealsGrid.Name = "AdditionalSealsGrid";
            this.AdditionalSealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(443, 172, true);
            this.AdditionalSealsGrid.TabIndex = 0;
            // 
            // ContainerSpecificUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.SealTabControl);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 205, true);
            this.Name = "ContainerSpecificUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 205, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.SealTabControl.ResumeLayout(false);
            this.SealTabControl.PerformLayout();
            this.AdditionalSealsTabPage.ResumeLayout(false);
            this.AdditionalSealsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).EndInit();
            this.AdditionalSealsGrid.ResumeLayout(false);
            this.AdditionalSealsGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZTabControl SealTabControl;
		private ZTabPage AdditionalSealsTabPage;
		internal ZGrid AdditionalSealsGrid;
	}
}
