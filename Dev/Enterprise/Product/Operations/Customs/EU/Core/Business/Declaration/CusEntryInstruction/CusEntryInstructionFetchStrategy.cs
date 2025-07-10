using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.FetchStrategies
{
	public class CusEntryInstructionFetchStrategy : Customs.Business.FetchStrategies.CusEntryInstructionFetchStrategy
	{
		public CusEntryInstructionFetchStrategy(CusEntryInstruction cusEntryInstruction)
			: base(cusEntryInstruction)
		{
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(CusReferenceSchema.CFR_ParentID, BusinessObject.PK);
			Factory.AddFetchHint(CusAuthorizationUsageSchema.AGC_ParentID, BusinessObject.PK);
		}
	}
}
