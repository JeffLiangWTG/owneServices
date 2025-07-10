using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public sealed class JPTemporaryLandingControlBag : ControlBag
	{
		public static JPTemporaryLandingControlBag Instance => instance ?? (instance = new JPTemporaryLandingControlBag());

		[ThreadStatic]
		static JPTemporaryLandingControlBag instance;

		JPTemporaryLandingControlBag()
		{
			TemporaryLandingReasonDropEdit = RegisterControl(nameof(TemporaryLandingReasonDropEdit));
			TemporaryLandingPeriodDaysCalcEdit = RegisterControl(nameof(TemporaryLandingPeriodDaysCalcEdit));
			TemporaryLandingStartDateEdit = RegisterControl(nameof(TemporaryLandingStartDateEdit));
			TemporaryLandingEndDateEdit = RegisterControl(nameof(TemporaryLandingEndDateEdit));
			TemporaryLandingBondedTransportCodeDropEdit = RegisterControl(nameof(TemporaryLandingBondedTransportCodeDropEdit));
			GoodsLocationCodeFindBox = RegisterControl(nameof(GoodsLocationCodeFindBox));
		}

		protected override Control CreateTemplate()
		{
			return new JPTemporaryLandingSpecificUserControl();
		}

		public ControlReference TemporaryLandingReasonDropEdit { get; }
		public ControlReference TemporaryLandingPeriodDaysCalcEdit { get; }
		public ControlReference TemporaryLandingStartDateEdit { get; }
		public ControlReference TemporaryLandingEndDateEdit { get; }
		public ControlReference TemporaryLandingBondedTransportCodeDropEdit { get; }
		public ControlReference GoodsLocationCodeFindBox { get; }
	}
}
