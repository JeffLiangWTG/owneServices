using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.UI;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public class ZNavigationBarTest : WebControlTest
	{
		#region TestRendering

		public void TestRendering()
		{
			TestControl.NavigationElements.Add(new NavigationElement("Menu1", "Menu1.aspx"));
			TestControl.NavigationElements.Add(new NavigationElement("Menu2", "Menu2.aspx"));

			NavigationElement menuWithSubMenuItems = new NavigationElement("Menu3", "Menu3.aspx");
			menuWithSubMenuItems.SubMenuItems.Add(new NavigationElement("SubMenuItem1", "SubMenuItem1.aspx"));
			menuWithSubMenuItems.SubMenuItems.Add(new NavigationElement("SubMenuItem2", "SubMenuItem2.aspx"));
			menuWithSubMenuItems.SubMenuItems.Add(new NavigationElement("SubMenuItem3", "SubMenuItem3.aspx"));

			TestControl.NavigationElements.Add(menuWithSubMenuItems);
			TestControl.NavigationElements.Add(new NavigationElement("Menu4", "Menu4.aspx"));

			StringBuilder renderedControl = new StringBuilder();
			HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(renderedControl));
			TestControl.RenderControl(writer);

			string expectedRenderedOutput = @"<!-- start of navigation bar -->
<ul id=""menu"">
	<li><a href=""/Menu1.aspx"">Menu1</a></li>
	<li><a href=""/Menu2.aspx"">Menu2</a></li>
	<li><a href=""/Menu3.aspx"">Menu3</a>
	<ul>
		<li><a href=""/SubMenuItem1.aspx"">SubMenuItem1</a></li>
		<li><a href=""/SubMenuItem2.aspx"">SubMenuItem2</a></li>
		<li><a href=""/SubMenuItem3.aspx"">SubMenuItem3</a></li>

	</ul>
	</li>
	<li><a href=""/Menu4.aspx"">Menu4</a></li>

</ul><!-- end of navigation bar -->
";
			AssertMultilineASCIIEquals("Rendering", expectedRenderedOutput, renderedControl.ToString());
		}
		#endregion

		#region TestHomePageLinkRendering

		public void TestHomePageLinkRendering()
		{
			TestControl.ShowHomeLink = true;
			TestControl.HomePageName = "My Home";
			TestControl.NavigationElements.Add(new NavigationElement("Menu1", "Menu1.aspx"));
			TestControl.NavigationElements.Add(new NavigationElement("Menu2", "Menu2.aspx"));

			StringBuilder renderedControl = new StringBuilder();
			HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(renderedControl));
			TestControl.RenderControl(writer);

			string expectedRenderedOutput = @"<!-- start of navigation bar -->
<ul id=""menu"">
	<li><a href=""http://www.test.cargowise.com/"">My Home</a></li>
	<li><a href=""/Menu1.aspx"">Menu1</a></li>
	<li><a href=""/Menu2.aspx"">Menu2</a></li>

</ul><!-- end of navigation bar -->
";
			AssertEquals("Rendering", expectedRenderedOutput, renderedControl.ToString());
		}
		#endregion

		#region TestResources

		public void TestResources()
		{
			string version = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(TestControl.GetType().Assembly, typeof(AssemblyFileVersionAttribute))).Version;
			string expRuntimeDirectory = String.Format(@"/Runtime/Enterprise.ZArchitecture.Web.GUI/{0}/ZNavigationBar/", version);
			AssertEquals("NavigationBarScript", String.Format("{0}{1}", expRuntimeDirectory.Replace(".", "_"), "NavigationBar.js"), TestControl.NavigationBarScriptResource.FileName);

			AssertNotNull("Resources", TestControl.Resources);
			AssertEquals("Should contain 1 Resources", 1, TestControl.Resources.Count);
			AssertCollectionContains("NavigationBarScript should be in Resources", TestControl.NavigationBarScriptResource, TestControl.Resources);
		}
		#endregion

		#region Implementation

		protected ZNavigationBar TestControl
		{
			get { return Control as ZNavigationBar; }
		}

		protected override Control GetNewControl()
		{
			return new ZDummyNavigationBar();
		}

		public class ZDummyNavigationBar : ZNavigationBar
		{
			protected override Uri RequestUrl
			{
				get
				{
					if (fRequestUrl == null)
					{
						fRequestUrl = new Uri("http://www.test.com/Tracking/");
					}
					return fRequestUrl;
				}
			}
			Uri fRequestUrl;

			public void PreRenderForTesting()
			{
				base.OnPreRender(EventArgs.Empty);
			}
		}
		#endregion
	}
}
