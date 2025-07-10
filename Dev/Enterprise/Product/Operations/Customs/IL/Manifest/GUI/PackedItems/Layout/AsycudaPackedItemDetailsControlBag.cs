using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	public class AsycudaPackedItemDetailsControlBag : ControlBag
	{
		public AsycudaPackedItemDetailsControlBag()
		{
			SeqTextBox = RegisterControl(nameof(AsycudaPackedItemDetailsControl.SeqCalcEdit));
			TariffCodeFindBox = RegisterControl(nameof(AsycudaPackedItemDetailsControl.TariffCodeFindBox));
			GoodDescriptionTextBox = RegisterControl(nameof(AsycudaPackedItemDetailsControl.GoodsDescriptionTextBox));
			GrossWeightCalcEdit = RegisterControl(nameof(AsycudaPackedItemDetailsControl.GrossWeightCalcEdit));
			UQDropCodeBox = RegisterControl(nameof(AsycudaPackedItemDetailsControl.UQDropEdit));
			PackStatusDropCodeBox = RegisterControl(nameof(AsycudaPackedItemDetailsControl.PackStatusDropEdit));
		}

		public ControlReference SeqTextBox { get; }

		public ControlReference TariffCodeFindBox { get; }

		public ControlReference GoodDescriptionTextBox { get; }

		public ControlReference GrossWeightCalcEdit { get; }

		public ControlReference UQDropCodeBox { get; }

		public ControlReference PackStatusDropCodeBox { get; }

		public static AsycudaPackedItemDetailsControlBag Instance => asycudaPackedItemDetailsControlBag.Value;

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<AsycudaPackedItemDetailsControlBag> asycudaPackedItemDetailsControlBag = new Lazy<AsycudaPackedItemDetailsControlBag>(() => new AsycudaPackedItemDetailsControlBag());
		protected override Control CreateTemplate() => new AsycudaPackedItemDetailsControl();
	}
}
