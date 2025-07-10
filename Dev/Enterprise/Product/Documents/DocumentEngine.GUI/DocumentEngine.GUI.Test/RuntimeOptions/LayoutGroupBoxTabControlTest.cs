using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed class LayoutGroupBoxTabControlTest : TestCase
	{
		[RequiresSTA]
		public void TestRearrangeGroupBoxesInNColumns()
		{
			LayoutGroupBoxTabControlForTest tabs = new LayoutGroupBoxTabControlForTest();

			tabs.AddPageAndGroupBox("GroupName1", "Long Group Description1");
			tabs.AddPageAndGroupBox("GroupName2", "Long Group Description2");

			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control1");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control2");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control3");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control4");

			CreateAndAddTextControlToGroup(tabs, "GroupName2", "Control5");

			tabs.RearrangeGroupBoxesInNColumns(2);

			AssertEquals("Each Page's height should accomodate the group box", true, tabs.GetPage("GroupName1").Height >= tabs.FilterGroupBoxes["GroupName1"].Height);
			AssertEquals("Each Page's height should accomodate the group box", true, tabs.GetPage("GroupName2").Height >= tabs.FilterGroupBoxes["GroupName2"].Height);

			tabs.FilterGroupBoxes["GroupName1"].Dispose();
			tabs.FilterGroupBoxes["GroupName2"].Dispose();
			tabs.Dispose();
		}

		void CreateAndAddTextControlToGroup(LayoutGroupBoxTabControlForTest tabs, string groupName, string controlName)
		{
			TextBox box = new TextBox();
			box.Name = controlName;
			tabs.AddControlToFilterGroup(groupName, box);
		}

		[RequiresSTA]
		public void TestAddPageAndGroupBoxAndAddControlToFilterGroup()
		{
			LayoutGroupBoxTabControlForTest tabs = new LayoutGroupBoxTabControlForTest();

			tabs.AddPageAndGroupBox("GroupName", "Long Group Description");
			AssertEquals("Tabpages contains 'GroupName' page", true, tabs.ContainsPage("GroupName"));
			ZTabPage page = tabs.GetPage("GroupName");
			AssertEquals("Page's text is 'GroupName'", "GroupName", page.Text);
			AssertEquals("Groupboxes contains 'GroupName' group", true, tabs.FilterGroupBoxes.Contains("GroupName"));
			AssertEquals("GroupName group box's description is 'Long Group Description'", "Long Group Description", tabs.FilterGroupBoxes["GroupName"].Text);

			tabs.AddPageAndGroupBox("", "");
			AssertEquals("Tabpages contains '' page", true, tabs.ContainsPage("GroupName"));
			page = tabs.GetPage("");
			AssertEquals("Page's text is 'Primary Filters'", "Primary Filters", page.Text);
			AssertEquals("Groupboxes contains '' group", true, tabs.FilterGroupBoxes.Contains(""));
			AssertEquals("'' group box's name is 'Primary Filters'", "Primary Filters", tabs.FilterGroupBoxes[""].Text);

			TextBox control1 = new TextBox();
			control1.Name = "Control1";
			tabs.AddControlToFilterGroup("GroupName", control1);
			AssertEquals("There should only be one control", 1, tabs.FilterGroupBoxes["GroupName"].Controls.Count);
			AssertEquals("'GroupName' group contains Control1", true, tabs.FilterGroupBoxes["GroupName"].Contains(control1));

			TextBox control2 = new TextBox();
			control2.Name = "Control2";
			tabs.AddControlToFilterGroup("", control2);
			AssertEquals("There should only be one control", 1, tabs.FilterGroupBoxes[""].Controls.Count);
			AssertEquals("'GroupName' group contains Control2", true, tabs.FilterGroupBoxes[""].Contains(control2));

			tabs.FilterGroupBoxes["GroupName"].Dispose();
			tabs.FilterGroupBoxes[""].Dispose();
			tabs.Dispose();
		}

		[RequiresSTA]
		public void TestMakeTabControlHeightFitGroups()
		{
			LayoutGroupBoxTabControlForTest tabs = new LayoutGroupBoxTabControlForTest();

			tabs.AddPageAndGroupBox("GroupName1", "Long Group Description1");
			tabs.AddPageAndGroupBox("GroupName2", "Long Group Description2");

			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control1");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control2");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control3");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control4");

			CreateAndAddTextControlToGroup(tabs, "GroupName2", "Control5");

			tabs.MakeTabControlFitGroups();

			AssertEquals("When desired height less than max height tab control height should be group height + tab label height (8 + ItemSize.Height)", tabs.FilterGroupBoxes.MaxDesiredHeight + tabs.MysteryPixelsFudge + tabs.ItemSize.Height, tabs.Height);

			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control6");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control7");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control8");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control9");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control10");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control11");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control12");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control13");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control14");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control15");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control16");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control17");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control18");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control19");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control20");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control21");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control22");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control23");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control24");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control25");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control26");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control27");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control28");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control29");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control30");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control31");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control32");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control33");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control34");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control35");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control36");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control37");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control38");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control39");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control40");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control41");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control42");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control43");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control44");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control45");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control46");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control47");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control48");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control49");
			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control50");
			tabs.MakeTabControlFitGroups();
			AssertEquals("When desired height greater than max height tab control height should be MaxHeight + tab label height (8 + ItemSize.Height)", tabs.MaxPageHeight + tabs.MysteryPixelsFudge + tabs.ItemSize.Height, tabs.Height);

			foreach (AutoLayoutGroupBox box in tabs.FilterGroupBoxes)
			{
				box.Dispose();
			}
			tabs.Dispose();
		}

		[RequiresSTA]
		public void TestNeedsToShow()
		{
			LayoutGroupBoxTabControlForTest tabs = new LayoutGroupBoxTabControlForTest();
			AssertEquals("Shouldn't need to show when there's no groups", false, tabs.NeedsToShow);

			tabs.AddPageAndGroupBox("GroupName1", "Long Group Description1");
			tabs.AddPageAndGroupBox("GroupName2", "Long Group Description2");
			AssertEquals("Shouldn't need to show when there's no controls in groups", false, tabs.NeedsToShow);

			CreateAndAddTextControlToGroup(tabs, "GroupName1", "Control1");
			AssertEquals("Should need to show even when there's controls in only one group", true, tabs.NeedsToShow);

			CreateAndAddTextControlToGroup(tabs, "GroupName2", "Control5");
			AssertEquals("Should need to show when there's controls in groups", true, tabs.NeedsToShow);

			tabs.FilterGroupBoxes["GroupName1"].Dispose();
			tabs.FilterGroupBoxes["GroupName2"].Dispose();
			tabs.Dispose();
		}
	}
}
