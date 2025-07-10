
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;
using Enterprise.Customs.EU.Business.CusTempStorage;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStorageContainerControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.AdditionalSealsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AdditionalSealsGrid = new Enterprise.ZArchitecture.ZGrid();
            this.containerDataSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
            this.containersGrid = new Enterprise.ZArchitecture.ZGrid();
            this.splitContainerGrid = new CargoWise.Windows.UI.KSplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.AdditionalSealsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).BeginInit();
            this.AdditionalSealsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.containerDataSplitContainer)).BeginInit();
            this.containerDataSplitContainer.Panel1.SuspendLayout();
            this.containerDataSplitContainer.Panel2.SuspendLayout();
            this.containerDataSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.containersGrid)).BeginInit();
            this.containersGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGrid)).BeginInit();
            this.splitContainerGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader);
            // 
            // AdditionalSealsGroupBox
            // 
            this.AdditionalSealsGroupBox.CaptionResourceString = Res.GetData("BADFE87D-B2F7-436E-898F-EEFB9158EE05", "Additional Seals");
            this.AdditionalSealsGroupBox.Controls.Add(this.AdditionalSealsGrid);
            this.AdditionalSealsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdditionalSealsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.AdditionalSealsGroupBox.Name = "AdditionalSealsGroupBox";
            this.AdditionalSealsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 145, true);
            this.AdditionalSealsGroupBox.TabIndex = 1;
            this.AdditionalSealsGroupBox.TabStop = false;
            // 
            // AdditionalSealsGrid
            // 
            this.AdditionalSealsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.AdditionalSealsGrid, "Containers.AdditionalSeals");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Containers)).SyncRoot)).AdditionalSeals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.EU.Business.Declaration.CusSeal)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Containers)).SyncRoot)).AdditionalSeals)).SyncRoot)).BK_SequenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusSeal)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Containers)).SyncRoot)).AdditionalSeals)).SyncRoot)).BK_SealNumber)));
			this.AdditionalSealsGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo.ColumnName = "BK_SequenceNumber";
			zTextBoxColumnStyleInfo.IsReadOnly = true;
			zTextBoxColumnStyleInfo.IsMandatory = true;
            zTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo.CaptionResourceString = Res.GetData("266BB91A-E1A9-49B8-BA18-3BE9532555CC", "Seq.");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "BK_SealNumber";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("A466BB25-960F-4313-8FBE-EDCDC39C714A", "Seal Number");
			this.AdditionalSealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo);
			this.AdditionalSealsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalSealsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdditionalSealsGrid.GridId = "988c43d6-91d5-4bfd-9220-f0988cd8a4c8";
            this.AdditionalSealsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.AdditionalSealsGrid.LayoutKey = "additionalSealsGrid";
            this.AdditionalSealsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.AdditionalSealsGrid.Name = "AdditionalSealsGrid";
            this.AdditionalSealsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(902, 128, true);
            this.AdditionalSealsGrid.TabIndex = 1;
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
            this.containerDataSplitContainer.Panel2.Controls.Add(this.AdditionalSealsGroupBox);
            this.containerDataSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 295, true);
            this.containerDataSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(146);
            this.containerDataSplitContainer.SplitterWidth = 6;
            this.containerDataSplitContainer.TabIndex = 0;
            // 
            // containersGrid
            // 
            this.containersGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.containersGrid, "Containers");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Containers)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Containers)).SyncRoot)).ACN_ContainerNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Containers)).SyncRoot)).ACN_RC_ContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Containers)).SyncRoot)).ACN_EmptyFullIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Containers)).SyncRoot)).ACN_Seal1)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Containers)).SyncRoot)).ACN_Seal2)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageHeader)(null)).Containers)).SyncRoot)).ACN_Seal3)));
            this.containersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "ACN_ContainerNumber";
			zTextBoxColumnStyleInfo2.IsVisible = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
            zGuidFindBoxColumnStyleInfo1.ColumnName = "ACN_RC_ContainerType";
            zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
            zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "ACN_EmptyFullIndicator";
			zDropEditColumnStyleInfo1.IsVisible = true;
			zDropEditColumnStyleInfo1.MaxLengthOverride = 1;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(79);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo3.ColumnName = "ACN_Seal1";
			zTextBoxColumnStyleInfo3.IsVisible = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
            zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo4.ColumnName = "ACN_Seal2";
            zTextBoxColumnStyleInfo4.IsVisible = true;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
            zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zTextBoxColumnStyleInfo5.ColumnName = "ACN_Seal3";
            zTextBoxColumnStyleInfo5.IsVisible = true;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
           	this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.containersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
            this.containersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.containersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.containersGrid.GridId = "96ca5084-a46f-46ad-9713-fb2f83b49ae2";
            this.containersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.containersGrid.LayoutKey = "containersGrid";
            this.containersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.containersGrid.Name = "ContainersGrid";
            this.containersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 146, true);
            this.containersGrid.TabIndex = 0;
            // 
            // splitContainerGrid
            // 
            this.splitContainerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.splitContainerGrid.Name = "splitContainerGrid";
            this.splitContainerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 67, true);
            this.splitContainerGrid.SplitterWidth = 6;
            this.splitContainerGrid.TabIndex = 0;
			// 
			// UCC6TemporaryStorageContainerControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.containerDataSplitContainer);
            this.Name = "UCC6TemporaryStorageContainerControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(906, 295, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.AdditionalSealsGroupBox.ResumeLayout(false);
            this.AdditionalSealsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.AdditionalSealsGrid)).EndInit();
            this.AdditionalSealsGrid.ResumeLayout(false);
            this.AdditionalSealsGrid.PerformLayout();
            this.containerDataSplitContainer.Panel1.ResumeLayout(false);
            this.containerDataSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.containerDataSplitContainer)).EndInit();
            this.containerDataSplitContainer.ResumeLayout(false);
            this.containerDataSplitContainer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.containersGrid)).EndInit();
            this.containersGrid.ResumeLayout(false);
            this.containersGrid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerGrid)).EndInit();
            this.splitContainerGrid.ResumeLayout(false);
            this.splitContainerGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		CargoWise.Windows.UI.KSplitContainer containerDataSplitContainer;
		CargoWise.Windows.UI.KSplitContainer splitContainerGrid;
		ZGrid containersGrid;
		ZGroupBox AdditionalSealsGroupBox;
		ZGrid AdditionalSealsGrid;

		#endregion
	}
}
