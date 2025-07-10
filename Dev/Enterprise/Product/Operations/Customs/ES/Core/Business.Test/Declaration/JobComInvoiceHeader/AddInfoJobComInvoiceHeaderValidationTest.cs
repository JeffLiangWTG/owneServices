using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	sealed class AddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_AgreedPlaceCode_IsRequired()
		{
			const string messageError = "Incoterm Place Code or Country Code is required";
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine1.JI_CEI = entryInstruction.PK;

					invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
					invoiceHeader.ZG_AgreedPlaceCode = ZString.Empty;

					AssertHasMessageError("Empty and A Instruction", invoiceHeader.ZG_AgreedPlaceCodeInfo, messageError);

					invoiceHeader.ZG_AgreedPlaceCode = "ESMAD";
					invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertNoNotifications("Declaration Place Code is filled", invoiceHeader.ZG_AgreedPlaceCodeInfo);

					invoiceHeader.ZG_AgreedPlaceCode = ZString.Empty;
					entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
					invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertNoNotifications("Empty and EXS instruction", invoiceHeader.ZG_AgreedPlaceCodeInfo);

					var instruction2 = declaration.CustomsEntryInstructions.AddNew();
					instruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
					invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertNoNotifications("No error for A entry instruction not associated", invoiceHeader.ZG_AgreedPlaceCodeInfo);

					var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
					invoiceLine2.JI_CEI = instruction2.PK;
					invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertHasMessageErrorContaining("Error for A entry instruction associated", invoiceHeader.ZG_AgreedPlaceCodeInfo, messageError);

					instruction2.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
					invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertNoNotifications("No error for all entry instruction associated EXS", invoiceHeader.ZG_AgreedPlaceCodeInfo);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoiceHeader = declaration.Invoices.AddNew();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
		}
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		CusEntryInstruction entryInstruction;
	}
}
