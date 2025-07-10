using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.H7.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class ESH7PackTabUserControl : EUH7PackUserControl
	{
		protected override Dictionary<bool, string[]> PacksGridColumnAvailability()
		{
			return new Dictionary<bool, string[]>
			{
				{
					false,
					new[] {
						AsycudaPack.Schema.APA_CommodityCode,
						AsycudaPack.Schema.APA_LineNo,
						AsycudaPack.Schema.APA_VINNumber,
						AsycudaPack.Schema.LinePrice,
						AsycudaPack.Schema.LinePriceCurrency,
					}
				}
			};
		}
	}
}
