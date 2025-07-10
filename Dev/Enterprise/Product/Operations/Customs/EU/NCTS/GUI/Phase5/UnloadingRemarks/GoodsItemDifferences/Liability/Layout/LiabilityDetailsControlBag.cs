using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public sealed class LiabilityDetailsControlBag : ControlBag
	{
		LiabilityDetailsControlBag()
		{
			CountryOfOriginDropEdit = RegisterControl(nameof(LiabilityDetailsUserControl.CountryOfOriginDropEdit));
			CommodityCodeTariffFindBox = RegisterControl(nameof(LiabilityDetailsUserControl.CommodityCodeTariffFindBox));
			SupplementaryUnitsCalcDropEdit = RegisterControl(nameof(LiabilityDetailsUserControl.SupplementaryUnitsCalcDropEdit));
			CustomsThirdQuantityDropEdit = RegisterControl(nameof(LiabilityDetailsUserControl.CustomsThirdQuantityDropEdit));
			CustomsFourthQuantityDropEdit = RegisterControl(nameof(LiabilityDetailsUserControl.CustomsFourthQuantityDropEdit));
			CustomsValueCalcDropEdit = RegisterControl(nameof(LiabilityDetailsUserControl.CustomsValueCalcDropEdit));
			AdditionalSupplementaryCodesUserControl = RegisterControl(nameof(LiabilityDetailsUserControl.AdditionalSupplementaryCodesUserControl));
			FeesUserControl = RegisterControl(nameof(LiabilityDetailsUserControl.FeesUserControl));
		}

		public static LiabilityDetailsControlBag Instance => instance ?? (instance = new LiabilityDetailsControlBag());

		[ThreadStatic]
		static LiabilityDetailsControlBag instance;

		public ControlReference CountryOfOriginDropEdit { get; }

		public ControlReference CommodityCodeTariffFindBox { get; }

		public ControlReference SupplementaryUnitsCalcDropEdit { get; }

		public ControlReference CustomsThirdQuantityDropEdit { get; }

		public ControlReference CustomsFourthQuantityDropEdit { get; }

		public ControlReference CustomsValueCalcDropEdit { get; }

		public ControlReference AdditionalSupplementaryCodesUserControl { get; }

		public ControlReference FeesUserControl { get; }

		protected override Control CreateTemplate() => new LiabilityDetailsUserControl();
	}
}
