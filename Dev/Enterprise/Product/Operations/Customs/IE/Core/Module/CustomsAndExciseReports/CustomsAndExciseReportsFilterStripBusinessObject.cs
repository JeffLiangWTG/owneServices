using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Module
{
	public class CustomsAndExciseReportsFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string MessageNumber = "Message Number";
			public const string ReportType = "Report Type";
			public const string Status = "Status";
		}

		public override ZQuery Filter
		{
			get
			{
				var filter = base.Filter;

				filter
					.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.IECustomsAndExcise)
					.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);

				return filter;
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var reportTypeFilter = result.AddTextFilter(Schema.ReportType, EDIMessageSchema.EM_MessageType);
			reportTypeFilter.MultilingualDescription = ResString.GetMultilingualString("DA1976C9-D060-4CC7-8CF2-10E20B1925EF", Schema.ReportType);

			var messageNumberFilter = result.AddTextFilter(Schema.MessageNumber, EDIMessageSchema.EM_MessageNum);
			messageNumberFilter.MultilingualDescription = ResString.GetMultilingualString("DCFD6565-4B01-4A59-A532-C19FF40AB791", Schema.MessageNumber);

			var statusFilter = result.AddTextFilter(Schema.Status, EDIMessageSchema.EM_Status, Factory.GetCachedValue<EDIMessageStatusList>());
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("7A13A8B6-5606-4E90-952C-CC81D7305556", Schema.Status);

			return result;
		}
	}
}
