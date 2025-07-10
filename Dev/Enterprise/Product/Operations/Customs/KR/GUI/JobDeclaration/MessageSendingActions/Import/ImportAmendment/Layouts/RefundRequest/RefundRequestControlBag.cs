using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class RefundRequestControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new RefundRequestUserControl();
		[ThreadStatic]
		static RefundRequestControlBag instance;

		public static RefundRequestControlBag Instance => instance ?? (instance = new RefundRequestControlBag());
		RefundRequestControlBag()
		{
			RefundTypeDropEdit = RegisterControl(nameof(RefundRequestUserControl.RefundTypeDropEdit));
			RefundCauseDropEdit = RegisterControl(nameof(RefundRequestUserControl.RefundCauseDropEdit));
			RefundReasonDropEdit = RegisterControl(nameof(RefundRequestUserControl.RefundReasonDropEdit));
			RefundSentWith5FEDropEdit = RegisterControl(nameof(RefundRequestUserControl.RefundSentWith5FEDropEdit));
			TaxOfficeCodeFindBox = RegisterControl(nameof(RefundRequestUserControl.TaxOfficeCodeFindBox));
			CustomsDisbursementBillTextBox = RegisterControl(nameof(RefundRequestUserControl.CustomsDisbursementBillTextBox));
			VersionNoCalcEdit = RegisterControl(nameof(RefundRequestUserControl.VersionNoCalcEdit));
			RefundAmountOfValueForVATCalcEdit = RegisterControl(nameof(RefundRequestUserControl.RefundAmountOfValueForVATCalcEdit));
			RefundAmountOfVATExemptionValueCalcEdit = RegisterControl(nameof(RefundRequestUserControl.RefundAmountOfVATExemptionValueCalcEdit));
		}

		public ControlReference RefundTypeDropEdit { get; }
		public ControlReference RefundCauseDropEdit { get; }
		public ControlReference RefundReasonDropEdit { get; }
		public ControlReference RefundSentWith5FEDropEdit { get; }
		public ControlReference TaxOfficeCodeFindBox { get; }
		public ControlReference CustomsDisbursementBillTextBox { get; }
		public ControlReference VersionNoCalcEdit { get; }
		public ControlReference RefundAmountOfValueForVATCalcEdit { get; }
		public ControlReference RefundAmountOfVATExemptionValueCalcEdit { get; }
	}
}
