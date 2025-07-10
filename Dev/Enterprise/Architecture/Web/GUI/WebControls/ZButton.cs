using System;
using System.ComponentModel;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	[DefaultProperty("Text"), ToolboxData("<{0}:ZButton runat=server></{0}:ZButton>")]
	public class ZButton : Button, INamingContainer
	{
		#region Overrides

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			AddPleaseWaitPanel();
		}

		protected internal void AddPleaseWaitPanel()
		{
			if (!string.IsNullOrEmpty(ShowEvent) && !string.IsNullOrEmpty(HideEvent) && !string.IsNullOrEmpty(HideEventObject))
			{
				Controls.AddAt(0, PleaseWaitPanel);
				Attributes.Add(ShowEvent, String.Format("ShowHide('{0}', true);", PleaseWaitPanel.ClientID));
				if (!Page.ZClientScript.IsClientForEventScriptBlockRegistered(HideEventObject, HideEvent, GetType(), "HidePleaseWaitPanel"))
				{
					Page.ZClientScript.RegisterClientForEventScriptBlock(HideEventObject, HideEvent, GetType(), "HidePleaseWaitPanel", String.Format("ShowHide('{0}', false);", PleaseWaitPanel.ClientID));
				}
				if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), "ShowHideScript"))
				{
					Page.ZClientScript.RegisterClientScriptBlock(GetType(), "ShowHideScript", ShowHideScript);
				}
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			foreach (Control ctl in Controls)
			{
				ctl.RenderControl(writer);
			}
			base.Render(writer);
			writer.RenderEndTag();
		}

		#endregion Overrides

		protected new ZPage Page
		{
			get { return base.Page as ZPage; }
		}

		#region Properties

		#region Appearance

		public string Message
		{
			get { return fMessage; }
			set { fMessage = value; }
		}
		string fMessage;

		public string ShowEvent
		{
			get { return fShowEvent; }
			set { fShowEvent = value; }
		}
		string fShowEvent;

		public string HideEvent
		{
			get { return fHideEvent; }
			set { fHideEvent = value; }
		}
		string fHideEvent;

		public string HideEventObject
		{
			get { return fHideEventObject; }
			set { fHideEventObject = value; }
		}
		string fHideEventObject;

		#endregion Appearance

		#region Controls

		protected internal string ShowHideScript
		{
			get
			{
				StringBuilder script = new StringBuilder();
				script.Append("<SCRIPT language='javascript'>\n");
				script.Append("function ShowHide(ControlID, Show) { var Control = document.getElementById(ControlID); if (Control != null) { if (Show) {Control.style.display=''; } else { Control.style.display='none'} } }\n");
				script.Append("</SCRIPT>");
				return script.ToString();
			}
		}

		#region PleaseWaitPanel

		protected internal Panel PleaseWaitPanel
		{
			get
			{
				if (fPleaseWaitPanel == null)
				{
					fPleaseWaitPanel = new Panel();
					fPleaseWaitPanel.ID = "PleaseWaitPanel";
					fPleaseWaitPanel.Style.Add("display", "none");
					fPleaseWaitPanel.Style.Add("background-color", "lightyellow");
					fPleaseWaitPanel.Style.Add("border-style", "solid");
					fPleaseWaitPanel.Style.Add("border-color", "black");
					fPleaseWaitPanel.Style.Add("border-width", "1px");
					fPleaseWaitPanel.Style.Add("position", "relative");
					fPleaseWaitPanel.Style.Add("left", "0px");
					fPleaseWaitPanel.Style.Add("top", "0px");
					fPleaseWaitPanel.Style.Add("z-index", "999");
					HtmlGenericControl messageText = new HtmlGenericControl(nameof(HtmlTextWriterTag.Div));
					messageText.InnerHtml = Message;
					fPleaseWaitPanel.Controls.Add(messageText);
				}
				return fPleaseWaitPanel;
			}
		}
		Panel fPleaseWaitPanel;

		#endregion PleaseWaitPanel

		#endregion Controls

		#endregion Properties
	}

	#endregion
}
