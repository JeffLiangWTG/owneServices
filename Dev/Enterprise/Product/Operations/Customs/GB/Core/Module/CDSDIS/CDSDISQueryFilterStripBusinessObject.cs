using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.GB.Module
{
	public class CDSDISQueryFilterStripBusinessObject : FilterStripBusinessObject
	{
		public override ZQuery Filter
		{
			get
			{
				var filter = base.Filter;

				filter
					.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbCDSDISQuery)
					.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, Direction.Transmit);

				return filter;
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			result.AddTextFilter("Parameters/Reference", EDIMessageSchema.EM_ApplicationReference)
				.MultilingualDescription = ResString.GetMultilingualString("57EDB5E9-9012-41C4-A016-185A05C93905", "Parameters/Reference");
			result.AddTextFilter("Status", EDIMessageSchema.EM_Status)
				.MultilingualDescription = ResString.GetMultilingualString("AC6E2305-0E7C-4491-8CFD-176745C8AD9D", "Status");
			result.AddTextFilter("Owner", EDIMessageSchema.EM_MessageOwner)
				.MultilingualDescription = ResString.GetMultilingualString("53D783ED-930E-4BCE-AFDF-D69B68EBD8F9", "Owner");
			return result;
		}
	}
}
