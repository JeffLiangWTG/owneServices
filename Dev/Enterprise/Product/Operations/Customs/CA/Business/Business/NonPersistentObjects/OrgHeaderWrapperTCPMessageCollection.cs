using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class OrgHeaderWrapperTCPMessageCollection : EDIMessageCollection
	{
		public OrgHeaderWrapperTCPMessageCollection(OrgHeader master)
			: base(master)
		{
		}

		public new OrgHeader Master
		{
			get { return (OrgHeader)base.Master; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = null;
			if (Master == null)
			{
				result = ZQuery.NoResultQuery;
			}
			else
			{
				result = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, Master.PK);
				result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);
				result.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.TradeChainPartner);
				result.FetchOnlyFromLocalCache = !Master.IsInDatabase;
			}
			return result;
		}
	}
}
