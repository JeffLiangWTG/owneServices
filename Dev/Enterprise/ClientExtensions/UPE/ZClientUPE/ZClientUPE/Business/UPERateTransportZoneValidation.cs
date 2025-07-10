using System.Collections;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPERateTransportZoneValidation : RateTransportZonesValidation
	{
		public UPERateTransportZoneValidation(UPERateTransportZone parent)
			: base(parent)
		{
		}

		protected override void CheckTZ_ZoneName()
		{
			base.CheckTZ_ZoneName();
			if (Parent.TransportProvider.TP_OH_RelatedParty == GlbCompany.CurrentCompany.GC_OH_OrgProxy)
			{
				IList lowercaseWords = Parent.TZ_ZoneName.ToString().ToLower().Split(' ');
				if (!lowercaseWords.Contains(UPERateTransportZone.MetroKeyword) && !lowercaseWords.Contains(UPERateTransportZone.CountryKeyword))
				{
					Parent.TZ_ZoneNameInfo.AddError("The word 'country' or 'metro' must be present");
				}
			}
		}
	}
}
