using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.GB.CDS.Organisation
{
	public class OrgHeaderWrapperMessageCollection : EDIMessageCollection
	{
		public OrgHeaderWrapperMessageCollection(OrgHeader master)
			: base(master)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			result.AddToFilter(new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodes.GbCDSDISQuery));
			return result;
		}
	}
}
