using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public class ExitSummaryMainPanelLayout : IPanelLayoutProvider
	{
		PanelLayout ExitSummaryMainPanel { get; }

		PanelLayout IPanelLayoutProvider.Layout => ExitSummaryMainPanel;

		public ExitSummaryMainPanelLayout()
		{
			ExitSummaryMainPanel = CreateExitSummaryMainPanelLayout();
		}

		PanelLayout CreateExitSummaryMainPanelLayout()
		{
			var builder = new EU.GUI.PlugIn.ExitSummaryMainPanelLayoutBuilder<CusExitControlHeader>();
			var euBag = builder.CommonBag;
			var esBag = ExitSummaryMainPanelControlBag.Instance;
			builder.AddControlBag(esBag);

			builder.AddColumn();
			builder.Add(euBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.HeaderCustomsOfficeCodeFindBox, ControlWidthClass.Auto);
			builder.Add(euBag.HeaderArrivalNotificationDateDateEdit, ControlWidthClass.Auto);
			builder.Add(euBag.HeaderArrivalNotificationPlaceTextBox, ControlWidthClass.Auto);
			builder.Add(euBag.HeaderExitDateDateEdit, ControlWidthClass.Auto);
			builder.Add(euBag.HeaderTransportIdTextBox, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(esBag.BrokerCodeFindBox, ControlWidthClass.Medium);
			builder.Add(esBag.CertificateDropEdit, ControlWidthClass.Medium);
			builder.Add(esBag.DeclEmailAddrTextBox, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(euBag.AgentOrgAddressControl, ControlWidthClass.Medium);

			builder.AddColumn();
			builder.Add(euBag.HeaderCarrierOrgAddressControl, ControlWidthClass.Medium);

			return builder.Build();
		}
	}
}
