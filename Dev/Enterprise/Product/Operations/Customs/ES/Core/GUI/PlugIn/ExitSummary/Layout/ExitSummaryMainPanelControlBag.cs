using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public sealed class ExitSummaryMainPanelControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ExitSummaryMainPanelFieldsUserControl();

		public static ExitSummaryMainPanelControlBag Instance => instance ?? (instance = new ExitSummaryMainPanelControlBag());

		[ThreadStatic]
		static ExitSummaryMainPanelControlBag instance;

		ExitSummaryMainPanelControlBag()
		{
			BrokerCodeFindBox = RegisterControl(nameof(ExitSummaryMainPanelFieldsUserControl.BrokerCodeFindBox));
			CertificateDropEdit = RegisterControl(nameof(ExitSummaryMainPanelFieldsUserControl.CertificateDropEdit));
			DeclEmailAddrTextBox = RegisterControl(nameof(ExitSummaryMainPanelFieldsUserControl.DeclEmailAddrTextBox));
		}

		public ControlReference BrokerCodeFindBox { get; }

		public ControlReference CertificateDropEdit { get; }

		public ControlReference DeclEmailAddrTextBox { get; }
	}
}
