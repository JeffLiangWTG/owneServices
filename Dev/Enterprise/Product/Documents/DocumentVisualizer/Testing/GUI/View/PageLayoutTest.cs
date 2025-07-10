using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.GUI;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	sealed class PageLayoutTest : TestCaseWithFactory
	{
		public void PerformLayout_HorizontalPageAlignment()
		{
			var page = new Control();
			page.Width = 100;
			page.Height = 50;

			var pagesContainer = new ContainerControl();
			pagesContainer.Width = 200;
			pagesContainer.Controls.Add(page);

			var layoutEngine = new PageLayoutEngine(pagesContainer);
			layoutEngine.PerformLayout();

			AssertEquals(page.Left, page.Right - page.Width);

			page.Parent.Width /= 2;
			layoutEngine.PerformLayout();

			AssertEquals(page.Left, page.Right - page.Width);
		}

		public void TestPerformLayout_AllPagesFit()
		{
			var page1 = CreatePageControl();
			var page2 = CreatePageControl();
			var page3 = CreatePageControl();

			var pagesContainer = new Panel();
			pagesContainer.AutoScroll = true;
			pagesContainer.Width = 200;
			pagesContainer.Controls.Add(page1);
			pagesContainer.Controls.Add(page2);
			pagesContainer.Controls.Add(page3);

			var layoutEngine = new PageLayoutEngine(pagesContainer);
			layoutEngine.PerformLayout();

			AssertContainsExactElementsInExactOrder("all pages have been laid out vertically",
				new[] { page1, page2, page3 },
				pagesContainer
					.Controls
					.Cast<Control>()
					.Where(c => c.Visible)
					.OrderBy(c => c.Location.Y));
		}

		public void TestPerformLayout_PagesOverflowMaximumCapacity()
		{
			var pages = Enumerable
				.Range(1, 25)
				.Select(n => CreatePageControl($"page{n}"))
				.ToArray();

			var pagesContainer = new Panel();
			pagesContainer.AutoScroll = true;
			pagesContainer.Width = 200;
			pagesContainer.Controls.AddRange(pages);

			var layoutEngine = new PageLayoutEngine(pagesContainer);
			layoutEngine.PerformLayout();

			var expectedPagesThatFit = CalculatePagesThatFit(pagesContainer.DisplayRectangle.Height, pages);

			AssertContainsExactElementsInExactOrder($"{expectedPagesThatFit} pages have been laid out vertically",
				pages.Take(expectedPagesThatFit).Select(p => p.Name),
				pagesContainer
					.Controls
					.Cast<Control>()
					.Where(c => c.Visible)
					.OrderBy(c => c.Location.Y)
					.Select(p => p.Name));

			AssertContainsExactElementsInExactOrder($"Rest of the pages that didn't fit aren't visible",
				pages.Skip(expectedPagesThatFit).Select(p => p.Name),
				pagesContainer
					.Controls
					.Cast<Control>()
					.Where(c => !c.Visible)
					.Select(p => p.Name));
		}

		public void TestPerformLayout_PagesOverflowMaximumCapacity_LoadMorePagesOnScroll()
		{
			var pages = Enumerable
				.Range(1, 25)
				.Select(n => CreatePageControl($"page{n}"))
				.ToArray();

			var pagesContainer = new Panel();
			pagesContainer.AutoScroll = true;
			pagesContainer.Width = 200;
			pagesContainer.Controls.AddRange(pages);

			var layoutEngine = new PageLayoutEngine(pagesContainer);
			layoutEngine.PerformLayout();

			string[] GetVisiblePages() => pagesContainer
				.Controls
				.Cast<Control>()
				.Where(c => c.Visible)
				.OrderBy(c => c.Location.Y)
				.Select(p => p.Name)
				.ToArray();

			var visiblePages = GetVisiblePages();
			Assert("some pages were laid out", visiblePages.Length > 0);

			ScrollDown(pagesContainer);
			layoutEngine.HandleScroll();

			var visiblePages2 = GetVisiblePages();
			Assert("more pages were laid out after scrolling", visiblePages2.Length > visiblePages.Length);

			ScrollDown(pagesContainer);
			layoutEngine.HandleScroll();

			var visiblePages3 = GetVisiblePages();
			Assert("more pages were laid out after scrolling", visiblePages3.Length > visiblePages2.Length);
		}

		int CalculatePagesThatFit(int availableHeight, Control[] pages)
		{
			var expectedPagesThatFit = 0;
			var totalHeight = 0;

			foreach (var page in pages)
			{
				totalHeight += page.Margin.Top + page.Height;

				if (totalHeight > availableHeight)
				{
					break;
				}

				expectedPagesThatFit++;
			}

			return expectedPagesThatFit;
		}

		Control CreatePageControl(string name = "page") => new Control
		{
			Name = name,
			Width = 1191,  // actual A4 page width
			Height = 1684, // actual A4 page height
			Margin = new Padding(10)
		};

		void ScrollDown(Panel panel, int pos = 1)
		{
			panel.VerticalScroll.Maximum = panel.Controls.Cast<Control>().Sum(p => p.Margin.Top + p.Height);
			panel.VerticalScroll.Value = panel.VerticalScroll.Maximum - 10;

			using (Control dummy = new Control { Parent = panel, Height = 1000, Top = panel.DisplayRectangle.Height + pos })
			{
				panel.ScrollControlIntoView(dummy);
			}
		}
	}
}
