namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ExportAddInfoJobComInvoiceHeaderValidationTest : CAAddInfoValidationTest<AddInfoJobComInvoiceHeader>
	{
		protected override AddInfoJobComInvoiceHeader GetNewAddInfo()
		{
			return new AddInfoJobComInvoiceHeader(Factory.New<JobDeclaration>().Invoices.AddNew().JZ_AddInfoInfo);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew();
		}

		#endregion

	}
}
