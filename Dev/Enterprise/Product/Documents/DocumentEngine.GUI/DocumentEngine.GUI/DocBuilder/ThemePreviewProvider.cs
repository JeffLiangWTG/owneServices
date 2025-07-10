using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using Enterprise.DocumentEngine.DocBuilder.Styling;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Core;
using FlexCel.Render;

namespace Enterprise.DocumentEngine.GUI.DocBuilder
{
	public class ThemePreviewProvider : IThemePreviewProvider
	{
		const int MaximumWidth = 800;
		const int MaximumHeight = 500;
		const PixelFormat Format = PixelFormat.Format24bppRgb;
		const string PreviewResourceName = "Enterprise.DocumentEngine.GUI.DocBuilder.Preview.xls";

		public Image GetPreview(IDocBuilderTheme theme)
		{
			Image result = null;

			using (var excelInterface = new ExcelInterface())
			{
				using (var stream = GetType().Assembly.GetManifestResourceStream(PreviewResourceName))
				{
					excelInterface.LoadExcelFile(stream);
				}

				TranslateContent(excelInterface.WorkSheets[0]);

				var stylizer = new TemplateStylizer(excelInterface, theme as DocBuilderTheme);
				stylizer.Stylize();

				result = CreatePreview(excelInterface);
			}

			return result;
		}

		Image CreatePreview(ExcelInterface excelInterface)
		{
			Image result = null;

			using (var bitmap = new Bitmap(MaximumWidth, MaximumHeight, Format))
			{
				using (var graphics = Graphics.FromImage(bitmap))
				{
					graphics.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);

					using (var exporter = new FlexCelImgExport(excelInterface.Xls, true))
					{
						TImgExportInfo exportInfo = null;
						exporter.ExportNext(graphics, ref exportInfo);
					}
				}

				result = bitmap.Crop(183, 38, 612, 222);
			}

			return result;
		}

		internal void TranslateContent(ExcelWorkSheet workSheet)
		{
			int secondaryBodyIndex = 1;
			for (int i = 0; i < workSheet.RowCount; i++)
			{
				for (int j = 0; j < workSheet.ColumnCount; j++)
				{
					var key = workSheet[i, j].ToString();
					if (!string.IsNullOrEmpty(key))
					{
						if (translationData.ContainsKey(key))
						{
							workSheet[i, j] = translationData[key].ToString();
						}
						else if (key.StartsWith((NoResString)"Secondary Body", StringComparison.OrdinalIgnoreCase))
						{
							workSheet[i, j] = ResString.GetMultilingualString("C02703D8-BCC6-4023-9DB1-AA5F8D7876E1", "Secondary Body {0}", secondaryBodyIndex.ToString(CultureInfo.InvariantCulture));
							secondaryBodyIndex++;
						}
					}
				}
			}
		}

		readonly Dictionary<string, MultilingualString> translationData = new Dictionary<string, MultilingualString>()
		{
			{ (NoResString)"Document Heading", ResString.GetMultilingualString("36AACA72-113C-42E1-B0D6-F4535C5D4A39", "Document Heading") },
			{ (NoResString)"Primary Heading", ResString.GetMultilingualString("61A7471E-441C-485C-A94D-A58C57E803F1", "Primary Heading") },
			{ (NoResString)"Primary Body", ResString.GetMultilingualString("DA51A174-2774-4886-B729-B52D159ACBB4", "Primary Body") },
			{ (NoResString)"Secondary Heading", ResString.GetMultilingualString("8562394A-1163-4322-BE22-A3194C713AAA", "Secondary Heading") },
			{ (NoResString)"Amount", ResString.GetMultilingualString("46718FD4-760F-4219-B50A-8B31232B5453", "Amount") },
			{ (NoResString)"Account", ResString.GetMultilingualString("FACC9D71-1D72-46CF-BC66-29356F85986D", "Account") },
			{ (NoResString)"Page 1 of 1", ResString.GetMultilingualString("3C262CEF-8EF4-4CBF-AF8E-BBC71D09E9F4", "Page 1 of 1") },
			{ (NoResString)"Date", ResString.GetMultilingualString("8934534A-7320-4DB3-A922-6316D7670E47", "Date") }
		};
	}

	static partial class MethodExtensions
	{
		internal static Bitmap Crop(this Bitmap bitmap, int left, int top, int right, int bottom)
		{
			var rectangle = Rectangle.FromLTRB(left, top, right, bottom);
			return bitmap.Clone(rectangle, bitmap.PixelFormat);
		}
	}
}
