using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public class EUH7ItemDetailsCommonControlBag : ControlBag
	{
		public static EUH7ItemDetailsCommonControlBag Instance => packedItemDetailControlBag.Value;

		public EUH7ItemDetailsCommonControlBag()
		{
			TariffFindBox = RegisterControl(nameof(EUH7ItemDetailsFieldsUserControl.TariffFindBox));
			IntrinsicValueConvertToLocalCurrencyControl = RegisterControl(nameof(EUH7ItemDetailsFieldsUserControl.IntrinsicValueConvertToLocalCurrencyControl));
			GoodsOriginCodeFindBox = RegisterControl(nameof(EUH7ItemDetailsFieldsUserControl.GoodsOriginCodeFindBox));
			SupplementaryDropEdit = RegisterControl(nameof(EUH7ItemDetailsFieldsUserControl.SupplementaryDropEdit));
			GoodsDescriptionTextBox = RegisterControl(nameof(EUH7ItemDetailsFieldsUserControl.GoodsDescriptionTextBox));
			CustomEntriesSeparatorUserControl = RegisterControl(nameof(EUH7ItemDetailsFieldsUserControl.CustomEntriesSeparatorUserControl));
			CustomEntriesGrid = RegisterControl(nameof(EUH7ItemDetailsFieldsUserControl.CustomEntriesGrid));
			GrossWeightCalcDropEdit = RegisterControl(nameof(EUH7ItemDetailsFieldsUserControl.GrossWeightCalcDropEdit));
			NetWeightCalcDropEdit = RegisterControl(nameof(EUH7ItemDetailsFieldsUserControl.NetWeightCalcDropEdit));
			QuantityCalcEdit = RegisterControl(nameof(EUH7ItemDetailsFieldsUserControl.QuantityCalcEdit));
		}

		public ControlReference TariffFindBox { get; }
		public ControlReference IntrinsicValueConvertToLocalCurrencyControl { get; }
		public ControlReference GoodsOriginCodeFindBox { get; }
		public ControlReference SupplementaryDropEdit { get; }
		public ControlReference GoodsDescriptionTextBox { get; }
		public ControlReference CustomEntriesSeparatorUserControl { get; }
		public ControlReference CustomEntriesGrid { get; }
		public ControlReference GrossWeightCalcDropEdit { get; }
		public ControlReference NetWeightCalcDropEdit { get; }
		public ControlReference QuantityCalcEdit { get; }

		protected override Control CreateTemplate() => new EUH7ItemDetailsFieldsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		readonly static Lazy<EUH7ItemDetailsCommonControlBag> packedItemDetailControlBag = new(() => new EUH7ItemDetailsCommonControlBag());
	}
}
