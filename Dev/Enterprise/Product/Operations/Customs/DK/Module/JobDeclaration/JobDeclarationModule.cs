using Enterprise.Customs.DK.Business.Declaration;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Customs.DK.Module
{
	[UniversalCopyInstanceType(InstanceType = typeof(JobDeclaration))]
	public class JobDeclarationModule : EU.Module.JobDeclarationModule
	{
		protected override Customs.Module.JobDeclarationController GetControllerForStandAlone() => new JobDeclarationController();
	}
}
