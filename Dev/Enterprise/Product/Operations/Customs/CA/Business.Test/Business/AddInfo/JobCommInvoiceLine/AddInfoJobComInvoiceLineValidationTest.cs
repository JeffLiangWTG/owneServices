using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	class AddInfoJobComInvoiceLineValidationTest : CAAddInfoValidationTest<AddInfoJobComInvoiceLine>
	{
		protected override AddInfoJobComInvoiceLine GetNewAddInfo()
		{
			return new AddInfoJobComInvoiceLine(invoiceLine.JI_AddInfoInfo);
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = messageInitiator;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		public void TestCheckCA_OriginalLineNo()
		{
			AssertCheckCA_OriginalLineNo(JobMessageTypeList.Codes.B2Adjustments);
			AssertCheckCA_OriginalLineNo(JobMessageTypeList.Codes.XTypeEntry);
		}

		void AssertCheckCA_OriginalLineNo(ZString messageType)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			var invoiceLine = declaration.B2AsAccountedForInvoices.AddNew().AsAccountForFilteredInvoiceLines.AddNew();
			invoiceLine.AddInfoValidation.ValidateCA_OriginalLineNo();
			AssertHasMessageError(messageType, invoiceLine.CA_OriginalLineNoInfo, "You have not entered a value.");

			invoiceLine.CA_OriginalLineNo = "123";
			invoiceLine.AddInfoValidation.ValidateCA_OriginalLineNo();
			AssertNoWarning(messageType, invoiceLine.CA_OriginalLineNoInfo, AddInfoJobComInvoiceLineValidation.AsAccountedOriginalLineNoWarning);

			invoiceLine.CA_OriginalLineNo = "123s";
			invoiceLine.AddInfoValidation.ValidateCA_OriginalLineNo();
			AssertHasWarning(messageType, invoiceLine.CA_OriginalLineNoInfo, AddInfoJobComInvoiceLineValidation.AsAccountedOriginalLineNoWarning);

			invoiceLine.CA_OriginalLineNo = "123/SL";
			invoiceLine.AddInfoValidation.ValidateCA_OriginalLineNo();
			AssertHasWarning(messageType, invoiceLine.CA_OriginalLineNoInfo, AddInfoJobComInvoiceLineValidation.AsAccountedOriginalLineNoWarning);

			invoiceLine = declaration.B2AsClaimedForInvoices.AddNew().AsClaimForFilteredInvoiceLines.AddNew();
			invoiceLine.AddInfoValidation.ValidateCA_OriginalLineNo();
			AssertHasWarning(messageType, invoiceLine.CA_OriginalLineNoInfo, AddInfoJobComInvoiceLineValidation.AsClaimedOriginalLineNoWarning);

			invoiceLine.CA_OriginalLineNoInfo.ClearAllNotifications();
			invoiceLine.CA_OriginalLineNo = "123";
			invoiceLine.AddInfoValidation.ValidateCA_OriginalLineNo();
			AssertNoWarning(messageType, invoiceLine.CA_OriginalLineNoInfo, AddInfoJobComInvoiceLineValidation.AsClaimedOriginalLineNoWarning);

			invoiceLine.CA_OriginalLineNo = "/SL";
			invoiceLine.AddInfoValidation.ValidateCA_OriginalLineNo();
			AssertHasWarning(messageType, invoiceLine.CA_OriginalLineNoInfo, AddInfoJobComInvoiceLineValidation.AsClaimedOriginalLineNoWarning);

			invoiceLine.CA_OriginalLineNoInfo.ClearAllNotifications();
			invoiceLine.CA_OriginalLineNo = "1/SL";
			invoiceLine.AddInfoValidation.ValidateCA_OriginalLineNo();
			AssertNoWarning(messageType, invoiceLine.CA_OriginalLineNoInfo, AddInfoJobComInvoiceLineValidation.AsClaimedOriginalLineNoWarning);
		}
	}
}
