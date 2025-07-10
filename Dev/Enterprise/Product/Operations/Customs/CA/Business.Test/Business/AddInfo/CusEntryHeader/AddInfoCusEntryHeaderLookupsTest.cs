namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AddInfoCusEntryHeaderLookupsTest : CAAddInfoLookupsTest
	{
		protected override AddInfo GetNewAddInfo()
		{
			return new AddInfoCusEntryHeader(Factory.New<CusEntryHeader>().CH_AddInfoInfo);
		}
	}
}
