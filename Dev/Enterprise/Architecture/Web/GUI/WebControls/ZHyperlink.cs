using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Control for Hyperlinks on ZPages
	/// Text is bound automatically
	/// Hyperlink should be set on PageLoad, because otherwise business logic will contain 
	/// web pages addresses, which is a violation of separation of layers.
	/// </summary>
	[DefaultProperty("Text"),
	ToolboxData("<{0}:ZHyperlink runat=server></{0}:ZHyperlink>")]
	public class ZHyperlink : HyperLink, IBindTo, ISelfBindingWebControl, INotificationProvider, IHtmlEncodableLabelControl
	{
		public ZHyperlink()
		{
			fBindTo = "";
			fDataTextFields = Array.Empty<string>();
			fDataTextFormatString = "{0}";
			fDataNavigateUrlFields = Array.Empty<string>();
			fDataNavigateUrlFormatString = "";
			fDataImageUrlFields = Array.Empty<string>();
			fDataImageUrlFormatString = "";
			EnableHtmlEncoding = true;
			IsExternalHyperLink = false;
		}

		#region Additional formatiing properties

		#region DataNavigateUrlFormatString

		public string DataNavigateUrlFormatString
		{
			get { return fDataNavigateUrlFormatString; }
			set { fDataNavigateUrlFormatString = value; }
		}
		string fDataNavigateUrlFormatString;

		#endregion

		#region DataNavigateUrlFields

		public string[] DataNavigateUrlFields
		{
			get { return fDataNavigateUrlFields; }
			set { fDataNavigateUrlFields = value; }
		}
		string[] fDataNavigateUrlFields;

		#endregion

		#region DataTextFields

		public string[] DataTextFields
		{
			get { return fDataTextFields; }
			set { fDataTextFields = value; }
		}
		string[] fDataTextFields;

		#endregion

		#region DataTextFormatString

		public string DataTextFormatString
		{
			get { return fDataTextFormatString; }
			set { fDataTextFormatString = value; }
		}
		string fDataTextFormatString;

		#endregion

		#region DataImageUrlFormatString

		public string DataImageUrlFormatString
		{
			get { return fDataImageUrlFormatString; }
			set { fDataImageUrlFormatString = value; }
		}
		string fDataImageUrlFormatString;

		#endregion

		#region DataImageUrlFields

		public string[] DataImageUrlFields
		{
			get { return fDataImageUrlFields; }
			set { fDataImageUrlFields = value; }
		}
		string[] fDataImageUrlFields;

		#endregion

		#endregion

		#region WindowsStyle

		public string WindowStyle
		{
			get { return fWindowStyle; }
			set { fWindowStyle = value; }
		}
		string fWindowStyle;

		#endregion

		#region IBindTo Members

		[AttributeProvider(ZGUIConstants.BindToPropertyAttributes, nameof(BindTo))]
		public string BindTo
		{
			get { return fBindTo; }
			set { fBindTo = value; }
		}
		string fBindTo = "";

		#endregion

		#region ISelfBindingWebControl Members

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null;
		}

		public void Bind(object dataSource)
		{
			#region SuppressResourceStringsCheckRegion

			Text = this.GetHtmlEncodableLabelContent(GetText(dataSource));
			NavigateUrl = GetNavigateUrl(dataSource);
			ImageUrl = GetImageUrl(dataSource);
			if (dataSource != null && !string.IsNullOrEmpty(BindTo))
			{
				Info = (ZPropertyInfo)ZPropertyAccessor.Get(dataSource, BindTo + "Info");
			}
			if (!string.IsNullOrEmpty(WindowStyle) && !string.IsNullOrEmpty(Target))
			{
				Attributes.Add("onclick", String.Format("javascript: window.open('{0}','{1}','{2}'); return false;", NavigateUrl, Target, WindowStyle));
			}

			#endregion
		}
		ZPropertyInfo Info;

		public bool IsExternalHyperLink
		{
			get;
			set;
		}

		#region Implementation of Text and Url Formatting

		protected string GetText(object dataSource)
		{
			return (string.IsNullOrEmpty(DataTextFormatString) || (string.IsNullOrEmpty(BindTo) && DataTextFields.Length == 0)) ? Text : CreateDynamicContent(dataSource, DataTextFormatString, GetParameters(DataTextFields), false);
		}

		ZString PrependHttpToUrl(ZString url)
		{
			return (url.IsEmpty || IsValidWebPrefix(url)) ? url : new ZString("http://" + url);
		}

		bool IsValidWebPrefix(ZString url)
		{
			string urlToLower = url.ToLower();

			return
				urlToLower.StartsWith("http://") ||
				urlToLower.StartsWith("https://") ||
				urlToLower.StartsWith("ftp://") ||
				urlToLower.StartsWith("/");
		}

		protected string GetNavigateUrl(object dataSource)
		{
			string result = (!string.IsNullOrEmpty(DataNavigateUrlFormatString)) ?
			CreateDynamicContent(dataSource, DataNavigateUrlFormatString, GetParameters(DataNavigateUrlFields), false) : NavigateUrl;

			if (IsExternalHyperLink)
			{
				result = PrependHttpToUrl(result);
			}

			return result;
		}

		protected string GetImageUrl(object dataSource)
		{
			return (!string.IsNullOrEmpty(DataImageUrlFormatString)) ? CreateDynamicContent(dataSource, DataImageUrlFormatString, GetParameters(DataImageUrlFields), false) : ImageUrl;
			//return PrependHttpToUrl(result);
		}

		protected string[] GetParameters(string[] property)
		{
			string[] parameters;
			if (property.Length > 0)
			{
				parameters = new string[property.Length];
				for (int i = 0; i < property.Length; i++)
				{
					parameters[i] = property[i];
				}
			}
			else if (!string.IsNullOrEmpty(BindTo) && BindTo != null)
			{
				parameters = new string[1];
				parameters[0] = BindTo;
			}
			else
			{
				parameters = Array.Empty<string>();
			}

			return parameters;
		}

		protected string CreateDynamicContent(object dataSource, string formatString, string[] fieldsToGetValueFrom, bool encodeUrlParams)
		{
			string result = formatString;

			string[] @params = new string[fieldsToGetValueFrom.Length];
			for (int i = 0; i < @params.Length; i++)
			{
				object value = null;
				try
				{
					value = ZPropertyAccessor.Get(dataSource, fieldsToGetValueFrom[i]);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{ }
				@params[i] = (value != null) ? value.ToString() : "";
				if (encodeUrlParams)
				{
					@params[i] = WebUtility.UrlEncode(@params[i]);
				}
			}
			if (@params.Length > 0)
			{
				result = String.Format(result, @params);
			}
			return result;
		}

		#endregion

		public IEnumerable<INotification> Notifications
		{
			get { return Info != null ? Info.Notifications : NotificationCollection.Empty; }
		}

		bool INotificationProvider.HasNotifications()
		{
			return Info.HasNotifications();
		}

		bool INotificationProvider.HasNotifications(INotificationType type)
		{
			return Notifications.HasNotifications(type);
		}

		INotificationType INotificationProvider.GetHighestSeverityNotificationType()
		{
			return Info.GetHighestSeverityNotificationType();
		}

		public void UnBind()
		{
			BindTo = null;
			Text = "";
		}

		#endregion

		#region IHtmlEncodableLabelControl Members

		public bool EnableHtmlEncoding
		{
			get;
			set;
		}

		#endregion
	}
}
