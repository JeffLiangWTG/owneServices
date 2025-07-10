using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
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
			var commonInvoiceLineDetailsLayoutBuilder = new ExitSummaryMainPanelLayoutBuilder<CusExitControlHeader>();
			var commonBag = commonInvoiceLineDetailsLayoutBuilder.CommonBag;
			commonInvoiceLineDetailsLayoutBuilder.AddColumn();
			commonInvoiceLineDetailsLayoutBuilder.Add(commonBag.ReferenceNumberTextBox, ControlWidthClass.Auto);
			commonInvoiceLineDetailsLayoutBuilder.Add(commonBag.HeaderCustomsOfficeCodeFindBox, ControlWidthClass.Auto);
			commonInvoiceLineDetailsLayoutBuilder.Add(commonBag.HeaderArrivalNotificationDateDateEdit, ControlWidthClass.Auto);
			commonInvoiceLineDetailsLayoutBuilder.Add(commonBag.HeaderArrivalNotificationPlaceTextBox, ControlWidthClass.Auto);
			commonInvoiceLineDetailsLayoutBuilder.Add(commonBag.HeaderExitDateDateEdit, ControlWidthClass.Auto);
			commonInvoiceLineDetailsLayoutBuilder.Add(commonBag.HeaderTransportIdTextBox, ControlWidthClass.Auto);

			commonInvoiceLineDetailsLayoutBuilder.AddColumn();
			commonInvoiceLineDetailsLayoutBuilder.Add(commonBag.AgentOrgAddressControl, ControlWidthClass.Auto);

			commonInvoiceLineDetailsLayoutBuilder.AddColumn();
			commonInvoiceLineDetailsLayoutBuilder.Add(commonBag.HeaderCarrierOrgAddressControl, ControlWidthClass.Auto);
			return commonInvoiceLineDetailsLayoutBuilder.Build();
		}
	}
}
