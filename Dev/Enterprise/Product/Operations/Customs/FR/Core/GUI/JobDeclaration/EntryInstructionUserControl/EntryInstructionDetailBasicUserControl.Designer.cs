using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public partial class EntryInstructionDetailBasicUserControl
	{
		ZPanel MainLowerPanel;
		ZPanel OtherPartiesDetailsPanel;
		ZGroupBox OtherPartiesGroupBox;
		ZGroupBox FromWarehouseGroupBox;
		ZArchitecture.ZTextBox FromWarehouseCodeTextBox;
		ZAddressControl FromWarehouseAddressControl;
		ZGroupBox ToWarehouseGroupBox;
		ZArchitecture.ZTextBox ToWarehouseCodeTextBox;
		ZGroupBox DetailsGroupBox;
		ZDropEditWithFixedWidth StyleDropEdit;
		ZDateEdit AssessmentDateEdit;
		ZAddressControl ToWarehouseAddressControl;
		ZArchitecture.GUI.ZDropEdit ValuationBypassCodeDropEdit;
		ZArchitecture.ZTextBox ValuationBypassReasonTextBox;

		void InitializeComponent()
		{
			this.MainLowerPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LocationOfGoodsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.LocationOfGoodsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LocationOfGoodsUserControl = new Enterprise.Customs.EU.GUI.LocationOfGoodsUserControl();
			this.OtherPartiesDetailsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OtherPartiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FromWarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FromWarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FromWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ToWarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ToWarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TransactionNatureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CPCDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SubStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.AssessmentDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ValuationBypassReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ValuationBypassCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TotalInnerPackagesIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainLowerPanel.SuspendLayout();
			this.LocationOfGoodsPanel.SuspendLayout();
			this.LocationOfGoodsGroupBox.SuspendLayout();
			this.LocationOfGoodsUserControl.SuspendLayout();
			this.OtherPartiesDetailsPanel.SuspendLayout();
			this.OtherPartiesGroupBox.SuspendLayout();
			this.FromWarehouseGroupBox.SuspendLayout();
			this.FromWarehouseAddressControl.SuspendLayout();
			this.ToWarehouseGroupBox.SuspendLayout();
			this.ToWarehouseAddressControl.SuspendLayout();
			this.DetailsGroupBox.SuspendLayout();
			this.TransactionNatureDropEdit.SuspendLayout();
			this.CPCDropEdit.SuspendLayout();
			this.SubStyleDropEdit.SuspendLayout();
			this.StyleDropEdit.SuspendLayout();
			this.AssessmentDateEdit.SuspendLayout();
			this.ValuationBypassCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.Declaration.JobDeclaration);
			// 
			// MainLowerPanel
			// 
			this.MainLowerPanel.Controls.Add(this.LocationOfGoodsPanel);
			this.MainLowerPanel.Controls.Add(this.OtherPartiesDetailsPanel);
			this.MainLowerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainLowerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 131, true);
			this.MainLowerPanel.Name = "MainLowerPanel";
			this.MainLowerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 229, true);
			this.MainLowerPanel.TabIndex = 2;
			// 
			// LocationOfGoodsPanel
			// 
			this.LocationOfGoodsPanel.AutoScroll = true;
			this.LocationOfGoodsPanel.Controls.Add(this.LocationOfGoodsGroupBox);
			this.LocationOfGoodsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 110, true);
			this.LocationOfGoodsPanel.Name = "LocationOfGoodsPanel";
			this.LocationOfGoodsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 80, true);
			this.LocationOfGoodsPanel.TabIndex = 3;
			// 
			// LocationOfGoodsGroupBox
			// 
			this.LocationOfGoodsGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("E60AA974-49B1-439C-A7D1-B0802770080B", "Location Of Goods");
			this.LocationOfGoodsGroupBox.Controls.Add(this.LocationOfGoodsUserControl);
			this.LocationOfGoodsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LocationOfGoodsGroupBox.Name = "LocationOfGoodsGroupBox";
			this.LocationOfGoodsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 70, true);
			this.LocationOfGoodsGroupBox.TabIndex = 1;
			this.LocationOfGoodsGroupBox.TabStop = false;
			// 
			// LocationOfGoodsUserControl
			// 
			this.LocationOfGoodsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationOfGoodsUserControl, "CustomsEntryInstructions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.ICusGoodsLocationProvider)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)))));
			this.LocationOfGoodsUserControl.CusGoodsLocationProviderType = null;
			this.LocationOfGoodsUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.LocationOfGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 16, true);
			this.LocationOfGoodsUserControl.Name = "LocationOfGoodsUserControl";
			this.LocationOfGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(378, 40, true);
			this.LocationOfGoodsUserControl.TabIndex = 4;
			this.LocationOfGoodsUserControl.TabStop = false;
			// 
			// OtherPartiesDetailsPanel
			// 
			this.OtherPartiesDetailsPanel.AutoScroll = true;
			this.OtherPartiesDetailsPanel.Controls.Add(this.OtherPartiesGroupBox);
			this.OtherPartiesDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherPartiesDetailsPanel.Name = "OtherPartiesDetailsPanel";
			this.OtherPartiesDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 110, true);
			this.OtherPartiesDetailsPanel.TabIndex = 2;
			// 
			// OtherPartiesGroupBox
			// 
			this.OtherPartiesGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("8e30b5e8-c211-4ad2-b8bb-6e70c7b26edc", "Other Parties");
			this.OtherPartiesGroupBox.Controls.Add(this.FromWarehouseGroupBox);
			this.OtherPartiesGroupBox.Controls.Add(this.ToWarehouseGroupBox);
			this.OtherPartiesGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.OtherPartiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OtherPartiesGroupBox.Name = "OtherPartiesGroupBox";
			this.OtherPartiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 110, true);
			this.OtherPartiesGroupBox.TabIndex = 1;
			this.OtherPartiesGroupBox.TabStop = false;
			// 
			// FromWarehouseGroupBox
			// 
			this.FromWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("B14C19E3-7F04-42AD-A71D-7C9CF713E914", "From Warehouse");
			this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseCodeTextBox);
			this.FromWarehouseGroupBox.Controls.Add(this.FromWarehouseAddressControl);
			this.FromWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.FromWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 61, true);
			this.FromWarehouseGroupBox.Name = "FromWarehouseGroupBox";
			this.FromWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 45, true);
			this.FromWarehouseGroupBox.TabIndex = 4;
			this.FromWarehouseGroupBox.TabStop = false;
			// 
			// FromWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.FromWarehouseCodeTextBox, "CustomsEntryInstructions.FromWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).FromWarehouseCode)));
			this.FromWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("094AE903-F4E9-4CBA-B1AB-0E87836A81E3", "From Warehouse Code");
			this.FromWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.FromWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 16, true);
			this.FromWarehouseCodeTextBox.Name = "FromWarehouseCodeTextBox";
			this.FromWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 18, true);
			this.FromWarehouseCodeTextBox.TabIndex = 3;
			// 
			// FromWarehouseAddressControl
			// 
			this.FromWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseAddressControl, "CustomsEntryInstructions.CEI_OA_Warehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse)));
			this.FromWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.FromWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.FromWarehouseAddressControl, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FromWarehouseAddressControl, false);
			this.FromWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 16, true);
			this.FromWarehouseAddressControl.Name = "FromWarehouseAddressControl";
			this.FromWarehouseAddressControl.PopupCaption = "";
			this.FromWarehouseAddressControl.ShowAddress = false;
			this.FromWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 28, true);
			this.FromWarehouseAddressControl.TabIndex = 0;
			// 
			// ToWarehouseGroupBox
			// 
			this.ToWarehouseGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("5C5841B4-1B91-4F57-8389-D7BBB689DCE4", "To Warehouse");
			this.ToWarehouseGroupBox.Controls.Add(this.ToWarehouseCodeTextBox);
			this.ToWarehouseGroupBox.Controls.Add(this.ToWarehouseAddressControl);
			this.ToWarehouseGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ToWarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 16, true);
			this.ToWarehouseGroupBox.Name = "ToWarehouseGroupBox";
			this.ToWarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 45, true);
			this.ToWarehouseGroupBox.TabIndex = 5;
			this.ToWarehouseGroupBox.TabStop = false;
			// 
			// ToWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToWarehouseCodeTextBox, "CustomsEntryInstructions.ToWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ToWarehouseCode)));
			this.ToWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("24983C37-0821-4948-9E62-8A22716A88C6", "To Warehouse Code");
			this.ToWarehouseCodeTextBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ToWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(554, 16, true);
			this.ToWarehouseCodeTextBox.Name = "ToWarehouseCodeTextBox";
			this.ToWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 18, true);
			this.ToWarehouseCodeTextBox.TabIndex = 3;
			// 
			// ToWarehouseAddressControl
			// 
			this.ToWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseAddressControl, "CustomsEntryInstructions.CEI_OA_Warehouse2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_OA_Warehouse2)));
			this.ToWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.ToWarehouseAddressControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToWarehouseAddressControl, false);
			this.ToWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 16, true);
			this.ToWarehouseAddressControl.Name = "ToWarehouseAddressControl";
			this.ToWarehouseAddressControl.PopupCaption = "";
			this.ToWarehouseAddressControl.ShowAddress = false;
			this.ToWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 28, true);
			this.ToWarehouseAddressControl.TabIndex = 0;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("a215f2db-1737-4e6a-a930-ef8774a564c0", "Details");
			this.DetailsGroupBox.Controls.Add(this.TotalInnerPackagesIntEdit);
			this.DetailsGroupBox.Controls.Add(this.TransactionNatureDropEdit);
			this.DetailsGroupBox.Controls.Add(this.CPCDropEdit);
			this.DetailsGroupBox.Controls.Add(this.DescriptionTextBox);
			this.DetailsGroupBox.Controls.Add(this.SubStyleDropEdit);
			this.DetailsGroupBox.Controls.Add(this.StyleDropEdit);
			this.DetailsGroupBox.Controls.Add(this.AssessmentDateEdit);
			this.DetailsGroupBox.Controls.Add(this.ValuationBypassReasonTextBox);
			this.DetailsGroupBox.Controls.Add(this.ValuationBypassCodeDropEdit);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 131, true);
			this.DetailsGroupBox.TabIndex = 3;
			this.DetailsGroupBox.TabStop = false;
			// 
			// TransactionNatureDropEdit
			// 
			this.TransactionNatureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransactionNatureDropEdit, "CustomsEntryInstructions.ZG_TransNature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_TransNature)));
			this.TransactionNatureDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("79118cd6-32b3-4849-9d1b-71701fbb73d1", "[24] Tran. Nature");
			this.TransactionNatureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 82, true);
			this.TransactionNatureDropEdit.Name = "TransactionNatureDropEdit";
			this.TransactionNatureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 18, true);
			this.TransactionNatureDropEdit.TabIndex = 22;
			// 
			// CPCDropEdit
			// 
			this.CPCDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CPCDropEdit, "CustomsEntryInstructions.CEI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Procedure)));
			this.CPCDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 60, true);
			this.CPCDropEdit.Name = "CPCDropEdit";
			this.CPCDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 18, true);
			this.CPCDropEdit.TabIndex = 4;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CustomsEntryInstructions.CEI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Description)));
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 60, true);
			this.DescriptionTextBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 2, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 18, true);
			this.DescriptionTextBox.TabIndex = 5;
			// 
			// SubStyleDropEdit
			// 
			this.SubStyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubStyleDropEdit, "CustomsEntryInstructions.CEI_SubStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_SubStyle)));
			this.SubStyleDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SubStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 38, true);
			this.SubStyleDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 0, true);
			this.SubStyleDropEdit.Name = "SubStyleDropEdit";
			this.SubStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 18, true);
			this.SubStyleDropEdit.TabIndex = 2;
			// 
			// StyleDropEdit
			// 
			this.StyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StyleDropEdit, "CustomsEntryInstructions.CEI_Style");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_Style)));
			this.StyleDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("43CA8B37-5FF1-423B-865D-7A641DA63E24", "Declaration Type");
			this.StyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 17, true);
			this.StyleDropEdit.Name = "StyleDropEdit";
			this.StyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 18, true);
			this.StyleDropEdit.TabIndex = 0;
			// 
			// AssessmentDateEdit
			// 
			this.AssessmentDateEdit.AllowDrop = true;
			this.AssessmentDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AssessmentDateEdit, "CustomsEntryInstructions.CEI_DateForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_DateForDuty)));
			this.AssessmentDateEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("85396507-FC60-48A5-AB11-6B4D3F1130AB", "Assessment Date");
			this.AssessmentDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.AssessmentDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 17, true);
			this.AssessmentDateEdit.Name = "AssessmentDateEdit";
			this.AssessmentDateEdit.TabIndex = 1;
			// 
			// ValuationBypassReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.ValuationBypassReasonTextBox, "CustomsEntryInstructions.ZG_BypassReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_BypassReason)));
			this.ValuationBypassReasonTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("15888E03-704D-4000-A669-ED1AE50EE30C", "Reason");
			this.ValuationBypassReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 104, true);
			this.ValuationBypassReasonTextBox.Name = "ValuationBypassReasonTextBox";
			this.ValuationBypassReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 18, true);
			this.ValuationBypassReasonTextBox.TabIndex = 21;
			// 
			// ValuationBypassCodeDropEdit
			// 
			this.ValuationBypassCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationBypassCodeDropEdit, "CustomsEntryInstructions.ZG_BypassCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_BypassCode)));
			this.ValuationBypassCodeDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("E2AE137B-3447-4E1F-8795-DE0347A273A7", "Valuation Bypass", "Valuation Bypass Code");
			this.ValuationBypassCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 82, true);
			this.ValuationBypassCodeDropEdit.Name = "ValuationBypassCodeDropEdit";
			this.ValuationBypassCodeDropEdit.PreBoundMaxLength = 1;
			this.ValuationBypassCodeDropEdit.ShowDescriptionBox = false;
			this.ValuationBypassCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 18, true);
			this.ValuationBypassCodeDropEdit.TabIndex = 20;
			// 
			// TotalInnerPackagesIntEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalInnerPackagesIntEdit, "CustomsEntryInstructions.CEI_TotalInnerPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.FR.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).CEI_TotalInnerPackages)));
			this.TotalInnerPackagesIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(479, 38, true);
			this.TotalInnerPackagesIntEdit.Name = "TotalInnerPackagesIntEdit";
			this.TotalInnerPackagesIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.TotalInnerPackagesIntEdit.TabIndex = 23;
			// 
			// EntryInstructionDetailBasicUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainLowerPanel);
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "EntryInstructionDetailBasicUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 360, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainLowerPanel.ResumeLayout(false);
			this.MainLowerPanel.PerformLayout();
			this.LocationOfGoodsPanel.ResumeLayout(false);
			this.LocationOfGoodsPanel.PerformLayout();
			this.LocationOfGoodsGroupBox.ResumeLayout(false);
			this.LocationOfGoodsGroupBox.PerformLayout();
			this.LocationOfGoodsUserControl.ResumeLayout(true);
			this.LocationOfGoodsUserControl.PerformLayout();
			this.OtherPartiesDetailsPanel.ResumeLayout(false);
			this.OtherPartiesDetailsPanel.PerformLayout();
			this.OtherPartiesGroupBox.ResumeLayout(false);
			this.OtherPartiesGroupBox.PerformLayout();
			this.FromWarehouseGroupBox.ResumeLayout(false);
			this.FromWarehouseGroupBox.PerformLayout();
			this.FromWarehouseAddressControl.ResumeLayout(true);
			this.FromWarehouseAddressControl.PerformLayout();
			this.ToWarehouseGroupBox.ResumeLayout(false);
			this.ToWarehouseGroupBox.PerformLayout();
			this.ToWarehouseAddressControl.ResumeLayout(true);
			this.ToWarehouseAddressControl.PerformLayout();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.TransactionNatureDropEdit.ResumeLayout(true);
			this.TransactionNatureDropEdit.PerformLayout();
			this.CPCDropEdit.ResumeLayout(true);
			this.CPCDropEdit.PerformLayout();
			this.SubStyleDropEdit.ResumeLayout(true);
			this.SubStyleDropEdit.PerformLayout();
			this.StyleDropEdit.ResumeLayout(true);
			this.StyleDropEdit.PerformLayout();
			this.AssessmentDateEdit.ResumeLayout(true);
			this.AssessmentDateEdit.PerformLayout();
			this.ValuationBypassCodeDropEdit.ResumeLayout(true);
			this.ValuationBypassCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZDropEdit CPCDropEdit;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZDropEdit SubStyleDropEdit;
		private ZPanel LocationOfGoodsPanel;
		private ZGroupBox LocationOfGoodsGroupBox;
		private LocationOfGoodsUserControl LocationOfGoodsUserControl;
		private ZDropEdit TransactionNatureDropEdit;
		private ZIntEdit TotalInnerPackagesIntEdit;
	}
}
