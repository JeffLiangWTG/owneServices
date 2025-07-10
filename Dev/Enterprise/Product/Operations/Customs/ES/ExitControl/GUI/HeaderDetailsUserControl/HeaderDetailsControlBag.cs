using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public sealed class HeaderDetailsControlBag : ControlBag
	{
		HeaderDetailsControlBag()
		{
			BrokerCodeFindBox = RegisterControl(nameof(HeaderDetailsUserControl.BrokerCodeFindBox));
			CertificateDropEdit = RegisterControl(nameof(HeaderDetailsUserControl.CertificateDropEdit));
			TrainingCheckBox = RegisterControl(nameof(HeaderDetailsUserControl.TrainingCheckBox));
		}

		public static HeaderDetailsControlBag Instance => instance ?? (instance = new HeaderDetailsControlBag());

		[ThreadStatic]
		static HeaderDetailsControlBag instance;

		protected override Control CreateTemplate() => new HeaderDetailsUserControl();

		public ControlReference BrokerCodeFindBox { get; }

		public ControlReference CertificateDropEdit { get; }
		public ControlReference TrainingCheckBox { get; }
	}
}
