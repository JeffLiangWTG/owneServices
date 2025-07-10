using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AURollAddInfoLookups : AutoAURollAddInfoLookups
	{
		public AURollAddInfoLookups(AutoAURollAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ClientRolls
		{
			get { return Factory.GetCachedValue<CMRClientRolls>(); }
		}
	}
}
