using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPECusHAWBFetchStrategy : CusHAWBFetchStrategy
	{
		public UPECusHAWBFetchStrategy(UPECusHAWB uPECusHAWB)
			: base(uPECusHAWB)
		{
		}

		protected override void FetchForLoadCore()
		{
			Factory.AddFetchHint(ProcessQueueSchema.P4_ParentID, BusinessObject.PK);
			base.FetchForLoadCore();
		}
	}
}
