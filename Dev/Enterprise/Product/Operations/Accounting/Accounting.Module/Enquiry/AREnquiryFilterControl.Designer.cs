namespace Enterprise.Accounting.Module
{
	public partial class AREnquiryFilterControl
	{
		#region Windows Form Designer generated code

		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit6;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit7;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit8;
		Enterprise.ZArchitecture.ZCalcEdit CurrentDisbursementOutstandingTextBox;
		Enterprise.ZArchitecture.ZCalcEdit TotalOutstandingAmountCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit ThreePeriodTotalCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit TwoPeriodTotalCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit OnePeriodTotalCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit CurrentTotalCalcEdit;
		Enterprise.ZArchitecture.GUI.ZGroupBox TreatDisbAsStandardGroupBox;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.ZLabel zLabel2;
		Enterprise.ZArchitecture.ZCalcEdit OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit;
		private Enterprise.ZArchitecture.ZLabel standardLabel;
		internal Enterprise.ZArchitecture.ZCalcEdit SumOfTotalWIPAndRevenueCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit PostedRevenueCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit RecognizedWIPCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit UnrecognizedWIPCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit zCalcEdit9;
		private Enterprise.ZArchitecture.GUI.ZGroupBox SettlementGroupGroupBox;
		internal Enterprise.ZArchitecture.ZTextBox SettlementGroupTextBox;
		internal Enterprise.ZArchitecture.ZLabel SettlementGroupInfoLabel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox overdueGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox creditOnHoldCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit totalOverdueCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit dsbOverdueCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit stdOverdueCalcEdit;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox UseSettlementGroupCreditLimitCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox GlobalAccountCreditStatusGroupBox;
		internal Enterprise.ZArchitecture.ZTextBox GlobalCreditGroupTextBox;
		internal Enterprise.ZArchitecture.ZCalcEdit GlobalCreditLimitCalcEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit GlobalCreditAvailableCalcEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit TotalBatchedOutstandingAmountCalcEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit ThreePeriodTotalBatchedCalcEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit TwoPeriodTotalBatchedCalcEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit OnePeriodTotalBatchedCalcEdit;
		protected Enterprise.ZArchitecture.ZCalcEdit CurrentTotalBatchedCalcEdit;
		internal Enterprise.ZArchitecture.ZTextBox GlobalCreditCurrencyTextBox;

		void InitializeComponent()
		{
			this.zCalcEdit6 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit7 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit8 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit9 = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CurrentDisbursementOutstandingTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalOutstandingAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ThreePeriodTotalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TwoPeriodTotalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OnePeriodTotalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CurrentTotalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TreatDisbAsStandardGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.standardLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UnrecognizedWIPCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RecognizedWIPCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PostedRevenueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SumOfTotalWIPAndRevenueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SettlementGroupGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SettlementGroupInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UseSettlementGroupCreditLimitCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SettlementGroupTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GlobalAccountCreditStatusGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GlobalCreditGroupTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GlobalCreditLimitCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GlobalCreditAvailableCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GlobalCreditCurrencyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.overdueGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.totalOverdueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.dsbOverdueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.stdOverdueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.creditOnHoldCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TotalBatchedOutstandingAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ThreePeriodTotalBatchedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TwoPeriodTotalBatchedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OnePeriodTotalBatchedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CurrentTotalBatchedCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.InvoiceAgeingGroupBox.SuspendLayout();
			this.AccountBalanceGroupBox.SuspendLayout();
			this.ReadOnlyDetailsPanel.SuspendLayout();
			this.AverageDaysToFullyPayGroupBox.SuspendLayout();
			this.AgingOptionsDropEdit1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TreatDisbAsStandardGroupBox.SuspendLayout();
			this.SettlementGroupGroupBox.SuspendLayout();
			this.GlobalAccountCreditStatusGroupBox.SuspendLayout();
			this.overdueGroupBox.SuspendLayout();
			this.SuspendLayout();
			//
			// InvoiceAgeingGroupBox
			//
			this.InvoiceAgeingGroupBox.Controls.Add(this.TotalBatchedOutstandingAmountCalcEdit);
			this.InvoiceAgeingGroupBox.Controls.Add(this.ThreePeriodTotalBatchedCalcEdit);
			this.InvoiceAgeingGroupBox.Controls.Add(this.TwoPeriodTotalBatchedCalcEdit);
			this.InvoiceAgeingGroupBox.Controls.Add(this.OnePeriodTotalBatchedCalcEdit);
			this.InvoiceAgeingGroupBox.Controls.Add(this.CurrentTotalBatchedCalcEdit);
			this.InvoiceAgeingGroupBox.Controls.Add(this.standardLabel);
			this.InvoiceAgeingGroupBox.Controls.Add(this.CurrentTotalCalcEdit);
			this.InvoiceAgeingGroupBox.Controls.Add(this.OnePeriodTotalCalcEdit);
			this.InvoiceAgeingGroupBox.Controls.Add(this.TwoPeriodTotalCalcEdit);
			this.InvoiceAgeingGroupBox.Controls.Add(this.ThreePeriodTotalCalcEdit);
			this.InvoiceAgeingGroupBox.Controls.Add(this.zCalcEdit7);
			this.InvoiceAgeingGroupBox.Controls.Add(this.zCalcEdit8);
			this.InvoiceAgeingGroupBox.Controls.Add(this.zCalcEdit9);
			this.InvoiceAgeingGroupBox.Controls.Add(this.zCalcEdit6);
			this.InvoiceAgeingGroupBox.Controls.Add(this.CurrentDisbursementOutstandingTextBox);
			this.InvoiceAgeingGroupBox.Controls.Add(this.TotalOutstandingAmountCalcEdit);
			this.InvoiceAgeingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 187, true);
			this.InvoiceAgeingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 181, true);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.AgingOptionsDropEdit1, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.TotalOutstandingAmountCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.CurrentDisbursementOutstandingTextBox, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.zCalcEdit6, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.zCalcEdit9, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.zCalcEdit8, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.zCalcEdit7, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.CurrentCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.OnePeriodCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.TwoPeriodCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.ThreePeriodCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.TotalCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.ThreePeriodTotalCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.TwoPeriodTotalCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.OnePeriodTotalCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.CurrentTotalCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.standardLabel, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.CurrentTotalBatchedCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.OnePeriodTotalBatchedCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.TwoPeriodTotalBatchedCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.ThreePeriodTotalBatchedCalcEdit, 0);
			this.InvoiceAgeingGroupBox.Controls.SetChildIndex(this.TotalBatchedOutstandingAmountCalcEdit, 0);
			//
			// TotalCalcEdit
			//
			this.TotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 65, true);
			//
			// ThreePeriodCalcEdit
			//
			this.ThreePeriodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 65, true);
			//
			// TwoPeriodCalcEdit
			//
			this.TwoPeriodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 65, true);
			//
			// OnePeriodCalcEdit
			//
			this.OnePeriodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 65, true);
			//
			// CurrentCalcEdit
			//
			this.CurrentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 65, true);
			//
			// AccountBalanceGroupBox
			//
			this.AccountBalanceGroupBox.Controls.Add(this.SumOfTotalWIPAndRevenueCalcEdit);
			this.AccountBalanceGroupBox.Controls.Add(this.PostedRevenueCalcEdit);
			this.AccountBalanceGroupBox.Controls.Add(this.RecognizedWIPCalcEdit);
			this.AccountBalanceGroupBox.Controls.Add(this.UnrecognizedWIPCalcEdit);
			this.AccountBalanceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 123, true);
			this.AccountBalanceGroupBox.Controls.SetChildIndex(this.UnrecognizedWIPCalcEdit, 0);
			this.AccountBalanceGroupBox.Controls.SetChildIndex(this.RecognizedWIPCalcEdit, 0);
			this.AccountBalanceGroupBox.Controls.SetChildIndex(this.PostedRevenueCalcEdit, 0);
			this.AccountBalanceGroupBox.Controls.SetChildIndex(this.SumOfTotalWIPAndRevenueCalcEdit, 0);
			this.AccountBalanceGroupBox.Controls.SetChildIndex(this.LastPurchaseSaleCalcEdit, 0);
			this.AccountBalanceGroupBox.Controls.SetChildIndex(this.LastPaymentReceiptCalcEdit, 0);
			this.AccountBalanceGroupBox.Controls.SetChildIndex(this.LastPaymentReceiptDateTextBox, 0);
			this.AccountBalanceGroupBox.Controls.SetChildIndex(this.MTD_PTDSalesCalcEdit, 0);
			this.AccountBalanceGroupBox.Controls.SetChildIndex(this.YTDPurchaseSalesCalcEdit, 0);
			this.AccountBalanceGroupBox.Controls.SetChildIndex(this.LVRSalesCalcEdit, 0);
			//
			// LastPaymentReceiptCalcEdit
			//
			this.LastPaymentReceiptCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AREnquiryFilterControl|e1a32613-f56a-4242-93be-dca2ae80dde2", "Last Receipt");
			//
			// LastPurchaseSaleCalcEdit
			//
			this.LastPurchaseSaleCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AREnquiryFilterControl|175ffee0-70c4-4d99-bc30-870fb32fa975", "Last Sale");
			//
			// ReadOnlyDetailsPanel
			//
			this.ReadOnlyDetailsPanel.Controls.Add(this.creditOnHoldCheckBox);
			this.ReadOnlyDetailsPanel.Controls.Add(this.overdueGroupBox);
			this.ReadOnlyDetailsPanel.Controls.Add(this.GlobalAccountCreditStatusGroupBox);
			this.ReadOnlyDetailsPanel.Controls.Add(this.SettlementGroupGroupBox);
			this.ReadOnlyDetailsPanel.Controls.Add(this.TreatDisbAsStandardGroupBox);
			this.ReadOnlyDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 149, true);
			this.ReadOnlyDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(989, 431, true);
			this.ReadOnlyDetailsPanel.Controls.SetChildIndex(this.AccountBalanceGroupBox, 0);
			this.ReadOnlyDetailsPanel.Controls.SetChildIndex(this.AverageDaysToFullyPayGroupBox, 0);
			this.ReadOnlyDetailsPanel.Controls.SetChildIndex(this.TreatDisbAsStandardGroupBox, 0);
			this.ReadOnlyDetailsPanel.Controls.SetChildIndex(this.InvoiceAgeingGroupBox, 0);
			this.ReadOnlyDetailsPanel.Controls.SetChildIndex(this.SettlementGroupGroupBox, 0);
			this.ReadOnlyDetailsPanel.Controls.SetChildIndex(this.GlobalAccountCreditStatusGroupBox, 0);
			this.ReadOnlyDetailsPanel.Controls.SetChildIndex(this.overdueGroupBox, 0);
			this.ReadOnlyDetailsPanel.Controls.SetChildIndex(this.creditOnHoldCheckBox, 0);
			//
			// AverageDaysToFullyPayGroupBox
			//
			this.AverageDaysToFullyPayGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 374, true);
			//
			// LastPaymentReceiptDateTextBox
			//
			this.LastPaymentReceiptDateTextBox.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("43e66206-2d51-4ac2-8ddd-81a1534b3078", "Date Received");
			//
			// AgingOptionsDropEdit1
			//
			this.AgingOptionsDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 23, true);
			//
			// grid
			//
			this.grid.ColorContextKey = "AREnquiryFilterControl";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(989, 125, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Module.AREnquiryFilterBusinessObject);
			//
			// zCalcEdit6
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit6, "TotalDisbursementOutstanding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).TotalDisbursementOutstanding)));
			this.zCalcEdit6.CaptionResourceString = null;
			this.zCalcEdit6.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit6, false);
			this.zCalcEdit6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 89, true);
			this.zCalcEdit6.Name = "zCalcEdit6";
			this.zCalcEdit6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.zCalcEdit6.TabIndex = 16;
			this.zCalcEdit6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit7
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit7, "ThirdAgeingDisbursementOutstanding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).ThirdAgeingDisbursementOutstanding)));
			this.zCalcEdit7.CaptionResourceString = null;
			this.zCalcEdit7.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit7, false);
			this.zCalcEdit7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 89, true);
			this.zCalcEdit7.Name = "zCalcEdit7";
			this.zCalcEdit7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.zCalcEdit7.TabIndex = 15;
			this.zCalcEdit7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit8
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit8, "SecondAgeingDisbursementOutstanding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).SecondAgeingDisbursementOutstanding)));
			this.zCalcEdit8.CaptionResourceString = null;
			this.zCalcEdit8.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit8, false);
			this.zCalcEdit8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 89, true);
			this.zCalcEdit8.Name = "zCalcEdit8";
			this.zCalcEdit8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.zCalcEdit8.TabIndex = 14;
			this.zCalcEdit8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zCalcEdit9
			//
			this.BindingSource.SetBindingMember(this.zCalcEdit9, "FirstAgeingDisbursementOutstanding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).FirstAgeingDisbursementOutstanding)));
			this.zCalcEdit9.CaptionResourceString = null;
			this.zCalcEdit9.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zCalcEdit9, false);
			this.zCalcEdit9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 89, true);
			this.zCalcEdit9.Name = "zCalcEdit9";
			this.zCalcEdit9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.zCalcEdit9.TabIndex = 13;
			this.zCalcEdit9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// CurrentDisbursementOutstandingTextBox
			//
			this.BindingSource.SetBindingMember(this.CurrentDisbursementOutstandingTextBox, "CurrentDisbursementOutstanding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).CurrentDisbursementOutstanding)));
			this.CurrentDisbursementOutstandingTextBox.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AREnquiryFilterControl|9eba4781-41ce-4362-b234-593126bef35d", "Disbursement");
			this.CurrentDisbursementOutstandingTextBox.DecimalPlaces = 2;
			this.CurrentDisbursementOutstandingTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 89, true);
			this.CurrentDisbursementOutstandingTextBox.Name = "CurrentDisbursementOutstandingTextBox";
			this.CurrentDisbursementOutstandingTextBox.ReadOnly = true;
			this.CurrentDisbursementOutstandingTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.CurrentDisbursementOutstandingTextBox.TabIndex = 12;
			this.CurrentDisbursementOutstandingTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// TotalOutstandingAmountCalcEdit
			//
			this.BindingSource.SetBindingMember(this.TotalOutstandingAmountCalcEdit, "TotalOutstandingAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).TotalOutstandingAmount)));
			this.TotalOutstandingAmountCalcEdit.CaptionResourceString = null;
			this.TotalOutstandingAmountCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalOutstandingAmountCalcEdit, false);
			this.TotalOutstandingAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 113, true);
			this.TotalOutstandingAmountCalcEdit.Name = "TotalOutstandingAmountCalcEdit";
			this.TotalOutstandingAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.TotalOutstandingAmountCalcEdit.TabIndex = 1;
			this.TotalOutstandingAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ThreePeriodTotalCalcEdit
			//
			this.BindingSource.SetBindingMember(this.ThreePeriodTotalCalcEdit, "ThreePeriodTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).ThreePeriodTotal)));
			this.ThreePeriodTotalCalcEdit.CaptionResourceString = null;
			this.ThreePeriodTotalCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ThreePeriodTotalCalcEdit, false);
			this.ThreePeriodTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 113, true);
			this.ThreePeriodTotalCalcEdit.Name = "ThreePeriodTotalCalcEdit";
			this.ThreePeriodTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.ThreePeriodTotalCalcEdit.TabIndex = 20;
			this.ThreePeriodTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// TwoPeriodTotalCalcEdit
			//
			this.BindingSource.SetBindingMember(this.TwoPeriodTotalCalcEdit, "TwoPeriodTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).TwoPeriodTotal)));
			this.TwoPeriodTotalCalcEdit.CaptionResourceString = null;
			this.TwoPeriodTotalCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TwoPeriodTotalCalcEdit, false);
			this.TwoPeriodTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 113, true);
			this.TwoPeriodTotalCalcEdit.Name = "TwoPeriodTotalCalcEdit";
			this.TwoPeriodTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.TwoPeriodTotalCalcEdit.TabIndex = 21;
			this.TwoPeriodTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// OnePeriodTotalCalcEdit
			//
			this.BindingSource.SetBindingMember(this.OnePeriodTotalCalcEdit, "OnePeriodTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).OnePeriodTotal)));
			this.OnePeriodTotalCalcEdit.CaptionResourceString = null;
			this.OnePeriodTotalCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OnePeriodTotalCalcEdit, false);
			this.OnePeriodTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 113, true);
			this.OnePeriodTotalCalcEdit.Name = "OnePeriodTotalCalcEdit";
			this.OnePeriodTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.OnePeriodTotalCalcEdit.TabIndex = 22;
			this.OnePeriodTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// CurrentTotalCalcEdit
			//
			this.BindingSource.SetBindingMember(this.CurrentTotalCalcEdit, "CurrentTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).CurrentTotal)));
			this.CurrentTotalCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AREnquiryFilterControl|9bd7c625-a425-4e9f-ba0a-d351eaa1e4b9", "Total");
			this.CurrentTotalCalcEdit.DecimalPlaces = 2;
			this.CurrentTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 113, true);
			this.CurrentTotalCalcEdit.Name = "CurrentTotalCalcEdit";
			this.CurrentTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.CurrentTotalCalcEdit.TabIndex = 23;
			this.CurrentTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// TreatDisbAsStandardGroupBox
			//
			this.TreatDisbAsStandardGroupBox.Controls.Add(this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit);
			this.TreatDisbAsStandardGroupBox.Controls.Add(this.zLabel2);
			this.TreatDisbAsStandardGroupBox.Controls.Add(this.zLabel1);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TreatDisbAsStandardGroupBox, false);
			this.TreatDisbAsStandardGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(608, 187, true);
			this.TreatDisbAsStandardGroupBox.Name = "TreatDisbAsStandardGroupBox";
			this.TreatDisbAsStandardGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 80, true);
			this.TreatDisbAsStandardGroupBox.TabIndex = 14;
			this.TreatDisbAsStandardGroupBox.TabStop = false;
			//
			// OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit
			//
			this.BindingSource.SetBindingMember(this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit, "OM_ARTreatDisbursementsAsStandardValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).OM_ARTreatDisbursementsAsStandardValue)));
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AREnquiryFilterControl|25cf04fc-a6fa-4c8f-b5fa-1af5d1c74675", "Value");
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.DecimalPlaces = 2;
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 51, true);
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.Name = "OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit";
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.TabIndex = 3;
			this.OM_ARTreatDisbursementsAsStandardValueBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel2
			//
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AREnquiryFilterControl|12b33784-a03e-4659-b6d4-7c5462dcdd2b", "As Standard Under");
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 30, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 13, true);
			this.zLabel2.TabIndex = 1;
			//
			// zLabel1
			//
			this.zLabel1.AutoSize = true;
			this.zLabel1.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AREnquiryFilterControl|3c5afda6-1aad-4833-9897-e106695a09c6", "Treat Disbursements");
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 10, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 13, true);
			this.zLabel1.TabIndex = 0;
			//
			// standardLabel
			//
			this.standardLabel.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("07927a1f-9c3e-4c96-9b2f-1a12eb458a27", "Standard:");
			this.standardLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.standardLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 61, true);
			this.standardLabel.Name = "standardLabel";
			this.standardLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 23, true);
			this.standardLabel.TabIndex = 24;
			this.standardLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			//
			// UnrecognizedWIPCalcEdit
			//
			this.BindingSource.SetBindingMember(this.UnrecognizedWIPCalcEdit, "UnrecognisedWIP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).UnrecognisedWIP)));
			this.UnrecognizedWIPCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AREnquiryFilterControl|8e4de7df-e622-4e91-89b3-4c277575e5a3", "Unrecognized WIP");
			this.UnrecognizedWIPCalcEdit.DecimalPlaces = 2;
			this.UnrecognizedWIPCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 94, true);
			this.UnrecognizedWIPCalcEdit.Name = "UnrecognizedWIPCalcEdit";
			this.UnrecognizedWIPCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.UnrecognizedWIPCalcEdit.TabIndex = 24;
			this.UnrecognizedWIPCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// RecognizedWIPCalcEdit
			//
			this.BindingSource.SetBindingMember(this.RecognizedWIPCalcEdit, "RecognisedWIP");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).RecognisedWIP)));
			this.RecognizedWIPCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AREnquiryFilterControl|50584fa9-5280-4619-974c-eaf86ccbdb7b", "Recognized WIP");
			this.RecognizedWIPCalcEdit.DecimalPlaces = 2;
			this.RecognizedWIPCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 94, true);
			this.RecognizedWIPCalcEdit.Name = "RecognizedWIPCalcEdit";
			this.RecognizedWIPCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.RecognizedWIPCalcEdit.TabIndex = 25;
			this.RecognizedWIPCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// PostedRevenueCalcEdit
			//
			this.BindingSource.SetBindingMember(this.PostedRevenueCalcEdit, "PostedRevenue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).PostedRevenue)));
			this.PostedRevenueCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AREnquiryFilterControl|d7d6f8a5-b706-42f8-acf9-3063e5a341d6", "Posted Revenue");
			this.PostedRevenueCalcEdit.DecimalPlaces = 2;
			this.PostedRevenueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 94, true);
			this.PostedRevenueCalcEdit.Name = "PostedRevenueCalcEdit";
			this.PostedRevenueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.PostedRevenueCalcEdit.TabIndex = 26;
			this.PostedRevenueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// SumOfTotalWIPAndRevenueCalcEdit
			//
			this.BindingSource.SetBindingMember(this.SumOfTotalWIPAndRevenueCalcEdit, "SumOfTotalWIPAndRevenue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).SumOfTotalWIPAndRevenue)));
			this.SumOfTotalWIPAndRevenueCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("AREnquiryFilterControl|84a698fc-0128-42ba-921d-25fc370063ed", "Total WIP + Revenue");
			this.SumOfTotalWIPAndRevenueCalcEdit.DecimalPlaces = 2;
			this.SumOfTotalWIPAndRevenueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(637, 94, true);
			this.SumOfTotalWIPAndRevenueCalcEdit.Name = "SumOfTotalWIPAndRevenueCalcEdit";
			this.SumOfTotalWIPAndRevenueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.SumOfTotalWIPAndRevenueCalcEdit.TabIndex = 27;
			this.SumOfTotalWIPAndRevenueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// SettlementGroupGroupBox
			//
			this.SettlementGroupGroupBox.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("b063fd76-b30b-45ae-ad1f-e7c2cd71d52a", "Settlement Group");
			this.SettlementGroupGroupBox.Controls.Add(this.SettlementGroupInfoLabel);
			this.SettlementGroupGroupBox.Controls.Add(this.UseSettlementGroupCreditLimitCheckBox);
			this.SettlementGroupGroupBox.Controls.Add(this.SettlementGroupTextBox);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SettlementGroupGroupBox, false);
			this.SettlementGroupGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(730, 3, true);
			this.SettlementGroupGroupBox.Name = "SettlementGroupGroupBox";
			this.SettlementGroupGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(245, 123, true);
			this.SettlementGroupGroupBox.TabIndex = 15;
			this.SettlementGroupGroupBox.TabStop = false;
			//
			// SettlementGroupInfoLabel
			//
			this.BindingSource.SetBindingMember(this.SettlementGroupInfoLabel, "SettlementGroupString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).SettlementGroupString)));
			this.SettlementGroupInfoLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.SettlementGroupInfoLabel.IsFontBold = true;
			this.SettlementGroupInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 72, true);
			this.SettlementGroupInfoLabel.Name = "SettlementGroupInfoLabel";
			this.SettlementGroupInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 42, true);
			this.SettlementGroupInfoLabel.TabIndex = 4;
			//
			// UseSettlementGroupCreditLimitCheckBox
			//
			this.BindingSource.SetBindingMember(this.UseSettlementGroupCreditLimitCheckBox, "UseSettlementGroupCreditLimit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).UseSettlementGroupCreditLimit)));
			this.UseSettlementGroupCreditLimitCheckBox.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("c17959bd-9913-483a-ad6d-4567e1dc188e", "Use Settlement Group Credit Limit:");
			this.UseSettlementGroupCreditLimitCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.UseSettlementGroupCreditLimitCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseSettlementGroupCreditLimitCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(27, 39, true);
			this.UseSettlementGroupCreditLimitCheckBox.Name = "UseSettlementGroupCreditLimitCheckBox";
			this.UseSettlementGroupCreditLimitCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 30, true);
			this.UseSettlementGroupCreditLimitCheckBox.TabIndex = 3;
			//
			// SettlementGroupTextBox
			//
			this.BindingSource.SetBindingMember(this.SettlementGroupTextBox, "SettlementGroupCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).SettlementGroupCode)));
			this.SettlementGroupTextBox.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("133a9197-d0ed-491b-89e1-c2fc97b79bfe", "Settlement Group:");
			this.SettlementGroupTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 18, true);
			this.SettlementGroupTextBox.Name = "SettlementGroupTextBox";
			this.SettlementGroupTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.SettlementGroupTextBox.TabIndex = 0;
			//
			// GlobalAccountCreditStatusGroupBox
			//
			this.GlobalAccountCreditStatusGroupBox.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("EnquiryFilterControl|eac7650c-1ba8-4b08-8440-b4884fd08c62", "Global Account Balance and Credit Status");
			this.GlobalAccountCreditStatusGroupBox.Controls.Add(this.GlobalCreditGroupTextBox);
			this.GlobalAccountCreditStatusGroupBox.Controls.Add(this.GlobalCreditLimitCalcEdit);
			this.GlobalAccountCreditStatusGroupBox.Controls.Add(this.GlobalCreditAvailableCalcEdit);
			this.GlobalAccountCreditStatusGroupBox.Controls.Add(this.GlobalCreditCurrencyTextBox);
			this.GlobalAccountCreditStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 129, true);
			this.GlobalAccountCreditStatusGroupBox.Name = "GlobalAccountCreditStatusGroupBox";
			this.GlobalAccountCreditStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 52, true);
			this.GlobalAccountCreditStatusGroupBox.TabIndex = 7;
			this.GlobalAccountCreditStatusGroupBox.TabStop = false;
			//
			// GlobalCreditGroupTextBox
			//
			this.BindingSource.SetBindingMember(this.GlobalCreditGroupTextBox, "GlobalCreditGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).GlobalCreditGroup)));
			this.GlobalCreditGroupTextBox.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("3fd749b1-d8c6-4a67-aed2-f021eefdf447", "Global Credit Group");
			this.GlobalCreditGroupTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 19, true);
			this.GlobalCreditGroupTextBox.Name = "GlobalCreditGroupTextBox";
			this.GlobalCreditGroupTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
			this.GlobalCreditGroupTextBox.TabIndex = 1;
			//
			// GlobalCreditLimitCalcEdit
			//
			this.GlobalCreditLimitCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.GlobalCreditLimitCalcEdit, "GlobalCreditLimit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).GlobalCreditLimit)));
			this.GlobalCreditLimitCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("118bd1d6-7ba8-4f7c-a717-f9307ea3cb96", "Global Credit Limit");
			this.GlobalCreditLimitCalcEdit.DecimalPlaces = 2;
			this.GlobalCreditLimitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 19, true);
			this.GlobalCreditLimitCalcEdit.Name = "GlobalCreditLimitCalcEdit";
			this.GlobalCreditLimitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.GlobalCreditLimitCalcEdit.TabIndex = 2;
			this.GlobalCreditLimitCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// GlobalCreditAvailableCalcEdit
			//
			this.GlobalCreditAvailableCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.GlobalCreditAvailableCalcEdit, "GlobalCreditAvailable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).GlobalCreditAvailable)));
			this.GlobalCreditAvailableCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("c65dab57-ab12-4eb5-9874-e4deb68b6a3b", "Global Credit Available");
			this.GlobalCreditAvailableCalcEdit.DecimalPlaces = 2;
			this.GlobalCreditAvailableCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(515, 19, true);
			this.GlobalCreditAvailableCalcEdit.Name = "GlobalCreditAvailableCalcEdit";
			this.GlobalCreditAvailableCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.GlobalCreditAvailableCalcEdit.TabIndex = 3;
			this.GlobalCreditAvailableCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// GlobalCreditCurrencyTextBox
			//
			this.BindingSource.SetBindingMember(this.GlobalCreditCurrencyTextBox, "GlobalCreditCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).GlobalCreditCurrency)));
			this.GlobalCreditCurrencyTextBox.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("57a2bd59-bae9-4af6-9f86-beeb3858dc5f", "Currency");
			this.GlobalCreditCurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(668, 19, true);
			this.GlobalCreditCurrencyTextBox.Name = "GlobalCreditCurrencyTextBox";
			this.GlobalCreditCurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.GlobalCreditCurrencyTextBox.TabIndex = 4;
			//
			// overdueGroupBox
			//
			this.overdueGroupBox.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("bafee16d-537f-4bd5-86f7-82a0732fac84", "Overdue");
			this.overdueGroupBox.Controls.Add(this.totalOverdueCalcEdit);
			this.overdueGroupBox.Controls.Add(this.dsbOverdueCalcEdit);
			this.overdueGroupBox.Controls.Add(this.stdOverdueCalcEdit);
			this.overdueGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(730, 131, true);
			this.overdueGroupBox.Name = "overdueGroupBox";
			this.overdueGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 95, true);
			this.overdueGroupBox.TabIndex = 16;
			this.overdueGroupBox.TabStop = false;
			//
			// totalOverdueCalcEdit
			//
			this.BindingSource.SetBindingMember(this.totalOverdueCalcEdit, "TotalOverdueAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).TotalOverdueAmount)));
			this.totalOverdueCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("3ec7dfa7-eb0d-4c26-be5b-f4fa8a66389c", "Total");
			this.totalOverdueCalcEdit.DecimalPlaces = 2;
			this.totalOverdueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 64, true);
			this.totalOverdueCalcEdit.Name = "totalOverdueCalcEdit";
			this.totalOverdueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.totalOverdueCalcEdit.TabIndex = 2;
			this.totalOverdueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// dsbOverdueCalcEdit
			//
			this.BindingSource.SetBindingMember(this.dsbOverdueCalcEdit, "DisbursementOverdueAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).DisbursementOverdueAmount)));
			this.dsbOverdueCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("2f049349-9f05-4f40-9cd1-777d8f581986", "Disbursement");
			this.dsbOverdueCalcEdit.DecimalPlaces = 2;
			this.dsbOverdueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 41, true);
			this.dsbOverdueCalcEdit.Name = "dsbOverdueCalcEdit";
			this.dsbOverdueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.dsbOverdueCalcEdit.TabIndex = 1;
			this.dsbOverdueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// stdOverdueCalcEdit
			//
			this.BindingSource.SetBindingMember(this.stdOverdueCalcEdit, "StandardOverdueAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).StandardOverdueAmount)));
			this.stdOverdueCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("085b865f-6f3e-4842-9bf1-39f401af8471", "Standard");
			this.stdOverdueCalcEdit.DecimalPlaces = 2;
			this.stdOverdueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 18, true);
			this.stdOverdueCalcEdit.Name = "stdOverdueCalcEdit";
			this.stdOverdueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.stdOverdueCalcEdit.TabIndex = 0;
			this.stdOverdueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// creditOnHoldCheckBox
			//
			this.BindingSource.SetBindingMember(this.creditOnHoldCheckBox, "IsCreditOnHold");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).IsCreditOnHold)));
			this.creditOnHoldCheckBox.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("e59bfdcf-5d81-4449-be6d-bfdddf925ad4", "Credit On Hold");
			this.creditOnHoldCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.creditOnHoldCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.creditOnHoldCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(757, 232, true);
			this.creditOnHoldCheckBox.Name = "creditOnHoldCheckBox";
			this.creditOnHoldCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 24, true);
			this.creditOnHoldCheckBox.TabIndex = 17;
			this.creditOnHoldCheckBox.UseVisualStyleBackColor = true;
			//
			// TotalBatchedOutstandingAmountCalcEdit
			//
			this.TotalBatchedOutstandingAmountCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TotalBatchedOutstandingAmountCalcEdit, "BatchedOutstandingAmountTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).BatchedOutstandingAmountTotal)));
			this.TotalBatchedOutstandingAmountCalcEdit.CaptionResourceString = null;
			this.TotalBatchedOutstandingAmountCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalBatchedOutstandingAmountCalcEdit, false);
			this.TotalBatchedOutstandingAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 139, true);
			this.TotalBatchedOutstandingAmountCalcEdit.Name = "TotalBatchedOutstandingAmountCalcEdit";
			this.TotalBatchedOutstandingAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.TotalBatchedOutstandingAmountCalcEdit.TabIndex = 35;
			this.TotalBatchedOutstandingAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ThreePeriodTotalBatchedCalcEdit
			//
			this.ThreePeriodTotalBatchedCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.ThreePeriodTotalBatchedCalcEdit, "ThreePeriodBatchedTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).ThreePeriodBatchedTotal)));
			this.ThreePeriodTotalBatchedCalcEdit.CaptionResourceString = null;
			this.ThreePeriodTotalBatchedCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ThreePeriodTotalBatchedCalcEdit, false);
			this.ThreePeriodTotalBatchedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 139, true);
			this.ThreePeriodTotalBatchedCalcEdit.Name = "ThreePeriodTotalBatchedCalcEdit";
			this.ThreePeriodTotalBatchedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.ThreePeriodTotalBatchedCalcEdit.TabIndex = 34;
			this.ThreePeriodTotalBatchedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// TwoPeriodTotalBatchedCalcEdit
			//
			this.TwoPeriodTotalBatchedCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.TwoPeriodTotalBatchedCalcEdit, "TwoPeriodBatchedTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).TwoPeriodBatchedTotal)));
			this.TwoPeriodTotalBatchedCalcEdit.CaptionResourceString = null;
			this.TwoPeriodTotalBatchedCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TwoPeriodTotalBatchedCalcEdit, false);
			this.TwoPeriodTotalBatchedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 139, true);
			this.TwoPeriodTotalBatchedCalcEdit.Name = "TwoPeriodTotalBatchedCalcEdit";
			this.TwoPeriodTotalBatchedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.TwoPeriodTotalBatchedCalcEdit.TabIndex = 33;
			this.TwoPeriodTotalBatchedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// OnePeriodTotalBatchedCalcEdit
			//
			this.OnePeriodTotalBatchedCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OnePeriodTotalBatchedCalcEdit, "OnePeriodBatchedTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).OnePeriodBatchedTotal)));
			this.OnePeriodTotalBatchedCalcEdit.CaptionResourceString = null;
			this.OnePeriodTotalBatchedCalcEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.OnePeriodTotalBatchedCalcEdit, false);
			this.OnePeriodTotalBatchedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 139, true);
			this.OnePeriodTotalBatchedCalcEdit.Name = "OnePeriodTotalBatchedCalcEdit";
			this.OnePeriodTotalBatchedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.OnePeriodTotalBatchedCalcEdit.TabIndex = 32;
			this.OnePeriodTotalBatchedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// CurrentTotalBatchedCalcEdit
			//
			this.CurrentTotalBatchedCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.CurrentTotalBatchedCalcEdit, "CurrentBatchedTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Module.AREnquiryFilterBusinessObject)(null)).CurrentBatchedTotal)));
			this.CurrentTotalBatchedCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("5fd69e15-887a-4db2-a309-bdc891e1380b", "Total (Batched)");
			this.CurrentTotalBatchedCalcEdit.DecimalPlaces = 2;
			this.CurrentTotalBatchedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 139, true);
			this.CurrentTotalBatchedCalcEdit.Name = "CurrentTotalBatchedCalcEdit";
			this.CurrentTotalBatchedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 20, true);
			this.CurrentTotalBatchedCalcEdit.TabIndex = 31;
			this.CurrentTotalBatchedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// AREnquiryFilterControl
			//
			this.Name = "AREnquiryFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(989, 580, true);
			this.InvoiceAgeingGroupBox.ResumeLayout(false);
			this.InvoiceAgeingGroupBox.PerformLayout();
			this.AccountBalanceGroupBox.ResumeLayout(false);
			this.AccountBalanceGroupBox.PerformLayout();
			this.ReadOnlyDetailsPanel.ResumeLayout(false);
			this.ReadOnlyDetailsPanel.PerformLayout();
			this.AverageDaysToFullyPayGroupBox.ResumeLayout(false);
			this.AverageDaysToFullyPayGroupBox.PerformLayout();
			this.AgingOptionsDropEdit1.ResumeLayout(true);
			this.AgingOptionsDropEdit1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TreatDisbAsStandardGroupBox.ResumeLayout(false);
			this.TreatDisbAsStandardGroupBox.PerformLayout();
			this.SettlementGroupGroupBox.ResumeLayout(false);
			this.SettlementGroupGroupBox.PerformLayout();
			this.GlobalAccountCreditStatusGroupBox.ResumeLayout(false);
			this.GlobalAccountCreditStatusGroupBox.PerformLayout();
			this.overdueGroupBox.ResumeLayout(false);
			this.overdueGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
