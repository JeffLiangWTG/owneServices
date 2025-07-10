using System.ComponentModel;

using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	[DefaultProperty("CaptionCollapsed"), ToolboxData("<{0}:ZDiv runat=server></{0}:ZDiv>")]
	public class ZDiv : HtmlGenericControl
	{
		#region Constructors

		public ZDiv()
			: base("div")
		{ }

		public ZDiv(string tag)
			: base("div")
		{ }

		#endregion

		#region Overrides
		protected override void OnPreRender(System.EventArgs e)
		{
			base.OnPreRender(e);

			Header = new HtmlGenericControl("div");
			Header.Attributes.Add("class", HeaderCssClass);

			Label label = new Label();
			label.Text = Caption;
			label.CssClass = CaptionCssClass;

			LinkButton link = new LinkButton();
			link.ID = ButtonID;
			link.Text = ExpandedByDefault ? ExpandedButtonCaption : CollapsedButtonCaption;
			link.CssClass = ButtonCssClass;
			link.Attributes.Add("onMouseOver", "this.style.cursor='hand'");//javascript
			link.OnClientClick = OnClickScript;
			link.PostBackUrl = "javascript:return false;";

			Header.Controls.Add(label);
			if (RenderControlButton)
			{
				Header.Controls.Add(link);
			}
		}
		protected override void OnInit(System.EventArgs e)
		{
			base.OnInit(e);
			this.Attributes.Add("class", CssClass);
		}
		HtmlGenericControl Header;

		protected override void RenderChildren(HtmlTextWriter writer)
		{
			Header.RenderControl(writer);
			writer.Write(string.Format("<div id=\"{0}\" class=\"{1}\"style=\"display: {2}\">", InternalDivId, InternalDivCssClass, RenderControlButton && !ExpandedByDefault ? "none" : "block"));
			base.RenderChildren(writer);
			writer.Write("</div>");
		}

		#endregion

		#region IDs

		string ButtonID
		{
			get { return string.Format("{0}LinkButton", ID); }
		}

		string InternalDivId
		{
			get { return string.Format("{0}Internal", ID); }
		}

		#endregion IDs

		#region scripts

		string OnClickScript
		{
			get
			{
				return string.Format("javascript:{0}{1} return false;", UnCollapseDivScript, HideButtonAfterClick ? HideButtonScript : RenameButtonScript);
			}
		}

		string UnCollapseDivScript
		{
			get
			{
				return string.Format("var divToHandle = $('{0}'); divToHandle.style.display = divToHandle.style.display == 'block' ? 'none' : 'block';", InternalDivId);
			}
		}

		string HideButtonScript
		{
			get
			{
				return string.Format("$('{0}').style.display = 'none';", ButtonID);
			}
		}

		string RenameButtonScript
		{
			get
			{
				return string.Format("var button = $('{0}'); var state = button.innerText.contains('{1}'); button.innerText = state ? '{2}':'{1}';", ButtonID, ExpandedButtonCaption, CollapsedButtonCaption);
			}
		}

		#endregion		

		#region Properties

		public bool RenderCaption
		{
			get { return !ExpandedByDefault && RenderControlButton; }
			set { this.ExpandedByDefault = !value; this.RenderControlButton = value; }
		}

		public string CollapsedButtonCaption
		{
			get { return fCollapsedButtonCaption; }
			set { fCollapsedButtonCaption = value; }
		}
		string fCollapsedButtonCaption = "(Show)";

		public string ExpandedButtonCaption
		{
			get { return fExpandedButtonCaption; }
			set { fExpandedButtonCaption = value; }
		}
		string fExpandedButtonCaption = "(Hide)";

		public string Caption
		{
			get { return fCaption; }
			set { fCaption = value; }
		}
		string fCaption = string.Empty;

		public bool HideButtonAfterClick
		{
			get { return fHideButtonAfterClick; }
			set { fHideButtonAfterClick = value; }
		}
		bool fHideButtonAfterClick;

		public bool ExpandedByDefault
		{
			get { return fExpandedByDefault; }
			set { fExpandedByDefault = value; }
		}
		bool fExpandedByDefault;

		public bool RenderControlButton
		{
			get { return fRenderControlButton; }
			set
			{
				fRenderControlButton = value;
			}
		}
		bool fRenderControlButton = true;

		#endregion

		#region Css

		public string CssClass
		{
			get { return fCssClass; }
			set { fCssClass = value; }
		}
		string fCssClass = "ZDetailsDiv";

		public string ButtonCssClass
		{
			get { return fButtonCssClass; }
			set { fButtonCssClass = value; }
		}
		string fButtonCssClass = "ZDetailsDivButton";

		public string CaptionCssClass
		{
			get { return fCaptionCssClass; }
			set { fCaptionCssClass = value; }
		}
		string fCaptionCssClass = "SectionTitle";

		public string HeaderCssClass
		{
			get { return fHeaderCssClass; }
			set { fHeaderCssClass = value; }
		}
		string fHeaderCssClass = "ZDetailsDivHeader";

		public string InternalDivCssClass
		{
			get { return fInternalDivCssClass; }
			set { fInternalDivCssClass = value; }
		}
		string fInternalDivCssClass = "ZDetailsDivInternal";

		#endregion
	}

	#endregion
}
