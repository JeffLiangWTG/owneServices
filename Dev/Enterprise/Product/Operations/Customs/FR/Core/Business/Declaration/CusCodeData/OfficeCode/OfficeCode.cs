using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.Declaration;

public class OfficeCode : EuOfficeCode
{
	public OfficeCode(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

	protected override CusCodeDataValidation GetNewValidation() => new OfficeCodeValidation(this);
}
