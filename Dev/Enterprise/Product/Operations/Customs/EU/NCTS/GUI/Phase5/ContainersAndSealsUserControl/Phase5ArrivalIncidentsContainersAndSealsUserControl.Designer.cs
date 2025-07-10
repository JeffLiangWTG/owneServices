namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5ArrivalIncidentsContainersAndSealsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.SealTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ContainerTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalSealsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.AdditionalSealsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalSealsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ItemNumbersTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ItemNumbersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ItemNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SealTabControl.SuspendLayout();
			this.ContainerTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.ContainersGrid.SuspendLayout();
			this.AdditionalSealsTabControl.SuspendLayout();
			this.AdditionalSealsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).BeginInit();
			this.AdditionalSealsGrid.SuspendLayout();
			this.ItemNumbersTabControl.SuspendLayout();
			this.ItemNumbersTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemNumbersGrid)).BeginInit();
			this.ItemNumbersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.EnRouteIncident);
			// 
			// SealTabControl
			// 
			this.SealTabControl.Controls.Add(this.ContainerTabPage);
			this.SealTabControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.SealTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SealTabControl.Name = "SealTabControl";
			this.SealTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 130, true);
			this.SealTabControl.TabIndex = 2;
			// 
			// ContainerTabPage
			// 
			this.ContainerTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("78274E47-4C47-4077-95CD-2E2B35DC6FBF", "Containers/Equipment");
			this.ContainerTabPage.Controls.Add(this.ContainersGrid);
			this.ContainerTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.ContainerTabPage.Name = "ContainerTabPage";
			this.ContainerTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContainerTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(346, 104, true);
			this.ContainerTabPage.TabIndex = 0;
			this.ContainerTabPage.UseVisualStyleBackColor = true;
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ContainersGrid, "IncidentContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).IncidentContainers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).IncidentContainers)).SyncRoot)).BC_Mode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).IncidentContainers)).SyncRoot)).BC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).IncidentContainers)).SyncRoot)).BC_Seal1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).IncidentContainers)).SyncRoot)).BC_Seal2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).IncidentContainers)).SyncRoot)).TotalSealCount)));
			this.ContainersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "BC_Mode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "BC_ContainerNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(175);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "BC_Seal1";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "BC_Seal2";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TotalSealCount";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.ContainersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContainersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ContainersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersGrid.GridId = "40A0E638-0EEF-4339-8AAD-2D4AA86DF68A";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "zGrid1";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 98, true);
			this.ContainersGrid.TabIndex = 3;
			// 
			// AdditionalSealsTabControl
			// 
			this.AdditionalSealsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AdditionalSealsTabControl.Controls.Add(this.AdditionalSealsTabPage);
			this.AdditionalSealsTabControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.AdditionalSealsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 130, true);
			this.AdditionalSealsTabControl.Name = "AdditionalSealsTabControl";
			this.AdditionalSealsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(217, 90, true);
			this.AdditionalSealsTabControl.TabIndex = 3;
			// 
			// AdditionalSealsTabPage
			// 
			this.AdditionalSealsTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("95B96A32-ABB3-43F0-A4FE-DB151F443297", "Additional Seals");
			this.AdditionalSealsTabPage.Controls.Add(this.AdditionalSealsGrid);
			this.AdditionalSealsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.AdditionalSealsTabPage.Name = "AdditionalSealsTabPage";
			this.AdditionalSealsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AdditionalSealsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 65, true);
			this.AdditionalSealsTabPage.TabIndex = 0;
			this.AdditionalSealsTabPage.UseVisualStyleBackColor = true;
			// 
			// AdditionalSealsGrid
			// 
			this.AdditionalSealsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalSealsGrid, "IncidentContainers.Seals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).IncidentContainers)).SyncRoot)).Seals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CusSeal)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).IncidentContainers)).SyncRoot)).Seals)).SyncRoot)).BK_SealNumber)));
			this.AdditionalSealsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "BK_SealNumber";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			this.AdditionalSealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AdditionalSealsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalSealsGrid.GridId = "51685E7D-0296-44A3-A093-27F9399A4E20";
			this.AdditionalSealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalSealsGrid.LayoutKey = "AdditionalSealsGrid";
			this.AdditionalSealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalSealsGrid.Name = "AdditionalSealsGrid";
			this.AdditionalSealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(202, 59, true);
			this.AdditionalSealsGrid.TabIndex = 1;
			// 
			// ItemNumbersTabControl
			// 
			this.ItemNumbersTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ItemNumbersTabControl.Controls.Add(this.ItemNumbersTabPage);
			this.ItemNumbersTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemNumbersTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(217, 130, true);
			this.ItemNumbersTabControl.Name = "ItemNumbersTabControl";
			this.ItemNumbersTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 90, true);
			this.ItemNumbersTabControl.TabIndex = 4;
			// 
			// ItemNumbersTabPage
			// 
			this.ItemNumbersTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("076854E9-8274-4F9E-B003-FD94EDC976D0", "Item numbers");
			this.ItemNumbersTabPage.Controls.Add(this.ItemNumbersGrid);
			this.ItemNumbersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.ItemNumbersTabPage.Name = "ItemNumbersTabPage";
			this.ItemNumbersTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ItemNumbersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 65, true);
			this.ItemNumbersTabPage.TabIndex = 0;
			this.ItemNumbersTabPage.UseVisualStyleBackColor = true;
			// 
			// ItemNumbersGrid
			// 
			this.ItemNumbersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItemNumbersGrid, "IncidentContainers.ItemNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).IncidentContainers)).SyncRoot)).ItemNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsContainerItem)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.EnRouteIncident)(null)).IncidentContainers)).SyncRoot)).ItemNumbers)).SyncRoot)).CY_DataNumeric)));
			this.ItemNumbersGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CY_DataNumeric";
			zCalcEditColumnStyleInfo2.ShowGroupSeparators = false;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			this.ItemNumbersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ItemNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemNumbersGrid.GridId = "0C3130F2-7752-4A77-850C-BF4083927655";
			this.ItemNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItemNumbersGrid.LayoutKey = "ItemNumbersGrid";
			this.ItemNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ItemNumbersGrid.Name = "ItemNumbersGrid";
			this.ItemNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 59, true);
			this.ItemNumbersGrid.TabIndex = 1;
			// 
			// Phase5ArrivalIncidentsContainersAndSealsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ItemNumbersTabControl);
			this.Controls.Add(this.AdditionalSealsTabControl);
			this.Controls.Add(this.SealTabControl);
			this.Name = "Phase5ArrivalIncidentsContainersAndSealsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 220, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SealTabControl.ResumeLayout(false);
			this.SealTabControl.PerformLayout();
			this.ContainerTabPage.ResumeLayout(false);
			this.ContainerTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ContainersGrid.ResumeLayout(false);
			this.ContainersGrid.PerformLayout();
			this.AdditionalSealsTabControl.ResumeLayout(false);
			this.AdditionalSealsTabControl.PerformLayout();
			this.AdditionalSealsTabPage.ResumeLayout(false);
			this.AdditionalSealsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).EndInit();
			this.AdditionalSealsGrid.ResumeLayout(false);
			this.AdditionalSealsGrid.PerformLayout();
			this.ItemNumbersTabControl.ResumeLayout(false);
			this.ItemNumbersTabControl.PerformLayout();
			this.ItemNumbersTabPage.ResumeLayout(false);
			this.ItemNumbersTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemNumbersGrid)).EndInit();
			this.ItemNumbersGrid.ResumeLayout(false);
			this.ItemNumbersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZTabControl SealTabControl;
		internal ZArchitecture.GUI.ZTabPage ContainerTabPage;
		internal ZArchitecture.ZGrid ContainersGrid;
		internal ZArchitecture.GUI.ZTabControl AdditionalSealsTabControl;
		internal ZArchitecture.GUI.ZTabControl ItemNumbersTabControl;
		internal ZArchitecture.GUI.ZTabPage AdditionalSealsTabPage;
		internal ZArchitecture.GUI.ZTabPage ItemNumbersTabPage;
		internal ZArchitecture.ZGrid AdditionalSealsGrid;
		internal ZArchitecture.ZGrid ItemNumbersGrid;
	}
}
