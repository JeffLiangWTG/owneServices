using Enterprise.Customs.EU.Registry;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsDefaultConsignorAndConsigneeRegistryManager
	{
		bool IsRegistryEnabled();

		bool IsLeaveBlank();

		bool IsConsignor();

		bool IsConsignee();
	}

	public class NctsDefaultConsignorConsigneeRegistryManager : INctsDefaultConsignorAndConsigneeRegistryManager
	{
		public NctsDefaultConsignorConsigneeRegistryManager()
		{
		}

		NctsDefaultConsignorConsignee NctsDefaultConsignorConsignee => EUCustomsDataRegistry.Instance.NctsDefaultConsignorConsignee.Value;

		public bool IsRegistryEnabled()
		{
			return NctsDefaultConsignorConsignee.LeaveBlank || NctsDefaultConsignorConsignee.ValueFrom;
		}

		public bool IsLeaveBlank()
		{
			return NctsDefaultConsignorConsignee.LeaveBlank;
		}

		public bool IsConsignor()
		{
			return NctsDefaultConsignorConsignee.ValueFrom && NctsDefaultConsignorConsignee.Consignor;
		}

		public bool IsConsignee()
		{
			return NctsDefaultConsignorConsignee.ValueFrom && NctsDefaultConsignorConsignee.Consignee;
		}
	}
}
