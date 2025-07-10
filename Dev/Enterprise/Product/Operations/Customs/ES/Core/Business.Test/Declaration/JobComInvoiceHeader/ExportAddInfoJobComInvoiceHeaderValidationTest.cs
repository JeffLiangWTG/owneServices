using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	sealed class ExportAddInfoJobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_AgreedPlaceCode_IsRequired()
		{
			const string messageError = "Incoterm Place Code or Country Code is required";
			CombineAssertions(() =>
			{
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;

				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
				invoiceHeader.ZG_AgreedPlaceCode = ZString.Empty;

				AssertHasMessageError("Empty ZG_AgreedPlaceCode", invoiceHeader.ZG_AgreedPlaceCodeInfo, messageError);

				declaration.ZG_AgreedPlaceCode = "ESMAD";
				invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertNoNotifications("Declaration Place Code is filled", invoiceHeader.ZG_AgreedPlaceCodeInfo);

				declaration.ZG_AgreedPlaceCode = ZString.Empty;
				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
				invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertNoNotifications("Type is XXX (Other)", invoiceHeader.ZG_AgreedPlaceCodeInfo);

				invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
				invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertHasMessageError("Empty ZG_AgreedPlaceCode", invoiceHeader.ZG_AgreedPlaceCodeInfo, messageError);

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.T2L;
				invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertNoNotifications("Is T2L", invoiceHeader.ZG_AgreedPlaceCodeInfo);

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.T2C;
				invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertNoNotifications("Is T2C", invoiceHeader.ZG_AgreedPlaceCodeInfo);

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				invoiceHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertNoNotifications("Is EXS", invoiceHeader.ZG_AgreedPlaceCodeInfo);

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
				AssertNoNotifications("No error for all entry instruction associated T2L T2C or EXS", invoiceHeader.ZG_AgreedPlaceCodeInfo);
			});
		}

		public void TestCheckZG_TransportChargesMethodOfPayment_Mandatory()
		{
			invoiceHeader.ZG_TransportChargesMethodOfPayment = ZString.Empty;
			AssertNoNotifications("No Message Error if empty", invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo);
		}

		public void TestCheckZG_TransportChargesMethodOfPayment_ListValidation()
		{
			CombineAssertions(() =>
			{
				invoiceHeader.ZG_TransportChargesMethodOfPayment = TransportChargesMethodOfPaymentList.Codes.A;
				AssertNoNotifications("Valid and instruction != EXS", invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo);

				invoiceHeader.ZG_TransportChargesMethodOfPayment = "1";
				AssertHasMessageErrorContaining("Invalid and instruction A", invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo, ListValidation.InvalidCodeMessageError.ToString());

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;

				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;

				invoiceHeader.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
				AssertNoNotifications("Invalid and EXS but no A instruction associated", invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;

				invoiceHeader.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
				AssertHasMessageErrorContaining("Invalid, EXS and instruction A associated", invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo, ListValidation.InvalidCodeMessageError.ToString());

				entryInstruction2.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				invoiceHeader.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
				AssertNoNotifications("Empty and both EXS associated", invoiceHeader.ZG_TransportChargesMethodOfPaymentInfo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
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
