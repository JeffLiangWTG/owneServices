using System;
using System.Net;
using System.Web;
using CargoWise.Common;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class Notification : BasePage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		public enum NotificationType
		{
			Info,
			Success,
			Warning,
			Error
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			var queryStrings = Request.QueryString[SecureQueryString.QueryStringKey];
			try
			{
				var queryData = new SecureQueryString(queryStrings);
				Title = queryData["Title"] ?? "MyAccount";
				Message = HttpUtility.HtmlEncode(queryData["Message"]);
				if (Enum.TryParse<NotificationType>(queryData["NotificationType"], out var result))
				{
					Type = result;
				}
				else
				{
					Type = NotificationType.Info;
				}
			}
			catch
			{
				Title = "MyAccount";
				Message = "Invalid Data";
				Type = NotificationType.Error;
			}
		}

		public string Message { get; private set; }

		public NotificationType Type { get; private set; }

		public string GetAlertClass(NotificationType type)
		{
			return type switch
			{
				NotificationType.Success => "notification-success",
				NotificationType.Warning => "notification-warning",
				NotificationType.Error => "notification-error",
				_ => "notification-info",
			};
		}

		public static void ShowMessage(HttpResponse response, string pageTitle, string message, NotificationType notificationType)
		{
			var query = new SecureQueryString();
			query["Title"] = pageTitle;
			query["Message"] = message;
			query["NotificationType"] = notificationType.ToString();

			response.Redirect($"~/my-account/Notification.aspx?{SecureQueryString.QueryStringKey}={WebUtility.UrlEncode(query.ToString())}");
		}
	}
}
