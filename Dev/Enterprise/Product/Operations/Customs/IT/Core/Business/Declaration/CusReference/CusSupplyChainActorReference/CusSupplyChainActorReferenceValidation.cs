namespace Enterprise.Customs.IT.Business.Declaration;

public class CusSupplyChainActorReferenceValidation : EU.Business.Declaration.CusSupplyChainActorReferenceValidation
{
	public CusSupplyChainActorReferenceValidation(CusSupplyChainActorReference parent) : base(parent)
	{
	}

	protected override string GetCustomsCodeMissingErrorForSelectedOwner() => ValidationCaptions.CusSupplyChainActorReference.SelectedOwnerDoesNotContainEORCode;
}
