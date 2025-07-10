namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class ReferralRequestHeaderDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo ZGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.IdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RequestTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGroupBoxRequestInformations = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGridRequestInformations = new Enterprise.ZArchitecture.ZGrid();
			this.zGroupBoxSupportingDocuments = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGridSupportingDocuments = new Enterprise.ZArchitecture.ZGrid();
			this.MessageElementTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBoxReply = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zCheckBoxIncludeHRCMDetails = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HouseBillNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGridAttachments = new Enterprise.ZArchitecture.ZGrid();
			this.zGridRequestResponses = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RequestTypeDropEdit.SuspendLayout();
			this.zGroupBoxRequestInformations.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridRequestInformations)).BeginInit();
			this.zGridRequestInformations.SuspendLayout();
			this.zGroupBoxSupportingDocuments.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridSupportingDocuments)).BeginInit();
			this.zGridSupportingDocuments.SuspendLayout();
			this.zGroupBoxReply.SuspendLayout();
			this.HouseBillNumberDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridAttachments)).BeginInit();
			this.zGridAttachments.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridRequestResponses)).BeginInit();
			this.zGridRequestResponses.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader);
			// 
			// IdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.IdentifierTextBox, "EUS_Identifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).EUS_Identifier)));
			this.IdentifierTextBox.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("a913b96b-486e-44f0-b0ef-3faf436d5fdb", "Reference Id");
			this.IdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 1, true);
			this.IdentifierTextBox.Name = "IdentifierTextBox";
			this.IdentifierTextBox.ReadOnly = true;
			this.IdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.IdentifierTextBox.TabIndex = 0;
			// 
			// RequestTypeDropEdit
			// 
			this.RequestTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RequestTypeDropEdit, "EUS_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).EUS_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).Lookups.RequestTypeList)));
			this.RequestTypeDropEdit.BindToList = "Lookups.RequestTypeList";
			this.RequestTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(274, 1, true);
			this.RequestTypeDropEdit.Name = "RequestTypeDropEdit";
			this.RequestTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 15, true);
			this.RequestTypeDropEdit.TabIndex = 1;
			this.RequestTypeDropEdit.ReadOnly = true;
			// 
			// zGroupBoxRequestInformations
			// 
			this.zGroupBoxRequestInformations.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("cc58e576-d5a1-48dc-9c29-4f383cc5f06e", "Request Information");
			this.zGroupBoxRequestInformations.Controls.Add(this.zGridRequestInformations);
			this.zGroupBoxRequestInformations.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.zGroupBoxRequestInformations.Name = "zGroupBoxRequestInformations";
			this.zGroupBoxRequestInformations.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zGroupBoxRequestInformations.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 128, true);
			this.zGroupBoxRequestInformations.TabIndex = 2;
			this.zGroupBoxRequestInformations.TabStop = false;
			// 
			// zGridRequestInformations
			// 
			this.zGridRequestInformations.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGridRequestInformations, "RequestInformations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).RequestInformations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestInformation)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).RequestInformations)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestInformation)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).RequestInformations)).SyncRoot)).CodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestInformation)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).RequestInformations)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestInformation)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).RequestInformations)).SyncRoot)).SubTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestInformation)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).RequestInformations)).SyncRoot)).CSI_Description)));
			this.zGridRequestInformations.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.ColumnName = "CodeDescription";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo6.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.ColumnName = "SubTypeDescription";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo12.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.zGridRequestInformations.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGridRequestInformations.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.zGridRequestInformations.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.zGridRequestInformations.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.zGridRequestInformations.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.zGridRequestInformations.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGridRequestInformations.GridId = "ab24da6b-66eb-4ec3-877f-72ebc844bb6d";
			this.zGridRequestInformations.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGridRequestInformations.LayoutKey = "zGridRequestInformations";
			this.zGridRequestInformations.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.zGridRequestInformations.Name = "zGridRequestInformations";
			this.zGridRequestInformations.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(644, 110, true);
			this.zGridRequestInformations.GridId = "0A1B2086-253B-436B-8AAE-D92FB6463B50";
			this.zGridRequestInformations.TabIndex = 2;
			this.zGridRequestInformations.ReadOnly = true;
			// 
			// zGroupBoxSupportingDocuments
			// 
			this.zGroupBoxSupportingDocuments.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("bd4dcb5f-465c-4ea7-bf12-a50408bced75", "Supporting Documents");
			this.zGroupBoxSupportingDocuments.Controls.Add(this.zGridSupportingDocuments);
			this.zGroupBoxSupportingDocuments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(652, 29, true);
			this.zGroupBoxSupportingDocuments.Name = "zGroupBoxSupportingDocuments";
			this.zGroupBoxSupportingDocuments.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zGroupBoxSupportingDocuments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 128, true);
			this.zGroupBoxSupportingDocuments.TabIndex = 3;
			this.zGroupBoxSupportingDocuments.TabStop = false;
			// 
			// zGridSupportingDocuments
			// 
			this.zGridSupportingDocuments.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGridSupportingDocuments, "SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).SupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).SupportingDocuments)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).SupportingDocuments)).SyncRoot)).DocumentDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).SupportingDocuments)).SyncRoot)).CSI_ReferenceNumber)));
			this.zGridSupportingDocuments.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.ColumnName = "DocumentDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.zGridSupportingDocuments.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.zGridSupportingDocuments.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGridSupportingDocuments.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zGridSupportingDocuments.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGridSupportingDocuments.GridId = "4237b85a-a19c-48e1-8153-401b14f2450c";
			this.zGridSupportingDocuments.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGridSupportingDocuments.LayoutKey = "zGridSupportingDocuments";
			this.zGridSupportingDocuments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.zGridSupportingDocuments.Name = "zGridSupportingDocuments";
			this.zGridSupportingDocuments.GridId = "A5E44994-D27D-4EAD-AD05-4206E7A92022";
			this.zGridSupportingDocuments.ReadOnly = true;
			this.zGridSupportingDocuments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(564, 110, true);
			this.zGridSupportingDocuments.TabIndex = 3;
			// 
			// MessageElementTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageElementTextBox, "EUS_MessageElement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).EUS_MessageElement)));
			this.MessageElementTextBox.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("6b61520e-2129-4dcb-810a-9dea71ef0528", "Message Element");
			this.MessageElementTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 161, true);
			this.MessageElementTextBox.Name = "MessageElementTextBox";
			this.MessageElementTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.MessageElementTextBox.TabIndex = 4;
			this.MessageElementTextBox.ReadOnly = true;
			// 
			// zGroupBoxReply
			// 
			this.zGroupBoxReply.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("cc260f08-e505-4a9c-bdd1-454ab99e270f", "Reply");
			this.zGroupBoxReply.Controls.Add(this.zCheckBoxIncludeHRCMDetails);
			this.zGroupBoxReply.Controls.Add(this.HouseBillNumberDropEdit);
			this.zGroupBoxReply.Controls.Add(this.zGridAttachments);
			this.zGroupBoxReply.Controls.Add(this.zGridRequestResponses);
			this.zGroupBoxReply.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 199, true);
			this.zGroupBoxReply.Name = "zGroupBoxReply";
			this.zGroupBoxReply.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zGroupBoxReply.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1222, 180, true);
			this.zGroupBoxReply.TabIndex = 5;
			this.zGroupBoxReply.TabStop = false;
			// 
			// HouseBillNumberDropEdit
			// 
			this.HouseBillNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseBillNumberDropEdit, "EUS_HouseBillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).EUS_HouseBillNumber)));
			this.HouseBillNumberDropEdit.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("5ecec1c7-e587-4b74-b390-508e4f3e7abe", "House Bill");
			this.HouseBillNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 155, true);
			this.HouseBillNumberDropEdit.Name = "HouseBillNumberDropEdit";
			this.HouseBillNumberDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.HouseBillNumberDropEdit.ShowDescriptionBox = false;
			this.HouseBillNumberDropEdit.TabIndex = 7;
			this.HouseBillNumberDropEdit.ShouldResizeByMaxLength = false;
			// 
			// zCheckBoxIncludeHRCMDetails
			// 
			this.BindingSource.SetBindingMember(this.zCheckBoxIncludeHRCMDetails, "EUS_IncludeScreeningDetails");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).EUS_IncludeScreeningDetails)));
			this.zCheckBoxIncludeHRCMDetails.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("48b159d2-56b6-4e5a-a871-0158e293bc11", "Include HRCM Details");
			this.zCheckBoxIncludeHRCMDetails.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 156, true);
			this.zCheckBoxIncludeHRCMDetails.Name = "zCheckBoxIncludeHRCMDetails";
			this.zCheckBoxIncludeHRCMDetails.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 15, true);
			this.zCheckBoxIncludeHRCMDetails.TabIndex = 8;
			// 
			// zGridAttachments
			// 
			this.zGridAttachments.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGridAttachments, "Attachments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).Attachments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Manifest.ICS2.Business.CusStorageDocPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).Attachments)).SyncRoot)).CSD_StorageDocReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.CusStorageDocPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).Attachments)).SyncRoot)).CSD_DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.CusStorageDocPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).Attachments)).SyncRoot)).Lookups.AvailableEDocs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.CusStorageDocPivot)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).Attachments)).SyncRoot)).CSD_Description)));
			this.zGridAttachments.CaptionVisible = false;
			ZGuidDropEditColumnStyleInfo1.BindToList = "Lookups.AvailableEDocs";
			ZGuidDropEditColumnStyleInfo1.ColumnName = "CSD_StorageDocReference";
			ZGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.ColumnName = "CSD_DocType";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "CSD_Description";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.zGridAttachments.ColumnStyles.Add(ZGuidDropEditColumnStyleInfo1);
			this.zGridAttachments.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.zGridAttachments.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.zGridAttachments.GridId = "75e702e8-9de5-49f7-9511-51b4ee96ac52";
			this.zGridAttachments.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGridAttachments.LayoutKey = "zGridAttachments";
			this.zGridAttachments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(653, 18, true);
			this.zGridAttachments.Name = "zGridAttachments";
			this.zGridAttachments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(565, 127, true);
			this.zGridAttachments.TabIndex = 6;
			// 
			// zGridRequestResponses
			// 
			this.zGridRequestResponses.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGridRequestResponses, "RequestResponses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).RequestResponses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestResponse)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).RequestResponses)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestResponse)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).RequestResponses)).SyncRoot)).CodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestResponse)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).RequestResponses)).SyncRoot)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestResponse)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).RequestResponses)).SyncRoot)).SubTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestResponse)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.RequestHeader)(null)).RequestResponses)).SyncRoot)).CSI_Description)));
			this.zGridRequestResponses.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "CodeDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "SubTypeDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.zGridRequestResponses.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGridRequestResponses.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.zGridRequestResponses.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.zGridRequestResponses.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zGridRequestResponses.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.zGridRequestResponses.GridId = "332bdc00-8074-4bf3-9157-9552439e33f8";
			this.zGridRequestResponses.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGridRequestResponses.LayoutKey = "zGridRequestResponses";
			this.zGridRequestResponses.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 18, true);
			this.zGridRequestResponses.Name = "zGridRequestResponses";
			this.zGridRequestResponses.GridId = "006BBC00-C984-43FA-823F-1C1A1F3E90CB";
			this.zGridRequestResponses.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 100, true);
			this.zGridRequestResponses.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 127, true);
			this.zGridRequestResponses.TabIndex = 5;
			// 
			// ReferralRequestHeaderDetailsUserControl
			// 
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IdentifierTextBox);
			this.Controls.Add(this.RequestTypeDropEdit);
			this.Controls.Add(this.zGroupBoxRequestInformations);
			this.Controls.Add(this.zGroupBoxSupportingDocuments);
			this.Controls.Add(this.MessageElementTextBox);
			this.Controls.Add(this.zGroupBoxReply);
			this.Name = "ReferralRequestHeaderDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 387, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RequestTypeDropEdit.ResumeLayout(true);
			this.RequestTypeDropEdit.PerformLayout();
			this.zGroupBoxRequestInformations.ResumeLayout(false);
			this.zGroupBoxRequestInformations.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridRequestInformations)).EndInit();
			this.zGridRequestInformations.ResumeLayout(false);
			this.zGridRequestInformations.PerformLayout();
			this.zGroupBoxSupportingDocuments.ResumeLayout(false);
			this.zGroupBoxSupportingDocuments.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridSupportingDocuments)).EndInit();
			this.zGridSupportingDocuments.ResumeLayout(false);
			this.zGridSupportingDocuments.PerformLayout();
			this.zGroupBoxReply.ResumeLayout(false);
			this.zGroupBoxReply.PerformLayout();
			this.HouseBillNumberDropEdit.ResumeLayout(true);
			this.HouseBillNumberDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridAttachments)).EndInit();
			this.zGridAttachments.ResumeLayout(false);
			this.zGridAttachments.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridRequestResponses)).EndInit();
			this.zGridRequestResponses.ResumeLayout(false);
			this.zGridRequestResponses.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.ZTextBox IdentifierTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit RequestTypeDropEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBoxRequestInformations;
		Enterprise.ZArchitecture.ZGrid zGridRequestInformations;
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBoxSupportingDocuments;
		Enterprise.ZArchitecture.ZGrid zGridSupportingDocuments;
		Enterprise.ZArchitecture.ZTextBox MessageElementTextBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBoxReply;
		Enterprise.ZArchitecture.ZGrid zGridRequestResponses;
		Enterprise.ZArchitecture.ZGrid zGridAttachments;
		Enterprise.ZArchitecture.GUI.ZDropEdit HouseBillNumberDropEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBoxIncludeHRCMDetails;
	}
}
