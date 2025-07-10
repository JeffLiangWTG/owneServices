namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class HRCMAdditionalInfoLookupsTest : BaseAdditionalInfoLookupsTest
	{
		protected override BaseAdditionalInfoLookups GetLookups()
		{
			return Factory.New<HRCMAdditionalInfo>().Lookups;
		}
	}
}
