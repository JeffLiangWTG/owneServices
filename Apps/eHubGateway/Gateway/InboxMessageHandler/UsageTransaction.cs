using System;

namespace CargoWise.eHub.Gateway.V2_1
{
	[Serializable]
	public class UsageTransaction
	{
		public int UsageCount { get; set; }
		public DateTime ServiceOccuredUTC { get; set; }
		public string AdditionalRefs { get; set; }
		public string EnterpriseCode { get; set; }
		public string ServerCode { get; set; }
		public string Environment { get; set; }
		public string CompanyCode { get; set; }
		public string CompanyName { get; set; }
		public string BranchCode { get; set; }
		public string UsageCode { get; set; }

		public override string ToString()
		{
			return string.Format("Count: {0}, ServiceOccuredUTC: {1}, AdditionalRefs: {2}, EnterpriseCode: {3}, ServerCode: {4}, Environment: {5}, CompanyCode: {6}, CompanyName: {7}, BranchCode: {8}, UsageCode: {9}",
				UsageCount,
				ServiceOccuredUTC.ToString("s"),
				AdditionalRefs,
				EnterpriseCode,
				ServerCode,
				Environment,
				CompanyCode,
				CompanyName,
				BranchCode,
				UsageCode);
		}
	}
}
