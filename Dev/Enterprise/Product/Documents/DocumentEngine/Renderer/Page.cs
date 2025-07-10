using System.Linq;
using Enterprise.DocumentEngine.Areas;

namespace Enterprise.DocumentEngine.Renderer
{
	internal class Page
	{
		internal readonly AreaCollection Areas = new AreaCollection();
		int height;

		internal Area LastArea
		{
			get
			{
				Area result = null;
				if (Areas.Count > 0)
				{
					result = Areas[Areas.Count - 1];
				}

				return result;
			}
		}

		internal int Height
		{
			get { return height; }
			set { height = value; }
		}

		internal int AvailableHeight
		{
			get { return Height - Areas.Height; }
		}

		internal bool PageBroken { get; set; }

		internal bool ContainsAnySectionBodyOrDocumentHeaderArea
		{
			get { return Areas.Any(area => area is SectionBodyArea || area is DocumentHeaderArea); }
		}
	}
}
