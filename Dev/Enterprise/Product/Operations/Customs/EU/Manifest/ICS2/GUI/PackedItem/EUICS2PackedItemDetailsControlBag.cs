using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	public class EUICS2PackedItemDetailsControlBag : ControlBag
	{
		public static EUICS2PackedItemDetailsControlBag Instance => packedItemDetailsControlBag.Value;

		EUICS2PackedItemDetailsControlBag()
		{
			CusCodeFindBox = RegisterControl(nameof(EUICS2PackedItemDetailsUserControl.CusCodeFindBox));
			PostalValueCalcFindBox = RegisterControl(nameof(EUICS2PackedItemDetailsUserControl.PostalValueCalcFindBox));
			TypeOfGoodsDropEdit = RegisterControl(nameof(EUICS2PackedItemDetailsUserControl.TypeOfGoodsDropEdit));
		}

		public ControlReference CusCodeFindBox { get; }
		public ControlReference PostalValueCalcFindBox { get; }
		public ControlReference TypeOfGoodsDropEdit { get; }

		protected override Control CreateTemplate() => new EUICS2PackedItemDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<EUICS2PackedItemDetailsControlBag> packedItemDetailsControlBag = new (() => new EUICS2PackedItemDetailsControlBag());
	}
}
