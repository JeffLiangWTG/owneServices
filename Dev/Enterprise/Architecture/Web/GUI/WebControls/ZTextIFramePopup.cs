using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// This control provides an abstract class for displaying pop up in an IFRAME
	/// </summary>
	public abstract class ZTextIFramePopup : ZTextPopup
	{
		public enum FrameScroll { auto, yes, no }

		public delegate void GetAdditionalParameters(object sender, GetAdditionalParametersEventArgs e);

		public class GetAdditionalParametersEventArgs : EventArgs
		{
			public GetAdditionalParametersEventArgs(NameValueCollection additionalParameters)
				: base()
			{
				AdditionalParameters = additionalParameters;
			}

			public NameValueCollection AdditionalParameters;
		}

		protected virtual FrameScroll EnableFrameScrolling
		{
			get { return FrameScroll.no; }
		}

		protected virtual string CallerPK
		{
			get
			{
				if (Page != null)
				{
					return Page.DataSourceIndexer.ToString();
				}
				return null;
			}
		}

		protected virtual NameValueCollection AdditionalParameters
		{
			get
			{
#if DEBUG
				fBaseAdditionalParametersCalled = true;
#endif
				NameValueCollection result = new NameValueCollection();
				if (!string.IsNullOrEmpty(CallerPK))
				{
					result.Add(ZIFramePage.CallerPKQuery, CallerPK);
				}

				if (OnGetAdditionalParameters != null)
				{
					OnGetAdditionalParameters(this, new GetAdditionalParametersEventArgs(result));
				}
				return result;
			}
		}

		public GetAdditionalParameters OnGetAdditionalParameters;

#if DEBUG
		bool fBaseAdditionalParametersCalled;

		[Browsable(false)]
		public bool BaseAdditionalParametersCalled
		{
			get { return fBaseAdditionalParametersCalled; }
		}
#endif

		protected string AdditionalQueryString
		{
			get
			{
				string queryString = "";
				for (int i = 0; i < AdditionalParameters.Count; i++)
				{
					if (HasPageServer)
					{
						string encKey = Page.Server.UrlEncode(AdditionalParameters.Keys[i]);
						string encValue = Page.Server.UrlEncode(AdditionalParameters[i]);
						queryString += "&" + encKey + "=" + encValue;
					}
				}
				return queryString;
			}
		}

		protected bool HasPageServer
		{
			get
			{
				try
				{
					return Page?.Server != null;
				}
				catch (NullReferenceException)
				{
					// yes this can still happen. HttpContext.Current is wonderfully defensive.... Not
				}
				catch (HttpException) { }
				catch (Exception e) when (!e.IsCriticalException())
				{
					ErrorReporter.ReportOnce(e.Message, e);
					throw;
				}
				return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected override string ButtonClickHandler
		{
			get
			{
				EnsureChildControls();

				string formatString = LoadIFrameOnDemand ?
					"ZTextPopup_ShowIFramePopup('{0}', '{1}', '{2}', '{3}', true);{4}" :
					"ZTextPopup_ShowPopup('{0}', '{1}', '{2}');{4}";

				return string.Format(formatString,
					TextBoxControl.ClientID,			// 0
					ButtonControl.ClientID,				// 1
					PopupID,							// 2
					IFrameSourceString,					// 3
					AdditionalButtonClickHandler);		// 4
			}
		}

		protected virtual string IFrameEventHandlers
		{
			get { return ZString.Empty; }
		}

		protected virtual string OKFunctionName
		{
			get { return "ZTextPopup_SetValueAndHidePopup"; }
		}

		protected virtual string CancelFunctionName
		{
			get { return "ZTextPopup_HidePopup"; }
		}

		protected virtual string PopupCssClass
		{
			get { return CssConstants.ZFindBox; }
		}

		#region Resources

		public override ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = base.Resources;
				result.Add(IFrameSourcePage);
				return result;
			}
		}

		protected abstract string IFrameSourcePageName { get; }

		protected virtual Type IFrameSourcePageContainerType
		{
			get { return GetType(); }
		}

		protected internal ZWebResource IFrameSourcePage
		{
			get
			{
				if (fIFrameSourcePage == null)
				{
					fIFrameSourcePage = new ZWebResource(IFrameSourcePageContainerType, IFrameSourcePageName, Page);
				}

				return fIFrameSourcePage;
			}
		}
		ZWebResource fIFrameSourcePage;

		#endregion

		public bool LoadIFrameOnDemand
		{
			get { return fLoadIFrameOnDemand; }
		}
		protected bool fLoadIFrameOnDemand = true;

		protected string IFrameSourceString
		{
			get
			{
				if (fIFrameSourceString == null)
				{
					string aSPXFile = IFrameSourcePage.FileName;

					string encOKQuery = WebUtility.UrlEncode(ZIFramePage.OKFunctionQuery);
					string encCancelQuery = WebUtility.UrlEncode(ZIFramePage.CancelFunctionQuery);
					string encControlIDQuery = WebUtility.UrlEncode(ZIFramePage.ControlIDQuery);
					string encControlID = WebUtility.UrlEncode(ClientID);

					fIFrameSourceString =
						string.Format("{0}?{1}={2}&{3}={4}&{5}={6}{7}",
						aSPXFile,					// 0
						encOKQuery,					// 1
						OKFunctionName,				// 2
						encCancelQuery,				// 3
						CancelFunctionName,			// 4
						encControlIDQuery,			// 5
						encControlID,				// 6
						AdditionalQueryString		// 7
						);
				}
				return fIFrameSourceString;
			}
		}
		string fIFrameSourceString;

		public virtual bool CanAutosizeSelf
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "javascript code should not be translated, May be an identifier or GUID.")]
		protected override void RenderPopupContainer(HtmlTextWriter writer)
		{
			string onloadEvent = string.Empty;
			if (CanAutosizeSelf)
			{
				onloadEvent = string.Format("onload=autoSizeIframe('{0}');", PopupID);
			}
			string formatString = LoadIFrameOnDemand ?
					"<IFRAME id='{0}' {2} frameborder='no' style='{3};' scrolling='{4}' class='{5}' {6}></IFRAME>" :
					"<IFRAME id='{0}' src='{1}' {2} frameborder='no' style='{3};' scrolling='{4}' class='{5}' {6}></IFRAME>";

			writer.Write(
				string.Format(formatString,
				PopupID,					// 0
				IFrameSourceString,			// 1
				IFrameEventHandlers,		// 2
				DisplayStyle,				// 3
				EnableFrameScrolling,		// 4
				PopupCssClass,				// 5
				onloadEvent					// 6
				));
		}

		#region Internal Properties

		internal FrameScroll EnableFrameScrollingInternal => EnableFrameScrolling;
		internal NameValueCollection AdditionalParametersInternal => AdditionalParameters;
		internal Type IFrameSourcePageContainerTypeInternal => IFrameSourcePageContainerType;
		internal string IFrameEventHandlersInternal => IFrameEventHandlers;
		internal string CancelFunctionNameInternal => CancelFunctionName;
		internal string OKFunctionNameInternal => OKFunctionName;
		internal string CallerPKInternal => CallerPK;

		#endregion
	}
}
