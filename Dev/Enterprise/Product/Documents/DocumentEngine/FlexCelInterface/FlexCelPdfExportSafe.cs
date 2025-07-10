using System;
using System.Drawing;
using System.Runtime.InteropServices;
using CargoWise.Common;
using Enterprise.DocumentEngineCore.Registry;
using FlexCel.Core;
using FlexCel.Pdf;
using FlexCel.Render;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	public class FlexCelPdfExportSafe : FlexCelPdfExport
	{
		public FlexCelPdfExportSafe()
			: base()
		{
			Initialize(TPdfType.Standard);
		}

		public FlexCelPdfExportSafe(ExcelFile aWorkbook)
			: base(aWorkbook)
		{
			Initialize(TPdfType.Standard);
		}

#if DEBUG
		public FlexCelPdfExportSafe(bool ariaUnicodeFontInstalled)
			: base()
		{
			this.ariaUnicodeFontInstalled = ariaUnicodeFontInstalled;
			Initialize(TPdfType.Standard);
		}
#endif

		public FlexCelPdfExportSafe(ExcelFile aWorkbook, bool aAllowOverwritingFiles)
			: base(aWorkbook, aAllowOverwritingFiles)
		{
			Initialize(TPdfType.Standard);
		}

		public FlexCelPdfExportSafe(ExcelFile aWorkbook, bool aAllowOverwritingFiles, TPdfType pdfType)
			: base(aWorkbook, aAllowOverwritingFiles)
		{
			Initialize(pdfType);
		}

		public FlexCelPdfExportSafe(ExcelFile aWorkbook, bool aAllowOverwritingFiles, TPdfType pdfType, TPdfVersion pdfVersion)
			: base(aWorkbook, aAllowOverwritingFiles)
		{
			PdfVersion = pdfVersion;
			Initialize(pdfType);
		}

		public const TPdfVersion DefaultPdfVersion = TPdfVersion.v16;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a font name")]
		const string khmerFontName = "Leelawadee UI";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a font name")]
		const string arialUnicodeFontName = "Arial Unicode MS";

		bool? ariaUnicodeFontInstalled;
		bool AriaUnicodeFontInstalled
		{
			get
			{
				if (!ariaUnicodeFontInstalled.HasValue)
				{
					using (var font = new Font(arialUnicodeFontName, 10))
					{
						ariaUnicodeFontInstalled = string.Equals(font.Name.Trim(), arialUnicodeFontName, StringComparison.OrdinalIgnoreCase);
					}
				}

				return ariaUnicodeFontInstalled.Value;
			}
		}

		void Initialize(TPdfType pdfType)
		{
			this.FontEmbed = DocumentsDataRegistry.Instance.EmbedFontsInPDF.Value ? FlexCel.Pdf.TFontEmbed.Embed : FlexCel.Pdf.TFontEmbed.None;
			if (FallbackFonts.IndexOf(khmerFontName, StringComparison.OrdinalIgnoreCase) < 0)
			{
				this.FallbackFonts = FallbackFonts.TrimEnd(';') + ";" + khmerFontName;
			}

			if (!AriaUnicodeFontInstalled)
			{
				this.FallbackFonts = this.FallbackFonts.Replace(arialUnicodeFontName, "Microsoft YaHei;Microsoft JhengHei;Malgun Gothic;Nirmala UI");
			}

			this.UseExcelProperties = false;
			GetFontData += new FlexCel.Pdf.GetFontDataEventHandler(FlexCelPdfExportSafeHelper.OnGetFontData);

			if (pdfType != TPdfType.Standard)
			{
				this.UnlicensedFontAction = FlexCel.Pdf.TUnlicensedFontAction.Replace;
				this.PdfType = pdfType;
			}
			else
			{
				this.UnlicensedFontAction = FlexCel.Pdf.TUnlicensedFontAction.Ignore;
			}
		}

		static class FlexCelPdfExportSafeHelper
		{
			/// Make an unmanaged call to the Win32 API to return font information
			/// and avoid scanning the "fonts" folder.
			internal static void OnGetFontData(object sender, FlexCel.Pdf.GetFontDataEventArgs e)
			{
				//Actually make the WIN32 call.
				uint ttcf = 0x66637474; //return full true type collections.

				// Allocate a handle for the font
				IntPtr fontHandle = IntPtr.Zero;
				try
				{
					var flexcelInputFont = (FlexCel.Draw.TGdipUIFont)e.InputFont;
					var font = flexcelInputFont.Handle;
					fontHandle = font.ToHfont();
					using (Graphics gr = Graphics.FromHwnd(IntPtr.Zero))
					{
						IntPtr grHandle = gr.GetHdc();
						try
						{
							IntPtr objHandle = SelectObject(grHandle, fontHandle);
							try
							{
								//First find out the sizes
								uint size = GetFontData(grHandle, ttcf, 0, null, 0);
								if (unchecked((int)size) < 0) //error
								{
									ttcf = 0; //This might not be a true type collection, try again.
									size = GetFontData(grHandle, ttcf, 0, null, 0);

									if (unchecked((int)size) < 0) //nothing else to do, exit.
									{
										e.Applied = false;
										return;
									}
								}

								//Now get the font data.
								e.FontData = new byte[(int)size];
								uint result = GetFontData(grHandle, ttcf, 0, e.FontData, size);

								if (unchecked((int)result) < 0)
								{
									e.Applied = false;
									return;
								}
								e.Applied = true;
							}
							finally
							{
								DeleteObject(objHandle);
							}
						}
						finally
						{
							gr.ReleaseHdc(grHandle);
						}
					}
				}
				catch (Exception exception)
				{
					if (exception.IsCriticalException())
					{
						throw;
					}

					e.Applied = false;
				}
				finally
				{
					try
					{
						if (fontHandle != IntPtr.Zero)
						{
							DeleteObject(fontHandle);
						}
					}
					catch (Exception exception)
					{
						if (exception.IsCriticalException())
						{
							throw;
						}
					}
				}
			}

			/// <summary>
			/// The Win32 call.
			/// </summary>
			/// <param name="hdc"></param>
			/// <param name="dwTable"></param>
			/// <param name="dwOffset"></param>
			/// <param name="lpvBuffer"></param>
			/// <param name="cbData"></param>
			/// <returns></returns>
			[DllImport("gdi32.dll")]
			static extern uint GetFontData(IntPtr hdc, uint dwTable, uint dwOffset,
				[In, Out] byte[] lpvBuffer, uint cbData);

			[DllImport("GDI32.dll")]
			static extern bool DeleteObject(IntPtr objectHandle);

			[DllImport("gdi32.dll")]
			static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
		}
	}
}
