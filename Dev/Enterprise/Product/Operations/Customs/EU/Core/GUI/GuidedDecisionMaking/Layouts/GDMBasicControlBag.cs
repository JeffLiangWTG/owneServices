using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class GDMBasicControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new GDMBasicUserControl();

		public static GDMBasicControlBag Instance => instance ?? (instance = new GDMBasicControlBag());

		[ThreadStatic]
		static GDMBasicControlBag instance;

		GDMBasicControlBag()
		{
			EffectiveDateDateEdit = RegisterControl(nameof(GDMBasicUserControl.EffectiveDateDateEdit));
			TariffCodeFindBox = RegisterControl(nameof(GDMBasicUserControl.TariffCodeFindBox));
			CountryOfOriginDropEdit = RegisterControl(nameof(GDMBasicUserControl.CountryOfOriginDropEdit));
			CountryOfDestinationDropEdit = RegisterControl(nameof(GDMBasicUserControl.CountryOfDestinationDropEdit));
			PreferenceDropEdit = RegisterControl(nameof(GDMBasicUserControl.PreferenceDropEdit));
			QuotaOrderNumberDropEdit = RegisterControl(nameof(GDMBasicUserControl.QuotaOrderNumberDropEdit));
			CustomsFirstQuantityCalcEdit = RegisterControl(nameof(GDMBasicUserControl.CustomsFirstQuantityCalcEdit));
			CustomsSecondQuantityCalcDropEdit = RegisterControl(nameof(GDMBasicUserControl.CustomsSecondQuantityCalcDropEdit));
			CustomsThirdQuantityCalcDropEdit = RegisterControl(nameof(GDMBasicUserControl.CustomsThirdQuantityCalcDropEdit));
		}

		public ControlReference EffectiveDateDateEdit { get; }
		public ControlReference TariffCodeFindBox { get; }
		public ControlReference CountryOfOriginDropEdit { get; }
		public ControlReference CountryOfDestinationDropEdit { get; }
		public ControlReference PreferenceDropEdit { get; }
		public ControlReference QuotaOrderNumberDropEdit { get; }
		public ControlReference CustomsFirstQuantityCalcEdit { get; }
		public ControlReference CustomsSecondQuantityCalcDropEdit { get; }
		public ControlReference CustomsThirdQuantityCalcDropEdit { get; }
	}
}
