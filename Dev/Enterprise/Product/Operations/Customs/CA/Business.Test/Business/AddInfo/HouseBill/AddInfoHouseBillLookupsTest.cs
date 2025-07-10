namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AddInfoHouseBillLookupsTest : CAAddInfoLookupsTest
	{
		protected override AddInfo GetNewAddInfo()
		{
			return new AddInfoHouseBill(Factory.New<Bill>().CU_AddInfoInfo);
		}
	}
}
