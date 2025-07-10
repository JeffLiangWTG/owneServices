using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace CargoWise.PdfiumWrapper.Native
{
	static class SafeNativeMethods
	{
		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "This is a hack to call DestroyLibrary. When the application is closed the GC will run, calling the InitWrapper destructor and destroying pdfium")]
		[SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "It is used - we need it to stay in scope while application is open")]
		[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "It is used - we need it to stay in scope while application is open")]
		static readonly InitWrapper libraryInitialisation;

		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "We must load the pdfium library before we access it, and there is no better time to do so than when creating the class. Creating it inline failed because the field was never accessed (and thus never initialised).")]
		static SafeNativeMethods()
		{
			libraryInitialisation = new InitWrapper();
		}

		public static DocumentHandle CreateNewDocument()
		{
			return new DocumentHandle(UnsafeNativeMethods.FPDF_CreateNewDocument());
		}

		[Obsolete("This will not support unicode file paths", true)]
		public static DocumentHandle LoadDocument(string path, string password = null)
		{
			return new DocumentHandle(UnsafeNativeMethods.FPDF_LoadDocument(path, password));
		}

		public static DocumentHandle LoadDocument(Stream stream, string password = null)
		{
			if (!stream.CanSeek)
			{
				throw new ArgumentException("Pdfium requires a seekable stream. Consider a memory buffer or saving to disk.", nameof(stream));
			}

			var getBlockDelegate = CreateGetBlockDelegateForStream(stream);
			var fileAccess = new FPDF_FILEACCESS
			{
				m_Param = IntPtr.Zero,
				m_FileLen = (uint)stream.Length,
				GetBlock = Marshal.GetFunctionPointerForDelegate(getBlockDelegate)
			};

			var ptr = UnsafeNativeMethods.FPDF_LoadCustomDocument(fileAccess, password);
			return new DocumentHandle(ptr, stream, getBlockDelegate, fileAccess);
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Throwing an exception in a native context will crash the entire application. Instead we pass it around the native code.")]
		static FPDF_GetBlockDelegate CreateGetBlockDelegateForStream(Stream stream)
		{
			return (IntPtr param, uint position, IntPtr pBuf, uint size) =>
			{
				try
				{
					var localBuffer = new byte[size];
					stream.Seek(position, SeekOrigin.Begin);

					if (stream.Read(localBuffer, 0, (int)size) != (int)size)
					{
						return 0;
					}

					Marshal.Copy(localBuffer, 0, pBuf, (int)size);
					return 1;
				}
				catch (Exception ex)
				{
					UnsafeNativeMethods.NotifyExceptionThrownInNativeContext(ex);
					return 0;
				}
			};
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is not only used for the UI. Handle DPI scaling higher up the stack")]
		internal static void RenderPageToBitmap(Bitmap bitmap, DocumentHandle document, PageHandle page, Point location, Size size, RenderPageRotation rotation, PdfRenderFlags renderFlags)
		{
			var data = bitmap.LockBits(new Rectangle(location, size), ImageLockMode.ReadWrite, bitmap.PixelFormat);
			var bitmapHandle = IntPtr.Zero;
			try
			{
				bitmapHandle = UnsafeNativeMethods.FPDFBitmap_CreateEx(size.Width, size.Height, DeterminePdfiumPixelFormat(bitmap.PixelFormat), data.Scan0, data.Stride);

				UnsafeNativeMethods.FPDF_RenderPageBitmap(bitmapHandle, page, location.X, location.Y, size.Width, size.Height, rotation, renderFlags);

				if (renderFlags.HasFlag(PdfRenderFlags.ANNOT))
				{
					var formEnvironment = UnsafeNativeMethods.FPDFDOC_InitFormFillEnvironment(document, new FPDF_FORMFILLINFO());

					UnsafeNativeMethods.FPDF_FFLDraw(formEnvironment, bitmapHandle, page, location.X, location.Y, size.Width, size.Height, rotation, PdfRenderFlags.NONE);
				}
			}
			finally
			{
				if (bitmapHandle != IntPtr.Zero)
				{
					UnsafeNativeMethods.FPDFBitmap_Destroy(bitmapHandle);
				}

				bitmap.UnlockBits(data);
			}
		}

		static PdfBitmapFlags DeterminePdfiumPixelFormat(PixelFormat format)
		{
			switch (format)
			{
				case PixelFormat.Format24bppRgb: return PdfBitmapFlags.FPDFBitmap_BGR;
				case PixelFormat.Format32bppRgb: return PdfBitmapFlags.FPDFBitmap_BGRx;
				case PixelFormat.Format32bppArgb: return PdfBitmapFlags.FPDFBitmap_BGRA;

				default:
					throw new ArgumentException("Unsupported format: " + format);
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Throwing an exception in a native context will crash the entire application. Instead we pass it around the native code.")]
		[SuppressMessage("Microsoft.Reliability", "CA2004:RemoveCallsToGCKeepAlive", Justification = "The document safe handle does not (and should not) reference the save delegate. It might be GC'd while we're still attempting to write to the document")]
		public static void Save(DocumentHandle doc, Stream s)
		{
			int SaveBlock(IntPtr pThis, IntPtr pData, uint size)
			{
				try
				{
					var buffer = new byte[size];
					Marshal.Copy(pData, buffer, 0, (int)size);
					s.Write(buffer, 0, (int)size);
					return 1;
				}
				catch (Exception ex)
				{
					UnsafeNativeMethods.NotifyExceptionThrownInNativeContext(ex);
					return 0;
				}
			}

			var writeBlock = new FPDF_SaveBlockDelegate(SaveBlock);
			var fileWrite = new FPDF_FILEWRITE
			{
				version = 1,
				WriteBlock = Marshal.GetFunctionPointerForDelegate(writeBlock)
			};

			if (!UnsafeNativeMethods.FPDF_SaveAsCopy(doc, fileWrite, PdfSaveFlags.NoIncremental))
			{
				throw new PdfiumException(PdfiumError.Unknown);
			}

			GC.KeepAlive(writeBlock);
		}

		public static PageHandle LoadPage(DocumentHandle document, int pageNum)
		{
			if (pageNum < 0 || pageNum >= GetPageCount(document))
			{
				throw new ArgumentOutOfRangeException(nameof(pageNum), "Should be greater than zero and less than document page count");
			}

			var pageHandle = new PageHandle(UnsafeNativeMethods.FPDF_LoadPage(document, pageNum));

			pageHandle.Parent = document;
			document.Children.Add(pageHandle);

			return pageHandle;
		}

		public static int GetPageCount(DocumentHandle document)
			=> UnsafeNativeMethods.FPDF_GetPageCount(document);

		public static SizeF GetPageSizeInPoints(DocumentHandle document, int index)
		{
			if (index < 0 || index >= GetPageCount(document))
			{
				throw new ArgumentOutOfRangeException(nameof(index), "Should be greater than zero and less than document page count");
			}

			double w = 0, h = 0;
			var result = UnsafeNativeMethods.FPDF_GetPageSizeByIndex(document, index, ref w, ref h);
			if (result == 0)
			{
				if (index > 0)
				{
					throw new ArgumentException("Not all pages could be loaded.", nameof(document));
				}
				else
				{
					throw new ArgumentException("Document could not be found - has your handle been disposed?", nameof(document));
				}
			}

			return new SizeF((float)w, (float)h);
		}

		public static void RenderPage(Graphics g, PageHandle page, Point location, Size size, RenderPageRotation rotate, PdfRenderFlags flags)
		{
			var hdc = g.GetHdc();
			try
			{
				UnsafeNativeMethods.FPDF_RenderPage(hdc, page, location.X, location.Y, size.Width, size.Height, rotate, flags);
			}
			finally
			{
				g.ReleaseHdc(hdc);
			}
		}

		public static void ImportPages(DocumentHandle destination, DocumentHandle source, int[] pages, int index)
		{
			if (!TryConvertToPageRange(source, pages, out var pageRange))
			{
				throw new ArgumentOutOfRangeException(nameof(pages), "all values must be between 0 and PageCount or NULL");
			}

			if (index > GetPageCount(destination))
			{
				throw new ArgumentOutOfRangeException(nameof(index), "Must be greater than zero and less than or equal to PageCount");
			}

			if (!UnsafeNativeMethods.FPDF_ImportPages(destination, source, pageRange, index))
			{
				throw new PdfiumException(PdfiumError.CannotImportPages); //If no exception occurs, and import pages unsuccessfully, let's throw a custom CannotImportPages PdfiumException
			}
		}

		static bool TryConvertToPageRange(DocumentHandle doc, int[] indicies, out string range)
		{
			if (indicies == null || indicies.Length == 0)
			{
				range = null;
				return true;
			}

			var pageCount = GetPageCount(doc);
			if (indicies.Any(i => i < 0 || i >= pageCount))
			{
				range = null;
				return false;
			}

			range = string.Join(",", indicies.Select(i => i + 1));
			return true;
		}

		public static TextPageHandle LoadTextPage(PageHandle page)
		{
			var textPageHandle = new TextPageHandle(UnsafeNativeMethods.FPDFText_LoadPage(page));
			textPageHandle.Parent = page;
			page.Children.Add(textPageHandle);

			return textPageHandle;
		}

		public static int GetPageCharactersCount(TextPageHandle textPage)
		{
			return UnsafeNativeMethods.FPDFText_CountChars(textPage);
		}

		public static string GetPageText(TextPageHandle textPage)
		{
			const int kBytesPerCharacter = 2;
			var characterCount = GetPageCharactersCount(textPage);
			var bufferSize = (characterCount + 1) * kBytesPerCharacter;
			string result;

			try
			{
				var localBuffer = new byte[bufferSize];
				UnsafeNativeMethods.FPDFText_GetText(textPage, 0, characterCount, localBuffer);
				result = System.Text.Encoding.Unicode.GetString(localBuffer);
			}
			catch (ArgumentOutOfRangeException)
			{
				result = String.Empty;
			}

			return result;
		}

		public static SearchHandle StartSearchPage(TextPageHandle textPage, string findWhat, PdfSearchFlags flags, int startIndex)
		{
			var searchHandle = new SearchHandle(UnsafeNativeMethods.FPDFText_FindStart(textPage, findWhat, flags, startIndex));
			searchHandle.Parent = textPage;
			textPage.Children.Add(searchHandle);

			return searchHandle;
		}

		public static bool FindNext(SearchHandle handle)
		{
			return UnsafeNativeMethods.FPDFText_FindNext(handle);
		}

		public static int GetSearchResultIndex(SearchHandle handle)
		{
			return UnsafeNativeMethods.FPDFText_GetSchResultIndex(handle);
		}

		public static int GetSignatureCount(DocumentHandle doc)
		{
			return UnsafeNativeMethods.FPDFText_GetSignatureCount(doc);
		}

		class InitWrapper
		{
			public InitWrapper()
			{
				UnsafeNativeMethods.LoadPdfiumLibrary();
				UnsafeNativeMethods.FPDF_InitLibrary();
			}

			~InitWrapper()
			{
				try
				{
					UnsafeNativeMethods.FPDF_DestroyLibrary();
				}
				catch (DllNotFoundException ex)
				{
					var filePath = UnsafeNativeMethods.GetPdfiumLibraryFilePath();
					if (!File.Exists(filePath))
					{
						throw new CannotFoundPdfiumLibraryException("Can not found pdfium.dll", ex);
					}
					else
					{
						RetryDestroyLibrary();
					}
				}
			}

			void RetryDestroyLibrary()
			{
				try
				{
					UnsafeNativeMethods.FPDF_DestroyLibrary();
				}
				catch (Exception ex)
				{
					throw new CannotDestroyPdfiumLibraryException("Can not destroy pdfium.dll", ex);
				}
			}
		}
	}
}
