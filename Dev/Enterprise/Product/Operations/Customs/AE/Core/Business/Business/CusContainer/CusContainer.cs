using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AE.Business;

public class CusContainer : BaseCusContainer, Integration.Customs.AE.ICusContainer
{
	public CusContainer(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	#region Typed 'New' methods, functions and properties

	public new JobDeclaration Declaration
	{
		get { return (JobDeclaration)base.Declaration; }
	}

	public new CusContainerLookups Lookups => (CusContainerLookups)base.Lookups;

	protected override Customs.Business.CusContainerLookups GetNewLookups()
	{
		return new CusContainerLookups(this);
	}

	#endregion

	#region Overrides

	public new CusContainerValidation Validation
	{
		get { return new CusContainerValidation(this); }
	}

	protected override System.Type GetJobDeclarationType()
	{
		return typeof(JobDeclaration);
	}

	#endregion
}
