using System;
using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Summary description for ZNavigationBar.
	/// </summary>
	public class ZNavigationBar : BaseUserControl, IContainResources
	{
		#region Navigation Elements

		public NavigationElementCollection NavigationElements
		{
			get { return fNavigationElements; }
		}
		readonly NavigationElementCollection fNavigationElements = new NavigationElementCollection();

		#endregion

		#region Home link element

		#region ShowHomeLink

		public bool ShowHomeLink
		{
			get { return fShowHomeLink; }
			set { fShowHomeLink = value; }
		}
		bool fShowHomeLink;

		#endregion

		#region Homepage name

		public string HomePageName
		{
			get { return fHomePageName; }
			set { fHomePageName = value; }
		}
		string fHomePageName = Res.GetString("a3a4ad70-646b-4e3b-9980-23a90b922f98", "Home");

		#endregion

		protected string HomePageUrl
		{
			get { return Page.AppInstance.HomePage; }
		}

		#endregion

		#region Show When Login only

		public bool ShowWhenLoginOnly
		{
			get { return fShowWhenLoginOnly; }
			set { fShowWhenLoginOnly = value; }
		}
		bool fShowWhenLoginOnly;

		#endregion

		#region Rendering

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), "NavigationBar_NavigationBarScript"))
			{
				Page.ZClientScript.RegisterClientScriptBlock(GetType(), "NavigationBar_NavigationBarScript", ShowHideScriptBlock);
			}
		}

		#region Render

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html comment should not be translated")]
		protected override void Render(HtmlTextWriter output)
		{
			output.WriteLine("<!-- start of navigation bar -->");
			RenderNavigationBar(output);
			output.WriteLine("<!-- end of navigation bar -->");
		}
		#endregion

		#region RenderNavigationBar

		/// <summary>
		/// Renders the whole menu bar 
		/// </summary>
		/// <param name="output">Target output</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html element id should not be translated")]
		protected void RenderNavigationBar(HtmlTextWriter output)
		{
			if (ShowWhenLoginOnly && SiteUser != null && !SiteUser.IsLoggedIn)
			{
				return;
			}

			output.AddAttribute(HtmlTextWriterAttribute.Id, "menu");

			output.RenderBeginTag(Level1Tag);
			RenderTopLevelMenuItems(output);
			output.RenderEndTag();
		}
		#endregion

		#region RenderMenuItems

		/// <summary>
		/// Render all menu items (Shipments, Orders, Bookings, etc)
		/// </summary>
		/// <param name="output">Output target</param>
		protected void RenderTopLevelMenuItems(HtmlTextWriter output)
		{
			if (ShowHomeLink)
			{
				RenderNavigationElement(new NavigationElement(HomePageName, HomePageUrl), output);
			}
			RenderMenuItems(output, NavigationElements);
		}

		protected void RenderMenuItems(HtmlTextWriter output, NavigationElementCollection navigationElements)
		{
			foreach (NavigationElement element in navigationElements)
			{
				if (element.Visible)
				{
					RenderNavigationElement(element, output);
				}
			}
		}

		#endregion

		#region RenderElements

		/// <summary>
		/// Render a menu item
		/// </summary>
		/// <param name="element">Menu Item</param>
		/// <param name="output">Output target</param>
		protected void RenderNavigationElement(NavigationElement element, HtmlTextWriter output)
		{
			bool isSubMenu = element.SubMenuItems.Count > 0;
			bool displaySubMenu = false;
			if (isSubMenu)
			{
				foreach (NavigationElement subMenuElement in element.SubMenuItems)
				{
					if (subMenuElement.Visible)
					{
						displaySubMenu = true;
						break;
					}
				}
			}

			if (!isSubMenu || displaySubMenu)
			{
				output.RenderBeginTag(Level2Tag);

				bool elementSelected = RequestUrl.ToString().IndexOf(element.URL) > -1;

				if (!elementSelected)
				{
					output.AddAttribute(HtmlTextWriterAttribute.Href, element.URL);
					output.RenderBeginTag(HtmlTextWriterTag.A);
				}
				output.Write(element.Title);
				if (!elementSelected)
				{
					output.RenderEndTag();
					if (displaySubMenu)
					{
						output.WriteLine();
					}
				}

				if (displaySubMenu)
				{
					output.RenderBeginTag(Level1Tag);
					RenderMenuItems(output, element.SubMenuItems);

					output.RenderEndTag();
					output.WriteLine();
				}
				output.RenderEndTag();
				output.WriteLine();
			}
		}

		protected
#if DEBUG
			virtual
#endif
			Uri RequestUrl
		{
			get { return Request.Url; }
		}

		#endregion

		protected const HtmlTextWriterTag Level1Tag = HtmlTextWriterTag.Ul;
		protected const HtmlTextWriterTag Level2Tag = HtmlTextWriterTag.Li;

		#endregion

		#region IContainResources Members

		public ZWebResourceCollection Resources
		{
			get
			{
				ZWebResourceCollection fResources = new ZWebResourceCollection();
				fResources.Add(NavigationBarScriptResource);
				return fResources;
			}
		}

		#region NavigationBarScriptResource

		public ZWebResource NavigationBarScriptResource
		{
			get
			{
				if (fNavigationBarScriptResource == null)
				{
					fNavigationBarScriptResource = new ZWebResource(typeof(ZNavigationBar), NavigationBarScriptResourceFileName, Page, "Enterprise.ZArchitecture.Web.GUI.Navigation");
				}
				return fNavigationBarScriptResource;
			}
		}
		ZWebResource fNavigationBarScriptResource;
		const string NavigationBarScriptResourceFileName = "NavigationBar.js";

		#region ScriptBlock

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "html markup should not be translated")]
		protected string ShowHideScriptBlock
		{
			get { return String.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", NavigationBarScriptResource.FileName); }
		}

		#endregion ScriptBlock

		#endregion

		#endregion
	}
}
