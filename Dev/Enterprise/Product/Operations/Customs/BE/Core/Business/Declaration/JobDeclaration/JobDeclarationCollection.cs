using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.BE.Business.Declaration;

[CodeAlive("Template")]
public partial class JobDeclarationCollection : EU.Business.Declaration.JobDeclarationCollection
{
	public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
		: base(factory, companyPkToFilterOn)
	{
	}

	protected override bool AllowNewCore => false;
}
