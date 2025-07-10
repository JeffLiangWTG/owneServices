using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Customs.EU.NCTS.Module
{
	public class NCTSPopupModuleDecisionProvider : PopupModuleDecisionProvider
	{
		public NCTSPopupModuleDecisionProvider(IFindBox findBox) : base(findBox)
		{
		}

		public override bool ShouldIgnoreAdditionalFilter => false;
	}
}
