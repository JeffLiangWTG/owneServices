using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSAddInfoCusContainerLookups : EUEMCSAddInfoLookups
	{
		public EMCSAddInfoCusContainerLookups(AutoEUEMCSAddInfo parent) : base(parent)
		{
		}

		public CodeDescriptionPairList EMCSDestinationTypeList => Factory.GetCachedValue<EMCSTransportUnitCodeList>();
	}
}
