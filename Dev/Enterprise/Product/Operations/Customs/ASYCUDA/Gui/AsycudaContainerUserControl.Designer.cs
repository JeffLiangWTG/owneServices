using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class AsycudaContainerUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo7 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo8 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo9 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.containerDataSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.containersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.countrySpecificPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.splitContainerGridVersusMessageDetails = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.containerDataSplitContainer)).BeginInit();
			this.containerDataSplitContainer.Panel1.SuspendLayout();
			this.containerDataSplitContainer.Panel2.SuspendLayout();
			this.containerDataSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.containersGrid)).BeginInit();
			this.containersGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerGridVersusMessageDetails)).BeginInit();
			this.splitContainerGridVersusMessageDetails.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader);
			// 
			// containerDataSplitContainer
			// 
			this.containerDataSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containerDataSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.containerDataSplitContainer.Name = "containerDataSplitContainer";
			this.containerDataSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// containerDataSplitContainer.Panel1
			// 
			this.containerDataSplitContainer.Panel1.Controls.Add(this.containersGrid);
			// 
			// containerDataSplitContainer.Panel2
			// 
			this.containerDataSplitContainer.Panel2.Controls.Add(this.countrySpecificPanel);
			this.containerDataSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 295, true);
			this.containerDataSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(147);
			this.containerDataSplitContainer.TabIndex = 0;
			// 
			// containersGrid
			// 
			this.containersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.containersGrid, "Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_EmptyFullIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_RC_ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_Seal1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_SealType1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_SealingPartyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_Seal2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_SealType2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_SealingPartyType2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_Seal3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_SealType3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_SealingPartyType3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_SealingPartyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_NumberOfPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_CommodityCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_GoodsWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_GoodsWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_StowageLocation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_Seal1UnloadingState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_Seal2UnloadingState)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaContainer)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaManifestHeader)(null)).Containers)).SyncRoot)).ACN_Seal3UnloadingState)));
			this.containersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "ACN_ContainerNumber";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "ACN_EmptyFullIndicator";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(74);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ACN_RC_ContainerType";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "ACN_Seal1";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "ACN_SealType1";
			zDropEditColumnStyleInfo2.MaxLengthOverride = 1;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(79);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "ACN_SealingPartyType";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "ACN_Seal2";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "ACN_SealType2";
			zDropEditColumnStyleInfo4.IsVisible = false;
			zDropEditColumnStyleInfo4.MaxLengthOverride = 1;
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(79);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "ACN_SealingPartyType2";
			zDropEditColumnStyleInfo5.IsVisible = false;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "ACN_Seal3";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			zDropEditColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo6.ColumnName = "ACN_SealType3";
			zDropEditColumnStyleInfo6.IsVisible = false;
			zDropEditColumnStyleInfo6.MaxLengthOverride = 1;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(79);
			zDropEditColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo7.ColumnName = "ACN_SealingPartyType3";
			zDropEditColumnStyleInfo7.IsVisible = false;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "ACN_SealingPartyName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "ACN_NumberOfPackages";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo8.ColumnName = "ACN_CommodityCode";
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zTextBoxColumnStyleInfo6.ColumnName = "ACN_GoodsWeight";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			zDropEditColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo9.ColumnName = "ACN_GoodsWeightUQ";
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "ACN_StowageLocation";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "ACN_Seal1UnloadingState";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "ACN_Seal2UnloadingState";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "ACN_Seal3UnloadingState";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.containersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.containersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.containersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.containersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.containersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.containersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.containersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.containersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.containersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.containersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.containersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.containersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containersGrid.GridId = "96ca5084-a46f-46ad-9713-fb2f83b49ae2";
			this.containersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.containersGrid.LayoutKey = "ContainersGrid";
			this.containersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.containersGrid.Name = "containersGrid";
			this.containersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 147, true);
			this.containersGrid.TabIndex = 0;
			// 
			// countrySpecificPanel
			// 
			this.countrySpecificPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.countrySpecificPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.countrySpecificPanel.Name = "countrySpecificPanel";
			this.countrySpecificPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 144, true);
			this.countrySpecificPanel.TabIndex = 0;
			this.countrySpecificPanel.VisibleChanged += new System.EventHandler(this.CountrySpecificPanel_VisibleChanged);
			// 
			// splitContainerGridVersusMessageDetails
			// 
			this.splitContainerGridVersusMessageDetails.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainerGridVersusMessageDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.splitContainerGridVersusMessageDetails.Name = "splitContainerGridVersusMessageDetails";
			this.splitContainerGridVersusMessageDetails.Orientation = System.Windows.Forms.Orientation.Horizontal;
			this.splitContainerGridVersusMessageDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 67, true);
			this.splitContainerGridVersusMessageDetails.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(33);
			this.splitContainerGridVersusMessageDetails.TabIndex = 0;
			// 
			// AsycudaContainerUserControl
			// 
			this.Controls.Add(this.containerDataSplitContainer);
			this.Name = "AsycudaContainerUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 295, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.containerDataSplitContainer.Panel1.ResumeLayout(false);
			this.containerDataSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.containerDataSplitContainer)).EndInit();
			this.containerDataSplitContainer.ResumeLayout(false);
			this.containerDataSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.containersGrid)).EndInit();
			this.containersGrid.ResumeLayout(false);
			this.containersGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainerGridVersusMessageDetails)).EndInit();
			this.splitContainerGridVersusMessageDetails.ResumeLayout(false);
			this.splitContainerGridVersusMessageDetails.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		public CargoWise.Windows.UI.KSplitContainer containerDataSplitContainer;
		CargoWise.Windows.UI.KSplitContainer splitContainerGridVersusMessageDetails;
		protected ZGrid containersGrid;
		ZPanel countrySpecificPanel;

		#endregion
	}
}
