using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.DocumentEngine.GUI.JSInterop;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;
using FlexCel.Render;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework;
using Watermark = Enterprise.RemotePrinting.Engine.Watermark;

namespace Enterprise.DocumentEngine.GUI
{
	[SuppressMessage("CargoWiseOne", "CW1068:Do Not Use Math.Round", Justification = "This is not used in CW1 but in Winzor.")]
	[SuppressMessage("CargoWiseOne", "CW1118:Use Keys.KeyCode and Keys.Modifiers Bitmask", Justification = "This is not used in CW1 but in Winzor.")]
	public partial class FlexCelPreview : UserControl
	{
		public FlexCelPreview()
		{
			Name = "FlexCelPreview";
			AutoScroll = true;
			PageXSeparation = 10;
			PageYSeparation = 10;
			BackColor = Color.Gray;
			PageShadowSize = 3;
			pageBorderPen = new Pen(Color.Black);
			pageShadowBrush = new SolidBrush(Color.Black);
			pageNumberBackBrush = new SolidBrush(Color.Gray);
			pageNumberBackPen = new Pen(Color.Gray);
			pageNumberSelectedBackBrush = new SolidBrush(Color.Navy);
			pageNumberSelectedBackPen = new Pen(Color.Navy);
			pageNumberTextBrush = new SolidBrush(Color.White);
			pageNumberSelectedTextBrush = new SolidBrush(Color.White);
			ShowThumbsPageNumber = true;

			zoom = 1;
			cacheSize = 64;
			PageInfo = new TPageInfoList();
			NumberSep = DefaultFont.Height;

			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			UpdateStyles();

			dotNetReference = DotNetObjectReference.Create(this);
		}

		#region Blazor

		readonly DotNetObjectReference<FlexCelPreview> dotNetReference;

		[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Analyzer runner incorrectly detecting")]
		ElementReference previewReference;

		public override bool UseParentDivForLayout => false;

		public List<Page> Pages
		{
			get
			{
				if (pages == null)
				{
					pages = GenerateSVGPages();
				}
				return pages;
			}
		}
		List<Page> pages;

		const int BufferSize = 65536; //default buffer size is set to 64kb, one page of encoded svg is typically < 60kb;
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "The string literal is safe to use in this context and does not need to be externalized.")]
		const string Delimiter = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>";

		List<Page> GenerateSVGPages()
		{
			var pageInfoList = new List<Page>();

			if (Document != null && Document.Workbook != null)
			{
				var svgPagesContent = GetSVGPagesContent();

				var svgPages = svgPagesContent.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries);
				pageInfoList = svgPages
					.Select((page, index) => new Page(page, index + 1))
					.ToList();

				var match = Regex.Match(svgPages.FirstOrDefault(string.Empty), @"<svg.*?width=""(.*?)pt"".*?height=""(.*?)pt""");
				if (match.Success)
				{
					svgPageWidth = double.TryParse(match.Groups[1].Value, out var width) ? width : 0;
					svgPageHeight = double.TryParse(match.Groups[2].Value, out var height) ? height : 0;
				}
			}

			return pageInfoList;
		}

		string GetSVGPagesContent()
		{
			var stream = new MemoryStream(BufferSize);
			try
			{
				Document.SaveAsImage(arg =>
				{
					arg.FileStream = stream;
				});
			}
			catch (Exception exception)
			{
				var errorMessage = Res.GetString("413D4A69-C6A8-4F7C-A33C-4D633F0BFFFF", @"An error has occured while previewing the document -
Error message is: {0}", exception.Message);
				Globals.Message.ShowError(errorMessage, Res.GetString("FF3D4A69-C6A8-4F7C-A33C-4D633F0BFFFF", "Error previewing document"));
			}

			stream.Position = 0;
			var reader = new StreamReader(stream);
			var svgPagesContent = reader.ReadToEnd();
			reader.Close();
			stream.Close();
			return svgPagesContent;
		}

		[JSInvokable]
		public async Task InvokeOnMouseUpAsync(WebMouseEventArgs e) => await OnMouseUpAsync(e);

		[JSInvokable]
		public async Task UpdateStartPageAfterScrollAsync(int pageNo)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				if (SetOnlyStartPage(pageNo))
				{
					UpdateStartPage();
				}
			});
		}

		protected override async Task<Task> OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
			{
				if (ThumbnailLarge == null)
				{
					try
					{
						// Initialize handlers and observers
						await (GetJSInterop<IFlexCelPreviewJSInterop>()?.RegisterMainKeyEventHandlerAsync(previewReference) ?? Task.CompletedTask);
						await (GetJSInterop<IFlexCelPreviewJSInterop>()?.InitializeMainIntersectionObserverAsync(dotNetReference, previewReference) ?? Task.CompletedTask);
					}
					catch (Exception ex)
					{
						ErrorReporter.ReportOnce(
							Res.GetString("B486A8C8-9556-4D37-85D7-DD3262B80B6F", "Error initializing document preview"),
							ex.Message,
							ex);
					}
				}
			}
			return base.OnAfterRenderAsync(firstRender);
		}

		#endregion

		#region Utilities

		public double ScreenResolution => screenResolution is 0 ? 96 : screenResolution;
		readonly double screenResolution;

		public double ScreenScale => ScreenResolution / 96.0;

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in the razor file in DocumentEngine.GUI.Winzor")]
		int PageXSeparationRes => (int)Math.Round(pageXSeparation * ScreenScale, 0);

		int PageYSeparationRes => (int)Math.Round(pageYSeparation * ScreenScale, 0);

		double PageWidth => svgPageHeight * ScreenResolution / 72.0;

		double PageHeight => svgPageHeight * ScreenResolution / 72.0;

#if DEBUG
		public
#endif
		double svgPageHeight;

#if DEBUG
		public
#endif
		double svgPageWidth;

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in the razor file in DocumentEngine.GUI.Winzor")]
		int LastPageHeight => (int)Math.Round(PageHeight * Zoom, 0);

		int GetVisiblePages()
		{
			// not 100% fool proof, but it does not need to be exact anyway.
			if (PageHeight <= 0)
			{
				return 1;
			}
			var visiblePageCount = ClientSize.Height / (PageHeight * Zoom + RealYSep);
			return visiblePageCount < 1 ? 1 : (int)visiblePageCount;
		}

		public int RealYSep
		{
			get
			{
				if (thumbnailLarge != null && ShowThumbsPageNumber)
				{
					return PageYSeparationRes + (NumberSep * 2);
				}

				return PageYSeparationRes;
			}
		}

		bool SetOnlyStartPage(int value)
		{
			if (value < 1)
			{
				value = 1;
			}

			if (value > TotalPages)
			{
				value = TotalPages;
			}

			return UpdateProperty(ref startPage, value);
		}

		#endregion

		#region Events

		public event EventHandler StartPageChanged;

		protected virtual void OnStartPageChanged(EventArgs e)
		{
			StartPageChanged?.Invoke(this, e);
		}

		public event EventHandler ZoomChanged;

		protected virtual void OnZoomChanged(EventArgs e)
		{
			ZoomChanged?.Invoke(this, e);
			NotifyRenderRequired();
		}

		#endregion

		#region Properties

		TImgExportInfo FirstPageExportInfo { get; set; }

		TPageInfoList PageInfo { get; set; }

		int NumberSep { get; set; }

		public FlexCelSVGExport Document { get; set; }

		public int PageXSeparation
		{
			get => pageXSeparation;
			set
			{
				if (value > 0)
				{
					pageXSeparation = value;
				}
			}
		}
		int pageXSeparation;

		public bool CenteredPreview
		{
			get => centeredPreview;
			set
			{
				centeredPreview = value;
				Invalidate();
			}
		}
		bool centeredPreview;

		public int PageYSeparation
		{
			get => pageYSeparation;
			set
			{
				if (value > 0)
				{
					pageYSeparation = value;
				}
			}
		}
		int pageYSeparation;

		public bool EndPreviewAtLastPage
		{
			get => endPreviewAtLastPage;
			set
			{
				endPreviewAtLastPage = value;
				ResizeCanvas();
			}
		}
		bool endPreviewAtLastPage;

		public TAutofitPreview AutofitPreview
		{
			get => autofitPreview;
			set
			{
				autofitPreview = value;
				if (value != TAutofitPreview.None)
				{
					ResizeCanvas();
				}
			}
		}
		TAutofitPreview autofitPreview;

		public int StartPage
		{
			get => startPage;
			set
			{
				if (SetOnlyStartPage(value))
				{
					UpdateStartPage();
					_ = ScrollIntoViewAsync(value);
				}
				else if (Pages.Count == 1)
				{
					_ = ScrollSinglePageIntoViewAsync(value < 2);
				}
			}
		}
		int startPage = 1;

		public int TotalPages => Pages.Count;

		public double Zoom
		{
			get => zoom;
			set => SetZoom(value, false);
		}
		double zoom;

		void SetZoom(double value, bool autosizing)
		{
			if (value < 0.1)
			{
				value = 0.1;
			}

			if (value > 4)
			{
				value = 4;
			}

			if (value != zoom)
			{
				if (!autosizing)
				{
					AutofitPreview = TAutofitPreview.None;
				}

				var ox = AutoScrollPosition.X / zoom;
				var oy = AutoScrollPosition.Y / zoom;
				zoom = value;
				if (!autosizing)
				{
					ResizeCanvas(new Point(-(int)Math.Round(ox * zoom), -(int)Math.Round(oy * zoom)), AutofitPreview);
				}

				PageInfo.ClearBitmaps();
				Invalidate();
				OnZoomChanged(new EventArgs());
			}
		}

		public int CacheSize
		{
			get => cacheSize;
			set
			{
				if (cacheSize < 0)
				{
					return;
				}

				cacheSize = value;
			}
		}
		int cacheSize;

		public bool ShowThumbsPageNumber { get; set; }

		public FlexCelPreview ThumbnailSmall
		{
			get => thumbnailSmall;
			set
			{
				if (value == this)
				{
					return;
				}

				thumbnailSmall = value;
				if (thumbnailSmall != null)
				{
					thumbnailLarge = null;
					thumbnailSmall.thumbnailLarge = this;
					thumbnailSmall.thumbnailSmall = null;
				}
			}
		}
		FlexCelPreview thumbnailSmall;

		public FlexCelPreview ThumbnailLarge
		{
			get => thumbnailLarge;
			set
			{
				if (value == this)
				{
					return;
				}

				thumbnailLarge = value;
				if (thumbnailLarge != null)
				{
					Zoom = 0.10;
					thumbnailSmall = null;
					thumbnailLarge.thumbnailSmall = this;
					thumbnailLarge.thumbnailLarge = null;
				}
			}
		}
		FlexCelPreview thumbnailLarge;

		public SmoothingMode SmoothingMode { get; set; }

		public InterpolationMode InterpolationMode { get; set; }

		public double PageShadowSize { get; set; }

		public Color PageShadowColor
		{
			get
			{
				var sb = pageShadowBrush as SolidBrush;
				return sb?.Color ?? Color.Empty;
			}
			set
			{
				pageShadowBrush?.Dispose();
				pageShadowBrush = value.IsEmpty ? null : new SolidBrush(value);
			}
		}
		Brush pageShadowBrush;

		public Color PageBorderColor
		{
			get => pageBorderPen?.Color ?? Color.Empty;
			set
			{
				if (value.IsEmpty)
				{
					pageBorderPen?.Dispose();
					pageBorderPen = null;
				}
				else
				{
					pageBorderPen ??= new Pen(value);
					pageBorderPen.Color = value;
				}
			}
		}

		public double PageBorderWidth
		{
			get => pageBorderPen?.Width ?? 0;
			set
			{
				pageBorderPen ??= new Pen(Color.Black);
				pageBorderPen.Width = (float)value;
			}
		}
		Pen pageBorderPen;

		public DashStyle PageBorderStyle
		{
			get => pageBorderPen?.DashStyle ?? DashStyle.Solid;
			set
			{
				pageBorderPen ??= new Pen(Color.Black);
				pageBorderPen.DashStyle = value;
			}
		}

		public Color PageNumberBgColor
		{
			get => pageNumberBackPen?.Color ?? Color.Empty;
			set
			{
				if (value.IsEmpty)
				{
					pageNumberBackPen?.Dispose();
					pageNumberBackPen = null;
				}
				else
				{
					pageNumberBackPen ??= new Pen(value);
					pageNumberBackPen.Color = value;
				}

				pageNumberBackBrush?.Dispose();
				pageNumberBackBrush = value.IsEmpty ? null : new SolidBrush(value);
			}
		}
		Pen pageNumberBackPen;
		Brush pageNumberBackBrush;

		public Color PageNumberSelectedBgColor
		{
			get => pageNumberSelectedBackPen?.Color ?? Color.Empty;
			set
			{
				if (value.IsEmpty)
				{
					pageNumberSelectedBackPen?.Dispose();
					pageNumberSelectedBackPen = null;
				}
				else
				{
					pageNumberSelectedBackPen ??= new Pen(value);
					pageNumberSelectedBackPen.Color = value;
				}

				pageNumberSelectedBackBrush?.Dispose();
				pageNumberSelectedBackBrush = value.IsEmpty ? null : new SolidBrush(value);
			}
		}
		Pen pageNumberSelectedBackPen;
		Brush pageNumberSelectedBackBrush;

		public Color PageNumberTextColor
		{
			get
			{
				var sb = pageNumberTextBrush as SolidBrush;
				return sb?.Color ?? Color.Empty;
			}
			set
			{
				pageNumberTextBrush?.Dispose();
				pageNumberTextBrush = value.IsEmpty ? null : new SolidBrush(value);
			}
		}
		Brush pageNumberTextBrush;

		public Color PageNumberSelectedTextColor
		{
			get
			{
				var sb = pageNumberSelectedTextBrush as SolidBrush;
				return sb?.Color ?? Color.Empty;
			}
			set
			{
				pageNumberSelectedTextBrush?.Dispose();
				pageNumberSelectedTextBrush = value.IsEmpty ? null : new SolidBrush(value);
			}
		}
		Brush pageNumberSelectedTextBrush;

		#endregion

		#region Public

		public void InvalidatePreview()
		{
			if (thumbnailLarge != null)
			{
				throw new InvalidOperationException("InvalidatePreview should be called on the main display component. This will also invalidate the Thumbnails");
			}

			ReloadDocument();

			thumbnailSmall?.ReloadDocument();
			thumbnailSmall?.Invalidate();

			Invalidate();
		}

		public TUISize MaxPageSize
		{
			get
			{
				if (FirstPageExportInfo == null)
				{
					return new TUISize(0, 0);
				}

				var resultH = 0d;
				var resultW = 0d;
				var sheetCount = FirstPageExportInfo.SheetCount;
				for (var i = 1; i <= sheetCount; i++)
				{
					var sheetInfo = FirstPageExportInfo.Sheet(i);
					if (sheetInfo == null)
					{
						continue;
					}

					var w = PageWidth;
					if (w > resultW)
					{
						resultW = w;
					}

					var h = PageHeight;
					if (h > resultH)
					{
						resultH = h;
					}
				}
				return new TUISize(resultW, resultH);
			}
		}

		public void AutofitPreviewOnce(TAutofitPreview value) => ResizeCanvas(new Point(-AutoScrollPosition.X, -AutoScrollPosition.Y), value);

		#endregion

		#region Implementation

		async Task ScrollIntoViewAsync(int pageNo)
		{
			if (pageNo <= Pages.Count)
			{
				if (thumbnailLarge != null)
				{
					await (GetJSInterop<IFlexCelPreviewJSInterop>()?.ScrollThumbPageIntoViewAsync(previewReference, pageNo, PageYSeparationRes, RealYSep) ?? Task.CompletedTask);
				}
				else
				{
					await (GetJSInterop<IFlexCelPreviewJSInterop>()?.ScrollMainPageIntoViewAsync(previewReference, pageNo) ?? Task.CompletedTask);
				}
			}
		}

		async Task ScrollSinglePageIntoViewAsync(bool scrollToTop)
		{
			if (Pages.Count == 1)
			{
				await (GetJSInterop<IFlexCelPreviewJSInterop>()?.ScrollSinglePageIntoViewAsync(previewReference, scrollToTop) ?? Task.CompletedTask);
			}
		}

		internal int ThumbnailPos
		{
			get => thumbnailPos;
			set
			{
				if (value > TotalPages)
				{
					value = TotalPages;
				}

				if (value < 1)
				{
					value = 1;
				}

				if (UpdateProperty(ref thumbnailPos, value))
				{
					startPage = value;
					_ = ScrollIntoViewAsync(value);
				}
			}
		}
		int thumbnailPos = 1;

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in the razor file in DocumentEngine.GUI.Winzor")]
		async Task OnWheelAsync(ThrottledWheelEventArgs e)
		{
			if (ThumbnailLarge != null)
			{
				return;
			}

			if (e.CtrlKey)
			{
				await InvokeWinzorDispatcherAsync(() => Zoom -= e.DeltaY / 120.0 / 10.0);
			}
		}

		void UpdateMainView()
		{
			if (thumbnailLarge?.SetOnlyStartPage(ThumbnailPos) ?? false)
			{
				_ = thumbnailLarge.ScrollIntoViewAsync(ThumbnailPos);
			}
		}

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in the razor file in DocumentEngine.GUI.Winzor")]
		async Task OnPageMouseDownAsync(WebMouseEventArgs e, int pageNo)
		{
			await OnMouseDownAsync(e);
			if (thumbnailLarge != null)
			{
				await InvokeWinzorDispatcherAsync(() =>
				{
					thumbnailLarge.StartPage = pageNo;
				});
			}
			else if (thumbnailSmall != null && pageNo <= Pages.Count)
			{
				await (GetJSInterop<IFlexCelPreviewJSInterop>()?.StartDragScrollAsync(dotNetReference, previewReference, pageNo) ?? Task.CompletedTask);
			}
		}

		bool HandleThumbKey(Keys e)
		{
			switch (e)
			{
				case Keys.Down:
				case Keys.Right:
					ThumbnailPos++;
					UpdateMainView();
					return true;

				case Keys.Up:
				case Keys.Left:
					ThumbnailPos--;
					UpdateMainView();
					return true;

				case Keys.Down | Keys.Control:
				case Keys.PageDown:
					ThumbnailPos += GetVisiblePages();
					UpdateMainView();
					return true;

				case Keys.Up | Keys.Control:
				case Keys.PageUp:
					ThumbnailPos -= GetVisiblePages();
					UpdateMainView();
					return true;

				case Keys.PageUp | Keys.Control:
				case Keys.Home | Keys.Control:
				case Keys.Home:
					ThumbnailPos = 1;
					UpdateMainView();
					return true;

				case Keys.PageDown | Keys.Control:
				case Keys.End | Keys.Control:
				case Keys.End:
					ThumbnailPos = TotalPages;
					UpdateMainView();
					return true;
			}
			return false;
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (thumbnailLarge != null)
			{
				if (HandleThumbKey(keyData))
				{
					return true;
				}
			}

			return base.ProcessCmdKey(ref msg, keyData);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			NumberSep = DefaultFont.Height;
		}

		protected override void OnResize(EventArgs e)
		{
			if (thumbnailLarge == null)
			{
				ResizeCanvas();
			}

			if (CenteredPreview)
			{
				Invalidate();
			}

			base.OnResize(e);
		}

		void ReloadDocument()
		{
			if (Document == null)
			{
				return;
			}

			pages = null;
			startPage = 1;

			ResizeCanvas(new Point(0, 0), AutofitPreview);
			OnZoomChanged(new EventArgs());
		}

		void ResizeCanvas() => ResizeCanvas(new Point(-AutoScrollPosition.X, -AutoScrollPosition.Y), AutofitPreview);

		void ResizeCanvas(Point newPosition, TAutofitPreview tmpAutofitPreview) => UpdateStartPage();

		[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used in the razor file in DocumentEngine.GUI.Winzor")]
		int CalcEndWhitespace()
		{
			if (thumbnailLarge != null || EndPreviewAtLastPage)
			{
				return 0;
			}

			var result = ClientSize.Height - LastPageHeight;
			return result < 0 ? 0 : result;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			UpdateStartPage();
		}

		void UpdateThumbs()
		{
			if (ThumbnailSmall != null)
			{
				ThumbnailSmall.ThumbnailPos = StartPage;
			}
		}

		void UpdateStartPage()
		{
			UpdateThumbs();
			OnStartPageChanged(new EventArgs());
		}

		#endregion

		#region Utility Classes

		class TPageInfoList : List<TPreviewPageInfo>, IDisposable
		{
			TPreviewPageInfo CacheFirst;
			TPreviewPageInfo CacheLast;
			int CacheCount;

			public TPageInfoList()
			{
			}

			public new void Clear()
			{
				Dispose();
				base.Clear();
				CacheCount = 0;
				CacheFirst = null;
				CacheLast = null;
			}

			bool GetNextImage(ref TImgExportInfo exportInfo, Bitmap bmp, FlexCelImgExport document, TImageAtts imageAtts)
			{
				if (bmp == null)
				{
					return document.ExportNext(null, ref exportInfo);
				}

				using (var gr = TUIGraphics.FromImage(bmp))
				{
					gr.Clear(Colors.White);
					gr.SetMode(imageAtts.InterpolationMode, imageAtts.SmoothingMode);
					return document.ExportNext(gr, ref exportInfo);
				}
			}

			internal Bitmap GetImage(int index, TImgExportInfo firstPageExportInfo, FlexCelImgExport document, TImageAtts imageAtts, int cacheSize, double screenResolution)
			{
				if (Count <= index)
				{
					TImgExportInfo ei;
					if (Count > 0)
					{
						ei = TImgExportInfo.Clone(this[Count - 1].ExportInfo);
					}
					else
					{
						ei = TImgExportInfo.Clone(firstPageExportInfo);
					}

					for (var i = Count; i <= index; i++)
					{
						GetNextImage(ref ei, null, document, imageAtts);
						Add(new TPreviewPageInfo(TImgExportInfo.Clone(ei), null));
					}
				}

				var pi = this[index];
				if (pi.Bmp == null)
				{
					var page = pi.ExportInfo.ActiveSheet.PageBounds;
					//pi.Bmp = GdipBitmapConstructor.CreateBitmap((int)(page.Width / 100.0 * screenResolution * imageAtts.Zoom), (int)(page.Height / 100.0 * screenResolution * imageAtts.Zoom));
					pi.Bmp.SetResolution((float)(screenResolution * imageAtts.Zoom), (float)(screenResolution * imageAtts.Zoom));
					TImgExportInfo exportInfo = null;
					if (index > 0)
					{
						exportInfo = TImgExportInfo.Clone(this[index - 1].ExportInfo);
					}
					else
					{
						exportInfo = TImgExportInfo.Clone(firstPageExportInfo);
					}

					GetNextImage(ref exportInfo, pi.Bmp, document, imageAtts);
					CacheCount++;
				}

				// Bring the item to last position on the cache.
				if (pi.Next != null)
				{
					pi.Next.Prev = pi.Prev;
					if (pi.Prev != null)
					{
						pi.Prev.Next = pi.Next;
					}
					else
					{
						CacheFirst = pi.Next;
					}
				}

				CacheFirst ??= pi;

				if (CacheLast != pi)
				{
					pi.Prev = CacheLast;
					if (CacheLast != null)
					{
						CacheLast.Next = pi;
					}

					CacheLast = pi;
					pi.Next = null;
				}

				// If we have more bitmaps on the cache that what is allowed, delete the one at the first position.
				if (CacheFirst != null && CacheCount > 1 + (cacheSize / (imageAtts.Zoom * imageAtts.Zoom)))
				{
					if (CacheFirst.Next != null)
					{
						CacheFirst.Bmp.Dispose();
						CacheFirst.Bmp = null;
						CacheCount--;

						var next = CacheFirst.Next;
						next.Prev = null;
						CacheFirst.Next = null;
						CacheFirst = next;
					}
				}

				Debug.Assert(CacheFirst == null || CacheFirst.Prev == null, (NoResString)"Error in cache");
				Debug.Assert(CacheLast == null || CacheLast.Next == null, (NoResString)"Error in cache");
				return pi.Bmp;
			}

			internal void ClearBitmaps()
			{
				var pi = CacheFirst;
				while (pi != null)
				{
					pi.Bmp.Dispose();
					pi.Bmp = null;
					pi.Prev = null;
					var tmpPi = pi;
					pi = tmpPi.Next;
					tmpPi.Next = null;
				}
				CacheCount = 0;
				CacheFirst = null;
				CacheLast = null;
			}

			public void Dispose()
			{
				for (var i = Count - 1; i >= 0; i--)
				{
					this[i].Dispose();
				}
				GC.SuppressFinalize(this);
			}
		}

		class TPreviewPageInfo : IDisposable
		{
			internal Bitmap Bmp;
			internal TImgExportInfo ExportInfo;

			internal TPreviewPageInfo Prev;
			internal TPreviewPageInfo Next;

			internal TPreviewPageInfo(TImgExportInfo aExportInfo, Bitmap aBmp)
			{
				Bmp = aBmp;
				ExportInfo = aExportInfo;
				Prev = null;
				Next = null;
			}

			public void Dispose()
			{
				Bmp?.Dispose();
				GC.SuppressFinalize(this);
			}
		}

		struct TImageAtts
		{
			public double Zoom;
			public InterpolationMode InterpolationMode;
			public SmoothingMode SmoothingMode;
		}

		public enum TAutofitPreview
		{
			None,
			Width,
			Height,
			Full
		}

		public Watermark WaterMark { get; set; }

		public record Page(string PageImg, int PageNo);
		#endregion
	}
}
