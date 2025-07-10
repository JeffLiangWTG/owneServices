using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CO.Manifest.GUI
{
	public sealed class COBillControlBag : ControlBag
	{
		public static COBillControlBag Instance => billControlBag.Value;

		COBillControlBag()
		{
			CargoDispositionDropEdit = RegisterControl(nameof(COBillCountrySpecificUserControl.CargoDispositionDropEdit));
			TravelDocumentTypeDropEdit = RegisterControl(nameof(COBillCountrySpecificUserControl.TravelDocumentTypeDropEdit));
			MultimodalCheckBox = RegisterControl(nameof(COBillCountrySpecificUserControl.MultimodalCheckBox));
			CarriersLiabilityCheckBox = RegisterControl(nameof(COBillCountrySpecificUserControl.CarriersLiabilityCheckBox));
			GoodsValueConvertToLocalCurrencyControl = RegisterControl(nameof(COBillCountrySpecificUserControl.GoodsValueConvertToLocalCurrencyControl));
			BillIssueDateEdit = RegisterControl(nameof(COBillCountrySpecificUserControl.BillIssueDateEdit));
		}

		public ControlReference CargoDispositionDropEdit { get; }
		public ControlReference TravelDocumentTypeDropEdit { get; }
		public ControlReference MultimodalCheckBox { get; }
		public ControlReference CarriersLiabilityCheckBox { get; }
		public ControlReference GoodsValueConvertToLocalCurrencyControl { get; }
		public ControlReference BillIssueDateEdit { get; }

		protected override Control CreateTemplate() => new COBillCountrySpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<COBillControlBag> billControlBag = new Lazy<COBillControlBag>(() => new COBillControlBag());
	}
}
