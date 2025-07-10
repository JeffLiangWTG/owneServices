using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class MiscOptionsControlBag : ControlBag
	{
		public static MiscOptionsControlBag Instance => instance ?? (instance = new MiscOptionsControlBag());

		[ThreadStatic]
		static MiscOptionsControlBag instance;

		protected override Control CreateTemplate() => new MiscOptionsUserControl();

		MiscOptionsControlBag()
		{
			ReturnReasonDropEdit = RegisterControl(nameof(MiscOptionsUserControl.ReturnReasonDropEdit));
			ReturnTypeDropEdit = RegisterControl(nameof(MiscOptionsUserControl.ReturnTypeDropEdit));
			SouthNorthTradeDropEdit = RegisterControl(nameof(MiscOptionsUserControl.SouthNorthTradeDropEdit));
			SouthNorthTradeAreaDropEdit = RegisterControl(nameof(MiscOptionsUserControl.SouthNorthTradeAreaDropEdit));
			BondedTransportationPeriodUserControl = RegisterControl(nameof(MiscOptionsUserControl.BondedTransportationPeriodUserControl));
			UCRTextBox = RegisterControl(nameof(MiscOptionsUserControl.UCRTextBox));
			LateDecPenaltyDateCodeDropEdit = RegisterControl(nameof(MiscOptionsUserControl.LateDecPenaltyDateCodeDropEdit));
			MissedDecPenaltyRateCalcEdit = RegisterControl(nameof(MiscOptionsUserControl.MissedDecPenaltyRateCalcEdit));
			PercentageLabel = RegisterControl(nameof(MiscOptionsUserControl.PercentageLabel));
			TaxOfficeCodeFindBox = RegisterControl(nameof(MiscOptionsUserControl.TaxOfficeCodeFindBox));
		}

		public ControlReference ReturnReasonDropEdit { get; }
		public ControlReference ReturnTypeDropEdit { get; }
		public ControlReference SouthNorthTradeDropEdit { get; }
		public ControlReference SouthNorthTradeAreaDropEdit { get; }
		public ControlReference BondedTransportationPeriodUserControl { get; }
		public ControlReference UCRTextBox { get; }
		public ControlReference LateDecPenaltyDateCodeDropEdit { get; }
		public ControlReference MissedDecPenaltyRateCalcEdit { get; }
		public ControlReference PercentageLabel { get; }
		public ControlReference TaxOfficeCodeFindBox { get; }
	}
}
