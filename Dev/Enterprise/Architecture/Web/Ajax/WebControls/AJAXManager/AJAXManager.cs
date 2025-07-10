using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Enterprise.Registry.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public class AJAXManager : CompositeControl, IContainResources
	{
		public AJAXManager()
		{
			EnsureChildControls();
		}

		#region Public Properties

		public ScriptManager ScriptManager
		{
			get
			{
				EnsureChildControls();
				return scriptManager;
			}
		}

		public Image IndicatorImage
		{
			get { return image; }
		}

		#endregion

		#region Event Handlers

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception string")]
		protected void OnAsyncPostBackError(object sender, AsyncPostBackErrorEventArgs e)
		{
			if (!e.Exception.Message.Contains("The client disconnected")
				&& !e.Exception.Message.Contains("Invalid postback or callback argument")
				&& !e.Exception.Message.Contains("Failed to load viewstate")
				&& !e.Exception.Message.Contains("The state information is invalid"))
			{
				string key = "Ajax Exception " + e.Exception.GetType().ToString() + ".";
				Enterprise.ZArchitecture.Environment.Globals.Message.ShowDeveloperException(
					key,
					key + " " + e.Exception.Message,
					e.Exception);
			}
		}

		#endregion

		#region Overriden

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is an internal ID, May be an identifier or GUID.")]
		protected override void CreateChildControls()
		{
			scriptManager = new ScriptManager();
			scriptManager.AsyncPostBackTimeout = WebDataRegistry.Instance.RequestTimeout.Value;
			scriptManager.AsyncPostBackError += OnAsyncPostBackError;
			scriptManager.ID = "sm";

			ajaxIndicator = new UpdateProgress();
			ajaxIndicator.ID = "updProgress";
			ajaxIndicator.DisplayAfter = 200;

			DynamicTemplate template = new DynamicTemplate();
			image = new Image();
			image.ID = "img";
			image.Style["position"] = "absolute";
			image.Style["z-index"] = "1000";
			image.Style["left"] = "50%";
			image.Style["top"] = "50%";
			template.Controls.Add(image);
			ajaxIndicator.ProgressTemplate = template;

			validationUpdatePanel = new UpdatePanel();

			HtmlGenericControl updatePanelContainer = new HtmlGenericControl("DIV");
			updatePanelContainer.Style["height"] = "13px";
			updatePanelContainer.Style["display"] = "none";
			updatePanelContainer.Controls.Add(validationUpdatePanel);

			Controls.Add(scriptManager);
			Controls.Add(ajaxIndicator);
			Controls.Add(updatePanelContainer);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			image.ImageUrl = AjaxIndicatorImage.FileName;

			ZPage page = (ZPage)Page;
			if (!page.ZClientScript.IsClientScriptIncludeRegistered("AjaxManager Script"))
			{
				page.ZClientScript.RegisterClientScriptInclude("AjaxManager Script", AjaxScript.FileName);
			}
		}

		#endregion

		#region Controls

		ScriptManager scriptManager;
		UpdateProgress ajaxIndicator;
		UpdatePanel validationUpdatePanel;
		Image image;

		public UpdatePanel ValidationUpdatePanel
		{
			get { return validationUpdatePanel; }
		}

		#endregion

		#region IContainResources Members

		protected ZWebResource AjaxIndicatorImage
		{
			get
			{
				if (ajaxIndicatorImage == null)
				{
					ajaxIndicatorImage = new ZWebResource(typeof(AJAXManager), "ajax-indicator.gif", (ZAjaxPage)Page, "Enterprise.ZArchitecture.Web.GUI.Ajax.WebControls.AJAXManager");
				}
				return ajaxIndicatorImage;
			}
		}
		ZWebResource ajaxIndicatorImage;

		public ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = new ZWebResourceCollection();
				result.Add(AjaxIndicatorImage);
				result.Add(AjaxScript);
				return result;
			}
		}

		protected ZWebResource AjaxScript
		{
			get
			{
				if (ajaxScript == null)
				{
					ajaxScript = new ZWebResource(typeof(AJAXManager), "ajax-script.v2.js", (ZAjaxPage)Page, "Enterprise.ZArchitecture.Web.GUI.Ajax.WebControls.AJAXManager");
				}
				return ajaxScript;
			}
		}
		ZWebResource ajaxScript;

		#endregion
	}
}
