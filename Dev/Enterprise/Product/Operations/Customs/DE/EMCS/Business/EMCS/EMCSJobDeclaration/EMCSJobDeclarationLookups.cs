using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSJobDeclarationLookups : EU.EMCS.Business.EMCSJobDeclarationLookups
	{
		public EMCSJobDeclarationLookups(EMCSJobDeclaration parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<EU.EMCS.Business.EMCSDestinationTypeList>();
	}
}
