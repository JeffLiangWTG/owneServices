using System.Collections.Specialized;
using System.Web.UI.WebControls;
using Enterprise.RemotePrinting.Server.Business;
using Enterprise.ZArchitecture.Web.GUI;

namespace Enterprise.RemotePrinting.Server.Controls
{
	public class RequestLogsPopupButton : ZButtonPopup
	{
		SignalRClientBusinessObject SignalRClientInfo { get; set; }

		protected override void BindCore(object dataSource)
		{
			if (dataSource is SignalRClientBusinessObject signalRClientInfo)
			{
				SignalRClientInfo = signalRClientInfo;
			}

			base.BindCore(dataSource);
		}

		protected override NameValueCollection AdditionalParameters
		{
			get
			{
				var result = base.AdditionalParameters;
				result.Add(ClientIdParam, SignalRClientInfo.ClientId);
				result.Add(ServerNameParam, SignalRClientInfo.ServerName);
				result.Add(WebServerAddressParam, SignalRClientInfo.WebPrintServerAddress);
				result.Add(WebServerHostNameParam, SignalRClientInfo.WebPrintServerHostName);
				return result;
			}
		}

		public const string ClientIdParam = "ClientId";
		public const string ServerNameParam = "ServerName";
		public const string WebServerAddressParam = "WebServerAddress";
		public const string WebServerHostNameParam = "WebServerHostName";

		#region Button

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override string ButtonTextCore => "Request Logs";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override string ButtonToolTip => "Request log files from select WebPrint Print Server Client";

		protected override int ButtonControlWidth => 160;

		#endregion

		#region Popup

		protected override Unit PopupWidth => 500;

		protected override Unit PopupHeight => 235;

		protected override string IFrameSourcePageName => "RequestLogsPage.aspx";

		#endregion
	}
}
