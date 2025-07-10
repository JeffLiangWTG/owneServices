using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDAManifest.GUI
{
	public class ASYCUDABillControlBag : ControlBag
	{
		public static ASYCUDABillControlBag Instance => aSYCUDABillControlBag.Value;

		ASYCUDABillControlBag()
		{
			BDEGMSeparatorUserControl = RegisterControl(nameof(ASYCUDABillCountrySpecificUserControl.BDEGMSeparatorUserControl));
			SADOfficeCodeDropEdit = RegisterControl(nameof(ASYCUDABillCountrySpecificUserControl.SADOfficeCodeDropEdit));
			SADRegistrationSerialTextBox = RegisterControl(nameof(ASYCUDABillCountrySpecificUserControl.SADRegistrationSerialTextBox));
			SADRegistrationNumberTextBox = RegisterControl(nameof(ASYCUDABillCountrySpecificUserControl.SADRegistrationNumberTextBox));
			SADRegistrationDateEdit = RegisterControl(nameof(ASYCUDABillCountrySpecificUserControl.SADRegistrationDateEdit));
		}

		public ControlReference BDEGMSeparatorUserControl { get; }
		public ControlReference SADOfficeCodeDropEdit { get; }
		public ControlReference SADRegistrationSerialTextBox { get; }
		public ControlReference SADRegistrationNumberTextBox { get; }
		public ControlReference SADRegistrationDateEdit { get; }

		protected override Control CreateTemplate() => new ASYCUDABillCountrySpecificUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<ASYCUDABillControlBag> aSYCUDABillControlBag = new Lazy<ASYCUDABillControlBag>(() => new ASYCUDABillControlBag());
	}
}
