using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class StatementHeaderControlBag : ControlBag
	{
		StatementHeaderControlBag()
		{
			ProcessPortCodeFindBox = RegisterControl(nameof(StatementUserControl.ProcessPortCodeFindBox));
			ProcessAndDueDateUserControl = RegisterControl(nameof(StatementUserControl.ProcessAndDueDateUserControl));
			StatementAmountCalcEdit = RegisterControl(nameof(StatementUserControl.StatementAmountCalcEdit));
			FormattedNumberTextBox = RegisterControl(nameof(StatementUserControl.FormattedNumberTextBox));
			StatusDropEdit = RegisterControl(nameof(StatementUserControl.StatusDropEdit));
			StatementTypeDropEdit = RegisterControl(nameof(StatementUserControl.StatementTypeDropEdit));
			PeriodDateUserControl = RegisterControl(nameof(StatementUserControl.PeriodDateUserControl));
			ImporterGuidFindBox = RegisterControl(nameof(StatementUserControl.ImporterGuidFindBox));
			PayerFromCustomsTextBox = RegisterControl(nameof(StatementUserControl.PayerFromCustomsTextBox));
			RelatedFormattedAccountNumberCodeFindBox = RegisterControl(nameof(StatementUserControl.RelatedFormattedAccountNumberCodeFindBox));
			RelatedProcessDateEdit = RegisterControl(nameof(StatementUserControl.RelatedProcessDateEdit));
			ProcessDateEdit = RegisterControl(nameof(StatementUserControl.ProcessDateEdit));
			PaymentDateEdit = RegisterControl(nameof(StatementUserControl.PaymentDateEdit));
			TotalVATAmountCalcEdit = RegisterControl(nameof(StatementUserControl.TotalVATAmountCalcEdit));
			PaymentTypeDropEdit = RegisterControl(nameof(StatementUserControl.PaymentTypeDropEdit));
			PaymentPartyDropEdit = RegisterControl(nameof(StatementUserControl.PaymentPartyDropEdit));
			FormattedEntryNumberTextBox = RegisterControl(nameof(StatementUserControl.FormattedEntryNumberTextBox));
			BillTypeDropEdit = RegisterControl(nameof(StatementUserControl.BillTypeDropEdit));
			CustomsAccountIDTextBox = RegisterControl(nameof(StatementUserControl.CustomsAccountIDTextBox));
			DueDateEdit = RegisterControl(nameof(StatementUserControl.DueDateEdit));
			IssueDateEdit = RegisterControl(nameof(StatementUserControl.IssueDateEdit));
			TotalAmountAfterDueDateCalcEdit = RegisterControl(nameof(StatementUserControl.TotalAmountAfterDueDateCalcEdit));
		}

		public static StatementHeaderControlBag Instance => instance ?? (instance = new StatementHeaderControlBag());

		[ThreadStatic]
		static StatementHeaderControlBag instance;

		protected override Control CreateTemplate() => new StatementUserControl();

		public ControlReference ProcessPortCodeFindBox { get; }
		public ControlReference ProcessAndDueDateUserControl { get; }
		public ControlReference StatementAmountCalcEdit { get; }
		public ControlReference FormattedNumberTextBox { get; }
		public ControlReference StatusDropEdit { get; }
		public ControlReference StatementTypeDropEdit { get; }
		public ControlReference PeriodDateUserControl { get; }
		public ControlReference ImporterGuidFindBox { get; }
		public ControlReference PayerFromCustomsTextBox { get; }
		public ControlReference RelatedFormattedAccountNumberCodeFindBox { get; }
		public ControlReference RelatedProcessDateEdit { get; }
		public ControlReference ProcessDateEdit { get; }
		public ControlReference PaymentDateEdit { get; }
		public ControlReference TotalVATAmountCalcEdit { get; }
		public ControlReference PaymentTypeDropEdit { get; }
		public ControlReference PaymentPartyDropEdit { get; }
		public ControlReference FormattedEntryNumberTextBox { get; }
		public ControlReference BillTypeDropEdit { get; }
		public ControlReference CustomsAccountIDTextBox { get; }
		public ControlReference DueDateEdit { get; }
		public ControlReference IssueDateEdit { get; }
		public ControlReference TotalAmountAfterDueDateCalcEdit { get; }
	}
}
