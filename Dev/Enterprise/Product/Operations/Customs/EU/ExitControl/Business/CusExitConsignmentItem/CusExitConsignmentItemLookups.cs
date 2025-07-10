using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitConsignmentItemLookups : ExitControlBase.Business.CusExitConsignmentItemLookups
	{
		public CusExitConsignmentItemLookups(AutoCusExitConsignmentItem parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList StatusList => StatusListCore;
		protected virtual CodeDescriptionPairList StatusListCore => new CodeDescriptionPairList();

		public CodeDescriptionPairList UCRStatusList => UCRStatusListCore;
		protected virtual CodeDescriptionPairList UCRStatusListCore => new CodeDescriptionPairList();
	}
}
