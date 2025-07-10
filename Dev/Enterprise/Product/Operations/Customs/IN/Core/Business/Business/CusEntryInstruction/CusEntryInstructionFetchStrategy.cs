using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IN.Business;

public class CusEntryInstructionFetchStrategy : Customs.Business.FetchStrategies.CusEntryInstructionFetchStrategy
{
	public CusEntryInstructionFetchStrategy(CusEntryInstruction instruction)
		: base(instruction)
	{
	}

	protected new CusEntryInstruction BusinessObject => (CusEntryInstruction)base.BusinessObject;

	protected override void FetchForLoadChildEditableObjectsCore()
	{
		base.FetchForLoadChildEditableObjectsCore();
		AddFetchHints();
	}

	protected override void FetchForValidateCore()
	{
		base.FetchForValidateCore();
		AddFetchHints();
	}

	void AddFetchHints()
	{
		Factory.AddFetchHint(CusEntryNumSchema.CE_ParentID, BusinessObject.PK);
	}
}
