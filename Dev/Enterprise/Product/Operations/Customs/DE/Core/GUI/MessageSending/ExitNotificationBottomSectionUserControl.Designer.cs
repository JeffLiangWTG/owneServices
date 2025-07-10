namespace Enterprise.Customs.DE.GUI
{
	partial class ExitNotificationBottomSectionUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MissingQuantityCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RedirectionCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IntendedExitCustomsOfficeFindbox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FinalizationCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PackagingGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.IntendedExitCustomsOfficeFindbox.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.PackingGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagingGrid)).BeginInit();
			this.PackagingGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.MissingQuantityCheckBox);
			this.MainPanel.Controls.Add(this.RedirectionCheckbox);
			this.MainPanel.Controls.Add(this.IntendedExitCustomsOfficeFindbox);
			this.MainPanel.Controls.Add(this.FinalizationCheckbox);
			this.MainPanel.Controls.Add(this.ItemsGroupBox);
			this.MainPanel.Controls.Add(this.PackingGroupBox);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 160, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 280, true);
			this.MainPanel.TabIndex = 1;
			// 
			// MissingQuantityCheckBox
			// 
			this.BindingSource.SetBindingMember(this.MissingQuantityCheckBox, "SendingObjectsCollection.MissingQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).MissingQuantity)));
			this.MissingQuantityCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.MissingQuantityCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MissingQuantityCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MissingQuantityCheckBox.Name = "MissingQuantityCheckBox";
			this.MissingQuantityCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 24, true);
			this.MissingQuantityCheckBox.TabIndex = 0;
			this.MissingQuantityCheckBox.UseVisualStyleBackColor = true;
			// 
			// RedirectionCheckbox
			// 
			this.BindingSource.SetBindingMember(this.RedirectionCheckbox, "SendingObjectsCollection.Redirection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Redirection)));
			this.RedirectionCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RedirectionCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RedirectionCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 3, true);
			this.RedirectionCheckbox.Name = "RedirectionCheckbox";
			this.RedirectionCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(137, 24, true);
			this.RedirectionCheckbox.TabIndex = 1;
			this.RedirectionCheckbox.UseVisualStyleBackColor = true;
			// 
			// IntendedExitCustomsOfficeFindbox
			// 
			this.IntendedExitCustomsOfficeFindbox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IntendedExitCustomsOfficeFindbox, "SendingObjectsCollection.IntendedExitCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).IntendedExitCustomsOffice)));
			this.IntendedExitCustomsOfficeFindbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(481, 5, true);
			this.IntendedExitCustomsOfficeFindbox.Name = "IntendedExitCustomsOfficeFindbox";
			this.IntendedExitCustomsOfficeFindbox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.IntendedExitCustomsOfficeFindbox.ParentType = null;
			this.IntendedExitCustomsOfficeFindbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.IntendedExitCustomsOfficeFindbox.TabIndex = 2;
			// 
			// FinalizationCheckbox
			// 
			this.BindingSource.SetBindingMember(this.FinalizationCheckbox, "SendingObjectsCollection.Finalization");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Finalization)));
			this.FinalizationCheckbox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.FinalizationCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FinalizationCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 33, true);
			this.FinalizationCheckbox.Name = "FinalizationCheckbox";
			this.FinalizationCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 24, true);
			this.FinalizationCheckbox.TabIndex = 3;
			this.FinalizationCheckbox.UseVisualStyleBackColor = true;
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ItemsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("8407e797-d351-443a-9b42-5b61a4023bf3", "Items");
			this.ItemsGroupBox.Controls.Add(this.ItemsGrid);
			this.ItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 63, true);
			this.ItemsGroupBox.Name = "ItemsGroupBox";
			this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 101, true);
			this.ItemsGroupBox.TabIndex = 4;
			this.ItemsGroupBox.TabStop = false;
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItemsGrid, "SendingObjectsCollection.Items");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Items)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.ExitNotificationItem)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Items)).SyncRoot)).ShouldSend)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.ExitNotificationItem)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Items)).SyncRoot)).LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.ExitNotificationItem)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Items)).SyncRoot)).GrossMass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.ExitNotificationItem)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Items)).SyncRoot)).Gross)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.DE.Business.ExitNotificationItem)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Items)).SyncRoot)).NetMass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.ExitNotificationItem)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Items)).SyncRoot)).Net)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.ExitNotificationItem)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Items)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.ExitNotificationItem)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Items)).SyncRoot)).StatusDescription)));
			this.ItemsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "ShouldSend";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "LineNo";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "GrossMass";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "Gross";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "NetMass";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "Net";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.ColumnName = "Status";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
			zTextBoxColumnStyleInfo4.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.ItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsGrid.GridId = "4a1f8180-1932-4c9e-8252-14928cceaa12";
			this.ItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItemsGrid.LayoutKey = "ItemsGrid";
			this.ItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ItemsGrid.Name = "ItemsGrid";
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 82, true);
			this.ItemsGrid.TabIndex = 0;
			// 
			// PackingGroupBox
			// 
			this.PackingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PackingGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("74d1bf6b-6700-4905-9fc2-62f3a63db0fb", "Packing Details");
			this.PackingGroupBox.Controls.Add(this.PackagingGrid);
			this.PackingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 167, true);
			this.PackingGroupBox.Name = "PackingGroupBox";
			this.PackingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(718, 110, true);
			this.PackingGroupBox.TabIndex = 5;
			this.PackingGroupBox.TabStop = false;
			// 
			// PackagingGrid
			// 
			this.PackagingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackagingGrid, "SendingObjectsCollection.Packages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Packages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.ExitNotificationPackage)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Packages)).SyncRoot)).ShouldSend)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.DE.Business.ExitNotificationPackage)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Packages)).SyncRoot)).ItemNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.ExitNotificationPackage)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Packages)).SyncRoot)).LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZLong)(((Enterprise.Customs.DE.Business.ExitNotificationPackage)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Packages)).SyncRoot)).PackQTY)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.ExitNotificationPackage)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Packages)).SyncRoot)).PackType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.ExitNotificationPackage)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingAction)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.ExitNotificationMessageSendingActionParent)(null)).SendingObjectsCollection)).SyncRoot)).Packages)).SyncRoot)).MarksNumbers)));
			this.PackagingGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo2.ColumnName = "ShouldSend";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo5.ColumnName = "ItemNo";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(66);
			zTextBoxColumnStyleInfo6.ColumnName = "LineNo";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			zTextBoxColumnStyleInfo7.ColumnName = "PackQTY";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "PackType";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "MarksNumbers";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(109);
			this.PackagingGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.PackagingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PackagingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.PackagingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PackagingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.PackagingGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.PackagingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagingGrid.GridId = "deb51300-62a9-4007-9fc3-865a134dd347";
			this.PackagingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackagingGrid.LayoutKey = "PackagingGrid";
			this.PackagingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.PackagingGrid.Name = "PackagingGrid";
			this.PackagingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 91, true);
			this.PackagingGrid.TabIndex = 0;
			// 
			// ExitNotificationBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 240, true);
			this.Name = "ExitNotificationBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(724, 280, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.IntendedExitCustomsOfficeFindbox.ResumeLayout(true);
			this.IntendedExitCustomsOfficeFindbox.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.PackingGroupBox.ResumeLayout(false);
			this.PackingGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagingGrid)).EndInit();
			this.PackagingGrid.ResumeLayout(false);
			this.PackagingGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZCheckBox MissingQuantityCheckBox;
		private ZArchitecture.GUI.ZCheckBox RedirectionCheckbox;
		private ZArchitecture.GUI.ZCodeFindBox IntendedExitCustomsOfficeFindbox;
		private ZArchitecture.GUI.ZCheckBox FinalizationCheckbox;
		private ZArchitecture.GUI.ZGroupBox ItemsGroupBox;
		private ZArchitecture.GUI.ZGroupBox PackingGroupBox;
		private ZArchitecture.ZGrid ItemsGrid;
		private ZArchitecture.ZGrid PackagingGrid;
	}
}
