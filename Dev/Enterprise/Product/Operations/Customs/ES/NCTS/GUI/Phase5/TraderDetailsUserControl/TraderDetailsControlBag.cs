using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public sealed class TraderDetailsControlBag : ControlBag
	{
		public TraderDetailsControlBag()
		{
			BrokerCodeFindBox = RegisterControl(nameof(TraderDetailsUserControl.BrokerCodeFindBox));
			CertificateDropEdit = RegisterControl(nameof(TraderDetailsUserControl.CertificateDropEdit));
			TrainingCheckBox = RegisterControl(nameof(TraderDetailsUserControl.TrainingCheckBox));
		}

		public static TraderDetailsControlBag Instance => instance ?? (instance = new TraderDetailsControlBag());

		[ThreadStatic]
		static TraderDetailsControlBag instance;

		public ControlReference BrokerCodeFindBox { get; }
		public ControlReference CertificateDropEdit { get; }
		public ControlReference TrainingCheckBox { get; }

		protected override Control CreateTemplate() => new TraderDetailsUserControl();
	}
}
