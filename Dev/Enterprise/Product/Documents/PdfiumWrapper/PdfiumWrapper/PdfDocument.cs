using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using CargoWise.PdfiumWrapper.Native;

namespace CargoWise.PdfiumWrapper
{
	public class PdfDocument : IDisposable
	{
		readonly DocumentHandle handle;

		PdfDocument(DocumentHandle ptr)
		{
			handle = Argument.NotNull(ptr, nameof(ptr));
		}

		public PdfDocument()
			: this(SafeNativeMethods.CreateNewDocument())
		{
		}

		public PdfDocument(Stream s, string password = null)
			: this(SafeNativeMethods.LoadDocument(s, password))
		{
		}

		public static PdfDocument LoadFile(string path, string password = null)
		{
			Stream readStream = null;
			try
			{
				readStream = File.OpenRead(path);
				return new PdfDocument(readStream, password);
			}
			catch
			{
				readStream?.Dispose();
				throw;
			}
		}

		~PdfDocument()
			=> Dispose(false);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Disposable pattern requires an isDisposing param, but we don't need to use it here")]
		void Dispose(bool isDisposing)
			=> handle?.Dispose();

		public int PageCount
			=> SafeNativeMethods.GetPageCount(handle);

		public int SignatureCount => SafeNativeMethods.GetSignatureCount(handle);

		public SizeF GetPageSizeInPoints(int pageNum)
			=> SafeNativeMethods.GetPageSizeInPoints(handle, pageNum);

		public void InsertPages(PdfDocument other, int insertIndex, params int[] pages)
			=> SafeNativeMethods.ImportPages(handle, Argument.NotNull(other, nameof(other)).handle, pages, insertIndex);

		public void Save(Stream stream)
			=> SafeNativeMethods.Save(handle, Argument.NotNull(stream, nameof(stream)));

		public PrintDocument ToPrintDocument()
			=> new PdfPrintDocument(this);

		public void Save(string path, FileMode mode = FileMode.Create)
		{
			using (var fileStream = new FileStream(path, mode))
			{
				Save(fileStream);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is not only used for the UI. Handle DPI scaling higher up the stack")]
		public void RenderPage(Graphics g, int pageNum, Size size, RenderPageRotation rotation = RenderPageRotation.CW0)
			=> RenderPage(g, pageNum, new Rectangle(Point.Empty, size), rotation);

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is not only used for the UI. Handle DPI scaling higher up the stack")]
		internal void RenderPage(Graphics g, int pageNum, Rectangle bounds, RenderPageRotation rotation = RenderPageRotation.CW0, bool fillBackgroundWithWhite = true)
		{
			using (var page = SafeNativeMethods.LoadPage(handle, pageNum))
			{
				if (fillBackgroundWithWhite)
				{
					g.FillRectangle(Brushes.White, bounds);
				}

				if (GetBackingImage(g) is Bitmap bmp)
				{
					SafeNativeMethods.RenderPageToBitmap(bmp, handle, page, bounds.Location, bounds.Size, rotation, PdfRenderFlags.ANNOT);
				}
				else
				{
					SafeNativeMethods.RenderPage(g, page, bounds.Location, bounds.Size, rotation, PdfRenderFlags.ANNOT);
				}
			}
		}

		static Image GetBackingImage(Graphics g)
		{
			var field = typeof(Graphics).GetField("backingImage", BindingFlags.NonPublic | BindingFlags.Instance)
				?? typeof(Graphics).GetField("_backingImage", BindingFlags.NonPublic | BindingFlags.Instance);
			return (Image)field.GetValue(g);
		}

		public string GetAllText()
		{
			var resultStringBuilder = new System.Text.StringBuilder();
			for (int i = 0; i < PageCount; i++)
			{
				using (var page = SafeNativeMethods.LoadPage(handle, i))
				using (var textPage = SafeNativeMethods.LoadTextPage(page))
				{
					var result = SafeNativeMethods.GetPageText(textPage);
					resultStringBuilder.Append(result);
				}
			}
			return resultStringBuilder.ToString();
		}

		public IEnumerable<SearchResult> Search(string text, int startPage = 0, int startCharacter = 0, PdfSearchFlags flags = PdfSearchFlags.MATCHCASE)
		{
			for (int i = startPage; i < PageCount; i++)
			{
				using (var page = SafeNativeMethods.LoadPage(handle, i))
				using (var textPage = SafeNativeMethods.LoadTextPage(page))
				using (var search = SafeNativeMethods.StartSearchPage(textPage, text, flags, startCharacter))
				{
					bool continuePage = true;
					while (continuePage)
					{
						continuePage = SafeNativeMethods.FindNext(search);
						if (continuePage)
						{
							int characterIndex = SafeNativeMethods.GetSearchResultIndex(search);
							yield return new SearchResult(i, characterIndex, true);
						}

						if (handle.IsClosed || handle.IsInvalid)
						{
							throw new ObjectDisposedException(nameof(PdfDocument));
						}
					}
				}
			}
		}

		public class SearchResult
		{
			public SearchResult(int page, int character, bool found)
			{
				Page = page;
				Character = character;
				Found = found;
			}

			public readonly bool Found;
			public readonly int Page;
			public readonly int Character;
		}
	}
}
