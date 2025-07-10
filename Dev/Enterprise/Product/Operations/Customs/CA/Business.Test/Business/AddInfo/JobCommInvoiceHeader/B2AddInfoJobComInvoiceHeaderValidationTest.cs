namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class B2AddInfoJobComInvoiceHeaderValidationTest : ComonImportAddInfoJobComInvoiceHeaderValidationTest
	{
		protected override CargoWise.Types.ZString GetMessageType()
		{
			return JobMessageTypeList.Codes.B2Adjustments;
		}
	}
}
