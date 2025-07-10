using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public sealed class ExitSummaryMainPanelControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ExitSummaryMainPanelFieldsUserControl();

		public static ExitSummaryMainPanelControlBag Instance => instance ?? (instance = new ExitSummaryMainPanelControlBag());

		[ThreadStatic]
		static ExitSummaryMainPanelControlBag instance;

		ExitSummaryMainPanelControlBag()
		{
			ReferenceNumberTextBox = RegisterControl(nameof(ExitSummaryMainPanelFieldsUserControl.ReferenceNumberTextBox));
			HeaderCustomsOfficeCodeFindBox = RegisterControl(nameof(ExitSummaryMainPanelFieldsUserControl.HeaderCustomsOfficeCodeFindBox));
			HeaderArrivalNotificationDateDateEdit = RegisterControl(nameof(ExitSummaryMainPanelFieldsUserControl.HeaderArrivalNotificationDateDateEdit));
			HeaderArrivalNotificationPlaceTextBox = RegisterControl(nameof(ExitSummaryMainPanelFieldsUserControl.HeaderArrivalNotificationPlaceTextBox));
			HeaderExitDateDateEdit = RegisterControl(nameof(ExitSummaryMainPanelFieldsUserControl.HeaderExitDateDateEdit));
			HeaderTransportIdTextBox = RegisterControl(nameof(ExitSummaryMainPanelFieldsUserControl.HeaderTransportIdTextBox));
			AgentOrgAddressControl = RegisterControl(nameof(ExitSummaryMainPanelFieldsUserControl.AgentOrgAddressControl));
			HeaderCarrierOrgAddressControl = RegisterControl(nameof(ExitSummaryMainPanelFieldsUserControl.HeaderCarrierOrgAddressControl));
		}

		public ControlReference ReferenceNumberTextBox { get; }

		public ControlReference HeaderCustomsOfficeCodeFindBox { get; }

		public ControlReference HeaderArrivalNotificationDateDateEdit { get; }

		public ControlReference HeaderArrivalNotificationPlaceTextBox { get; }

		public ControlReference HeaderExitDateDateEdit { get; }

		public ControlReference HeaderTransportIdTextBox { get; }

		public ControlReference AgentOrgAddressControl { get; }

		public ControlReference HeaderCarrierOrgAddressControl { get; }
	}
}
