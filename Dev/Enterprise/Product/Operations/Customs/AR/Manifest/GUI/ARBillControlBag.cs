using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AR.Manifest.GUI
{
	public class ARBillControlBag : ControlBag
	{
		public static ARBillControlBag Instance => billControlBag.Value;

		ARBillControlBag()
		{
			IsInformedToRenarCheckBox = RegisterControl(nameof(ARBillCountrySpecificUserControl.IsInformedToRenarCheckBox));
			IsMonitoredTransitCheckBox = RegisterControl(nameof(ARBillCountrySpecificUserControl.IsMonitoredTransitCheckBox));
		}

		public ControlReference IsInformedToRenarCheckBox { get; }
		public ControlReference IsMonitoredTransitCheckBox { get; }

		protected override Control CreateTemplate() => new ARBillCountrySpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<ARBillControlBag> billControlBag = new Lazy<ARBillControlBag>(() => new ARBillControlBag());
	}
}
