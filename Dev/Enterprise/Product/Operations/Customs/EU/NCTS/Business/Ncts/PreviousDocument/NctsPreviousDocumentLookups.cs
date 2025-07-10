using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NctsPreviousDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentLookups
	{
		public NctsPreviousDocumentLookups(NctsPreviousDocument parent)
			: base(parent)
		{ }

		public override CodeDescriptionPairList SubTypeList => Factory.GetCachedValue<PreviousDocumentClassList>();

		protected new NctsPreviousDocument Parent => (NctsPreviousDocument)base.Parent;
	}
}
