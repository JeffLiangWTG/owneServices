using System;
using System.Web.UI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.ServerServices;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class ZAjaxPage : ZPage, IZAjaxPage
	{
		#region AJAX

		public AJAXManager AJAX
		{
			get { return ajax ?? (ajax = new AJAXManager()); }
		}
		AJAXManager ajax;

		internal virtual bool IsAJAXEnabled
		{
			get { return true; }
		}

		public bool IsAsyncPostBack
		{
			get
			{
				if (!Globals.IsTest)
				{
					return ((ZAjaxPage)Page).IsAJAXEnabled && ScriptManager.GetCurrent(Page).IsInAsyncPostBack;
				}
				else
				{
					return false;
				}
			}
		}

		public string AsyncPostBackSourceElementID
		{
			get
			{
				if (!Globals.IsTest && ((ZAjaxPage)Page).IsAJAXEnabled)
				{
					return ScriptManager.GetCurrent(Page).AsyncPostBackSourceElementID;
				}
				else
				{
					return string.Empty;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Javascript segment")]
		public void UpdatePanelRedirect(string key, string redirectUrl)
		{
			ScriptManager.RegisterClientScriptBlock(this, this.GetType(), key, string.Format("window.location='{0}';", redirectUrl), true);
		}

		#endregion

		#region Overrides

		protected override bool GetExternalServicesSupported()
		{
			return true;
		}

		protected override void RegisterWebServerServiceMethodScripts(IWebServiceMethod webServiceMethod)
		{
			base.RegisterWebServerServiceMethodScripts(webServiceMethod);

			if (!this.AJAX.ScriptManager.Services.Contains(webServiceMethod.WebServiceReference))
			{
				this.AJAX.ScriptManager.Services.Add(webServiceMethod.WebServiceReference);
			}
		}

		protected override string GetPostbackAdditionalJavaScript()
		{
			string javaScript = base.GetPostbackAdditionalJavaScript();
			javaScript += string.Format(@"
							function PositionIndicatorImage()
							{{
								try
								{{
									var Indicator = $('{0}');
									if (Indicator)
									{{
										Indicator.style.top = (posTop() + (pageHeight()-20)/2) + ""px"";
										Indicator.style.left = (posLeft() + (pageWidth()-20)/2) + ""px"";
									}}
								}} catch(err) {{}}
							}}
						   ", AJAX.IndicatorImage.ClientID); // Javascript segment
			return javaScript;
		}

		protected override string GetPostbackPerformAdditionalFunctionCalls()
		{
			return base.GetPostbackPerformAdditionalFunctionCalls() + "PositionIndicatorImage();"; // Javascript segment
		}

		protected override void RenderPageAnchorScript()
		{
			base.RenderPageAnchorScript();

			if (!string.IsNullOrEmpty(GetPageAnchorName()))
			{
				if (!Page.ZClientScript.IsStartupScriptRegistered(GetType(), AnchorScriptKey))
				{
					Page.ZClientScript.RegisterStartupScript(GetType(), AnchorScriptKey, AnchorScriptBlock);
				}
			}
		}

		protected virtual string GetPageAnchorName()
		{
			return "";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Javascript segment")]
		protected string AnchorScriptBlock
		{
			get
			{
				return string.Format(@"<SCRIPT type=""text/javascript"">
                                function setupPageAnchor()
                                {{
                                    Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(pageJumpToAnchor);
                                }}
                                function pageJumpToAnchor(sender, args)
                                {{
                                    if (getElement('{0}')!=null)
                                    {{
                                        location.hash=""#{0}"";
                                    }}
                                }}    
                                document.addEvent('domready', setupPageAnchor);
                                </SCRIPT>", GetPageAnchorName());
			}
		}

		const string AnchorScriptKey = "ZPage_AnchorScript";

		protected override void OnPreInit(EventArgs e)
		{
			base.OnPreInit(e);

			if (IsAJAXEnabled && FormControl != null)
			{
				FormControl.Controls.AddAt(0, AJAX);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Javascript segment, Not a code smell")]
		protected override void AddClientSidePageConfirmationScript(string key, ZPageConfirmation pageConfirmation)
		{
			if (IsAsyncPostBack && (SaveRequest || pageConfirmation.AlwaysRegisterConfirmationScript))
			{
				string arrayButtonsScript = "";
				foreach (ZPageConfirmation.ZPageConfirmationButton button in pageConfirmation.Buttons)
				{
					arrayButtonsScript += (string.IsNullOrEmpty(arrayButtonsScript) ? "" : ",") + " {caption: '" + button.Caption + "', onclick: '" + (string.IsNullOrEmpty(pageConfirmation.UserReponseHolderID) ? "" : "$(\\'" + pageConfirmation.UserReponseHolderID + "\\').value = \\'" + button.Caption + "\\'; ") + button.OnClickScript + "'}";
				}

				string script = string.Format(@"
                        addLoadEvent(ShowLightBox('{0}', '{1}', new Array({2}), '{3}', false, '{4}'));
					", pageConfirmation.Title, pageConfirmation.ConfirmationMessage, arrayButtonsScript, pageConfirmation.UserReponseHolderID, pageConfirmation.ConfirmationImageURL);

				ScriptManager.RegisterStartupScript(this, typeof(Page), key, script, true);
			}
		}

		protected override ZClientScriptManager GetZClientScript()
		{
			return new ZAjaxClientScriptManager(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Javascript segment")]
		protected override void AddClientSideScript(string errorMessage, string key)
		{
			if (IsAsyncPostBack)
			{
				((ZAjaxClientScriptManager)ZClientScript).RegisterClientScriptInclude(key + "file", ScriptFile.FileName);

				string script = string.Format(@"{0}
					ZErrorProvider_PositionControls('{1}');

					function _Base_DisplayValidationMessage()
					{{
						alert('{2}');
					}}", DialogFunction, NotificationsArea.ClientID, errorMessage);

				ScriptManager.RegisterStartupScript(Page, typeof(Page), key, script, true);
			}
			else
			{
				base.AddClientSideScript(errorMessage, key);
			}
		}

		protected override void AddNotificationsArea()
		{
			AJAX.ValidationUpdatePanel.ContentTemplateContainer.Controls.Add(NotificationsArea);
		}

		#endregion

		#region Resources

		public override ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = base.Resources;
				result.Add(AjaxSearchControlResource);
				return result;
			}
		}

		protected ZWebResource AjaxSearchControlResource
		{
			get
			{
				if (ajaxSearchControlResource == null)
				{
					ajaxSearchControlResource = new ZWebResource(typeof(ZAjaxPage), "AjaxSearchControl.ascx", this, "Enterprise.ZArchitecture.Web.GUI.Ajax.WebControls.ZPage");
				}
				return ajaxSearchControlResource;
			}
		}
		ZWebResource ajaxSearchControlResource;

		#endregion
	}
}
