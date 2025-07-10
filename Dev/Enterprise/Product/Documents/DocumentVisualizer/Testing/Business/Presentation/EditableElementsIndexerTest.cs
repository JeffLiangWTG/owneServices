using System.Collections.Generic;
using System.Drawing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Presentation;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class EditableElementsIndexerTest : TestCase
	{
		public void TestIndexLayoutElements()
		{
			var documentView = new DummyDocumentView();

			var pageView1 = new DummyPageView();
			var pageView2 = new DummyPageView();

			documentView.PageViews.Add(pageView1);
			documentView.PageViews.Add(pageView2);

			var dummyEditableData = new KeyValuePair<string, IDynamicData>("xxx", "xxx".MakeDynamic());

			var cell1 = new DummyCell
			{
				EditableData = { dummyEditableData }
			};

			var cell2 = new DummyCell
			{
				EditableData = { dummyEditableData }
			};

			var cell3 = new DummyCell();

			var cell4 = new DummyCell
			{
				EditableData = { dummyEditableData }
			};

			var cell5 = new DummyCell
			{
				EditableData = { dummyEditableData }
			};

			var textLayoutElement = new TextLayoutElement(new Text(new PointF(5, 0), new SizeF(10, 10), "bla"));

			var dynamicContentLayout1 = new DynamicContentLayoutElement(
				new DynamicContent(cell1, new PointF(0, 0), new SizeF(10, 10)));

			var dynamicContentLayout2 = new DynamicContentLayoutElement(
				new DynamicContent(cell2, new PointF(10, 0), new SizeF(10, 10)));

			var dynamicContentLayout3 = new DynamicContentLayoutElement(
				new DynamicContent(cell3, new PointF(20, 0), new SizeF(10, 10)));

			var dynamicContentLayout4 = new DynamicContentLayoutElement(
				new DynamicContent(cell4, new PointF(0, 10), new SizeF(10, 10)));

			var dynamicContentLayout5 = new DynamicContentLayoutElement(
				new DynamicContent(cell5, new PointF(0, 0), new SizeF(10, 10)));

			pageView1.Elements = new ILayoutElement[]
			{
				textLayoutElement,
				dynamicContentLayout1,
				dynamicContentLayout2,
				dynamicContentLayout3,
				dynamicContentLayout4
			};

			pageView2.Elements = new ILayoutElement[]
			{
				dynamicContentLayout5
			};

			var tabManager = new EditableElementsIndexer();

			tabManager.Index(pageView1);
			tabManager.Index(pageView2);

			AssertEquals("first in tab order; page 1 @ [0,0]", dynamicContentLayout1, tabManager.GetNext(null));
			AssertEquals("second in tab order; page 1 @  [10, 0]", dynamicContentLayout2, tabManager.GetNext(dynamicContentLayout1));
			AssertEquals("third in tab order; page 1 @  [0, 10]", dynamicContentLayout4, tabManager.GetNext(dynamicContentLayout2));
			AssertEquals("fourth in tab order; page 2 @ [0, 0]", dynamicContentLayout5, tabManager.GetNext(dynamicContentLayout4));
			AssertEquals("after the last one back to first; page 1 @ [0, 0]", dynamicContentLayout1, tabManager.GetNext(dynamicContentLayout5));
		}
	}
}