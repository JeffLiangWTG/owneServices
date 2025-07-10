using Enterprise.Customs.FI.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Customs.FI.Module;

[UniversalCopyInstanceType(InstanceType = typeof(JobDeclaration))]
public class JobDeclarationModule : EU.Module.JobDeclarationModule
{
	protected override Customs.Module.JobDeclarationController GetControllerForStandAlone() => new JobDeclarationController();
}
