using System.Collections;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class TemporaryLandingInfoLookups : CusSupportingInfoLookups
	{
		public TemporaryLandingInfoLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public override ICollection CodeList => TemporaryLandingInfo.GetReasonList(Factory);

		public CodeDescriptionPairList BondedTransportList => TemporaryLandingInfo.GetBondedTransportList(Factory);
	}
}
