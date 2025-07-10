using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.EU.H7.GUI;

namespace Enterprise.Customs.FR.H7.GUI
{
	public class FRH7PackTabUserControl : EUH7PackUserControl
	{
		protected override Dictionary<bool, string[]> PacksGridColumnAvailability()
		{
			return new Dictionary<bool, string[]>
			{
				{
					false,
					[AsycudaPack.Schema.APA_VINNumber]
				}
			};
		}
	}
}
