namespace Enterprise.Customs.KR.GUI
{
	partial class SubsequentMessageDetailsUserControl
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
			this.ValuationDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EstimatedDateOfFinalPriceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AcceptedDate934DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.NoticeNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ValuationMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatus934DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CancellationOfImportDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReviewResult5BFDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AcceptedDate5BFDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CancellationReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReviewDate5BFDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MessageStatus5BFDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoodsRemovalPriorToCustomsReleaseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.SecurityAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReviewResult5BDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RequestReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReviewDate5BDDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SecurityPeriodEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SecurityPeriodStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AcceptedDate5BDDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SecurityTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatus5BDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GoldVATDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AcceptedDate5TMDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MessageStatus5TMDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ValuationDeclarationGroupBox.SuspendLayout();
			this.EstimatedDateOfFinalPriceDateEdit.SuspendLayout();
			this.AcceptedDate934DateEdit.SuspendLayout();
			this.ValuationMethodDropEdit.SuspendLayout();
			this.MessageStatus934DropEdit.SuspendLayout();
			this.CancellationOfImportDeclarationGroupBox.SuspendLayout();
			this.ReviewResult5BFDropEdit.SuspendLayout();
			this.AcceptedDate5BFDateEdit.SuspendLayout();
			this.ReviewDate5BFDateEdit.SuspendLayout();
			this.MessageStatus5BFDropEdit.SuspendLayout();
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.SuspendLayout();
			this.ReviewResult5BDDropEdit.SuspendLayout();
			this.ReviewDate5BDDateEdit.SuspendLayout();
			this.SecurityPeriodEndDateEdit.SuspendLayout();
			this.SecurityPeriodStartDateEdit.SuspendLayout();
			this.AcceptedDate5BDDateEdit.SuspendLayout();
			this.SecurityTypeDropEdit.SuspendLayout();
			this.MessageStatus5BDDropEdit.SuspendLayout();
			this.GoldVATDeclarationGroupBox.SuspendLayout();
			this.AcceptedDate5TMDateEdit.SuspendLayout();
			this.MessageStatus5TMDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.zGrid1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// ValuationDeclarationGroupBox
			// 
			this.ValuationDeclarationGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("68b0aaa2-e897-49ff-9478-2fd2cba5483a", "Valuation Declaration");
			this.ValuationDeclarationGroupBox.Controls.Add(this.EstimatedDateOfFinalPriceDateEdit);
			this.ValuationDeclarationGroupBox.Controls.Add(this.AcceptedDate934DateEdit);
			this.ValuationDeclarationGroupBox.Controls.Add(this.NoticeNumberTextBox);
			this.ValuationDeclarationGroupBox.Controls.Add(this.ValuationMethodDropEdit);
			this.ValuationDeclarationGroupBox.Controls.Add(this.MessageStatus934DropEdit);
			this.ValuationDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 6, true);
			this.ValuationDeclarationGroupBox.Name = "ValuationDeclarationGroupBox";
			this.ValuationDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 106, true);
			this.ValuationDeclarationGroupBox.TabIndex = 0;
			this.ValuationDeclarationGroupBox.TabStop = false;
			// 
			// EstimatedDateOfFinalPriceDateEdit
			// 
			this.EstimatedDateOfFinalPriceDateEdit.AllowDrop = true;
			this.EstimatedDateOfFinalPriceDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.EstimatedDateOfFinalPriceDateEdit, "CustomsEntryHeaders.SubsequentMessageDetails.EstimatedDateOfFinalPrice");
			this.EstimatedDateOfFinalPriceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 71, true);
			this.EstimatedDateOfFinalPriceDateEdit.Name = "EstimatedDateOfFinalPriceDateEdit";
			this.EstimatedDateOfFinalPriceDateEdit.TabIndex = 4;
			// 
			// AcceptedDate934DateEdit
			// 
			this.AcceptedDate934DateEdit.AllowDrop = true;
			this.AcceptedDate934DateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AcceptedDate934DateEdit, "CustomsEntryHeaders.SubsequentMessageDetails.AcceptedDate934");
			this.AcceptedDate934DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 45, true);
			this.AcceptedDate934DateEdit.Name = "AcceptedDate934DateEdit";
			this.AcceptedDate934DateEdit.TabIndex = 3;
			// 
			// NoticeNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.NoticeNumberTextBox, "CustomsEntryHeaders.SubsequentMessageDetails.NoticeNumber934");
			this.NoticeNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 50, true);
			this.NoticeNumberTextBox.Name = "NoticeNumberTextBox";
			this.NoticeNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.NoticeNumberTextBox.TabIndex = 2;
			// 
			// ValuationMethodDropEdit
			// 
			this.ValuationMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ValuationMethodDropEdit, "CustomsEntryHeaders.SubsequentMessageDetails.ValuationMethod");
			this.ValuationMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 19, true);
			this.ValuationMethodDropEdit.Name = "ValuationMethodDropEdit";
			this.ValuationMethodDropEdit.PreBoundMaxLength = 3;
			this.ValuationMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.ValuationMethodDropEdit.TabIndex = 1;
			// 
			// MessageStatus934DropEdit
			// 
			this.MessageStatus934DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatus934DropEdit, "CustomsEntryHeaders.SubsequentMessageDetails.MessageStatus934");
			this.MessageStatus934DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 19, true);
			this.MessageStatus934DropEdit.Name = "MessageStatus934DropEdit";
			this.MessageStatus934DropEdit.PreBoundMaxLength = 3;
			this.MessageStatus934DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.MessageStatus934DropEdit.TabIndex = 0;
			// 
			// CancellationOfImportDeclarationGroupBox
			// 
			this.CancellationOfImportDeclarationGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("D5E0AFDE-F5AE-4BF3-A7E2-F430F0C40D9F", "Cancellation of Import Declaration");
			this.CancellationOfImportDeclarationGroupBox.Controls.Add(this.ReviewResult5BFDropEdit);
			this.CancellationOfImportDeclarationGroupBox.Controls.Add(this.AcceptedDate5BFDateEdit);
			this.CancellationOfImportDeclarationGroupBox.Controls.Add(this.CancellationReasonTextBox);
			this.CancellationOfImportDeclarationGroupBox.Controls.Add(this.ReviewDate5BFDateEdit);
			this.CancellationOfImportDeclarationGroupBox.Controls.Add(this.MessageStatus5BFDropEdit);
			this.CancellationOfImportDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 117, true);
			this.CancellationOfImportDeclarationGroupBox.Name = "CancellationOfImportDeclarationGroupBox";
			this.CancellationOfImportDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 128, true);
			this.CancellationOfImportDeclarationGroupBox.TabIndex = 1;
			this.CancellationOfImportDeclarationGroupBox.TabStop = false;
			// 
			// ReviewResultDropEdit
			// 
			this.ReviewResult5BFDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReviewResult5BFDropEdit, "CustomsEntryHeaders.SubsequentMessageDetails.ReviewResult5BF");
			this.ReviewResult5BFDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 97, true);
			this.ReviewResult5BFDropEdit.Name = "ReviewResult5BFDropEdit";
			this.ReviewResult5BFDropEdit.PreBoundMaxLength = 3;
			this.ReviewResult5BFDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.ReviewResult5BFDropEdit.TabIndex = 7;
			// 
			// AcceptedDate5BFDateEdit
			// 
			this.AcceptedDate5BFDateEdit.AllowDrop = true;
			this.AcceptedDate5BFDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AcceptedDate5BFDateEdit, "CustomsEntryHeaders.SubsequentMessageDetails.AcceptedDate5BF");
			this.AcceptedDate5BFDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 45, true);
			this.AcceptedDate5BFDateEdit.Name = "AcceptedDate5BFDateEdit";
			this.AcceptedDate5BFDateEdit.TabIndex = 6;
			// 
			// CancellationReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.CancellationReasonTextBox, "CustomsEntryHeaders.SubsequentMessageDetails.CancellationReason");
			this.CancellationReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 19, true);
			this.CancellationReasonTextBox.Multiline = true;
			this.CancellationReasonTextBox.Name = "CancellationReasonTextBox";
			this.CancellationReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 98, true);
			this.CancellationReasonTextBox.TabIndex = 5;
			// 
			// ReviewDate5BFDateEdit
			// 
			this.ReviewDate5BFDateEdit.AllowDrop = true;
			this.ReviewDate5BFDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ReviewDate5BFDateEdit, "CustomsEntryHeaders.SubsequentMessageDetails.ReviewDate5BF");
			this.ReviewDate5BFDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 71, true);
			this.ReviewDate5BFDateEdit.Name = "DecisionDateDateEdit";
			this.ReviewDate5BFDateEdit.TabIndex = 4;
			// 
			// MessageStatus5BFDropEdit
			// 
			this.MessageStatus5BFDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatus5BFDropEdit, "CustomsEntryHeaders.SubsequentMessageDetails.MessageStatus5BF");
			this.MessageStatus5BFDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 19, true);
			this.MessageStatus5BFDropEdit.Name = "MessageStatus5BFDropEdit";
			this.MessageStatus5BFDropEdit.PreBoundMaxLength = 3;
			this.MessageStatus5BFDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.MessageStatus5BFDropEdit.TabIndex = 1;
			// 
			// GoodsRemovalPriorToCustomsReleaseGroupBox
			// 
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("2ED3666C-BA7A-4C7B-83B2-BD49DD650B55", "Goods Removal Prior to Customs Release");
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Controls.Add(this.zLabel1);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Controls.Add(this.SecurityAmountCalcEdit);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Controls.Add(this.ReviewResult5BDDropEdit);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Controls.Add(this.RequestReasonTextBox);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Controls.Add(this.ReviewDate5BDDateEdit);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Controls.Add(this.SecurityPeriodEndDateEdit);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Controls.Add(this.SecurityPeriodStartDateEdit);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Controls.Add(this.AcceptedDate5BDDateEdit);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Controls.Add(this.SecurityTypeDropEdit);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Controls.Add(this.MessageStatus5BDDropEdit);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 250, true);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Name = "GoodsRemovalPriorToCustomsReleaseGroupBox";
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 130, true);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.TabIndex = 2;
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.TabStop = false;
			// 
			// zLabel1
			// 
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(729, 71, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(16, 23, true);
			this.zLabel1.TabIndex = 17;
			this.zLabel1.Text = "~";
			this.zLabel1.UseMnemonic = false;
			// 
			// SecurityAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SecurityAmountCalcEdit, "CustomsEntryHeaders.SubsequentMessageDetails.SecurityAmount");
			this.SecurityAmountCalcEdit.DecimalPlaces = 2;
			this.SecurityAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 97, true);
			this.SecurityAmountCalcEdit.Name = "SecurityAmountCalcEdit";
			this.SecurityAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.SecurityAmountCalcEdit.TabIndex = 16;
			this.SecurityAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SecurityAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// ReviewResult5BDDropEdit
			// 
			this.ReviewResult5BDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReviewResult5BDDropEdit, "CustomsEntryHeaders.SubsequentMessageDetails.ReviewResult5BD");
			this.ReviewResult5BDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 97, true);
			this.ReviewResult5BDDropEdit.Name = "ReviewResult5BDDropEdit";
			this.ReviewResult5BDDropEdit.PreBoundMaxLength = 3;
			this.ReviewResult5BDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.ReviewResult5BDDropEdit.TabIndex = 9;
			// 
			// RequestReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.RequestReasonTextBox, "CustomsEntryHeaders.SubsequentMessageDetails.RequestReason");
			this.RequestReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 19, true);
			this.RequestReasonTextBox.Name = "RequestReasonTextBox";
			this.RequestReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 20, true);
			this.RequestReasonTextBox.TabIndex = 15;
			// 
			// ReviewDate5BDDateEdit
			// 
			this.ReviewDate5BDDateEdit.AllowDrop = true;
			this.ReviewDate5BDDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ReviewDate5BDDateEdit, "CustomsEntryHeaders.SubsequentMessageDetails.ReviewDate5BD");
			this.ReviewDate5BDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 71, true);
			this.ReviewDate5BDDateEdit.Name = "ReviewDateDateEdit";
			this.ReviewDate5BDDateEdit.TabIndex = 14;
			// 
			// SecurityPeriodEndDateEdit
			// 
			this.SecurityPeriodEndDateEdit.AllowDrop = true;
			this.SecurityPeriodEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.SecurityPeriodEndDateEdit, "CustomsEntryHeaders.SubsequentMessageDetails.SecurityPeriodEndDate");
			this.SecurityPeriodEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(751, 71, true);
			this.SecurityPeriodEndDateEdit.Name = "SecurityPeriodEndDateEdit";
			this.SecurityPeriodEndDateEdit.TabIndex = 13;
			// 
			// SecurityPeriodStartDateEdit
			// 
			this.SecurityPeriodStartDateEdit.AllowDrop = true;
			this.SecurityPeriodStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.SecurityPeriodStartDateEdit, "CustomsEntryHeaders.SubsequentMessageDetails.SecurityPeriodStartDate");
			this.SecurityPeriodStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 71, true);
			this.SecurityPeriodStartDateEdit.Name = "SecurityPeriodStartDateEdit";
			this.SecurityPeriodStartDateEdit.TabIndex = 12;
			// 
			// AcceptedDate5BDDateEdit
			// 
			this.AcceptedDate5BDDateEdit.AllowDrop = true;
			this.AcceptedDate5BDDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AcceptedDate5BDDateEdit, "CustomsEntryHeaders.SubsequentMessageDetails.AcceptedDate5BD");
			this.AcceptedDate5BDDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 45, true);
			this.AcceptedDate5BDDateEdit.Name = "AcceptedDate5BDDateEdit";
			this.AcceptedDate5BDDateEdit.TabIndex = 11;
			// 
			// SecurityTypeDropEdit
			// 
			this.SecurityTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecurityTypeDropEdit, "CustomsEntryHeaders.SubsequentMessageDetails.SecurityType");
			this.SecurityTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(638, 45, true);
			this.SecurityTypeDropEdit.Name = "SecurityTypeDropEdit";
			this.SecurityTypeDropEdit.PreBoundMaxLength = 3;
			this.SecurityTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.SecurityTypeDropEdit.TabIndex = 10;
			// 
			// MessageStatus5BDDropEdit
			// 
			this.MessageStatus5BDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatus5BDDropEdit, "CustomsEntryHeaders.SubsequentMessageDetails.MessageStatus5BD");
			this.MessageStatus5BDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 19, true);
			this.MessageStatus5BDDropEdit.Name = "MessageStatus5BDDropEdit";
			this.MessageStatus5BDDropEdit.PreBoundMaxLength = 3;
			this.MessageStatus5BDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.MessageStatus5BDDropEdit.TabIndex = 8;
			// 
			// GoldVATDeclarationGroupBox
			// 
			this.GoldVATDeclarationGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("E092812E-ACC0-4AA4-87D5-037C1D61431E", "Gold VAT Declaration");
			this.GoldVATDeclarationGroupBox.Controls.Add(this.AcceptedDate5TMDateEdit);
			this.GoldVATDeclarationGroupBox.Controls.Add(this.MessageStatus5TMDropEdit);
			this.GoldVATDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 385, true);
			this.GoldVATDeclarationGroupBox.Name = "GoldVATDeclarationGroupBox";
			this.GoldVATDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 77, true);
			this.GoldVATDeclarationGroupBox.TabIndex = 3;
			this.GoldVATDeclarationGroupBox.TabStop = false;
			// 
			// AcceptedDate5TMDateEdit
			// 
			this.AcceptedDate5TMDateEdit.AllowDrop = true;
			this.AcceptedDate5TMDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AcceptedDate5TMDateEdit, "CustomsEntryHeaders.SubsequentMessageDetails.AcceptedDate5TM");
			this.AcceptedDate5TMDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 45, true);
			this.AcceptedDate5TMDateEdit.Name = "AcceptedDate5TMDateEdit";
			this.AcceptedDate5TMDateEdit.TabIndex = 15;
			// 
			// MessageStatus5TMDropEdit
			// 
			this.MessageStatus5TMDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatus5TMDropEdit, "CustomsEntryHeaders.SubsequentMessageDetails.MessageStatus5TM");
			this.MessageStatus5TMDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(166, 19, true);
			this.MessageStatus5TMDropEdit.Name = "MessageStatus5TMDropEdit";
			this.MessageStatus5TMDropEdit.PreBoundMaxLength = 3;
			this.MessageStatus5TMDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(139, 20, true);
			this.MessageStatus5TMDropEdit.TabIndex = 9;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.zGrid1.CaptionVisible = false;
			this.zGrid1.Controls.Add(this.GoldVATDeclarationGroupBox);
			this.zGrid1.Controls.Add(this.GoodsRemovalPriorToCustomsReleaseGroupBox);
			this.zGrid1.Controls.Add(this.CancellationOfImportDeclarationGroupBox);
			this.zGrid1.Controls.Add(this.ValuationDeclarationGroupBox);
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.GridId = "cc81f713-f861-4ab0-8ff5-e59d0708d644";
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.ReadOnly = true;
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(967, 462, true);
			this.zGrid1.TabIndex = 8;
			// 
			// SubsequentMessageDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zGrid1);
			this.Name = "SubsequentMessageDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(967, 462, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ValuationDeclarationGroupBox.ResumeLayout(false);
			this.ValuationDeclarationGroupBox.PerformLayout();
			this.EstimatedDateOfFinalPriceDateEdit.ResumeLayout(true);
			this.EstimatedDateOfFinalPriceDateEdit.PerformLayout();
			this.AcceptedDate934DateEdit.ResumeLayout(true);
			this.AcceptedDate934DateEdit.PerformLayout();
			this.ValuationMethodDropEdit.ResumeLayout(true);
			this.ValuationMethodDropEdit.PerformLayout();
			this.MessageStatus934DropEdit.ResumeLayout(true);
			this.MessageStatus934DropEdit.PerformLayout();
			this.CancellationOfImportDeclarationGroupBox.ResumeLayout(false);
			this.CancellationOfImportDeclarationGroupBox.PerformLayout();
			this.ReviewResult5BFDropEdit.ResumeLayout(true);
			this.ReviewResult5BFDropEdit.PerformLayout();
			this.AcceptedDate5BFDateEdit.ResumeLayout(true);
			this.AcceptedDate5BFDateEdit.PerformLayout();
			this.ReviewDate5BFDateEdit.ResumeLayout(true);
			this.ReviewDate5BFDateEdit.PerformLayout();
			this.MessageStatus5BFDropEdit.ResumeLayout(true);
			this.MessageStatus5BFDropEdit.PerformLayout();
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.ResumeLayout(false);
			this.GoodsRemovalPriorToCustomsReleaseGroupBox.PerformLayout();
			this.ReviewResult5BDDropEdit.ResumeLayout(true);
			this.ReviewResult5BDDropEdit.PerformLayout();
			this.ReviewDate5BDDateEdit.ResumeLayout(true);
			this.ReviewDate5BDDateEdit.PerformLayout();
			this.SecurityPeriodEndDateEdit.ResumeLayout(true);
			this.SecurityPeriodEndDateEdit.PerformLayout();
			this.SecurityPeriodStartDateEdit.ResumeLayout(true);
			this.SecurityPeriodStartDateEdit.PerformLayout();
			this.AcceptedDate5BDDateEdit.ResumeLayout(true);
			this.AcceptedDate5BDDateEdit.PerformLayout();
			this.SecurityTypeDropEdit.ResumeLayout(true);
			this.SecurityTypeDropEdit.PerformLayout();
			this.MessageStatus5BDDropEdit.ResumeLayout(true);
			this.MessageStatus5BDDropEdit.PerformLayout();
			this.GoldVATDeclarationGroupBox.ResumeLayout(false);
			this.GoldVATDeclarationGroupBox.PerformLayout();
			this.AcceptedDate5TMDateEdit.ResumeLayout(true);
			this.AcceptedDate5TMDateEdit.PerformLayout();
			this.MessageStatus5TMDropEdit.ResumeLayout(true);
			this.MessageStatus5TMDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.zGrid1.ResumeLayout(false);
			this.zGrid1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox CancellationOfImportDeclarationGroupBox;
		private ZArchitecture.GUI.ZGroupBox GoodsRemovalPriorToCustomsReleaseGroupBox;
		private ZArchitecture.GUI.ZGroupBox GoldVATDeclarationGroupBox;
		private ZArchitecture.GUI.ZGroupBox ValuationDeclarationGroupBox;
		private ZArchitecture.GUI.ZDropEdit MessageStatus934DropEdit;
		private ZArchitecture.GUI.ZDateEdit EstimatedDateOfFinalPriceDateEdit;
		private ZArchitecture.GUI.ZDateEdit AcceptedDate934DateEdit;
		private ZArchitecture.ZTextBox NoticeNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit ValuationMethodDropEdit;
		private ZArchitecture.GUI.ZDropEdit ReviewResult5BFDropEdit;
		private ZArchitecture.GUI.ZDateEdit AcceptedDate5BFDateEdit;
		private ZArchitecture.ZTextBox CancellationReasonTextBox;
		private ZArchitecture.GUI.ZDateEdit ReviewDate5BFDateEdit;
		private ZArchitecture.GUI.ZDropEdit MessageStatus5BFDropEdit;
		private ZArchitecture.ZCalcEdit SecurityAmountCalcEdit;
		private ZArchitecture.ZTextBox RequestReasonTextBox;
		private ZArchitecture.GUI.ZDateEdit ReviewDate5BDDateEdit;
		private ZArchitecture.GUI.ZDateEdit SecurityPeriodEndDateEdit;
		private ZArchitecture.GUI.ZDateEdit SecurityPeriodStartDateEdit;
		private ZArchitecture.GUI.ZDateEdit AcceptedDate5BDDateEdit;
		private ZArchitecture.GUI.ZDropEdit SecurityTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit ReviewResult5BDDropEdit;
		private ZArchitecture.GUI.ZDropEdit MessageStatus5BDDropEdit;
		private ZArchitecture.GUI.ZDateEdit AcceptedDate5TMDateEdit;
		private ZArchitecture.GUI.ZDropEdit MessageStatus5TMDropEdit;
		private ZArchitecture.ZGrid zGrid1;
		private ZArchitecture.ZLabel zLabel1;
	}
}
