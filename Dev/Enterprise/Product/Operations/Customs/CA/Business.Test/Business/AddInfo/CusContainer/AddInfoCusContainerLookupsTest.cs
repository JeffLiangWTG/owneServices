namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AddInfoCusContainerLookupsTest : CAAddInfoLookupsTest
	{
		protected override AddInfo GetNewAddInfo()
		{
			return new AddInfoCusContainer(Factory.New<CusContainer>().CO_AddInfoInfo);
		}
	}
}
