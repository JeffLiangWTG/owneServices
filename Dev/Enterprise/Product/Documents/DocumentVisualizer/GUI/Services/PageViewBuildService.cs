using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class PageViewBuildService : IPageViewBuildService
	{
		#region CreatePageView

		public IPageView CreatePageView(IPage page, IPageViewPresenter presenter, bool isReadOnly)
		{
			var creator = new DocumentElementsCreator(page, isReadOnly);

			var pageElements = creator.CreateElements();

			var deDuplicatedElements = RemoveDuplicates(pageElements);

			var layoutElements = deDuplicatedElements
				.Select(LayoutElementFactory.Create)
				.ToArray();

			var pageSize = new SizeF((float)page.Width, (float)page.Height);

			var pageView = new PageView(pageSize, presenter);
			pageView.Elements = layoutElements;
#if WINZOR
			pageView.AddControls(layoutElements);
#endif
			return pageView;
		}
		IEnumerable<IDocumentElement> RemoveDuplicates(IEnumerable<IDocumentElement> documentElements)
		{
			return new HashSet<IDocumentElement>(documentElements, new DocumentElementComparer());
		}

#endregion

		#region DocumentElementComparer

		sealed class DocumentElementComparer : IEqualityComparer<IDocumentElement>
		{
			public bool Equals(IDocumentElement elem1, IDocumentElement elem2)
			{
				return elem1 != null && elem2 != null
					&& elem1.ElementType == elem2.ElementType
					&& elem1.Location == elem2.Location
					&& elem1.Size == elem2.Size
					&& elem1.ZOrder == elem2.ZOrder;
			}

			public int GetHashCode(IDocumentElement elem)
			{
				const int prime1 = 3;
				const int prime2 = 5;

				unchecked // Overflow is fine, just wrap
				{
					int hash = prime1;

					hash = hash * prime2 + elem.ElementType.GetHashCode();
					hash = hash * prime2 + elem.Location.GetHashCode();
					hash = hash * prime2 + elem.Size.GetHashCode();
					hash = hash * prime2 + elem.ZOrder.GetHashCode();

					return hash;
				}
			}
		}

		#endregion
	}
}
