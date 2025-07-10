using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	public class UCC6TemporaryStoragePackedItemDetailsControlBag : ControlBag
	{
		public UCC6TemporaryStoragePackedItemDetailsControlBag()
		{
			SeqTextBox = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.SeqCalcEdit));
			TariffCodeFindBox = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.TariffCodeFindBox));
			GoodDescriptionTextBox = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.GoodsDescriptionTextBox));
			GrossWeightCalcDropEdit = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.GrossWeightCalcDropEdit));
			CusCodeCodeFindBox = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.CusCodeFindBox));
			CountryOfOriginDropEdit = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.CountryOfOriginDropEdit));
			CustomsValueCalcDropEdit = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.CustomsValueCalcDropEdit));
			SupplementaryUnitsCalcDropEdit = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.SupplementaryUnitsCalcDropEdit));
			CustomsSecondQuantityDropEdit = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.CustomsSecondQuantityDropEdit));
			CustomsThirdQuantityDropEdit = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.CustomsThirdQuantityDropEdit));
			AdditionalSupplementaryCodesUserControl = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.AdditionalSupplementaryCodesUserControl));
			NetWeightCalcDropEdit = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.NetWeightCalcDropEdit));
			DutiesAndTaxesGrid = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.DutiesAndTaxesGrid));
			DutiesAndTaxesLabel = RegisterControl(nameof(UCC6TemporaryStoragePackedItemDetailsControl.DutiesAndTaxesLabel));
		}

		public ControlReference SeqTextBox { get; }

		public ControlReference TariffCodeFindBox { get; }

		public ControlReference GoodDescriptionTextBox { get; }

		public ControlReference GrossWeightCalcDropEdit { get; }

		public ControlReference CusCodeCodeFindBox { get; }

		public ControlReference CountryOfOriginDropEdit { get; }

		public ControlReference CustomsValueCalcDropEdit { get; }

		public ControlReference SupplementaryUnitsCalcDropEdit { get; }

		public ControlReference CustomsSecondQuantityDropEdit { get; }

		public ControlReference CustomsThirdQuantityDropEdit { get; }

		public ControlReference AdditionalSupplementaryCodesUserControl { get; }

		public ControlReference NetWeightCalcDropEdit { get; }

		public ControlReference DutiesAndTaxesGrid { get; }

		public ControlReference DutiesAndTaxesLabel { get; }

		public static UCC6TemporaryStoragePackedItemDetailsControlBag Instance => uCC6TemporaryStoragePackedItemDetailsControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<UCC6TemporaryStoragePackedItemDetailsControlBag> uCC6TemporaryStoragePackedItemDetailsControlBag = new Lazy<UCC6TemporaryStoragePackedItemDetailsControlBag>(() => new UCC6TemporaryStoragePackedItemDetailsControlBag());
		protected override Control CreateTemplate() => new UCC6TemporaryStoragePackedItemDetailsControl();
	}
}
