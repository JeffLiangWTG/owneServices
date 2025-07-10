using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryHeaderFetchStrategy : Customs.Business.FetchStrategies.CusEntryHeaderFetchStrategy
	{
		public CusEntryHeaderFetchStrategy(CusEntryHeader cusEntryHeader)
				: base(cusEntryHeader)
		{
		}

		protected new CusEntryHeader BusinessObject
		{
			get { return (CusEntryHeader)base.BusinessObject; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(StmALogSchema.SL_Parent, BusinessObject.PK);
			Factory.AddFetchHint(JobDocumentDataSchema.JDD_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
		}
	}
}
