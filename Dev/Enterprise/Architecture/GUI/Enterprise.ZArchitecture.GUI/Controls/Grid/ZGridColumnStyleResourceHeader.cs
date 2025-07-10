using System.Drawing;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	class ZGridColumnStyleResourceHeader
	{
		readonly ZGridColumnStyle column;

		public ZGridColumnStyleResourceHeader(ZGridColumnStyle column)
		{
			this.column = column;
		}

		public string GetHeaderText(string baseHeaderText, bool wasDefaulted)
		{
			return GetHeaderText(baseHeaderText, wasDefaulted, false);
		}

		internal string GetHeaderText(string baseHeaderText, bool wasDefaulted, bool useCaptionAlways)
		{
			var result = baseHeaderText;

			if (!string.IsNullOrEmpty(baseHeaderText) && wasDefaulted)
			{
				var text = GetResourceStringHeader(useCaptionAlways);
				result = string.IsNullOrEmpty(text) ? result : text;
			}

			return string.IsNullOrEmpty(result) ? GetResourceStringHeader(useCaptionAlways) : result;
		}

		string GetResourceStringHeader(bool useCaptionAlways)
		{
			if (data == null && !isDataAbsent)
			{
				if (column.CaptionResourceString != null && !column.CaptionResourceString.IsEmpty())
				{
					data = column.CaptionResourceString;
				}
				else
				{
					var cache = GetLabelCache();
					var grid = (ZGrid)((IGridColumnStyle)column).Grid;
					data = new ResourceStringKeyCalculator(grid, column.MappingName).DataString;
					isDataAbsent = (data == null);
				}
			}
			return data != null
						? useCaptionAlways
							? data.Caption
							: BestToFit(data.GetCaptions(), column.ColumnWidthForHeader, column.HeaderFont)
						: string.Empty;
		}

		public void RefreshHeader()
		{
			data = null;
			isDataAbsent = false;
		}

		protected virtual ZLabelCaptionCache GetLabelCache()
		{
			return ZLabelCaptionCache.Instance;
		}

		protected virtual string BestToFit(string[] captions, int width, Font font)
		{
			return StringRenderingHelper.MeasureBestFit(captions, width, font, font.Height);
		}

		internal ResourceStringData Data
		{
			get
			{
				GetResourceStringHeader(true); // force data to be assigned
				return data;
			}
		}

		ResourceStringData data;
		bool isDataAbsent;
	}
}
