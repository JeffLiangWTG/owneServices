namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class RequestInformationLookupsTest : BaseAdditionalInfoLookupsTest
	{
		protected override BaseAdditionalInfoLookups GetLookups()
		{
			return Factory.New<RequestInformation>().Lookups;
		}
	}
}
