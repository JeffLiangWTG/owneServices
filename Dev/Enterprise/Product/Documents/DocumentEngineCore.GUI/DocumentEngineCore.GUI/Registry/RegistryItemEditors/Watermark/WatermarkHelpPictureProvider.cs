using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;
using FlexCel.Render;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	class WatermarkHelpPictureProvider
	{
		const int MaximumWidth = 135;
		const int MaximumHeight = 188;
		const PixelFormat Format = PixelFormat.Format24bppRgb;
		const string PreviewResourceName = "Enterprise.DocumentEngineCore.GUI.Registry.RegistryItemEditors.Watermark.HelpPicture.xls";

		public Image GetPreview()
		{
			Image result = null;
			using (var excelInterface = new ExcelInterface())
			{
				using (var stream = GetType().Assembly.GetManifestResourceStream(PreviewResourceName))
				{
					excelInterface.LoadExcelFile(stream);
				}
				TranslateContent(excelInterface.WorkSheets[0]);
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
					graphics.TranslateTransform(-50, -55);
					graphics.FillRectangle(Brushes.White, 50, 55, bitmap.Width, bitmap.Height);

					using (var exporter = new FlexCelImgExport(excelInterface.Xls, true))
					{
						TImgExportInfo exportInfo = null;
						exporter.ExportNext(graphics, ref exportInfo);
					}
				}
				result = (Image)bitmap.Clone();
			}
			return result;
		}

		internal void TranslateContent(ExcelWorkSheet workSheet)
		{
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
					}
				}
			}
		}

		readonly Dictionary<string, MultilingualString> translationData = new Dictionary<string, MultilingualString>()
		{
			{ "Top/Left", ResString.GetMultilingualString("34BA9490-A244-4800-8941-B9F525A957BB", "Top/Left") },
			{ (NoResString)"Center", ResString.GetMultilingualString("4C622669-E2FC-4F83-BA53-7126322DF155", "Center") },
			{ (NoResString)"Right", ResString.GetMultilingualString("0351F594-B0D6-46E8-BB8C-E5263D238339", "Right") },
			{ (NoResString)"Middle", ResString.GetMultilingualString("6A3B4269-A648-403F-881D-EBF087D99392", "Middle") },
			{ (NoResString)"Bottom", ResString.GetMultilingualString("41E46FFF-82AC-4A96-A3E6-A0713700CB99", "Bottom") }
		};
	}
}
