using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ES.ExitControl.Module
{
	public class ExitControlModule : EU.ExitControl.Module.ExitControlModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ExitControlFilterBusinessObject();
	}
}
