using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Enterprise.DocumentVisualizer.Presentation
{
	[DebuggerDisplay("Elements = {indexedElements.Count}")]
	sealed class EditableElementsIndexer : IDisposable
	{
		SortedList<OrderKey, IDynamicContentLayoutElement> ElementsInTabOrder
		{
			get { return indexedElements ?? (indexedElements = new SortedList<OrderKey, IDynamicContentLayoutElement>(new OrderKeyComparer())); }
		}

		SortedList<OrderKey, IDynamicContentLayoutElement> indexedElements;

		#region Nested types

		[DebuggerDisplay("[Page {PageNumber}, Row {RowNumber}, Column {ColumnNumber}")]
		struct OrderKey
		{
			public int PageNumber { get; set; }
			public int RowNumber { get; set; }
			public int ColumnNumber { get; set; }
		}

		sealed class OrderKeyComparer : IComparer<OrderKey>
		{
			int IComparer<OrderKey>.Compare(OrderKey key1, OrderKey key2)
			{
				var result = key1.PageNumber.CompareTo(key2.PageNumber);

				if (result != 0)
				{
					return result;
				}

				result = key1.ColumnNumber.CompareTo(key2.ColumnNumber);

				if (result != 0)
				{
					return result;
				}

				result = key1.RowNumber.CompareTo(key2.RowNumber);

				return result;
			}
		}

		#endregion

		int pages;

		public void Index(IPageView pageView)
		{
			if (pageView != null && pageView.Elements != null)
			{
				var pageIndex = pages++;

				var elements = pageView.Elements
					.Where(elem => elem.Element.ElementType == ElementType.DynamicContent)
					.Cast<IDynamicContentLayoutElement>()
					.Where(elem => elem.EditableData.Any());

				IndexLayoutElements(pageIndex, elements);
			}
		}

		public IDynamicContentLayoutElement GetNext(IDynamicContentLayoutElement element)
		{
			IDynamicContentLayoutElement result = null;

			var index = element != null
				? ElementsInTabOrder.IndexOfValue(element)
				: -1;

			if (index < 0 || index >= ElementsInTabOrder.Count - 1)
			{
				result = ElementsInTabOrder.Any()
					? ElementsInTabOrder.First().Value
					: null;
			}
			else if (index >= 0)
			{
				result = ElementsInTabOrder.ElementAt(index + 1).Value;
			}

			return result;
		}

		public IDynamicContentLayoutElement GetPrevious(IDynamicContentLayoutElement element)
		{
			IDynamicContentLayoutElement result = null;

			var index = element != null
				? ElementsInTabOrder.IndexOfValue(element)
				: -1;

			if (index <= 0 || index > ElementsInTabOrder.Count - 1)
			{
				result = ElementsInTabOrder.Any()
					? ElementsInTabOrder.Last().Value
					: null;
			}
			else if (index >= 0)
			{
				result = ElementsInTabOrder.ElementAt(index - 1).Value;
			}

			return result;
		}

		#region Implementation

		void IndexLayoutElements(int pageIndex, IEnumerable<IDynamicContentLayoutElement> elements)
		{
			foreach (var element in elements)
			{
				var key = GetKey(pageIndex, element);

				if (!ElementsInTabOrder.ContainsKey(key))
				{
					ElementsInTabOrder.Add(key, element);
				}
			}
		}

		OrderKey GetKey(int pageIndex, IDynamicContentLayoutElement element)
		{
			return new OrderKey
			{
				PageNumber = pageIndex,
				RowNumber = (int)element.Element.Location.X,
				ColumnNumber = (int)element.Element.Location.Y,
			};
		}

		#endregion

		#region IDisposable members

		public void Dispose()
		{
			indexedElements = null;
		}

		#endregion
	}
}