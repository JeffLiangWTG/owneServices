using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.Module;

[UniversalCopyInstanceType(InstanceType = typeof(Business.Declaration.JobDeclaration))]
public class JobDeclarationModule : EU.Module.JobDeclarationModule
{
	protected override FilterBusinessObject GetNewFilterBusinessObject()
	{
		return new JobDeclarationFilterBusinessObject();
	}

	protected override IFilterControl GetNewFilterControl()
	{
		return new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);
	}
}
