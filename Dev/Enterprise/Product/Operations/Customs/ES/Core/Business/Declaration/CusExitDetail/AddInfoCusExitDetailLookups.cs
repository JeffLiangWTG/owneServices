using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class AddInfoCusExitDetailLookups : EUAddInfoLookups
	{
		public AddInfoCusExitDetailLookups(AddInfoCusExitDetail parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CircuitCodeList => Factory.GetCachedValue<CircuitCodeList>();
	}
}
