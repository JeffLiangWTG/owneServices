using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using CargoWise.Common;
using Enterprise.DocumentEngine.PreviewableDocument;

namespace Enterprise.DocumentEngine.Imaging
{
	public class PreviewableDocumentImageReader : BaseImageFileReader
	{
		public PreviewableDocumentImageReader(IPreviewableDocument document)
		{
			Document = document;
			PageSelector = new PreviewableImagePageSelector(document);
		}

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "We do not claim ownership of the IPreviewableDocument.")]
		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				PageSelector.Dispose();
			}
		}

		public override IImagePageSelector PageSelector { get; }
		public IPreviewableDocument Document { get; }
	}

	public class PreviewableImagePageSelector : Disposable, IImagePageSelector
	{
		readonly IPreviewableDocument document;

		public PreviewableImagePageSelector(IPreviewableDocument document)
		{
			this.document = document;
		}

		public int CurrentPageIndex
		{
			get { return currentIndex; }
			set
			{
				if (value >= 0 && value < document.NumberOfPages)
				{
					currentIndex = value;
				}
			}
		}
		int currentIndex;

		public IImagePageSelector PageSelector => this;

		public int TotalPages => document.NumberOfPages;

		public Image CurrentImage
		{
			get
			{
				Image rasterizedPage;
				if (!renderedPages.TryGetValue(CurrentPageIndex, out rasterizedPage))
				{
					rasterizedPage = RenderPage(CurrentPageIndex);
					renderedPages.Add(CurrentPageIndex, rasterizedPage);
				}
				return rasterizedPage;
			}
		}
		readonly Dictionary<int, Image> renderedPages = new Dictionary<int, Image>();

		public void ClearCache()
		{
			foreach (var page in renderedPages.Values)
			{
				page.Dispose();
			}
			renderedPages.Clear();
		}

		Bitmap RenderPage(int pageNb)
		{
			var size = document.GetPageSize(pageNb);
			Bitmap bitmap = null;
			try
			{
				bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
				using (var g = Graphics.FromImage(bitmap))
				{
					document.Render(g, pageNb, size);
				}

				return bitmap;
			}
			catch
			{
				bitmap?.Dispose();
				throw;
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				ClearCache();
				document.Dispose();
			}
		}
	}
}
