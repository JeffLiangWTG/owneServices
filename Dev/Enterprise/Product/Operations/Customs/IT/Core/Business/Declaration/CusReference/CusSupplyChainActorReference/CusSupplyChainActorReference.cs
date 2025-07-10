using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

public class CusSupplyChainActorReference : EU.Business.Declaration.CusSupplyChainActorReference
{
	public CusSupplyChainActorReference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override Customs.Business.CusReferenceValidation GetNewValidation() => new CusSupplyChainActorReferenceValidation(this);
}
