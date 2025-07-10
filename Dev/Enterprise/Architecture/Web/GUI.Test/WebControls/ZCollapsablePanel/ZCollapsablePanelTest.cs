using System;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZCollapsablePanelTest : WebControlTest
	{
		#region TestControls

		public void TestPanelReturnsSubPanelsControlCollection()
		{
			AssertSame("TestPanel Controls", TestPanel.CollapsablePanel.Controls, TestPanel.Controls);
		}

		public void TestCreateChildControls()
		{
			TestPanel.EnsureChildControlsForTesting();
			AssertEquals("ContentsTable should be added to the CollapsablePanel controls ", 1, TestPanel.BaseControls.Count);
			AssertEquals("DIV", TestPanel.DivAroundControl, TestPanel.BaseControls[0]);
			AssertEquals("ContentsTable", TestPanel.ContentsTable, TestPanel.BaseControls[0].Controls[0]);
		}

		public void TestContentsTable()
		{
			AssertNotNull("ContentsTable", TestPanel.ContentsTable);
			AssertEquals("ContentsTable should have 2 rows", 2, TestPanel.ContentsTable.Rows.Count);
			AssertEquals("ContentsTable should have 2 columns in top row", 2, TestPanel.ContentsTable.Rows[0].Cells.Count);
			AssertEquals("ContentsTable should have 2 columns in bottom row", 2, TestPanel.ContentsTable.Rows[1].Cells.Count);
			AssertEquals("ContentsTable should only contain Show/Hide icon in top left", 1, TestPanel.ContentsTable.Rows[0].Cells[0].Controls.Count);
			AssertEquals("ContentsTable should have Load/Expand icon in top left", TestPanel.ExpandCollapseIcon, TestPanel.ContentsTable.Rows[0].Cells[0].Controls[0]);
			AssertEquals("ContentsTable should only contain LabelControl in top right", 1, TestPanel.ContentsTable.Rows[0].Cells[1].Controls.Count);
			AssertEquals("ContentsTable should contain LabelControl in top right", TestPanel.LabelControl, TestPanel.ContentsTable.Rows[0].Cells[1].Controls[0]);
			AssertEquals("ContentsTable should contain nothing in bottom left corner", 0, TestPanel.ContentsTable.Rows[1].Cells[0].Controls.Count);
			AssertEquals("ContentsTable should contain the CollapsablePanel in the bottom left", 1, TestPanel.ContentsTable.Rows[1].Cells[1].Controls.Count);
			AssertEquals("ContentsTable should contain the CollapsablePanel in the bottom left", TestPanel.CollapsablePanel, TestPanel.ContentsTable.Rows[1].Cells[1].Controls[0]);
		}

		public void TestContentsTableWithDisabledCollapsing()
		{
			TestPanel.DisableCollapsing = true;

			AssertNotNull("ContentsTable", TestPanel.ContentsTable);
			AssertEquals("ContentsTable should have 2 rows", 2, TestPanel.ContentsTable.Rows.Count);
			AssertEquals("ContentsTable should have 1 column in top row", 1, TestPanel.ContentsTable.Rows[0].Cells.Count);
			AssertEquals("ContentsTable should have 1 column in bottom row", 1, TestPanel.ContentsTable.Rows[1].Cells.Count);
			AssertEquals("ContentsTable should only contain LabelControl in top", 1, TestPanel.ContentsTable.Rows[0].Cells[0].Controls.Count);
			AssertEquals("ContentsTable should contain LabelControl in top", TestPanel.LabelControl, TestPanel.ContentsTable.Rows[0].Cells[0].Controls[0]);
			AssertEquals("ContentsTable should contain the CollapsablePanel in the bottom", 1, TestPanel.ContentsTable.Rows[1].Cells[0].Controls.Count);
			AssertEquals("ContentsTable should contain the CollapsablePanel in the bottom", TestPanel.CollapsablePanel, TestPanel.ContentsTable.Rows[1].Cells[0].Controls[0]);
		}

		public void TestCompositeControls()
		{
			AssertEquals("LabelControl Type", typeof(ZTextLabel), TestPanel.LabelControl.GetType());
			AssertEquals("LabelControl ID", "Label", TestPanel.LabelControl.ID);

			AssertEquals("ExpandCollapseIcon Type", typeof(Image), TestPanel.ExpandCollapseIcon.GetType());
			AssertEquals("ExpandCollapseIcon ID", "ExpandCollapseIcon", TestPanel.ExpandCollapseIcon.ID);

			AssertEquals("CollapsablePanel Type", typeof(Panel), TestPanel.CollapsablePanel.GetType());
			AssertEquals("CollapsablePanel ID", "Panel", TestPanel.CollapsablePanel.ID);
		}

		public void TestCompositeControlsWithDisabledCollapsing()
		{
			TestPanel.DisableCollapsing = true;

			AssertEquals("LabelControl Type", typeof(ZTextLabel), TestPanel.LabelControl.GetType());
			AssertEquals("LabelControl ID", "Label", TestPanel.LabelControl.ID);

			AssertEquals("ExpandCollapseIcon Type", typeof(Image), TestPanel.ExpandCollapseIcon.GetType());
			AssertEquals("ExpandCollapseIcon ID", "ExpandCollapseIcon", TestPanel.ExpandCollapseIcon.ID);

			AssertEquals("CollapsablePanel Type", typeof(Panel), TestPanel.CollapsablePanel.GetType());
			AssertEquals("CollapsablePanel ID", "Panel", TestPanel.CollapsablePanel.ID);
		}

		#endregion TestControls

		#region TestOnPreRenderInternal

		public void TestOnPreRenderInternal()
		{
			TestPanel.Label = "Splurg";
			TestPanel.CssClass = "SplurgClass";
			TestPanel.Show = false;

			TestPanel.OnPreRenderInternal();

			AssertEquals("LabelControl Text", "Splurg", TestPanel.LabelControl.Text);
			AssertEquals("LabelControl CssClass", "ZCollapsablePanelLabel SplurgClass", TestPanel.LabelControl.CssClass);
			AssertEquals("LabelControl DoubleClick handler", TestPanel.ShowHideClickHandler, TestPanel.LabelControl.Attributes["ondblclick"]);
			AssertEquals("LabelControl should be unselectable", "on", TestPanel.LabelControl.Attributes["unselectable"]);

			AssertEquals("ExpandCollapseIcon should use hand cursor", "hand", TestPanel.ExpandCollapseIcon.Style["cursor"]);
			AssertEquals("ExpandCollapseIcon should display expand image", TestPanel.ExpandIcon.FileName, TestPanel.ExpandCollapseIcon.ImageUrl);
			AssertEquals("ExpandCollapseIcon click handler", TestPanel.ShowHideClickHandler, TestPanel.ExpandCollapseIcon.Attributes["onclick"]);

			AssertEquals("CollapsablePanel should not be displayed", "none", TestPanel.CollapsablePanel.Style["display"]);
		}

		public void TestOnPreRenderInternalWithDisabledCollapsing()
		{
			TestPanel.Label = "Splurg";
			TestPanel.CssClass = "SplurgClass";
			TestPanel.DisableCollapsing = true;

			TestPanel.OnPreRenderInternal();

			AssertEquals("LabelControl Text", "Splurg", TestPanel.LabelControl.Text);
			AssertEquals("LabelControl CssClass", "ZCollapsablePanelLabel SplurgClass", TestPanel.LabelControl.CssClass);
			AssertEquals("LabelControl DoubleClick handler should be unassigned", null, TestPanel.LabelControl.Attributes["ondblclick"]);
			AssertEquals("LabelControl should be unselectable", "on", TestPanel.LabelControl.Attributes["unselectable"]);

			AssertEquals("ExpandCollapseIcon cursor should be unassigned", null, TestPanel.ExpandCollapseIcon.Style["cursor"]);
			AssertEquals("ExpandCollapseIcon ImageUrl should be unassigned", "", TestPanel.ExpandCollapseIcon.ImageUrl);
			AssertEquals("ExpandCollapseIcon click handler should be unassigned", null, TestPanel.ExpandCollapseIcon.Attributes["onclick"]);

			AssertEquals("CollapsablePanel should not be displayed", "none", TestPanel.CollapsablePanel.Style["display"]);
		}

		#endregion TestOnPreRenderInternal

		#region TestProperties

		public void TestCssStyleIsAppliedToLabel()
		{
			AssertEquals("PreCondition: Label CSS should be empty", "", TestPanel.LabelControl.CssClass);
			AssertEquals("PreCondition: Panel CSS should be empty", "ZCollapsablePanelLabel", TestPanel.CssClass);
			TestPanel.CssClass = "TestCSS";
			AssertEquals("Label CSS", "TestCSS", TestPanel.LabelControl.CssClass);
			AssertEquals("Panel CSS should proxy through to Label", "ZCollapsablePanelLabel TestCSS", TestPanel.CssClass);
		}

		public void TestLabel()
		{
			AssertEquals("PreCondition: Label should be empty", "", TestPanel.Label);
			TestPanel.Label = "BlahdyBlah";
			AssertEquals("Label", "BlahdyBlah", TestPanel.Label);
		}

		public void TestDisableCollapsing()
		{
			AssertEquals("PreCondition: Panel Collapsing should be enabled by default", false, TestPanel.DisableCollapsing);
			AssertEquals("PreCondition: Panel should be expanded by default", true, TestPanel.Show);
			TestPanel.Show = false;
			AssertEquals("Panel should be collapsed", false, TestPanel.Show);
			TestPanel.DisableCollapsing = true;

			AssertEquals("Panel Collapsing should be disabled", true, TestPanel.DisableCollapsing);
			AssertEquals("PreCondition: Panel should be expanded", true, TestPanel.Show);
			TestPanel.Show = false;
			AssertEquals("Panel should be shown", true, TestPanel.Show);
		}

		public void TestShow()
		{
			AssertEquals("PreCondition: Panel should be expanded by default", true, TestPanel.Show);
			TestPanel.Show = false;
			AssertEquals("Panel should be hidden", false, TestPanel.Show);
			TestPanel.Show = true;
			AssertEquals("Panel should be shown", true, TestPanel.Show);
		}

		public void TestShowHideClickHandler()
		{
			TestPanel.ID = "TestPanel1";
			TestPanel.CreateChildControlsForTesting();

			AssertEquals("CollapsablePanel NamingContainer", TestPanel.ID, TestPanel.CollapsablePanel.NamingContainer.ID);
			AssertEquals("ExpandCollapseIcon NamingContainer", TestPanel.ID, TestPanel.ExpandCollapseIcon.NamingContainer.ID);

			var assemblyPath = ZString.Format(@"/Runtime/Enterprise.ZArchitecture.Web.GUI/{0}/ZCollapsablePanel/", TestPanel.CollapseIcon.AssemblyVersion).Replace(".", "_");
			var expandIconFilename = assemblyPath + "plus.bmp";
			var collapseIconFilename = assemblyPath + "minus.bmp";
			var expHandler = ZString.Format("ZCollapsablePanel_ExpandCollapse('{0}', '{1}', '{2}', '{3}', 'ShowHideValueTestPanel1')", "TestPanel1_Panel", "TestPanel1_ExpandCollapseIcon", expandIconFilename, collapseIconFilename);
			AssertEquals("ShowHideClickHandler", expHandler, TestPanel.ShowHideClickHandler);
		}

		public void TestResources()
		{
			var version = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(TestPanel.GetType().Assembly, typeof(AssemblyFileVersionAttribute))).Version;
			var expRuntimeDirectory = ZString.Format(@"/Runtime/Enterprise.ZArchitecture.Web.GUI/{0}/ZCollapsablePanel/", version).Replace(".", "_");
			AssertEquals("ShowHideScript", String.Format("{0}{1}", expRuntimeDirectory, "ZCollapsablePanelScript.js"), TestPanel.ShowHideScript.FileName);
			AssertEquals("ExpandIcon", String.Format("{0}{1}", expRuntimeDirectory, "plus.bmp"), TestPanel.ExpandIcon.FileName);
			AssertEquals("CollapseIcon", String.Format("{0}{1}", expRuntimeDirectory, "minus.bmp"), TestPanel.CollapseIcon.FileName);

			AssertNotNull("Resources", TestPanel.Resources);
			AssertEquals("Resources", 3, TestPanel.Resources.Count);

			AssertCollectionContains("Script Resource", TestPanel.ShowHideScript, TestPanel.Resources);
			AssertCollectionContains("CollapseIcon Resource", TestPanel.CollapseIcon, TestPanel.Resources);
			AssertCollectionContains("ExpandIcon Resource", TestPanel.ExpandIcon, TestPanel.Resources);
		}

		public void TestShowHideScriptBlock()
		{
			var version = ((AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(TestPanel.GetType().Assembly, typeof(AssemblyFileVersionAttribute))).Version;
			var expRuntimeDirectory = ZString.Format(@"/Runtime/Enterprise.ZArchitecture.Web.GUI/{0}/ZCollapsablePanel/", version);
			var expScriptBlock = String.Format("<script type=\"text/javascript\" src=\"{0}\"></script>", TestPanel.ShowHideScript.FileName);
			AssertEquals("ScriptBlock", expScriptBlock, TestPanel.ShowHideScriptBlock);
		}

		#endregion TestProperties

		#region Implementation

		ZCollapsablePanelForTest TestPanel
		{
			get { return Control as ZCollapsablePanelForTest; }
		}

		protected override Control GetNewControl()
		{
			return new ZCollapsablePanelForTest();
		}

		class ZCollapsablePanelForTest : ZCollapsablePanel
		{
			#region Test Properties

			public void EnsureChildControlsForTesting() => EnsureChildControls();
			public void CreateChildControlsForTesting() => CreateChildControls();

			#endregion
		}

		#endregion Implementation
	}
}
