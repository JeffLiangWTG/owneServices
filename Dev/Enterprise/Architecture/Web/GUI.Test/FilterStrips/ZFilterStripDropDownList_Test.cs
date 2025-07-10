using System.IO;
using System.Web.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	sealed class ZFilterStripDropDownList_Test : ZDropDownListTest
	{
		public void TestIsDescriptionsList()
		{
			AssertEquals("Should be False by default", false, FilterStripDropDown.IsDescriptionsList);
			FilterStripDropDown.IsDescriptionsList = true;
			AssertEquals("Should be True as set", true, FilterStripDropDown.IsDescriptionsList);
		}

		protected override void TestDisplayNotificationsCore()
		{
			Assert(!DropDown.DisplayNotifications);
		}

		ZFilterStripDropDownList FilterStripDropDown
		{
			get { return (ZFilterStripDropDownList)Control; }
		}

		protected override Control GetNewControl()
		{
			return new ZFilterStripDropDownList();
		}

		public void TestModuleFilterLocalization()
		{
			using (var mockRs = Res.UseMockData())
			{
				mockRs.Put("J", new ResourceStringData("J", "测试"));
				mockRs.Put("T", new ResourceStringData("T", "测试"));
				var moduleFilterCollection = new ModuleFilterCollection();
				var filter = new ModuleTextFilter("Test", DummyBizoSchema.Z0_Description);
				filter.MultilingualDescription = ResString.GetMultilingualString("T", "Test");
				moduleFilterCollection.AddFilter(filter);

				filter = new ModuleTextFilter("Zest", DummyBizoSchema.Z0_Code);
				moduleFilterCollection.AddFilter(filter);

				var filterStrip = new FilterStrip(moduleFilterCollection);
				var control = new ZFilterStripDropDownList();
				control.IsDescriptionsList = true;
				control.BindTo = "FilterDescription";
				control.BindToList = "FilterDescriptionList";
				control.Bind(filterStrip);
				((Control)control).Page = new ZPage();
				var s = new StringWriter();
				using (var writer = new HtmlTextWriter(s))
				{
					control.InternalRender(writer);
				}

				AssertContains("<option value=\"Test\" selected=\"selected\">测试</option>", s.ToString());
				AssertContains("<option value=\"Zest\">Zest</option>", s.ToString());

				moduleFilterCollection = new ModuleFilterCollection();
				filter = new ModuleTextFilter("Jest", DummyBizoSchema.Z0_Code);
				filter.MultilingualDescription = ResString.GetMultilingualString("J", "Jest");
				filter.Visibility = FilterVisibility.AlwaysVisible;
				moduleFilterCollection.AddFilter(filter);

				filterStrip = new FilterStrip(moduleFilterCollection);
				control.Bind(filterStrip);
				s = new StringWriter();
				using (var writer = new HtmlTextWriter(s))
				{
					control.InternalRender(writer);
				}

				AssertContains("<select disabled=\"disabled\" class=\"aspNetDisabled\">", s.ToString());
				AssertContains("<option value=\"Jest\" selected=\"selected\">测试</option>", s.ToString());
			}
		}
	}
}
