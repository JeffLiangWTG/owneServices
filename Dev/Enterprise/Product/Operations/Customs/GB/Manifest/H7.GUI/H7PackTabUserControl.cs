using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.H7.GUI;

namespace Enterprise.Customs.GB.H7.GUI
{
	public class H7PackTabUserControl : EUH7PackUserControl
	{
		protected override Dictionary<bool, string[]> PacksGridColumnAvailability()
		{
			return new Dictionary<bool, string[]>
			{
				{
					false,
					new[] {
						AsycudaPack.Schema.APA_VINNumber,
						AsycudaPack.Schema.LinePrice,
						AsycudaPack.Schema.LinePriceCurrency,
						AsycudaPack.Schema.APA_LineNo
					}
				}
			};
		}
	}
}
