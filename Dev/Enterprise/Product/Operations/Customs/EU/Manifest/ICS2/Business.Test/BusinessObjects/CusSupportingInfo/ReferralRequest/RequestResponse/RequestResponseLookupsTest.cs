namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class RequestResponseLookupsTest : BaseAdditionalInfoLookupsTest
	{
		protected override BaseAdditionalInfoLookups GetLookups()
		{
			return Factory.New<RequestResponse>().Lookups;
		}
	}
}
