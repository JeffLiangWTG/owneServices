using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class CusSeal : EU.Business.Declaration.CusSeal
{
	public CusSeal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusSealValidation Validation => (CusSealValidation)base.Validation;

	protected override Customs.Business.CusSealValidation GetNewValidation() => new CusSealValidation(this);
}
