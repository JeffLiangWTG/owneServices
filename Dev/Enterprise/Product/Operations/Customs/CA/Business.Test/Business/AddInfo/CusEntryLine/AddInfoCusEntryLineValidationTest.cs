namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class AddInfoCusEntryLineValidationTest : CAAddInfoValidationTest<AddInfoCusEntryLine>
	{
		protected override AddInfoCusEntryLine GetNewAddInfo()
		{
			return new AddInfoCusEntryLine(Factory.New<CusEntryLine>().CL_AddInfoInfo);
		}
	}
}
