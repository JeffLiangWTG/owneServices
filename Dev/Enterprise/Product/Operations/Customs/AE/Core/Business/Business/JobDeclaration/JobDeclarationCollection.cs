using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AE.Business;

public class JobDeclarationCollection : Customs.Business.BaseJobDeclarationCollection
{
	public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
		: base(factory, companyPkToFilterOn)
	{
	}

	protected override bool AllowNewCore => false;

	public new JobDeclaration this[int index] => (JobDeclaration)Elements[index];

	public new JobDeclaration AddNew() => (JobDeclaration)base.AddNew();
}
