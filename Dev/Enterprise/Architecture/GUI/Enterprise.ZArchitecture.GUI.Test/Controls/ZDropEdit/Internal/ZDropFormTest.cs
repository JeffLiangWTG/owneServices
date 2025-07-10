using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZDropFormTest : TestCaseWithFactory
	{
#if !WINZOR
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1067:DoNotApplicationThreadException", Justification = "Baseline")]
		public void TestMouseHookProcThrowsException()
		{
			var log = new List<string>();
			Application.ThreadException += (sender, a) => HandleThreadException(log, a.Exception);
			AppDomain.CurrentDomain.UnhandledException += (sender, a) => HandleUnhandledException(log, a);
			try
			{
				using (var dropEdit = new ZDropEdit())
				using (var dropForm = new MockZDropForm(dropEdit))
				{
					dropForm.NeedThrowException = true;
					dropForm.MouseHookProc(1, new IntPtr(0), new IntPtr(0));
					AssertEquals(1, log.Count);
					AssertEquals("Trigger ThreadException: TEST ERROR", log[0]);
				}
			}
			finally
			{
				Application.ThreadException -= (sender, a) => HandleThreadException(log, a.Exception);
				AppDomain.CurrentDomain.UnhandledException -= (sender, a) => HandleUnhandledException(log, a);
			}

			void HandleThreadException(List<string> list, Exception e)
			{
				list.Add("Trigger ThreadException: " + e.Message);
			}

			void HandleUnhandledException(List<string> list, UnhandledExceptionEventArgs e)
			{
				list.Add("Trigger UnhandledException");
			}
		}
#endif
		public void TestFindItem()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("CD1", "Description 1");
			list.AddPair("CD2", "Description 2");
			list.AddPair("CD3", "Description 3");

			using (var parent = new ZDropEdit { List = list })
			using (var dropForm = new ZDropForm(parent))
			{
				parent.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;

				dropForm.SelectItemFromCode("C");
				AssertEquals(0, dropForm.HighlightedItem_Exposed);

				dropForm.SelectItemFromCode("CD2");
				AssertEquals(1, dropForm.HighlightedItem_Exposed);

				dropForm.SelectItemFromCode("CD3");
				AssertEquals(2, dropForm.HighlightedItem_Exposed);

				AssertNoExceptionThrown(() => dropForm.SelectItemFromCode("D"));

				parent.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;

				dropForm.SelectItemFromCode("C");
				AssertEquals(0, dropForm.HighlightedItem_Exposed);

				dropForm.SelectItemFromCode("CD2");
				AssertEquals(1, dropForm.HighlightedItem_Exposed);

				dropForm.SelectItemFromCode("3");
				AssertEquals(-1, dropForm.HighlightedItem_Exposed);

				AssertNoExceptionThrown(() => dropForm.SelectItemFromCode("D"));
				AssertNoExceptionThrown(() => dropForm.SelectItemFromCode(" 3"));

				parent.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowDescription;

				dropForm.SelectItemFromCode("D");
				AssertEquals(0, dropForm.HighlightedItem_Exposed);

				dropForm.SelectItemFromCode("Description 2");
				AssertEquals(1, dropForm.HighlightedItem_Exposed);

				dropForm.SelectItemFromCode(" 3");
				AssertEquals(-1, dropForm.HighlightedItem_Exposed);

				AssertNoExceptionThrown(() => dropForm.SelectItemFromCode("C"));
				AssertNoExceptionThrown(() => dropForm.SelectItemFromCode("CD2"));
			}
		}

		public void TestSelectItemFromEmptyCode()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("", "Blank Code Description");
			list.AddPair("CD", "Code Description");

			using (var parent = new ZDropEdit { List = list, SupportsEmptyCode = false })
			using (var dropForm = new ZDropForm(parent))
			{
				dropForm.SelectItemFromCode("CD");
				AssertEquals(1, dropForm.HighlightedItem_Exposed);

				dropForm.SelectItemFromCode("");
				AssertEquals(-1, dropForm.HighlightedItem_Exposed);
			}

			using (var parent = new ZDropEdit { List = list, SupportsEmptyCode = true })
			using (var dropForm = new ZDropForm(parent))
			{
				dropForm.SelectItemFromCode("CD");
				AssertEquals(1, dropForm.HighlightedItem_Exposed);

				dropForm.SelectItemFromCode("");
				AssertEquals(0, dropForm.HighlightedItem_Exposed);
			}
		}
#if !WINZOR
		public void TestPaintMethods()
		{
			var expectedBrush = BrushProvider.FromColor(Color.FromArgb(Math.Max(SystemColors.Highlight.A - 120, 0), SystemColors.Highlight));
			var clipRect = new Rectangle();

			using (var dropEdit = new ZDropEdit())
			using (var dropForm = new MockZDropForm(dropEdit))
			using (var paintEvent = new PaintEventArgs(dropForm.CreateGraphics(), clipRect))
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("CD1", "Description 1");
				list.AddPair("CD2", "Description 2");
				list.AddPair("CD3", "Description 3");
				dropEdit.List = list;

				AssertPaintItem(dropForm, dropEdit, ZDropEdit.ShowInDropDownList.ShowCodeAndDescription, paintEvent, clipRect, list[0], "CD1", null, true, false, true);
				AssertPaintItem(dropForm, dropEdit, ZDropEdit.ShowInDropDownList.ShowCodeAndDescription, paintEvent, clipRect, list[0], "CD1", expectedBrush, false, true, true);

				AssertPaintItem(dropForm, dropEdit, ZDropEdit.ShowInDropDownList.OnlyShowCode, paintEvent, clipRect, list[0], "CD1", null, true, false, false);
				AssertPaintItem(dropForm, dropEdit, ZDropEdit.ShowInDropDownList.OnlyShowCode, paintEvent, clipRect, list[0], "CD1", null, false, true, false);

				AssertPaintItem(dropForm, dropEdit, ZDropEdit.ShowInDropDownList.OnlyShowDescription, paintEvent, clipRect, list[0], "Description 1", null, true, false, false);
				AssertPaintItem(dropForm, dropEdit, ZDropEdit.ShowInDropDownList.OnlyShowDescription, paintEvent, clipRect, list[0], "Description 1", null, false, true, false);
			}
		}

		void AssertPaintItem(MockZDropForm dropForm, ZDropEdit dropEdit, ZDropEdit.ShowInDropDownList showInDropDown, PaintEventArgs paintEvent, Rectangle clipRect, ICodeDescription item, string expectedItemText, Brush expectedBrush, bool isHighlighted, bool isHighlightedForMouseMove, bool isPaintDescriptionBackgroundCalled)
		{
			dropEdit.ShowInDropDown = showInDropDown;
			dropForm.OnPaintExposed(paintEvent);
			AssertEquals(isPaintDescriptionBackgroundCalled, dropForm.IsPaintDescriptionBackgroundCalled);
			dropForm.PaintItemExposed(paintEvent.Graphics, item, clipRect, isHighlighted, isHighlightedForMouseMove);
			AssertEquals(isPaintDescriptionBackgroundCalled, dropForm.IsPaintDarkenedDescriptionItemCalled);
			AssertEquals(expectedItemText, dropForm.ItemText);
			AssertEquals(expectedBrush, dropForm.DescriptionBrush);
			dropForm.DescriptionBrush = null;
		}

		public void TestPaintPassword()
		{
			var clipRect = new Rectangle();

			using (var dropEdit = new ZDropEdit())
			using (var dropForm = new MockZDropForm(dropEdit))
			using (var paintEvent = new PaintEventArgs(dropForm.CreateGraphics(), clipRect))
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("CD1", "Description 1");
				list.AddPair("CD2", "Description 2");
				dropEdit.List = list;
				dropEdit.PasswordChar = '*';
				dropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
				dropForm.OnPaintExposed(paintEvent);
				dropForm.PaintItemExposed(paintEvent.Graphics, list[0], clipRect, false, false);
				AssertEquals("***", dropForm.ItemText);
			}
		}

		public void TestCodeAndDescriptionWidth()
		{
			AssertCodeAndDescriptionWidth(ZDropEdit.ShowInDropDownList.ShowCodeAndDescription, true, true);
			AssertCodeAndDescriptionWidth(ZDropEdit.ShowInDropDownList.OnlyShowCode, true, false);
			AssertCodeAndDescriptionWidth(ZDropEdit.ShowInDropDownList.OnlyShowDescription, false, true);
		}

		void AssertCodeAndDescriptionWidth(ZDropEdit.ShowInDropDownList showInDropDown, bool hasCodeWidth, bool hasDescriptionWidth)
		{
			using (var dropEdit = new ZDropEdit())
			using (var dropForm = new MockZDropForm(dropEdit))
			{
				var list = new CodeDescriptionPairList();
				list.AddPair("1", "Description 1");
				list.AddPair("002", "Description 2");
				list.AddPair("03", "Description 3 - that's long");

				dropEdit.List = list;

				dropEdit.ShowInDropDown = showInDropDown;

				float codeWidth = -1f, descriptionWidth = 0.0f;
				using (var graphics = Graphics.FromHwnd(dropForm.Handle))
				{
					graphics.PageUnit = GraphicsUnit.Pixel;

					if (hasCodeWidth)
					{
						codeWidth = (int)Math.Ceiling(graphics.MeasureString("002", dropForm.Font).Width);
					}

					if (hasDescriptionWidth)
					{
						descriptionWidth = (int)Math.Ceiling(graphics.MeasureString(dropForm.ItemSpacerExposed() + "Description 3 - that's long", dropForm.Font).Width);
					}
				}

				AssertEquals(codeWidth, dropForm.CodeWidthExposed(), 1);
				AssertEquals(descriptionWidth, dropForm.DescriptionWidthExposed(), 1);
			}
		}

		public void TestUnhookWithHandlerOnDropDownClosed()
		{
			using (var dropEdit = new ZDropEdit())
			using (var dropForm = new MockZDropForm(dropEdit))
			{
				dropForm.ShowDropDown(new Point(0, 0), true);
				var mouseHookLocal = IntPtr.Zero;
				dropEdit.DropDownClosed += (o, e) => { mouseHookLocal = dropForm.mouseHook; };
				dropForm.HideDropDown();
				AssertEquals(IntPtr.Zero, mouseHookLocal);
			}
		}

		public void TestHorizontalScrollBar()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("CD1", "This is a very long description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   .");
			using (var dropEdit = new ZDropEdit { List = list })
			{
				dropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
				using (var dropForm = new MockZDropForm(dropEdit))
				{
					AssertNull("HorizontalScrollBar", dropForm.HorizontalScrollBar_Exposed());
				}

				dropEdit.ShowHorizontalScrollBar = true;
				using (var dropForm = new MockZDropForm(dropEdit))
				{
					AssertNotNull("HorizontalScrollBar", dropForm.HorizontalScrollBar_Exposed());
				}
			}
		}
#endif
	}
}
