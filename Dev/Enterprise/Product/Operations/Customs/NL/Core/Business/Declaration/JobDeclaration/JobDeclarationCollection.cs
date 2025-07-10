using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class JobDeclarationCollection : Customs.Business.BaseJobDeclarationCollection
{
	public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
		: base(factory, companyPkToFilterOn)
	{
	}

	public new JobDeclaration this[int index] => (JobDeclaration)Elements[index];

	public new JobDeclaration AddNew() => (JobDeclaration)base.AddNew();

	protected override bool AllowNewCore => false;
}
