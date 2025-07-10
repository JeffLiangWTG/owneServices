using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CL.Manifest.GUI
{
	public sealed class CLBillControlBag : ControlBag
	{
		public static CLBillControlBag Instance => billControlBag.Value;

		CLBillControlBag()
		{
			RoRoCheckBox = RegisterControl(nameof(CLBillCountrySpecificUserControl.RoRoCheckBox));
		}

		public ControlReference RoRoCheckBox { get; }

		protected override Control CreateTemplate() => new CLBillCountrySpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<CLBillControlBag> billControlBag = new Lazy<CLBillControlBag>(() => new CLBillControlBag());
	}
}
