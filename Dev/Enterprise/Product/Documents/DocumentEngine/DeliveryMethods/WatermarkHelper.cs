using System;
using System.Drawing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.DeliveryMethods
{
	public static class WatermarkHelper
	{
		public static TextWatermark GetNonCommercialUseWatermark()
		{
			return new TextWatermark(
				NonCommercialUseWatermarkText,
				Enterprise.RemotePrinting.Engine.Watermark.GetHorizontalAlignment(Enterprise.DocumentEngineCore.Registry.Watermark.HorizontalAlignmentCodes.Centre),
				Enterprise.RemotePrinting.Engine.Watermark.GetVerticalAlignment(Enterprise.DocumentEngineCore.Registry.Watermark.VerticalAlignmentCodes.Middle),
				0,
				0,
				45,
				Color.FromArgb(DocumentsDataRegistry.Instance.TestWatermarkOpacity.Value, 0, 0, 0), //60 by default
				(NoResString)"Arial",
				52,
				FontStyle.Bold);
		}

		public static string NonCommercialUseWatermarkText
		{
			get
			{
#if DEBUG
				++nonCommercialUseWatermarkCallCount;
#endif
				return Res.GetString("E21BDB71-489A-4862-8EFA-8759130B695F", "Training / Test \r\n Non Commercial Use only");
			}
		}

#if DEBUG
		[ThreadStatic]
		static int nonCommercialUseWatermarkCallCount;

		public static int NonCommercialUseWatermarkCallCount
		{
			get { return nonCommercialUseWatermarkCallCount; }
			set { nonCommercialUseWatermarkCallCount = value; }
		}
#endif
		}
}