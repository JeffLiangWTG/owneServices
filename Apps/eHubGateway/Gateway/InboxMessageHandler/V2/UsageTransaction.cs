using System;

namespace CargoWise.eHub.Gateway.V2
{
	[Serializable]
	public class UsageTransaction
	{
		public int UsageCount { get; set; }
		public DateTime ServiceOccuredUTC { get; set; }
		public string AdditionalRefs { get; set; }


		public override string ToString()
		{
			return string.Format("UsageCount: {0}, ServiceOccuredUTC: {1}, AdditionalRefs: {2}", UsageCount, ServiceOccuredUTC.ToString("s"), AdditionalRefs);
		}
	}
}
