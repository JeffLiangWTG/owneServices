using System.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.AuditDataServices.HVLV.Subscribers
{
	public struct ItemUsageData
	{
		public ItemUsageData(ZGuid itemPK, string userCode)
		{
			ItemPK = itemPK;
			UserCode = userCode;
		}

		public ItemUsageData(DataRow itemRow, string userCode)
			: this(new ZGuid(itemRow[HVLVItemSchema.Constants.PK]), userCode)
		{
		}

		public ZGuid ItemPK { get; private set; }
		public string UserCode { get; private set; }
		public string UsageCategory { get; set; }
		public string UsageCode { get; set; }
		public ZString CompanyCode { get; set; }
		public ZString BranchCode { get; set; }
	}
}
