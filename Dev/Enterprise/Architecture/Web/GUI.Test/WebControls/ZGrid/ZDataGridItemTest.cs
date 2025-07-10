using System;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDataGridItemTest : WebControlTest
	{
		public void TestItemsRefAttributeWhenGridHasIncludeItemDataRefKeySetToTrue()
		{
			TestGrid.IncludeItemDataRefKey = true;

			CreateLotsOfChildren(TestBizO, 4);
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, TestBizO.PK);
			TestBizO.Collection.Load(filter);
			TestGrid.BindTo = "Collection";
			TestGrid.Columns.Add(new ZTextEditColumn("Text", DummyBizoSchema.Z0_VarCharMax.Name));
			TestGrid.Bind(TestBizO);

			var items = TestGrid.Items.Cast<ZDataGridItem>();
			AssertEquals("Should have four grid items", 4, items.Count());
			AssertEquals("Each grid item should have a unique ref attribute", 4, items.GroupBy(x => x.Attributes["ref"]).Count());
			Assert("Each grid item ref attribute is referencing one of the business objects PK", items.All(i => TestBizO.Collection.Any(o => o.PK.ToString() == i.Attributes["ref"])));
		}

		public void TestItemsRefAttributeWhenGridHasIncludeItemDataRefKeySetToFalse()
		{
			TestGrid.IncludeItemDataRefKey = false;

			CreateLotsOfChildren(TestBizO, 4);
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, TestBizO.PK);
			TestBizO.Collection.Load(filter);
			TestGrid.BindTo = "Collection";
			TestGrid.Columns.Add(new ZTextEditColumn("Text", DummyBizoSchema.Z0_VarCharMax.Name));
			TestGrid.Bind(TestBizO);

			var items = TestGrid.Items.Cast<ZDataGridItem>();
			AssertEquals("Should have four grid items", 4, items.Count());
			Assert("Grid items should not have ref attribute", items.All(i => i.Attributes["ref"] == null));
		}

		protected override Control GetNewControl()
		{
			return new ZDataGridItem(0, 0, ListItemType.Item);
		}

		ZDataGrid TestGrid
		{
			get
			{
				if (fTestGrid == null)
				{
					fTestGrid = new ZDataGrid();
				}
				return fTestGrid;
			}
		}
		ZDataGrid fTestGrid;

		public void TestEmptyNotificationsWhenRowNotificationsAreNull()
		{
			ZDataGridItem testItem = Control as ZDataGridItem;
			AssertNotNull("TestItem should not be null", testItem);
			AssertEquals("Item should return ZNotification.Emtpy prior to binding", NotificationCollection.Empty, testItem.Notifications);
		}

		public void TestNotifications()
		{
			CreateLotsOfChildren(TestBizO, 10);
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, TestBizO.PK);
			TestBizO.Collection.Load(filter);
			TestGrid.AllowPaging = true;
			TestGrid.BindTo = "Collection";
			TestGrid.Columns.Add(new ZTextEditColumn("Text", DummyBizoSchema.Z0_VarCharMax.Name));

			TestGrid.Bind(TestBizO);

			Page.OnPreRenderForTesting();

			ZDataGridItem testItem = TestGrid.Items[2] as ZDataGridItem;
			AssertNotNull("DataGridItem should not be null", testItem);
			AssertEquals("Should contain no Errors", 0, testItem.Notifications.GetErrors().Count());
			AssertEquals("Should contain no MessageErrors", 0, testItem.Notifications.GetMessageErrors().Count());
			AssertEquals("Should contain no Warnings", 0, testItem.Notifications.GetWarnings().Count());

			TestBizO.Collection[2].AddRowError("This is a dummy error");
			TestGrid.Bind(TestBizO);

			Page.OnPreRenderForTesting();

			testItem = TestGrid.Items[2] as ZDataGridItem;
			AssertNotNull("DataGridItem should not be null", testItem);
			AssertEquals("Should contain single Error", 1, testItem.Notifications.GetErrors().Count());
			AssertEquals("Should contain no MessageErrors", 0, testItem.Notifications.GetMessageErrors().Count());
			AssertEquals("Should contain no Warnings", 0, testItem.Notifications.GetWarnings().Count());

			AssertEquals("Error Message", "This is a dummy error", testItem.Notifications.GetErrors().GetFirstMessage());
		}

		void CreateLotsOfChildren(DummyBusinessObject parent, int numToCreate)
		{
			for (int i = 0; i < numToCreate; i++)
			{
				DummyChildBusinessObject child = Factory.NewWithValidTestData<DummyChildBusinessObject>();
				child.Z0_Guid = parent.PK;
			}
			Factory.Save();
		}

		public void TestRenderChildrenWithoutZNewRowColumns()
		{
			TestGrid.Columns.Add(new ZTextEditColumn("Text", DummyBizoSchema.Z0_VarCharMax.Name));
			TestGrid.Columns.Add(new ZDateTimeColumn("Text", DummyBizoSchema.Z0_Date.Name));

			CreateLotsOfChildren(TestBizO, 5);
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, TestBizO.PK);
			TestBizO.Collection.Load(filter);
			TestGrid.BindTo = "Collection";
			TestGrid.Bind(TestBizO);
			Page.OnPreRenderForTesting();

			StringWriter sw = new StringWriter();
			HtmlTextWriter tw = new HtmlTextWriter(sw);

			tw.RenderBeginTag(HtmlTextWriterTag.Tr);

			foreach (ZDataGridItem item in TestGrid.Items)
			{
				item.RenderChildrenInternal(tw);
			}

			tw.RenderEndTag();

			tw.Flush();
			string renderedHtml = sw.ToString();

			AssertEquals("Should not contain a new row if there are no ZNewRowColumns.", false, renderedHtml.Replace(" ", String.Empty).Contains("</tr><tr>"));
		}

		public void TestRenderChildrenWithZNewRowColumns()
		{
			TestGrid.Columns.Add(new ZTextEditColumn("Text", DummyBizoSchema.Z0_VarCharMax.Name));
			TestGrid.Columns.Add(new ZDateTimeColumn("Text", DummyBizoSchema.Z0_Date.Name));
			TestGrid.Columns.Add(new ZNewRowColumn(DummyBizoSchema.Z0_VarCharMax.Name));

			CreateLotsOfChildren(TestBizO, 5);
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, TestBizO.PK);
			TestBizO.Collection.Load(filter);
			TestGrid.BindTo = "Collection";
			TestGrid.Bind(TestBizO);
			Page.OnPreRenderForTesting();

			StringWriter sw = new StringWriter();
			HtmlTextWriter tw = new HtmlTextWriter(sw);

			tw.RenderBeginTag(HtmlTextWriterTag.Tr);

			foreach (ZDataGridItem item in TestGrid.Items)
			{
				item.RenderChildrenInternal(tw);
			}

			tw.RenderEndTag();

			tw.Flush();
			string renderedHtml = sw.ToString();

			AssertEquals("Should contain a new row if there are no ZNewRowColumns.", true, renderedHtml.Replace(" ", String.Empty).Contains("</tr><tr"));
		}

		public void TestRenderChildrenWithZNewRowColumns_Collapsible_NotExpanded()
		{
			AssertRenderChildrenWithZNewRowColumns_Collapsible(false);
		}

		public void TestRenderChildrenWithZNewRowColumns_Collapsible_Expanded()
		{
			AssertRenderChildrenWithZNewRowColumns_Collapsible(true);
		}

		void AssertRenderChildrenWithZNewRowColumns_Collapsible(bool expand)
		{
			var newRowColumn = new ZNewRowColumn(DummyBizoSchema.Z0_VarCharMax.Name);
			newRowColumn.Collapsable = true;
			TestGrid.Columns.Add(newRowColumn);
			CreateLotsOfChildren(TestBizO, 1);

			var filter = new ZQuery(DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, TestBizO.PK);
			TestBizO.Collection.Load(filter);
			TestGrid.BindTo = "Collection";
			TestGrid.Bind(TestBizO);

			Page.OnPreRenderForTesting();

			var sw = new StringWriter();
			var tw = new HtmlTextWriter(sw);

			tw.RenderBeginTag(HtmlTextWriterTag.Tr);

			TestGrid.Items[0].Cells[0].Controls.Add(new ZExpandCollapseButton());
			((ZExpandCollapseButton)TestGrid.Items[0].Cells[0].Controls[0]).Expand = expand;

			foreach (ZDataGridItem item in TestGrid.Items)
			{
				item.RenderChildrenInternal(tw);
			}

			tw.RenderEndTag();

			tw.Flush();
			var renderedHtml = sw.ToString();

			AssertEquals("Row should not contain display none if button is expanded", !expand, renderedHtml.Contains("style=\"display:none;\""));
		}
	}
}
