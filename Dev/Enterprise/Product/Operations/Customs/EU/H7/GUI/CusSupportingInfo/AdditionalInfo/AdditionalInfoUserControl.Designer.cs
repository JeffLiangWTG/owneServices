using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class AdditionalInfoUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AdditionalInfosGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalInfosGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalInfosPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfosGrid)).BeginInit();
			this.AdditionalInfosGrid.SuspendLayout();
			this.AdditionalInfosGroupBox.SuspendLayout();
			this.AdditionalInfosPanel.SuspendLayout();
			this.DetailsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// AdditionalInfosGrid
			// 
			this.AdditionalInfosGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalInfosGrid, "AdditionalInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).AdditionalInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).AdditionalInfos)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).AdditionalInfos)).SyncRoot)).CSI_Description)));
			this.AdditionalInfosGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(340);
			this.AdditionalInfosGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalInfosGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfosGrid.GridId = "a4b169b8-f4be-4e0d-a655-587b8f439242";
			this.AdditionalInfosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalInfosGrid.LayoutKey = "AdditionalInfosGrid";
			this.AdditionalInfosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfosGrid.Name = "AdditionalInfosGrid";
			this.AdditionalInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 150, true);
			this.AdditionalInfosGrid.TabIndex = 0;
			// 
			// AdditionalInfosGroupBox
			// 
			this.AdditionalInfosGroupBox.CaptionResourceString = Res.GetData("2d9d4d5d-fc19-40d5-97bb-1bf82202014c", "Additional Info");
			this.AdditionalInfosGroupBox.Controls.Add(this.DetailsPanel);
			this.AdditionalInfosGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfosGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfosGroupBox.Name = "AdditionalInfosGroupBox";
			this.AdditionalInfosGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 221, true);
			this.AdditionalInfosGroupBox.TabIndex = 1;
			this.AdditionalInfosGroupBox.TabStop = false;
			// 
			// AdditionalInfosPanel
			// 
			this.AdditionalInfosPanel.Controls.Add(this.AdditionalInfosGroupBox);
			this.AdditionalInfosPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AdditionalInfosPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 370, true);
			this.AdditionalInfosPanel.Name = "AdditionalInfosPanel";
			this.AdditionalInfosPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 181, true);
			this.AdditionalInfosPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 181);
			this.AdditionalInfosPanel.TabIndex = 2;
			// 
			// DetailsPanel
			// 
			this.DetailsPanel.AllowDrop = true;
			this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsPanel.Name = "DetailsPanel";
			this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 221, true);
			this.DetailsPanel.TabIndex = 3;
			this.DetailsPanel.CaptionRenderingEnabled = true;
			// 
			// AdditionalInfoUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalInfosGrid);
			this.Controls.Add(this.AdditionalInfosPanel);
			this.Name = "AdditionalInfoUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(710, 490, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfosGrid)).EndInit();
			this.AdditionalInfosGrid.ResumeLayout(false);
			this.AdditionalInfosGrid.PerformLayout();
			this.AdditionalInfosGroupBox.ResumeLayout(false);
			this.AdditionalInfosGroupBox.PerformLayout();
			this.AdditionalInfosPanel.ResumeLayout(false);
			this.AdditionalInfosPanel.PerformLayout();
			this.DetailsPanel.ResumeLayout(true);
			this.DetailsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZGroupBox AdditionalInfosGroupBox;
		protected Enterprise.ZArchitecture.GUI.ZPanel AdditionalInfosPanel;
		protected Enterprise.ZArchitecture.ZGrid AdditionalInfosGrid;
		protected Enterprise.ZArchitecture.GUI.DynamicLayoutPanel DetailsPanel;
	}
}

