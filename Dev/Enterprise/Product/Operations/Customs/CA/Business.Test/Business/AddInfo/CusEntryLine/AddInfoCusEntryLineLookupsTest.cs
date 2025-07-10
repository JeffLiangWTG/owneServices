namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AddInfoCusEntryLineLookupsTest : CAAddInfoLookupsTest
	{
		protected override AddInfo GetNewAddInfo()
		{
			return new AddInfoCusEntryLine(Factory.New<CusEntryLine>().CL_AddInfoInfo);
		}
	}
}
