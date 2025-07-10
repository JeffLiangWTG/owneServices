using System.Collections;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AdditionalInfoLookups : CusSupportingInfoLookups
	{
		public AdditionalInfoLookups(AdditionalInfo parent) : base(parent)
		{
		}

		public override ICollection CodeList => Factory.GetCachedValue<EUICS2AdditionalInfoTypes>();
	}
}
