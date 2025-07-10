using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace CargoWise.PdfiumWrapper.Native
{
	static class UnsafeNativeMethods
	{
		public static void LoadPdfiumLibrary()
		{
			var filePath = GetPdfiumLibraryFilePath();
			LoadLibrary(filePath);
		}

		internal static string GetPdfiumLibraryFilePath()
		{
			var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var folder = (IntPtr.Size == 8) ? "x64" : "x86";
			var path = Path.Combine(binFolder, folder, "pdfium.dll");
			
			// In Winzor deployments, the executing assembly will be in the AppServer subfolder.
			// This will cause the generated path to be incorrect, so we use the parent directory as a fallback.
			if (!File.Exists(path))
			{
				binFolder = Path.GetDirectoryName(binFolder);
				path = Path.Combine(binFolder, folder, "pdfium.dll");
			}

			return path;
		}

		[DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
		static extern IntPtr LoadLibrary(string lpFileName);

		public static void FPDF_InitLibrary()
			=> PerformNativeCall(PdfiumImports.FPDF_InitLibrary);

		public static IntPtr FPDFDOC_InitFormFillEnvironment(DocumentHandle document, FPDF_FORMFILLINFO formInfo)
			=> PerformNativeCall(() => PdfiumImports.FPDFDOC_InitFormFillEnvironment(document, formInfo));

		public static void FPDF_DestroyLibrary()
			=> PerformNativeCall(PdfiumImports.FPDF_DestroyLibrary);

		public static IntPtr FPDF_LoadDocument(string path, string password)
			=> PerformNativeCall(() => PdfiumImports.FPDF_LoadDocument(path, password), checkErrorOnEmptyResponse: true);

		public static IntPtr FPDF_LoadCustomDocument(FPDF_FILEACCESS pFileAccess, string password)
			=> PerformNativeCall(() => PdfiumImports.FPDF_LoadCustomDocument(pFileAccess, password), checkErrorOnEmptyResponse: true);

		public static void FPDF_CloseDocument(IntPtr document)
			=> PerformNativeCall(() => PdfiumImports.FPDF_CloseDocument(document));

		public static IntPtr FPDFBitmap_CreateEx(int width, int height, PdfBitmapFlags format, IntPtr first_scan, int stride)
			=> PerformNativeCall(() => PdfiumImports.FPDFBitmap_CreateEx(width, height, format, first_scan, stride));

		public static void FPDFBitmap_Destroy(IntPtr bitmap)
			=> PerformNativeCall(() => PdfiumImports.FPDFBitmap_Destroy(bitmap));

		public static int FPDF_GetPageCount(DocumentHandle document)
			 => PerformNativeCall(() => PdfiumImports.FPDF_GetPageCount(document));

		public static IntPtr FPDF_LoadPage(DocumentHandle document, int page_index)
			=> PerformNativeCall(() => PdfiumImports.FPDF_LoadPage(document, page_index));

		public static void FPDF_ClosePage(IntPtr page)
			=> PerformNativeCall(() => PdfiumImports.FPDF_ClosePage(page));

		public static void FPDF_RenderPage(IntPtr dc, PageHandle page, int start_x, int start_y, int size_x, int size_y, RenderPageRotation rotate, PdfRenderFlags flags)
			=> PerformNativeCall(() => PdfiumImports.FPDF_RenderPage(dc, page, start_x, start_y, size_x, size_y, rotate, flags));

		public static void FPDF_RenderPageBitmap(IntPtr bitmapHandle, PageHandle page, int start_x, int start_y, int size_x, int size_y, RenderPageRotation rotate, PdfRenderFlags flags)
			=> PerformNativeCall(() => PdfiumImports.FPDF_RenderPageBitmap(bitmapHandle, page, start_x, start_y, size_x, size_y, rotate, flags));

		public static void FPDF_FFLDraw(IntPtr formHandle, IntPtr bitmapHandle, PageHandle page, int start_x, int start_y, int size_x, int size_y, RenderPageRotation rotate, PdfRenderFlags flags)
			=> PerformNativeCall(() => PdfiumImports.FPDF_FFLDraw(formHandle, bitmapHandle, page, start_x, start_y, size_x, size_y, rotate, flags));

		public static bool FPDF_ImportPages(DocumentHandle dest_doc, DocumentHandle src_doc, string pagerange, int index)
			=> PerformNativeCall(() => PdfiumImports.FPDF_ImportPages(dest_doc, src_doc, pagerange, index), checkErrorOnEmptyResponse: true);

		public static IntPtr FPDF_CreateNewDocument()
			=> PerformNativeCall(() => PdfiumImports.FPDF_CreateNewDocument());

		public static bool FPDF_SaveAsCopy(DocumentHandle document, FPDF_FILEWRITE pFileWrite, PdfSaveFlags flags)
			=> PerformNativeCall(() => PdfiumImports.FPDF_SaveAsCopy(document, pFileWrite, flags));

		public static int FPDF_GetPageSizeByIndex(DocumentHandle document, int page_index, ref double width, ref double height)
		{
			double w = 0, h = 0;
			var result = PerformNativeCall(() => PdfiumImports.FPDF_GetPageSizeByIndex(document, page_index, ref w, ref h));
			width = w;
			height = h;
			return result;
		}

		public static IntPtr FPDFText_LoadPage(PageHandle page)
			=> PerformNativeCall(() => PdfiumImports.FPDFText_LoadPage(page));

		public static void FPDFText_ClosePage(IntPtr textPage)
			=> PerformNativeCall(() => PdfiumImports.FPDFText_ClosePage(textPage));

		public static int FPDFText_CountChars(TextPageHandle textPage)
			=> PerformNativeCall(() => PdfiumImports.FPDFText_CountChars(textPage));

		public static int FPDFText_GetText(TextPageHandle textPage, int startIndex, int count, byte[] result)
			=> PerformNativeCall(() => PdfiumImports.FPDFText_GetText(textPage, startIndex, count, result));

		public static IntPtr FPDFText_FindStart(TextPageHandle textPage, string findWhat, PdfSearchFlags flags, int startIndex)
			=> PerformNativeCall(() => PdfiumImports.FPDFText_FindStart(textPage, findWhat, flags, startIndex));

		public static bool FPDFText_FindNext(SearchHandle handle)
			=> PerformNativeCall(() => PdfiumImports.FPDFText_FindNext(handle));

		public static void FPDFText_FindClose(IntPtr handle)
			=> PerformNativeCall(() => PdfiumImports.FPDFText_FindClose(handle));

		public static int FPDFText_GetSchResultIndex(SearchHandle handle)
			=> PerformNativeCall(() => PdfiumImports.FPDFText_GetSchResultIndex(handle));

		public static int FPDFText_GetSignatureCount(DocumentHandle documentHandle)
			=> PerformNativeCall(() => PdfiumImports.FPDF_GetSignatureCount(documentHandle));

		static void PerformNativeCall(Action call)
			=> PerformNativeCall(() => { call(); return 0; });

		static T PerformNativeCall<T>(Func<T> call, bool checkErrorOnEmptyResponse = false)
		{
			lock (nativeCallLock)
			{
				exceptionThrownByDelegateInNativeCode = null;

				var result = call();

				var error = (checkErrorOnEmptyResponse && Equals(result, default(T))) ?
					(PdfiumError)PdfiumImports.FPDF_GetLastError() :
					PdfiumError.None;

				if (exceptionThrownByDelegateInNativeCode != null || error != PdfiumError.None)
				{
					if (error == PdfiumError.FileNotFound)
					{
						throw new FileNotFoundException("File not found", exceptionThrownByDelegateInNativeCode);
					}
					else if (!Enum.IsDefined(typeof(PdfiumError), error))
					{
						error = PdfiumError.Unknown;
					}

					throw new PdfiumException(error, exceptionThrownByDelegateInNativeCode);
				}

				return result;
			}
		}

		public static void NotifyExceptionThrownInNativeContext(Exception ex)
			=> Interlocked.CompareExchange(ref exceptionThrownByDelegateInNativeCode, ex, null);

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Locked, but can't use ThreadSafeAttribute here")]
		static Exception exceptionThrownByDelegateInNativeCode;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Locked, but can't use ThreadSafeAttribute here")]
		static readonly object nativeCallLock = new object();

		static class PdfiumImports
		{
			/// <summary>
			/// Initialize the FPDFSDK library
			/// </summary>
			/// <remarks>
			/// Convenience function to call FPDF_InitLibraryWithConfig() for
			/// backwards comatibility purposes.
			/// </remarks>
			[DllImport("pdfium.dll")]
			public static extern void FPDF_InitLibrary();

			/// <summary>
			/// Release all resources allocated by the FPDFSDK library.
			/// </summary>
			/// <remarks>
			///  You can call this function to release all memory blocks allocated by
			///  the library.
			///  After this function is called, you should not call any PDF
			///  processing functions.
			/// </remarks>
			[DllImport("pdfium.dll")]
			public static extern void FPDF_DestroyLibrary();

			/// <summary>
			/// Initialises the form fill environment. This function should be called before any form fill operation.
			/// </summary>
			/// <param name="document">Handle to document. Returned by FPDF_LoadDocument function.</param>
			/// <param name="formInfo">Pointer to a FPDF_FORMFILLINFO structure.</param>
			/// <returns>Return handler to the form fill module. NULL means fail.</returns>
			[DllImport("pdfium.dll")]
			public static extern IntPtr FPDFDOC_InitFormFillEnvironment(DocumentHandle document, FPDF_FORMFILLINFO formInfo);

			/// <summary>
			/// Create a device independent bitmap (FXDIB)
			/// </summary>
			/// <param name="width">The number of pixels in width for the bitmap. Must be greater than 0. </param>
			/// <param name="height">The number of pixels in height for the bitmap. Must be greater than 0. </param>
			/// <param name="format">A number indicating for bitmap format, as defined below.</param>
			/// <param name="first_scan">A pointer to the first byte of the first line if using an external buffer. If this parameter is NULL, then the a new buffer will be created.</param>
			/// <param name="stride">Number of bytes for each scan line, for external buffer only.</param>
			/// <returns>The bitmap handle, or NULL if parameter error or out of memory.</returns>
			/// <remarks>
			///  Similar to FPDFBitmap_Create function, but allows for more formats
			///  and an external buffer is supported.The bitmap created by this
			///  function can be used in any place that a FPDF_BITMAP handle is
			///  required.
			///
			///  If an external buffer is used, then the application should destroy
			///  the buffer by itself.FPDFBitmap_Destroy function will not destroy
			///  the buffer.
			/// </remarks>
			[DllImport("pdfium.dll")]
			public static extern IntPtr FPDFBitmap_CreateEx(int width, int height, PdfBitmapFlags format, IntPtr first_scan, int stride);

			/// <summary>
			/// Destroy a bitmap and release all related buffers.
			/// </summary>
			/// <param name="bitmapHandle">Handle to the bitmap. Returned by FPDFBitmap_Create or FPDFImageObj_GetBitmap</param>
			/// <remarks>
			/// This function will not destroy any external buffers provided when the bitmap was created
			/// </remarks>
			[DllImport("pdfium.dll")]
			public static extern void FPDFBitmap_Destroy(IntPtr bitmapHandle);

			/// <summary>
			/// Open and load a PDF document.
			/// </summary>
			/// <param name="file_path">Path to the PDF file (including extension).</param>
			/// <param name="password">A string used as the password for the PDF file. If no password is needed, empty or NULL can be used.</param>
			/// <returns> A handle to the loaded document, or NULL on failure. </returns>
			/// <remarks>
			/// Loaded document can be closed by FPDF_CloseDocument().
			/// If this function fails, you can use FPDF_GetLastError() to retrieve
			/// the reason why it failed.
			/// </remarks>
			[DllImport("pdfium.dll", BestFitMapping = false, ThrowOnUnmappableChar = true)]
			public static extern IntPtr FPDF_LoadDocument([MarshalAs(UnmanagedType.LPStr)] string path, [MarshalAs(UnmanagedType.LPStr)] string password);

			/// <summary>
			/// Load PDF document from a custom access descriptor.
			/// </summary>
			/// <param name="pFileAccess">A structure for accessing the file.</param>
			/// <param name="password">Optional password for decrypting the PDF file.</param>
			/// <returns>A handle to the loaded document, or NULL on failure.</returns>
			/// <remarks>
			/// If PDFium is built with the XFA module, the application should call
			/// FPDF_LoadXFA() function after the PDF document loaded to support XFA
			/// fields defined in the fpdfformfill.h file.
			/// 
			/// (WTG: We aren't using XFA)
			/// </remarks>
			[DllImport("pdfium.dll", BestFitMapping = false, ThrowOnUnmappableChar = true)]
			public static extern IntPtr FPDF_LoadCustomDocument(FPDF_FILEACCESS pFileAccess, [MarshalAs(UnmanagedType.LPStr)] string password);

			/// <summary>
			/// Close a loaded PDF document.
			/// </summary>
			/// <param name="document">Handle to the loaded document.</param>
			[DllImport("pdfium.dll")]
			public static extern void FPDF_CloseDocument(IntPtr document);

			/// <summary>
			/// Get last error code when a function fails.
			/// </summary>
			/// <returns> A 32-bit integer indicating error code as defined above. </returns>
			/// <remarks>
			///  If the previous SDK call succeeded, the return value of this
			///  function is not defined.
			/// </remarks>
			[DllImport("pdfium.dll")]
			public static extern uint FPDF_GetLastError();

			/// <summary>
			/// Get total number of pages in the document.
			/// </summary>
			/// <param name="document">Handle to document. Returned by FPDF_LoadDocument.</param>
			/// <returns> Total number of pages in the document. </returns>
			[DllImport("pdfium.dll")]
			public static extern int FPDF_GetPageCount(DocumentHandle document);

			/// <summary>
			/// Get the size of the page at the given index.
			/// </summary>
			/// <param name="document">Handle to document. Returned by FPDF_LoadDocument.</param>
			/// <param name="page_index">Page index, zero for the first page.</param>
			/// <param name="width">Pointer to a double to receive the page width (in points).</param>
			/// <param name="height">Pointer to a double to receive the page height (in points).</param>
			/// <returns> Non-zero for success. 0 for error (document or page not found). </returns>
			[DllImport("pdfium.dll")]
			public static extern int FPDF_GetPageSizeByIndex(DocumentHandle document, int page_index, ref double width, ref double height);

			/// <summary>
			/// Load a page inside the document.
			/// </summary>
			/// <param name="document">Handle to document. Returned by FPDF_LoadDocument</param>
			/// <param name="page_index">Index number of the page. 0 for the first page.</param>
			/// <returns> A handle to the loaded page, or NULL if page load fails. </returns>
			/// <remarks>
			/// The loaded page can be rendered to devices using FPDF_RenderPage.
			/// The loaded page can be closed using FPDF_ClosePage.
			/// </remarks>
			[DllImport("pdfium.dll")]
			public static extern IntPtr FPDF_LoadPage(DocumentHandle document, int page_index);

			/// <summary>
			/// Close a loaded PDF page
			/// </summary>
			/// <param name="page">Handle to the loaded page</param>
			[DllImport("pdfium.dll")]
			public static extern void FPDF_ClosePage(IntPtr page);

			/// <summary>
			/// Render FormFields and popup windows on a page to a device independent bitmap.
			/// In order to implement the FormFill functions, implementation should call this function after rendering functions,
			/// such as FPDF_RenderPageBitmap or FPDF_RenderPageBitmap_Start, finish rendering the page contents.
			/// </summary>
			/// <param name="formHandle">Handle to the form fill module. Returned by FPDFDOC_InitFormFillEnvironment.</param>
			/// <param name="bitmapHandle"> Handle to the device independent bitmap (as the output buffer). The bitmap handle can be created by FPDFBitmap_Create fucntion.</param>
			/// <param name="page">Handle to the page. Returned by FPDF_LoadPage function.</param>
			/// <param name="start_x">Left pixel position of the display area in bitmap coordinates.</param>
			/// <param name="start_y">Top pixel position of the display area in bitmap coordinates.</param>
			/// <param name="size_x">Horizontal size (in pixels) for displaying the page.</param>
			/// <param name="size_y">Vertical size (in pixels) for displaying the page.</param>
			/// <param name="rotate">
			/// Page orientation:
			///     0 (normal)
			///     1 (rotated 90 degrees clockwise)
			///     2 (rotated 180 degrees)
			///     3 (rotated 90 degrees counter-clockwise)
			/// </param>
			/// <param name="flags">
			/// 0 for normal display, or combination of flags.
			/// With FPDF_ANNOT flag, this function will render popup annotation when users mouse-hover on non-widget annotation.
			/// Regardless of FPDF_ANNOT flag, this function will always render widget annotations for FormFields.
			[DllImport("pdfium.dll")]
			public static extern void FPDF_FFLDraw(IntPtr formHandle, IntPtr bitmapHandle, PageHandle page, int start_x, int start_y, int size_x, int size_y, RenderPageRotation rotate, PdfRenderFlags flags);

			/// <summary>
			/// Render contents of a page to a device (screen, bitmap, or printer).
			/// This function is only supported on Windows.
			/// </summary>
			/// <param name="dc">Handle to the device context.</param>
			/// <param name="page">Handle to the page. Returned by FPDF_LoadPage.</param>
			/// <param name="start_x">Left pixel position of the display area in device coordinates.</param>
			/// <param name="start_y">Top pixel position of the display area in device coordinates.</param>
			/// <param name="size_x">Horizontal size (in pixels) for displaying the page.</param>
			/// <param name="size_y">Vertical size (in pixels) for displaying the page.</param>
			/// <param name="rotate">
			/// Page orientation:
			///     0 (normal)
			///     1 (rotated 90 degrees clockwise)
			///     2 (rotated 180 degrees)
			///     3 (rotated 90 degrees counter-clockwise)
			/// </param>
			/// <param name="flags">0 for normal display, or combination of flags</param>
			[DllImport("pdfium.dll")]
			public static extern void FPDF_RenderPage(IntPtr dc, PageHandle page, int start_x, int start_y, int size_x, int size_y, RenderPageRotation rotate, PdfRenderFlags flags);

			/// <summary>
			/// Render contents of a page to a device independent bitmap.
			/// </summary>
			/// <param name="bitmapHandle"> Handle to the device independent bitmap (as the output buffer). The bitmap handle can be created by FPDFBitmap_Create or retrieved from an image object by FPDFImageObj_GetBitmap.</param>
			/// <param name="page">Handle to the page. Returned by FPDF_LoadPage</param>
			/// <param name="start_x">Left pixel position of the display area in bitmap coordinates.</param>
			/// <param name="start_y">Top pixel position of the display area in bitmap coordinates.</param>
			/// <param name="size_x">Horizontal size (in pixels) for displaying the page.</param>
			/// <param name="size_y">Vertical size (in pixels) for displaying the page.</param>
			/// <param name="rotate">
			/// Page orientation:
			///     0 (normal)
			///     1 (rotated 90 degrees clockwise)
			///     2 (rotated 180 degrees)
			///     3 (rotated 90 degrees counter-clockwise)
			/// </param>
			/// <param name="flags">
			/// 0 for normal display, or combination of flags.
			/// With the FPDF_ANNOT flag, it renders all annotations that do not require user-interaction, which are all annotations except widget and popup annotations.
			/// </param>
			[DllImport("pdfium.dll")]
			public static extern void FPDF_RenderPageBitmap(IntPtr bitmapHandle, PageHandle page, int start_x, int start_y, int size_x, int size_y, RenderPageRotation rotate, PdfRenderFlags flags);

			/// <summary>
			/// Import pages to a FPDF_DOCUMENT.
			/// </summary>
			/// <param name="dest_doc">The destination document for the pages.</param>
			/// <param name="src_doc">The document to be imported.</param>
			/// <param name="pagerange">
			/// A page range string, Such as "1,3,5-7". If |pagerange| is NULL,
			/// all pages from |src_doc| are imported.
			/// </param>
			/// <param name="index">THe page index to insert at.</param>
			/// <returns>Returns TRUE on success</returns>
			[DllImport("pdfium.dll", BestFitMapping = false, ThrowOnUnmappableChar = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool FPDF_ImportPages(DocumentHandle dest_doc, DocumentHandle src_doc, [MarshalAs(UnmanagedType.LPStr)] string pagerange, int index);

			/// <summary>
			/// Create a new PDF document.
			/// </summary>
			/// <returns>Returns a handle to a new document, or NULL on failure.</returns>
			[DllImport("pdfium.dll")]
			public static extern IntPtr FPDF_CreateNewDocument();

			/// <summary>
			/// Saves the copy of specified document in custom way.
			/// </summary>
			/// <param name="document">Handle to document. Returned by FPDF_LoadDocument and FPDF_CreateNewDocument.</param>
			/// <param name="pFileWrite">A pointer to a custom file write structure.</param>
			/// <param name="flags">The creating flags.</param>
			/// <returns>TRUE for succeed, FALSE for failed.</returns>
			[DllImport("pdfium.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool FPDF_SaveAsCopy(DocumentHandle document, FPDF_FILEWRITE pFileWrite, PdfSaveFlags flags);

			/// <summary>
			/// Prepare information about all characters in a page.
			/// </summary>
			/// <param name="page">Handle to a FPDF_PAGE object that specifies a PDF page.</param>
			/// <returns>
			/// Handle to a FPDF_TEXTPAGE object that receives the PDF text page object.
			/// If any error occurs, it will return NULL.
			/// </returns>
			[DllImport("pdfium.dll")]
			public static extern IntPtr FPDFText_LoadPage(PageHandle page);

			/// <summary>
			/// Release all resources allocated for text page information.
			/// </summary>
			/// <param name="textPage">Handle to a FPDF_TEXTPAGE object that specifies a valid PDF text page object.</param>
			[DllImport("pdfium.dll")]
			public static extern void FPDFText_ClosePage(IntPtr textPage);

			/// <summary>
			/// Get number of characters in a PDF page.
			/// </summary>
			/// <param name="textPage">Handle to a FPDF_TEXTPAGE object that specifies a valid PDF text page object.</param>
			/// <returns>
			/// Number of characters in the page. If any error occurs,
			/// it will return -1. Generated characters, like additional space characters, new line characters, are also counted.
			/// </returns>
			[DllImport("pdfium.dll")]
			public static extern int FPDFText_CountChars(TextPageHandle textPage);

			/// <summary>
			/// Extract unicode text string from the page, in UTF-16LE encoding.
			/// </summary>
			/// <param name="textPage">Handle to a FPDF_TEXTPAGE object that specifies a valid PDF text page object.</param>
			/// <param name="startIndex">Index of the start character.</param>
			/// <param name="count">Number of characters to be extracted.</param>
			/// <param name="result">
			/// Pointer to a string buffer (allocated by application) that receives the extracted text,
			/// in UTF-16LE encoding. The size of the buffer must be large enough to hold the number of characters plus a terminator.
			/// </param>
			/// <returns>Number of characters written into parameter result buffer, excluding the trailing terminator.</returns>
			[DllImport("pdfium.dll")]
			public static extern int FPDFText_GetText(TextPageHandle textPage, int startIndex, int count, [MarshalAs(UnmanagedType.LPArray)] byte[] result);

			/// <summary>
			/// Start a search.
			/// </summary>
			/// <param name="textPage">Handle to a FPDF_TEXTPAGE object that specifies a valid PDF text page object.</param>
			/// <param name="findWhat">A wide string, in UTF-16LE encoding, that specifies the match pattern.</param>
			/// <param name="flags">
			/// Matching option flags. 0 means no special finding options. And it can be one of the following macro definitions:
			/// * FPDF_MATCHCASE
			/// * FPDF_MATCHWHOLEWORD
			/// </param>
			/// <param name="startIndex">Index to specify from which character the search will start. -1 means from end of the page.</param>
			/// <returns>
			/// Handle to a FPDF_SCHHANDLE object that receives the search context.
			/// Function FPDFText_FindClose must be called to release this handle.
			/// </returns>
			[DllImport("pdfium.dll")]
			public static extern IntPtr FPDFText_FindStart(TextPageHandle textPage, [MarshalAs(UnmanagedType.LPWStr)] string pagerange, PdfSearchFlags flags, int startIndex);

			/// <summary>
			/// Search in the direction from page start to end.
			/// </summary>
			/// <param name="handle">Handle to a FPDF_SCHHANDLE object, returned by function FPDFText_FindStart.</param>
			/// <returns>Whether a match text is found. TRUE means a match found, while FALSE means no match is found.</returns>
			[DllImport("pdfium.dll")]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool FPDFText_FindNext(SearchHandle handle);

			/// <summary>
			/// Release a search context.
			/// </summary>
			/// <param name="handle">handle	Handle to a FPDF_SCHHANDLE object, returned by function FPDFText_FindStart.</param>
			[DllImport("pdfium.dll")]
			public static extern void FPDFText_FindClose(IntPtr handle);

			/// <summary>
			/// Get the starting character index of the search result.
			/// </summary>
			/// <param name="handle">Handle to a FPDF_SCHHANDLE object, returned by function FPDFText_FindStart</param>
			/// <returns>Index for the starting character in a match text.</returns>
			[DllImport("pdfium.dll")]
			public static extern int FPDFText_GetSchResultIndex(SearchHandle handle);

			/// <summary>
			/// Get total number of signatures in the document.
			/// </summary>
			/// <param name="document">Handle to document. Returned by FPDF_LoadDocument().</param>
			/// <returns>Total number of signatures in the document on success, -1 on error.</returns>
			[DllImport("pdfium.dll")]
			public static extern int FPDF_GetSignatureCount(DocumentHandle document);
		}
	}

	/// <summary>
	/// Structure for custom file write (see FPDF_FILEWRITE)
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	[SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable", Justification = "These IntPtrs aren't referencing things that need to be disposed")]
	class FPDF_FILEWRITE
	{
		/// <summary>
		/// Version number of the interface. Currently must be 1.
		/// </summary>
		public int version;

		/// <summary>
		/// Output a block of data in your custom way. (Function pointer to <see cref="FPDF_SaveBlockDelegate"/>)
		/// </summary>
		[SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
		[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
		public IntPtr WriteBlock;
	}

	// Structure for custom file access.
	[StructLayout(LayoutKind.Sequential)]
	[SuppressMessage("Microsoft.Design", "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable", Justification = "These IntPtrs aren't referencing things that need to be disposed")]
	class FPDF_FILEACCESS
	{
		/// <summary>
		/// File length, in bytes.
		/// </summary>
		public uint m_FileLen;

		/// <summary>
		/// Get a block of data in your custom way. (Function pointer to <see cref="FPDF_GetBlockDelegate"/>)
		/// </summary>
		[SuppressMessage("Microsoft.Security", "CA2111:PointersShouldNotBeVisible")]
		[SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
		public IntPtr GetBlock;

		/// <summary>
		/// A custom pointer for all implementation specific data.  This pointer will
		/// be used as the first parameter to the m_GetBlock callback.
		/// </summary>
		public IntPtr m_Param;
	}

	public enum PdfBitmapFlags
	{
		/// <summary>Unknown or unsupported format.</summary>
		FPDFBitmap_Unknown = 0,
		/// <summary>Gray scale bitmap, one byte per pixel.</summary>
		FPDFBitmap_Gray = 1,
		/// <summary>3 bytes per pixel, byte order: blue, green, red.</summary>
		FPDFBitmap_BGR = 2,
		/// <summary>4 bytes per pixel, byte order: blue, green, red, unused.</summary>
		FPDFBitmap_BGRx = 3,
		/// <summary>4 bytes per pixel, byte order: blue, green, red, alpha.</summary>
		FPDFBitmap_BGRA = 4
	}

	/// <summary>
	/// WTG: There is no documentation on what Incremental/NoIncremental means.
	/// 
	/// However Incremental will fail if you attempt to save an existing document with that flag. TestSave_SimpleCopy covers this case.
	/// </summary>
	[Flags]
	public enum PdfSaveFlags
	{
		Incrememtal = 1,
		NoIncremental = 2,
		RemoveSecurity = 3
	}

	[Flags]
	public enum PdfRenderFlags
	{
		NONE = 0,
		/// <summary> Set if annotations are to be rendered.</summary>
		ANNOT = 0x01,
		/// <summary> Set if using text rendering optimized for LCD display.</summary>
		LCD_TEXT = 0x02,
		/// <summary> Don't use the native text output available on some platforms</summary>
		NO_NATIVETEXT = 0x04,
		/// <summary> Grayscale output.</summary>
		GRAYSCALE = 0x08,
		/// <summary> Set if you want to get some debug info.</summary>
		DEBUG_INFO = 0x80,
		/// <summary> Set if you don't want to catch exceptions.</summary>
		NO_CATCH = 0x100,
		/// <summary> Limit image cache size.</summary>
		RENDER_LIMITEDIMAGECACHE = 0x200,
		/// <summary> Always use halftone for image stretching.</summary>
		RENDER_FORCEHALFTONE = 0x400,
		/// <summary> Render for printing.</summary>
		PRINTING = 0x800,
		/// <summary> Set to disable anti-aliasing on text.</summary>
		RENDER_NO_SMOOTHTEXT = 0x1000,
		/// <summary> Set to disable anti-aliasing on images.</summary>
		RENDER_NO_SMOOTHIMAGE = 0x2000,
		/// <summary> Set to disable anti-aliasing on paths.</summary>
		RENDER_NO_SMOOTHPATH = 0x4000,
		/// <summary> Set whether to render in a reverse Byte order, this flag is only used when rendering to a bitmap.</summary>
		REVERSE_BYTE_ORDER = 0x10,
	}

	[Flags]
	public enum PdfSearchFlags
	{
		NONE = 0x00,
		MATCHCASE = 0x01,
		MATCHWHOLEWORD = 0x02
	}

	[StructLayout(LayoutKind.Sequential)]
	public class FPDF_FORMFILLINFO
	{
		public int version = 1;
		readonly IntPtr Release;
		readonly IntPtr FFI_Invalidate;
		readonly IntPtr FFI_OutputSelectedRect;
		readonly IntPtr FFI_SetCursor;
		readonly IntPtr FFI_SetTimer;
		readonly IntPtr FFI_KillTimer;
		readonly IntPtr FFI_GetLocalTime;
		readonly IntPtr FFI_OnChange;
		readonly IntPtr FFI_GetPage;
		readonly IntPtr FFI_GetCurrentPage;
		readonly IntPtr FFI_GetRotation;
		readonly IntPtr FFI_ExecuteNamedAction;
		readonly IntPtr FFI_SetTextFieldFocus;
		readonly IntPtr FFI_DoURIAction;
		readonly IntPtr FFI_DoGoToAction;
		readonly IntPtr m_pJsPlatform;
	}

	/// <summary>
	/// Output a block of data in your custom way.
	/// </summary>
	/// <param name="fileWrite">Pointer to the structure itself (FPDF_FILEWRITE_*)</param>
	/// <param name="data">Pointer to a buffer to output</param>
	/// <param name="size">The size of the buffer.</param>
	/// <returns>Should be non-zero if successful, zero for error.</returns>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int FPDF_SaveBlockDelegate(IntPtr fileWrite, IntPtr data, uint size);

	/// <summary>
	/// A function pointer for getting a block of data from a specific position.
	/// Position is specified by byte offset from the beginning of the file.
	/// The position and size will never go out of range of the file length.
	/// It may be possible for FPDFSDK to call this function multiple times for
	/// the same position.
	/// </summary>
	/// <param name="param">The m_Param object from the FileAccess struct</param>
	/// <param name="position">The byte offset from the start of the file</param>
	/// <param name="pBuf">The buffer to place the data in</param>
	/// <param name="size">The buffer size</param>
	/// <returns>Should be non-zero if successful, zero for error.</returns>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	delegate int FPDF_GetBlockDelegate(IntPtr param, uint position, IntPtr pBuf, uint size);
}
