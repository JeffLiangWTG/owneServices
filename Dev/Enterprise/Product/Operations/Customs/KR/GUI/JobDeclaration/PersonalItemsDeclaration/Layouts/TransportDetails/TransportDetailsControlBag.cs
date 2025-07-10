using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class TransportDetailsControlBag : ControlBag
	{
		TransportDetailsControlBag()
		{
			HouseBillTextBox = RegisterControl(nameof(TransportDetailsControlBag.HouseBillTextBox));
			FreightCalcEdit = RegisterControl(nameof(TransportDetailsControlBag.FreightCalcEdit));
			StartDateDateEdit = RegisterControl(nameof(TransportDetailsControlBag.StartDateDateEdit));
			ArrivalDateDateEdit = RegisterControl(nameof(TransportDetailsControlBag.ArrivalDateDateEdit));
			PortofLoadingCodeFindBox = RegisterControl(nameof(TransportDetailsControlBag.PortofLoadingCodeFindBox));
			ForeignCityCodeFindBox = RegisterControl(nameof(TransportDetailsControlBag.ForeignCityCodeFindBox));
		}

		public static TransportDetailsControlBag Instance => instance ?? (instance = new TransportDetailsControlBag());

		[ThreadStatic]
		static TransportDetailsControlBag instance;

		public ControlReference HouseBillTextBox { get; }
		public ControlReference FreightCalcEdit { get; }
		public ControlReference StartDateDateEdit { get; }
		public ControlReference ArrivalDateDateEdit { get; }
		public ControlReference PortofLoadingCodeFindBox { get; }
		public ControlReference ForeignCityCodeFindBox { get; }

		protected override Control CreateTemplate() => new TransportDetailsUserControl();
	}
}
