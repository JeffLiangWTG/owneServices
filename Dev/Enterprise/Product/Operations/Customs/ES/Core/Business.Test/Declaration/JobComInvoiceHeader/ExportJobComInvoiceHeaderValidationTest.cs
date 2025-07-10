using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class ExportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		protected override BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			return invoice;
		}

		protected override Type GetTypeForTest() => typeof(ExportJobComInvoiceHeaderValidation);

		protected override string ExpectedBuyerAddressEmptyWarningMessage => "Buyer address should be selected";

		public void TestCheckJZ_IncoTerm_NonUCC6()
		{
			GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				invHeader.JZ_IncoTerm = "FOB";
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has value for Export", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);

				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for Export", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);
			}

			GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for T2LExpedition", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);
			}

			GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LClearanceExport);
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for T2LClearance", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);
			}

			GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for EXS", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);
			}

			GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				var invoiceLine2 = invHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invHeader.JZ_IncoTerm = "FOB";
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has value for Export with more than 1 entry instruction", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);

				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for Export with more than 1 entry instruction", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);
			}
		}

		public void TestCheckJZ_IncoTerm_UCC6()
		{
			using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				invHeader.JZ_IncoTerm = "FOB";
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has value for Export", invHeader.JZ_IncoTermInfo, MandatoryValidation.YouHaveNotEntered);

				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for Export", invHeader.JZ_IncoTermInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for T2LExpedition", invHeader.JZ_IncoTermInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LClearanceExport);
				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for T2LClearance", invHeader.JZ_IncoTermInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.B);
				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for B", invHeader.JZ_IncoTermInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.C);
				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for C", invHeader.JZ_IncoTermInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				var invoiceLine2 = invHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invHeader.JZ_IncoTerm = "AH3";
				AssertNoMessageErrorContaining("Assert no empty JZ_IncoTerm has invalid value for Export with more than 1 entry instruction", invHeader.JZ_IncoTermInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("Assert valid JZ_IncoTerm has invalid value for Export with more than 1 entry instruction", invHeader.JZ_IncoTermInfo, ListValidation.InvalidCodeMessageError.ToString());

				invHeader.JZ_IncoTerm = "FOB";
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has valid value for Export with more than 1 entry instruction", invHeader.JZ_IncoTermInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has valid value for Export with more than 1 entry instruction", invHeader.JZ_IncoTermInfo, ListValidation.InvalidCodeMessageError.ToString());

				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for Export with more than 1 entry instruction", invHeader.JZ_IncoTermInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_ShipmentIncoTerm = "AH3";
				invHeader.Validation.ValidateJZ_IncoTerm();
				AssertNoMessageErrorContaining("Assert mandatory JE_ShipmentIncoTerm has invalid value and JZ_IncoTerm empty", invHeader.JZ_IncoTermInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining("Assert mandatory JE_ShipmentIncoTerm has invalid value and JZ_IncoTerm empty", invHeader.JZ_IncoTermInfo, ListValidation.InvalidCodeMessageError.ToString());
			}
		}

		public void TestCheckJZ_IncoTermPlace_IncoTermXXX()
		{
			var messageErrorText = "You have not entered a Description for XXX Incoterm";

			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				invHeader.JZ_IncoTerm = "XXX";
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
				invHeader.JZ_IncoTermPlace = "Tomelloso";
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace has value for Export when incoterm (invoice header) is XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertHasMessageErrorContaining("Message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value for Export when incoterm (invoice header) is XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				declaration.JE_ShipmentIncoTermPlace = "madrid";
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace has no value but JE_ShipmentIncoTermPlace has value for Export when incoterm (invoice header) is XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				invHeader.JZ_IncoTerm = "AH3";
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value for Export when incoterm (invoice header) is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				invHeader.JZ_IncoTerm = ZString.Empty;
				declaration.JE_ShipmentIncoTerm = "XXX";
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertHasMessageErrorContaining("Message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value for Export when incoterm (declaration) is XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
				invHeader.JZ_IncoTerm = "XXX";
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value for T2LExpedition when incoterm is XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LClearanceExport);
				invHeader.JZ_IncoTerm = "XXX";
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value for T2LClearance when incoterm is XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
				invHeader.JZ_IncoTerm = "XXX";
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value for EXS when incoterm is XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				invHeader.JZ_IncoTerm = "XXX";
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				var invoiceLine2 = invHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invHeader.JZ_IncoTermPlace = "Madrid";
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace has value for Export with more than 1 entry instruction when incoterm is XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertHasMessageErrorContaining("Message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value for Export with more than 1 entry instruction when incoterm is XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);
			});
		}

		public void TestCheckJZ_IncoTermPlace_IncoTermNotXXX()
		{
			var messageErrorText = "Incoterm Location is required";

			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				invHeader.JZ_IncoTerm = "AH3";
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
				invHeader.ZG_AgreedPlaceCode = "AH";
				invHeader.JZ_IncoTermPlace = "Tomelloso";
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace has value and ZG_AgreedPlaceCode (invoice header) <= 2 for Export when incoterm (invoice header) is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertHasMessageErrorContaining("Message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value and ZG_AgreedPlaceCode (invoice header) <= 2 for Export when incoterm (invoice header) is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				declaration.JE_ShipmentIncoTermPlace = "madrid";
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace has no value but JE_ShipmentIncoTermPlace has value and ZG_AgreedPlaceCode (invoice header) <= 2 for Export when incoterm (invoice header) is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				invHeader.JZ_IncoTerm = "XXX";
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value and ZG_AgreedPlaceCode (invoice header) <= 2 for Export when incoterm (invoice header) is XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				invHeader.JZ_IncoTerm = ZString.Empty;
				invHeader.ZG_AgreedPlaceCode = "AH";
				declaration.JE_ShipmentIncoTerm = "AH3";
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertHasMessageErrorContaining("Message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value and ZG_AgreedPlaceCode (invoice header) <= 2 for Export when incoterm (declaration) is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				invHeader.ZG_AgreedPlaceCode = "AAA";
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value and ZG_AgreedPlaceCode (invoice header) > 2 for Export when incoterm (declaration) is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				invHeader.ZG_AgreedPlaceCode = ZString.Empty;
				declaration.ZG_AgreedPlaceCode = "AH";
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertHasMessageErrorContaining("Message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value and ZG_AgreedPlaceCode (declaration) <= 2 for Export when incoterm (declaration) is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				declaration.ZG_AgreedPlaceCode = "AAA";
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value and ZG_AgreedPlaceCode (declaration) > 2 for Export when incoterm (declaration) is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
				invHeader.JZ_IncoTerm = "AH3";
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value for T2LExpedition when incoterm is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LClearanceExport);
				invHeader.JZ_IncoTerm = "AH3";
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value for T2LClearance when incoterm is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
				invHeader.JZ_IncoTerm = "AH3";
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value for EXS when incoterm is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				invHeader.JZ_IncoTerm = "AH3";
				invHeader.ZG_AgreedPlaceCode = "AH";
				declaration.JE_ShipmentIncoTermPlace = ZString.Empty;

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				var invoiceLine2 = invHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invHeader.JZ_IncoTermPlace = "Madrid";
				AssertNoMessageErrorContaining("No message error when JZ_IncoTermPlace has value and ZG_AgreedPlaceCode (invoice header) > 2 for Export with more than 1 entry instruction when incoterm is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);

				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertHasMessageErrorContaining("Message error when JZ_IncoTermPlace and JE_ShipmentIncoTermPlace have no value and ZG_AgreedPlaceCode (invoice header) > 2 for Export with more than 1 entry instruction when incoterm is not XXX", invHeader.JZ_IncoTermPlaceInfo, messageErrorText);
			});
		}

		public void TestCheckJZ_InvoiceAmountInfo_Empty_OnlyOneInstructionEXS()
		{
			GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
			CombineAssertions(() =>
			{
				invHeader.JZ_InvoiceAmount = ZDecimal.Zero;
				invHeader.Validation.ValidateJZ_InvoiceAmount();
				AssertNoNotifications("Only one instruction with EXS not associated", invHeader.JZ_InvoiceAmountInfo);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				invHeader.Validation.ValidateJZ_InvoiceAmount();
				AssertNoNotifications("Two instruction with EXS not associated", invHeader.JZ_InvoiceAmountInfo);

				var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction3.CEI_SubStyle = EntrySubStyleList.Codes.A;
				invHeader.Validation.ValidateJZ_InvoiceAmount();
				AssertNoNotifications("Three instruction (EXS, EXS and A) not associated", invHeader.JZ_InvoiceAmountInfo);

				var invoiceLine2 = invHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;

				invHeader.Validation.ValidateJZ_InvoiceAmount();
				AssertNoNotifications("Three instruction (EXS, EXS associated and A not associated)", invHeader.JZ_InvoiceAmountInfo);

				var invoiceLine3 = invHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_CEI = entryInstruction3.PK;

				invHeader.Validation.ValidateJZ_InvoiceAmount();
				AssertHasMessageErrorContaining("Three instruction (EXS, EXS and A associated)", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJZ_IncotermInfo_Empty_OnlyOneInstructionEXS()
		{
			GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
			CombineAssertions(() =>
			{
				invHeader.JZ_IncoTerm = ZString.Empty;
				invHeader.Validation.ValidateJZ_IncoTerm();
				AssertNoNotifications("JZ_IncoTermInfo for only one instruction with EXS", invHeader.JZ_IncoTermInfo);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				var invoiceLine2 = invHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invHeader.Validation.ValidateJZ_IncoTerm();
				AssertNoNotifications("JZ_IncoTermInfo for two instruction with EXS", invHeader.JZ_IncoTermInfo);

				var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction3.CEI_SubStyle = EntrySubStyleList.Codes.A;
				var invoiceLine3 = invHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_CEI = entryInstruction3.PK;
				invHeader.Validation.ValidateJZ_IncoTerm();
				AssertHasMessageErrorContaining("JZ_IncoTermInfo for three instruction (EXS, EXS and A)", invHeader.JZ_IncoTermInfo, "You have not entered an Incoterm.");
			});
		}

		public void TestCheckZG_AgreedPlaceCodeInfo_Empty_OnlyOneInstructionEXS()
		{
			const string message = "Incoterm Place Code or Country Code is required";
			GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
			CombineAssertions(() =>
			{
				invHeader.JZ_IncoTerm = "AH3";
				invHeader.ZG_AgreedPlaceCode = ZString.Empty;
				var invline = invHeader.InvoiceLines.AddNew();
				invHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertNoNotifications("ZG_AgreedPlaceCodeInfo for only one instruction with EXS", invHeader.ZG_AgreedPlaceCodeInfo);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				invline.JI_CEI = entryInstruction2.PK;
				entryInstruction2.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				invHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
				AssertNoNotifications("ZG_AgreedPlaceCodeInfo for two instruction with EXS", invHeader.ZG_AgreedPlaceCodeInfo);

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetInvoiceHeaderConfiguration(declaration, true))
				{
					var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
					entryInstruction3.CEI_SubStyle = EntrySubStyleList.Codes.A;
					invline.JI_CEI = entryInstruction3.PK;
					invHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertHasMessageError("ZG_AgreedPlaceCodeInfo for three instruction (EXS, EXS and A)", invHeader.ZG_AgreedPlaceCodeInfo, message);

					invHeader.JZ_IncoTerm = ZString.Empty;
					invHeader.AddInfoValidation.ValidateZG_AgreedPlaceCode();
					AssertNoNotifications("ZG_AgreedPlaceCodeInfo for empty JZ_IncoTerm", invHeader.ZG_AgreedPlaceCodeInfo);
				}
			});
		}

		public void TestCheckJZ_ValuationCodeInfo_Empty_OnlyOneInstructionEXS()
		{
			GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
			CombineAssertions(() =>
			{
				invHeader.JZ_ValuationCode = ZString.Empty;
				invHeader.Validation.ValidateJZ_ValuationCode();
				AssertNoNotifications("Only one instruction with EXS associated", invHeader.JZ_ValuationCodeInfo);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				invHeader.Validation.ValidateJZ_ValuationCode();
				AssertNoNotifications("Two instruction with EXS, one not associated", invHeader.JZ_ValuationCodeInfo);

				var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction3.CEI_SubStyle = EntrySubStyleList.Codes.A;
				invHeader.Validation.ValidateJZ_ValuationCode();
				AssertNoNotifications("Three instruction (EXS, EXS and A not associated)", invHeader.JZ_ValuationCodeInfo);

				var invoiceLine2 = invHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;

				invHeader.Validation.ValidateJZ_ValuationCode();
				AssertNoNotifications("Three instruction (EXS, EXS associated and A not associated)", invHeader.JZ_ValuationCodeInfo);

				var invoiceLine3 = invHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_CEI = entryInstruction3.PK;

				invHeader.Validation.ValidateJZ_ValuationCode();
				AssertHasMessageErrorContaining("Three instruction (EXS, EXS and A associated)", invHeader.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckZG_TransportChargesMethodOfPaymentInfo_Empty_OnlyOneInstructionEXS()
		{
			GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
			CombineAssertions(() =>
			{
				invHeader.ZG_TransportChargesMethodOfPayment = ZString.Empty;
				invHeader.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
				AssertNoNotifications("ZG_TransportChargesMethodOfPaymentInfo for only one instruction with EXS", invHeader.ZG_TransportChargesMethodOfPaymentInfo);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				var invoiceLine2 = invHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invHeader.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
				AssertNoNotifications("ZG_TransportChargesMethodOfPaymentInfo for two instruction with EXS", invHeader.ZG_TransportChargesMethodOfPaymentInfo);

				var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction3.CEI_SubStyle = EntrySubStyleList.Codes.A;
				var invoiceLine3 = invHeader.InvoiceLines.AddNew();
				invoiceLine3.JI_CEI = entryInstruction3.PK;
				invHeader.AddInfoValidation.ValidateZG_TransportChargesMethodOfPayment();
				AssertNoNotifications("ZG_TransportChargesMethodOfPaymentInfo for three instruction (EXS, EXS and A)", invHeader.ZG_TransportChargesMethodOfPaymentInfo);
			});
		}

		public void TestCheckJZ_InvoiceAmount()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				var invoiceLine1 = invHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invHeader.JZ_InvoiceAmount = 66.6m;
				AssertNoMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has value for Export", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);

				invHeader.JZ_InvoiceAmount = ZDecimal.Zero;
				AssertHasMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has no value for Export", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
				invHeader.JZ_InvoiceAmount = ZDecimal.Zero;
				AssertNoMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has no value for T2LExpedition", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LClearanceExport);
				invHeader.JZ_InvoiceAmount = ZDecimal.Zero;
				AssertNoMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has no value for T2LClearance", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				var invoiceLine2 = invHeader.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = entryInstruction2.PK;
				invHeader.JZ_InvoiceAmount = 66.6m;
				AssertNoMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has value for Export with more than 1 entry instruction", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);

				invHeader.JZ_InvoiceAmount = ZDecimal.Zero;
				AssertHasMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has no value for Export with more than 1 entry instruction", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		enum GetDeclarationTypeForTesting { Export, T2LExpedition, T2LClearanceExport, EXS, B, C }

		void GetDeclarationForTesting(GetDeclarationTypeForTesting declarationType, ValidationModes validationMode = ValidationModes.None)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invHeader = declaration.Invoices.AddNew();
			invoiceLine = invHeader.InvoiceLines.AddNew();

			switch (declarationType)
			{
				case GetDeclarationTypeForTesting.Export:
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
					break;
				case GetDeclarationTypeForTesting.T2LExpedition:
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
					break;
				case GetDeclarationTypeForTesting.T2LClearanceExport:
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
					break;
				case GetDeclarationTypeForTesting.EXS:
					entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
					break;
				case GetDeclarationTypeForTesting.B:
					entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.B;
					break;
				case GetDeclarationTypeForTesting.C:
					entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.C;
					break;
				default:
					break;
			}

			invoiceLine.JI_CEI = entryInstruction.PK;

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.ValidationMode = validationMode;
		}

		new JobDeclaration declaration;
		JobComInvoiceHeader invHeader;
		JobComInvoiceLine invoiceLine;
		CusEntryInstruction entryInstruction;
	}
}
