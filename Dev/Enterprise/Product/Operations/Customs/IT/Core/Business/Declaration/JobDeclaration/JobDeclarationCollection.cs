using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IT.Business.Declaration;

[CodeAlive("Template")]
public partial class JobDeclarationCollection : EU.Business.Declaration.JobDeclarationCollection
{
	public JobDeclarationCollection(BusinessObjectFactory factory, ZGuid companyPkToFilterOn)
		: base(factory, companyPkToFilterOn)
	{
	}
}
