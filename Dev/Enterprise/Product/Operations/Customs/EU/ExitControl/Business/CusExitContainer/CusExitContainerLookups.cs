using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitContainerLookups : ExitControlBase.Business.CusExitContainerLookups
	{
		public CusExitContainerLookups(AutoCusExitContainer parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList StatusList => StatusListCore;
		protected virtual CodeDescriptionPairList StatusListCore => new();
	}
}
