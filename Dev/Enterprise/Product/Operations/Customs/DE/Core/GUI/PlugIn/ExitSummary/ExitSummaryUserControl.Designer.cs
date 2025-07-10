using Enterprise.Customs.EU.GUI.PlugIn;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	partial class ExitSummaryUserControl
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
			this.SplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AdditionalDocumentsUserControl = new Enterprise.Customs.EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DeclarantDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.RepresentativeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.HeaderLoadingPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LoadingPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReferenceNumberUCRTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RegistrationNumberAWBTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MovementGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MovementsGrid)).BeginInit();
			this.MovementsGrid.SuspendLayout();
			this.CustomsOfficeCodeFindBox.SuspendLayout();
			this.ArrivalNotificationDateDateEdit.SuspendLayout();
			this.ExitDateDateEdit.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.CarrierOrgAddressControl.SuspendLayout();
			this.HeaderCustomsOfficeCodeFindBox.SuspendLayout();
			this.HeaderArrivalNotificationDateDateEdit.SuspendLayout();
			this.HeaderExitDateDateEdit.SuspendLayout();
			this.MessagesGroupBox.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingGrid)).BeginInit();
			this.PackingGrid.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.MovementDetailsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.AdditionalDocumentsTabPage.SuspendLayout();
			this.AdditionalDocumentsUserControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.DeclarantDocAddressControl.SuspendLayout();
			this.RepresentativeDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MovementGroupBox
			// 
			this.MovementGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MovementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 171, true);
			this.MovementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 159, true);
			// 
			// MovementsGrid
			// 
			this.MovementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MovementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1294, 140, true);
			// 
			// ArrivalNotificationDateDateEdit
			// 
			this.ArrivalNotificationDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			// 
			// ExitDateDateEdit
			// 
			this.ExitDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			// 
			// CarrierOrgAddressControl
			// 
			this.CarrierOrgAddressControl.TabIndex = 13;
			// 
			// HeaderCustomsOfficeCodeFindBox
			// 
			this.HeaderCustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			// 
			// HeaderArrivalNotificationDateDateEdit
			// 
			this.HeaderArrivalNotificationDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			// 
			// HeaderExitDateDateEdit
			// 
			this.HeaderExitDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			// 
			// MessagesGroupBox
			// 
			this.MessagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1292, 466, true);
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.TabIndex = 14;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.HeaderLoadingPlaceTextBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Controls.SetChildIndex(this.HeaderLoadingPlaceTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.ReferenceNumberTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.HeaderCustomsOfficeCodeFindBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.HeaderArrivalNotificationDateDateEdit, 0);
			this.TopPanel.Controls.SetChildIndex(this.HeaderArrivalNotificationPlaceTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.HeaderExitDateDateEdit, 0);
			this.TopPanel.Controls.SetChildIndex(this.HeaderTransportIdTextBox, 0);
			// 
			// MovementDetailsPanel
			// 
			this.MovementDetailsPanel.Controls.Add(this.RegistrationNumberAWBTextBox);
			this.MovementDetailsPanel.Controls.Add(this.ReferenceNumberUCRTextBox);
			this.MovementDetailsPanel.Controls.Add(this.RepresentativeDocAddressControl);
			this.MovementDetailsPanel.Controls.Add(this.DeclarantDocAddressControl);
			this.MovementDetailsPanel.Controls.Add(this.LoadingPlaceTextBox);
			this.MovementDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MovementDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MovementDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1292, 466, true);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.ArrivalNotificationPlaceTextBox, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.LoadingPlaceTextBox, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.ItemsGroupBox, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.CarrierOrgAddressControl, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.DeclarantDocAddressControl, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.ExitDateDateEdit, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.ArrivalNotificationDateDateEdit, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.TransportIdTextBox, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.MovementReferenceNumberTextBox, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.StatusDropEdit, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.CustomsOfficeCodeFindBox, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.RepresentativeDocAddressControl, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.ReferenceNumberUCRTextBox, 0);
			this.MovementDetailsPanel.Controls.SetChildIndex(this.RegistrationNumberAWBTextBox, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusExitControlHeader);
			// 
			// SplitContainer
			// 
			this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SplitContainer.Name = "SplitContainer";
			this.SplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.MovementGroupBox);
			this.SplitContainer.Panel1.Controls.Add(this.TopPanel);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 827, true);
			this.SplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(279);
			// 
			// SplitContainer.Panel2
			// 
			this.SplitContainer.Panel2.AutoScroll = true;
			this.SplitContainer.Panel2.Controls.Add(this.TabControl);
			this.SplitContainer.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(432);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(330);
			this.SplitContainer.TabIndex = 3;
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.DetailsTabPage);
			this.TabControl.Controls.Add(this.AdditionalDocumentsTabPage);
			this.TabControl.Controls.Add(this.MessagesTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1300, 493, true);
			this.TabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("8333F795-6427-411C-A585-925228E1D5FC", "Details");
			this.DetailsTabPage.Controls.Add(this.MovementDetailsPanel);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1292, 466, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// AdditionalDocumentsTabPage
			// 
			this.AdditionalDocumentsTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("5646d7fb-ff7a-4d68-aba5-a8ba84845311", "Additional Documents");
			this.AdditionalDocumentsTabPage.Controls.Add(this.AdditionalDocumentsUserControl);
			this.AdditionalDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AdditionalDocumentsTabPage.Name = "AdditionalDocumentsTabPage";
			this.AdditionalDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1292, 466, true);
			this.AdditionalDocumentsTabPage.TabIndex = 1;
			// 
			// AdditionalDocumentsUserControl
			// 
			this.AdditionalDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalDocumentsUserControl, ".");
			this.AdditionalDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalDocumentsUserControl.Name = "AdditionalDocumentsUserControl";
			this.AdditionalDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1209, 393, true);
			this.AdditionalDocumentsUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("8F0B34B3-8D45-4AB4-901C-DC889613E049", "Messages");
			this.MessagesTabPage.Controls.Add(this.MessagesGroupBox);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1292, 466, true);
			this.MessagesTabPage.TabIndex = 0;
			// 
			// DeclarantDocAddressControl
			// 
			this.DeclarantDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarantDocAddressControl, "CusExitDetails.DeclarantDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.DE.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).DeclarantDocAddress)));
			this.DeclarantDocAddressControl.BindToOrganisations = "CusExitDetails.Lookups.OrgHeaderCollection";
			this.DeclarantDocAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("0638293d-329c-4edb-ab04-8954debb4782", "Declarant");
			this.DeclarantDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.DeclarantDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 240, true);
			this.DeclarantDocAddressControl.Name = "DeclarantDocAddressControl";
			this.DeclarantDocAddressControl.ReadOnly = false;
			this.DeclarantDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DeclarantDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.DeclarantDocAddressControl.TabIndex = 11;
			this.DeclarantDocAddressControl.ValidationJustForced = false;
			// 
			// RepresentativeDocAddressControl
			// 
			this.RepresentativeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeDocAddressControl, "CusExitDetails.RepresentativeDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.DE.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).RepresentativeDocAddress)));
			this.RepresentativeDocAddressControl.BindToOrganisations = "CusExitDetails.Lookups.OrgHeaderCollection";
			this.RepresentativeDocAddressControl.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("4f9466ee-1a93-4420-8726-416876ead5e7", "Representative");
			this.RepresentativeDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
			this.RepresentativeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 266, true);
			this.RepresentativeDocAddressControl.Name = "RepresentativeDocAddressControl";
			this.RepresentativeDocAddressControl.ReadOnly = false;
			this.RepresentativeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.RepresentativeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.RepresentativeDocAddressControl.TabIndex = 12;
			this.RepresentativeDocAddressControl.ValidationJustForced = false;
			// 
			// HeaderLoadingPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.HeaderLoadingPlaceTextBox, "CEH_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusExitControlHeader)(null)).CEH_LocationOfGoods)));
			this.HeaderLoadingPlaceTextBox.CaptionResourceString = null;
			this.HeaderLoadingPlaceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.HeaderLoadingPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 91, true);
			this.HeaderLoadingPlaceTextBox.Name = "HeaderLoadingPlaceTextBox";
			this.HeaderLoadingPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.HeaderLoadingPlaceTextBox.TabIndex = 6;
			// 
			// LoadingPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.LoadingPlaceTextBox, "CusExitDetails.CED_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).CED_LocationOfGoods)));
			this.LoadingPlaceTextBox.CaptionResourceString = null;
			this.LoadingPlaceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LoadingPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 84, true);
			this.LoadingPlaceTextBox.Name = "LoadingPlaceTextBox";
			this.LoadingPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.LoadingPlaceTextBox.TabIndex = 5;
			// 
			// ReferenceNumberUCRTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberUCRTextBox, "CusExitDetails.ReferenceNumberUCR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).ReferenceNumberUCR)));
			this.ReferenceNumberUCRTextBox.CaptionResourceString = null;
			this.ReferenceNumberUCRTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 188, true);
			this.ReferenceNumberUCRTextBox.Name = "ReferenceNumberUCRTextBox";
			this.ReferenceNumberUCRTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.ReferenceNumberUCRTextBox.TabIndex = 9;
			// 
			// RegistrationNumberAWBTextBox
			// 
			this.BindingSource.SetBindingMember(this.RegistrationNumberAWBTextBox, "CusExitDetails.RegistrationNumberAWB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusExitDetail)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusExitControlHeader)(null)).CusExitDetails)).SyncRoot)).RegistrationNumberAWB)));
			this.RegistrationNumberAWBTextBox.CaptionResourceString = null;
			this.RegistrationNumberAWBTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 214, true);
			this.RegistrationNumberAWBTextBox.Name = "RegistrationNumberAWBTextBox";
			this.RegistrationNumberAWBTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.RegistrationNumberAWBTextBox.TabIndex = 10;
			// 
			// ExitSummaryUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SplitContainer);
			this.Name = "ExitSummaryUserControl";
			this.MovementGroupBox.ResumeLayout(false);
			this.MovementGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MovementsGrid)).EndInit();
			this.MovementsGrid.ResumeLayout(false);
			this.MovementsGrid.PerformLayout();
			this.CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeCodeFindBox.PerformLayout();
			this.ArrivalNotificationDateDateEdit.ResumeLayout(true);
			this.ArrivalNotificationDateDateEdit.PerformLayout();
			this.ExitDateDateEdit.ResumeLayout(true);
			this.ExitDateDateEdit.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.CarrierOrgAddressControl.ResumeLayout(true);
			this.CarrierOrgAddressControl.PerformLayout();
			this.HeaderCustomsOfficeCodeFindBox.ResumeLayout(true);
			this.HeaderCustomsOfficeCodeFindBox.PerformLayout();
			this.HeaderArrivalNotificationDateDateEdit.ResumeLayout(true);
			this.HeaderArrivalNotificationDateDateEdit.PerformLayout();
			this.HeaderExitDateDateEdit.ResumeLayout(true);
			this.HeaderExitDateDateEdit.PerformLayout();
			this.MessagesGroupBox.ResumeLayout(false);
			this.MessagesGroupBox.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackingGrid)).EndInit();
			this.PackingGrid.ResumeLayout(false);
			this.PackingGrid.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.MovementDetailsPanel.ResumeLayout(false);
			this.MovementDetailsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.AdditionalDocumentsTabPage.ResumeLayout(false);
			this.AdditionalDocumentsTabPage.PerformLayout();
			this.AdditionalDocumentsUserControl.ResumeLayout(true);
			this.AdditionalDocumentsUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.DeclarantDocAddressControl.ResumeLayout(true);
			this.DeclarantDocAddressControl.PerformLayout();
			this.RepresentativeDocAddressControl.ResumeLayout(true);
			this.RepresentativeDocAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		CargoWise.Windows.UI.KSplitContainer SplitContainer;
		ZArchitecture.GUI.ZTabControl TabControl;
		ZArchitecture.GUI.ZTabPage DetailsTabPage;
		ZArchitecture.GUI.ZTabPage MessagesTabPage;
		MasterFiles.GUI.ZDocAddressControl DeclarantDocAddressControl;
		MasterFiles.GUI.ZDocAddressControl RepresentativeDocAddressControl;
		ZArchitecture.ZTextBox HeaderLoadingPlaceTextBox;
		ZArchitecture.ZTextBox LoadingPlaceTextBox;
		ZArchitecture.ZTextBox ReferenceNumberUCRTextBox;
		ZArchitecture.ZTextBox RegistrationNumberAWBTextBox;
		ZArchitecture.GUI.ZTabPage AdditionalDocumentsTabPage;
		AdditionalInfosUserControlWithGrid AdditionalDocumentsUserControl;
	}
}
