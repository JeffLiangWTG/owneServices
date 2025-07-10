using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusPollingTransactionCollection : ActiveBusinessObjectCollection<CusPollingTransaction>
	{
		public CusPollingTransactionCollection(EDIMessage master) : this(master, new ZQuery())
		{
		}
		public CusPollingTransactionCollection(EDIMessage master, ZQuery filter) : base(master.Factory, filter)
		{
			message = master;
		}
		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(new ZQuery(CusPollingTransactionSchema.CPT_ParentID, message.PK));
			return query;
		}
		readonly EDIMessage message;
		protected override bool AllowNew => false;
	}
}
