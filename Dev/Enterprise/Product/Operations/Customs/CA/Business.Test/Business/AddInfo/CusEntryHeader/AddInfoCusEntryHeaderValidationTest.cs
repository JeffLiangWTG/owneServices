namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AddInfoCusEntryHeaderValidationTest : CAAddInfoValidationTest<AddInfoCusEntryHeader>
	{
		protected override AddInfoCusEntryHeader GetNewAddInfo()
		{
			return new AddInfoCusEntryHeader(Factory.New<CusEntryHeader>().CH_AddInfoInfo);
		}
	}
}
