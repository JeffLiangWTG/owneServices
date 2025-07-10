using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class NonDependentDLTMessageCollection : BusinessObjectCollection<EDIMessage>
	{
		public NonDependentDLTMessageCollection(BusinessObjectFactory factory, GlbCompany company) : base(factory)
		{
			this.company = company;
		}
		readonly GlbCompany company;
		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			if (company != null)
			{
				query.AddToFilter(EDIMessageSchema.EM_GB, company.Branches.Select(x => x.PK));
			}
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.KRCustoms);
			query.AddToFilter(EDIMessageSchema.EM_MessageType, KR.Messaging.Constants.EDIInterchangeType.DLT);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			return query;
		}
	}
}
