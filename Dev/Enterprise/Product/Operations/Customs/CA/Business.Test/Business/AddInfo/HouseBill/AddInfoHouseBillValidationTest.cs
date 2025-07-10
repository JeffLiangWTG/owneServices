namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AddInfoHouseBillValidationTest : CAAddInfoValidationTest<AddInfoHouseBill>
	{
		protected override AddInfoHouseBill GetNewAddInfo()
		{
			return new AddInfoHouseBill(Factory.New<Bill>().CU_AddInfoInfo);
		}
	}
}
