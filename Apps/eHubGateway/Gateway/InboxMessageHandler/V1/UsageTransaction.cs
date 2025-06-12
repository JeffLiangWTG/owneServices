using System;

namespace CargoWise.eHub.Gateway.V1
{
	[Serializable]
	public class UsageTransaction
	{
		public int Count { get; set; }
		public DateTime ServiceOccuredUTC { get; set; }
		public string AdditionalRefs { get; set; }


		public override string ToString()
		{
			return string.Format("Count: {0}, ServiceOccuredUTC: {1}, AdditionalRefs: {2}", Count, ServiceOccuredUTC.ToString("s"), AdditionalRefs);
		}
	}
}
