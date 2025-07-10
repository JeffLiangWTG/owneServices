using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class StatementLineControlBag : ControlBag
	{
		StatementLineControlBag()
		{
			DutyAmountCalcEdit = RegisterControl(nameof(StatementLineUserControl.DutyAmountCalcEdit));
			LiquorTaxCalcEdit = RegisterControl(nameof(StatementLineUserControl.LiquorTaxCalcEdit));
			AgricultureTaxCalcEdit = RegisterControl(nameof(StatementLineUserControl.AgricultureTaxCalcEdit));
			TransportationTaxCalcEdit = RegisterControl(nameof(StatementLineUserControl.TransportationTaxCalcEdit));
			EducationTaxCalcEdit = RegisterControl(nameof(StatementLineUserControl.EducationTaxCalcEdit));
			InterestCalcEdit = RegisterControl(nameof(StatementLineUserControl.InterestCalcEdit));
			SpecialConsumptionTaxCalcEdit = RegisterControl(nameof(StatementLineUserControl.SpecialConsumptionTaxCalcEdit));
			VATCalcEdit = RegisterControl(nameof(StatementLineUserControl.VATCalcEdit));
			DeclarationPenaltyCalcEdit = RegisterControl(nameof(StatementLineUserControl.DeclarationPenaltyCalcEdit));
		}

		public static StatementLineControlBag Instance => instance ?? (instance = new StatementLineControlBag());

		[ThreadStatic]
		static StatementLineControlBag instance;

		protected override Control CreateTemplate() => new StatementLineUserControl();

		public ControlReference DutyAmountCalcEdit { get; }
		public ControlReference LiquorTaxCalcEdit { get; }
		public ControlReference AgricultureTaxCalcEdit { get; }
		public ControlReference TransportationTaxCalcEdit { get; }
		public ControlReference EducationTaxCalcEdit { get; }
		public ControlReference InterestCalcEdit { get; }
		public ControlReference SpecialConsumptionTaxCalcEdit { get; }
		public ControlReference VATCalcEdit { get; }
		public ControlReference DeclarationPenaltyCalcEdit { get; }
	}
}
