using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class RefundDeclarationControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new RefundDeclarationDetailUserControl();

		[ThreadStatic]
		static RefundDeclarationControlBag instance;

		public static RefundDeclarationControlBag Instance => instance ?? (instance = new RefundDeclarationControlBag());
		public RefundDeclarationControlBag()
		{
			EntryNumberTextBox = RegisterControl(nameof(RefundDeclarationDetailUserControl.EntryNumberTextBox));
			TotalRefundAmountCalcEdit = RegisterControl(nameof(RefundDeclarationDetailUserControl.TotalRefundAmountCalcEdit));
			MessageStatusDropEdit = RegisterControl(nameof(RefundDeclarationDetailUserControl.MessageStatusDropEdit));
			EntryStatusDropEdit = RegisterControl(nameof(RefundDeclarationDetailUserControl.EntryStatusDropEdit));
			AcceptedDateEdit = RegisterControl(nameof(RefundDeclarationDetailUserControl.AcceptedDateEdit));
			RefundApprovalDateEdit = RegisterControl(nameof(RefundDeclarationDetailUserControl.RefundApprovalDateEdit));
			RefundApprovalNumberTextBox = RegisterControl(nameof(RefundDeclarationDetailUserControl.RefundApprovalNumberTextBox));
			ProvisionDateEdit = RegisterControl(nameof(RefundDeclarationDetailUserControl.ProvisionDateEdit));
			ProvisionNumberTextBox = RegisterControl(nameof(RefundDeclarationDetailUserControl.ProvisionNumberTextBox));
			RefundTypeDropEdit = RegisterControl(nameof(RefundDeclarationDetailUserControl.RefundTypeDropEdit));
			RefundCauseDropEdit = RegisterControl(nameof(RefundDeclarationDetailUserControl.RefundCauseDropEdit));
			RefundReasonDropEdit = RegisterControl(nameof(RefundDeclarationDetailUserControl.RefundReasonDropEdit));
			CustomsOfficeCodeFindBox = RegisterControl(nameof(RefundDeclarationDetailUserControl.CustomsOfficeCodeFindBox));
			DepartmentCodeFindBox = RegisterControl(nameof(RefundDeclarationDetailUserControl.DepartmentCodeFindBox));
			TaxOfficeCodeFindBox = RegisterControl(nameof(RefundDeclarationDetailUserControl.TaxOfficeCodeFindBox));
			BranchCodeGuidFindBox = RegisterControl(nameof(RefundDeclarationDetailUserControl.BranchCodeGuidFindBox));
			BrokerCodeFindBox = RegisterControl(nameof(RefundDeclarationDetailUserControl.BrokerCodeFindBox));
			PayerAddressControl = RegisterControl(nameof(RefundDeclarationDetailUserControl.PayerAddressControl));
			RegistrationNumberOneTextBox = RegisterControl(nameof(RefundDeclarationDetailUserControl.RegistrationNumberOneTextBox));
			KoreanRegistrationNumberOfCEOTextBox = RegisterControl(nameof(RefundDeclarationDetailUserControl.KoreanRegistrationNumberOfCEOTextBox));
			PayerBankDropEdit = RegisterControl(nameof(RefundDeclarationDetailUserControl.PayerBankDropEdit));
			BankAccountNumberTextBox = RegisterControl(nameof(RefundDeclarationDetailUserControl.BankAccountNumberTextBox));
		}

		public ControlReference EntryNumberTextBox { get; }
		public ControlReference TotalRefundAmountCalcEdit { get; }
		public ControlReference MessageStatusDropEdit { get; }
		public ControlReference EntryStatusDropEdit { get; }
		public ControlReference AcceptedDateEdit { get; }
		public ControlReference RefundApprovalDateEdit { get; }
		public ControlReference RefundApprovalNumberTextBox { get; }
		public ControlReference ProvisionDateEdit { get; }
		public ControlReference ProvisionNumberTextBox { get; }
		public ControlReference RefundTypeDropEdit { get; }
		public ControlReference RefundCauseDropEdit { get; }
		public ControlReference RefundReasonDropEdit { get; }
		public ControlReference CustomsOfficeCodeFindBox { get; }
		public ControlReference DepartmentCodeFindBox { get; }
		public ControlReference TaxOfficeCodeFindBox { get; }
		public ControlReference BranchCodeGuidFindBox { get; }
		public ControlReference BrokerCodeFindBox { get; }
		public ControlReference PayerAddressControl { get; }
		public ControlReference BankAccountNumberTextBox { get; }
		public ControlReference RegistrationNumberOneTextBox { get; }
		public ControlReference KoreanRegistrationNumberOfCEOTextBox { get; }
		public ControlReference PayerBankDropEdit { get; }
	}
}
