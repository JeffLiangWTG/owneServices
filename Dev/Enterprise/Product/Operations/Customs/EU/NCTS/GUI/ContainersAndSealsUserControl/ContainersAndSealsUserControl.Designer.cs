
namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class ContainersAndSealsUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SealQtyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SealTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ContainerTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackageTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PackageSealGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SealTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SealTabControl.SuspendLayout();
			this.ContainerTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).BeginInit();
			this.ContainersGrid.SuspendLayout();
			this.PackageTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackageSealGrid)).BeginInit();
			this.PackageSealGrid.SuspendLayout();
			this.SealTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// SealQtyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SealQtyCalcEdit, "MovementHeader.BM_SealQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_SealQty)));
			this.SealQtyCalcEdit.CaptionResourceString = null;
			this.SealQtyCalcEdit.DecimalPlaces = 0;
			this.SealQtyCalcEdit.Decimals = 0;
			this.SealQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 6, true);
			this.SealQtyCalcEdit.Name = "SealQtyCalcEdit";
			this.SealQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 20, true);
			this.SealQtyCalcEdit.TabIndex = 2;
			this.SealQtyCalcEdit.Text = "0";
			this.SealQtyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SealTabControl
			// 
			this.SealTabControl.Controls.Add(this.ContainerTabPage);
			this.SealTabControl.Controls.Add(this.PackageTabPage);
			this.SealTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 33, true);
			this.SealTabControl.Name = "SealTabControl";
			this.SealTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 85, true);
			this.SealTabControl.TabIndex = 3;
			// 
			// ContainerTabPage
			// 
			this.ContainerTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("1a1a58f8-6920-48d7-b7f9-a7a46cc54d13", "Containers");
			this.ContainerTabPage.Controls.Add(this.ContainersGrid);
			this.ContainerTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ContainerTabPage.Name = "ContainerTabPage";
			this.ContainerTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ContainerTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 58, true);
			this.ContainerTabPage.TabIndex = 0;
			this.ContainerTabPage.UseVisualStyleBackColor = true;
			// 
			// ContainersGrid
			// 
			this.ContainersGrid.AllowNavigation = false;
			this.ContainersGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ContainersGrid, "DepartureHeaderContainers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).DepartureHeaderContainers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureHeaderContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).DepartureHeaderContainers)).SyncRoot)).BC_ContainerNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureHeaderContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).DepartureHeaderContainers)).SyncRoot)).BC_Seal1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureHeaderContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).DepartureHeaderContainers)).SyncRoot)).BC_Seal2)));
			this.ContainersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "BC_ContainerNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "BC_Seal1";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "BC_Seal2";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ContainersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ContainersGrid.GridId = "1eea3a76-d153-47d0-81d8-a88a6d6b1e48";
			this.ContainersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ContainersGrid.LayoutKey = "zGrid1";
			this.ContainersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ContainersGrid.Name = "ContainersGrid";
			this.ContainersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 55, true);
			this.ContainersGrid.TabIndex = 3;
			// 
			// PackageTabPage
			// 
			this.PackageTabPage.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("96b93898-55e2-4457-b1c5-fa5cbb8b84cb", "Package Seals");
			this.PackageTabPage.Controls.Add(this.PackageSealGrid);
			this.PackageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PackageTabPage.Name = "PackageTabPage";
			this.PackageTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 58, true);
			this.PackageTabPage.TabIndex = 1;
			this.PackageTabPage.UseVisualStyleBackColor = true;
			// 
			// PackageSealGrid
			// 
			this.PackageSealGrid.AllowNavigation = false;
			this.PackageSealGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PackageSealGrid, "Seals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Seals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Seal)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Seals)).SyncRoot)).CY_Data)));
			this.PackageSealGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			this.PackageSealGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PackageSealGrid.GridId = "c02a6408-0622-4e2c-8702-e52e5cb3cfc6";
			this.PackageSealGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackageSealGrid.LayoutKey = "PackageSealGrid";
			this.PackageSealGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PackageSealGrid.Name = "PackageSealGrid";
			this.PackageSealGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 59, true);
			this.PackageSealGrid.TabIndex = 0;
			// 
			// SealTypeDropEdit
			// 
			this.SealTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SealTypeDropEdit, "MovementHeader.BM_SealType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_SealType)));
			this.SealTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 6, true);
			this.SealTypeDropEdit.Name = "SealTypeDropEdit";
			this.SealTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.SealTypeDropEdit.TabIndex = 1;
			// 
			// ContainersAndSealsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SealQtyCalcEdit);
			this.Controls.Add(this.SealTabControl);
			this.Controls.Add(this.SealTypeDropEdit);
			this.Name = "ContainersAndSealsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 124, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SealTabControl.ResumeLayout(false);
			this.SealTabControl.PerformLayout();
			this.ContainerTabPage.ResumeLayout(false);
			this.ContainerTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ContainersGrid)).EndInit();
			this.ContainersGrid.ResumeLayout(false);
			this.ContainersGrid.PerformLayout();
			this.PackageTabPage.ResumeLayout(false);
			this.PackageTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackageSealGrid)).EndInit();
			this.PackageSealGrid.ResumeLayout(false);
			this.PackageSealGrid.PerformLayout();
			this.SealTypeDropEdit.ResumeLayout(true);
			this.SealTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit SealQtyCalcEdit;
		internal ZArchitecture.GUI.ZTabControl SealTabControl;
		internal ZArchitecture.GUI.ZTabPage ContainerTabPage;
		protected internal ZArchitecture.ZGrid ContainersGrid;
		internal ZArchitecture.GUI.ZTabPage PackageTabPage;
		internal ZArchitecture.ZGrid PackageSealGrid;
		internal ZArchitecture.GUI.ZDropEdit SealTypeDropEdit;
	}
}
