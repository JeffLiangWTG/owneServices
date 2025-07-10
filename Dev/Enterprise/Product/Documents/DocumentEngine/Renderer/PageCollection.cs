using System;
using System.Collections.ObjectModel;

namespace Enterprise.DocumentEngine.Renderer
{
	internal class PageCollection : Collection<Page>
	{
		internal PageCollection()
		{
		}

		internal int DefaultPageHeight { get; set; }

		protected override void InsertItem(int index, Page page)
		{
			ControlDpiScalingHelper.SetHeight(ref page, DefaultPageHeight, false);
			base.InsertItem(index, page);
		}

		public int FindIndex(int startIndex, Predicate<Page> match)
		{
			int result = -1;

			for (var index = startIndex; index < Count; index++)
			{
				var page = this[index];

				if (match(page))
				{
					result = IndexOf(page);
					break;
				}
			}

			return result;
		}

		internal Page AddNew()
		{
			var result = new Page();
			Add(result);
			return result;
		}

		internal Page LastPage
		{
			get
			{
				Page result = null;

				if (Count > 0)
				{
					result = this[Count - 1];
				}

				return result;
			}
		}
	}
}
