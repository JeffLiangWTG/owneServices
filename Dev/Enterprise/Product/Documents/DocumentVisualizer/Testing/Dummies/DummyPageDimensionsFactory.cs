using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyPageDimensionsFactory
	{
		public static PageDimensions Get(PaperType paperType)
		{
			var pageDimensions = new PageDimensions();

			switch (paperType)
			{
				case PaperType.A4:
					pageDimensions.Height = 1169;
					pageDimensions.Width = 827;
					break;

				case PaperType.Letter:
					pageDimensions.Height = 1100;
					pageDimensions.Width = 850;
					break;
			}

			return pageDimensions;
		}

		public enum PaperType
		{
			Letter,
			A4
		}
	}
}