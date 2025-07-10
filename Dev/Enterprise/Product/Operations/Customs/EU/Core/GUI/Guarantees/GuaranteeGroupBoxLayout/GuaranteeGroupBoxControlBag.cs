using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class GuaranteeGroupBoxControlBag : ControlBag
	{
		public static GuaranteeGroupBoxControlBag Instance => GuaranteeControlBag.Value;
		GuaranteeGroupBoxControlBag()
		{
			BondNumberCodeFindBox = RegisterControl(nameof(GuaranteeGroupBoxUserControl.BondNumberCodeFindBox));
			AmountCalcDropEdit = RegisterControl(nameof(GuaranteeGroupBoxUserControl.AmountCalcDropEdit));
			OverrideCheckBox = RegisterControl(nameof(GuaranteeGroupBoxUserControl.OverrideCheckBox));
		}

		public ControlReference BondNumberCodeFindBox { get; }

		public ControlReference AmountCalcDropEdit { get; }

		public ControlReference OverrideCheckBox { get; }

		protected override Control CreateTemplate() => new GuaranteeGroupBoxUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<GuaranteeGroupBoxControlBag> GuaranteeControlBag = new (() => new GuaranteeGroupBoxControlBag());
	}
}
