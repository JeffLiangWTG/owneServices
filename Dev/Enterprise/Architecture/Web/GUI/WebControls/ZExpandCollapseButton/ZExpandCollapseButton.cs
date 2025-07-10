using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	class ZExpandCollapseButton : Image, IContainResources
	{
		public ZExpandCollapseButton()
			: base()
		{
		}

		public WebControl ContentControl
		{ get; set; }

		public string ContentControlID
		{
			get { return ContentControl != null ? ContentControl.ClientID : contentControlID; }
			set { contentControlID = value; }
		}
		string contentControlID = string.Empty;

		#region Expand

		[Bindable(true),
		Category("Appearance")]
		public bool Expand
		{
			get
			{
				object obj = this.ViewState["ZExpandCollapseButton_Expand"];
				bool result = (obj != null) && (bool)obj;

				if (!string.IsNullOrEmpty(ExpandCollapse))
				{
					result = ExpandCollapse == ZBool.True.ToString();
				}
				return result;
			}
			set
			{
				this.ViewState["ZExpandCollapseButton_Expand"] = value;
			}
		}

		#endregion

		#region Control overrides

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);

			if (!BasePage.ZClientScript.IsClientScriptIncludeRegistered("ZExpandCollapseButton_ScriptKey"))
			{
				BasePage.ZClientScript.RegisterClientScriptInclude("ZExpandCollapseButton_ScriptKey", ExpandCollapseScript.FileName);
			}
			BasePage.ZClientScript.RegisterHiddenField(ExpandCollapseID, ExpandCollapse);
			OnPreRenderInternal();
		}

		string ExpandCollapseID
		{
			get { return "ExpandCollapseValue" + this.ClientID; }
		}

		string ExpandCollapse
		{
			get
			{
#if DEBUG
				if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
				{
					return string.Empty;
				}
				else
#endif
				{
					return Page.Request.Params[ExpandCollapseID];
				}
			}
		}

		protected void OnPreRenderInternal()
		{
			Style.Add("cursor", "hand");

			ImageUrl = Expand ? CollapseIcon.FileName : ExpandIcon.FileName;
			if (ContentControl != null)
			{
				ContentControl.Style["display"] = Expand ? "" : "none";
			}
		}

		public override void RenderBeginTag(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Onclick, ExpandCollapseClickHandler);
			base.RenderBeginTag(writer);
		}

		#endregion

		#region ExpandCollapseClickHandler

		protected ZString ExpandCollapseClickHandler
		{
			get
			{
				return ZString.Format("ZExpandCollapseButton_ExpandCollapse('{0}', '{1}', '{2}', '{3}', '{4}')", ContentControlID, this.ClientID, ExpandIcon.FileName, CollapseIcon.FileName, ExpandCollapseID);
			}
		}

		#endregion

		#region IContainResources Members

		protected ZWebResource ExpandIcon
		{
			get { return expandIcon ?? (expandIcon = new ZWebResource(typeof(ZExpandCollapseButton), "expand.gif", BasePage)); }
		}
		ZWebResource expandIcon;

		protected ZWebResource CollapseIcon
		{
			get { return collapseIcon ?? (collapseIcon = new ZWebResource(typeof(ZExpandCollapseButton), "collapse.gif", BasePage)); }
		}
		ZWebResource collapseIcon;

		protected ZWebResource ExpandCollapseScript
		{
			get { return expandCollapseScript ?? (expandCollapseScript = new ZWebResource(typeof(ZExpandCollapseButton), "ZExpandCollapseButtonScript.js", BasePage)); }
		}
		ZWebResource expandCollapseScript;

		public ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection result = new ZWebResourceCollection();
				result.Add(ExpandCollapseScript);
				result.Add(ExpandIcon);
				result.Add(CollapseIcon);
				return result;
			}
		}

		ZPage BasePage
		{
			get { return Page as ZPage; }
		}

		#endregion IContainResources members

	}

	#endregion
}
