using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class ImportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		protected override BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			return invoice;
		}

		protected override Type GetTypeForTest()
		{
			return typeof(ImportJobComInvoiceHeaderValidation);
		}

		public void TestCheckJZ_IncoTerm()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				invHeader.JZ_IncoTerm = "EXW";
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has value for Import", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);

				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for Import", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LReception);
				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for T2LReception", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LClearanceImport);
				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for T2LClearance", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				invHeader.JZ_IncoTerm = "EXW";
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTerm has value for Import with more than 1 entry instruction", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);

				invHeader.JZ_IncoTerm = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for Import with more than 1 entry instruction", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckJZ_IncoTerm_ForH2()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				invHeader.Validation.ValidateJZ_IncoTerm();
				AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for Import without H2", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.ImportWithH2);
				invHeader.JZ_IncoTerm = "";
				invHeader.Validation.ValidateJZ_IncoTerm();
				AssertNoMessageErrorContaining("Assert not mandatory JZ_IncoTerm has not value for Import with H2", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.ImportWithH2);
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				var invoiceLine1 = invHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction2.PK;
				invHeader.Validation.ValidateJZ_IncoTerm();
				AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTerm has no value for Import with two entry instruction, one and H2 and other without", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);

				invHeader.JZ_IncoTerm = "EXW";
				invHeader.Validation.ValidateJZ_IncoTerm();
				AssertNoMessageErrorContaining("Assert not mandatory JZ_IncoTerm has value for Import with two entry instruction, one and H2 and other without", invHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckJZ_IncoTermPlace()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				invHeader.JZ_IncoTermPlace = "Madrid";
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTermPlace has value for Import", invHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTermPlace has no value for Import", invHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import, ValidationModes.PDI);
				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTermPlace has no value for Import with validationMode PDI or PDS", invHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LReception);
				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTermPlace has no value for T2LReception", invHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LClearanceImport);
				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTermPlace has no value for T2LClearance", invHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				invHeader.JZ_IncoTermPlace = "Madrid";
				AssertNoMessageErrorContaining("Assert mandatory JZ_IncoTermPlace has value for Import with more than 1 entry instruction", invHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				invHeader.JZ_IncoTermPlace = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTermPlace has no value for Import with more than 1 entry instruction", invHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJZ_IncoTermPlace_ForH2()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				invHeader.JZ_IncoTermPlace = ZString.Empty;
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTermPlace has no value for Import without H2 entry instruction", invHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.ImportWithH2);
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertNoMessageErrorContaining("Assert not mandatory JZ_IncoTermPlace has no value for Import with H2 entry instruction", invHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				var invoiceLine1 = invHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction2.PK;
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertHasMessageErrorContaining("Assert mandatory JZ_IncoTermPlace has no value for Import with two entry instruction, one with H2 and other without", invHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);

				invHeader.JZ_IncoTermPlace = "Madrid";
				invHeader.Validation.ValidateJZ_IncoTermPlace();
				AssertNoMessageErrorContaining("Assert not mandatory JZ_IncoTermPlace has value for Import with two entry instruction, one with H2 and other without", invHeader.JZ_IncoTermPlaceInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJZ_ValuationCode_ForH2()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				invHeader.JZ_ValuationCode = ZString.Empty;
				invHeader.Validation.ValidateJZ_ValuationCode();
				AssertHasMessageErrorContaining("Assert mandatory JZ_ValuationCode has no value for Import without H2 entry instruction", invHeader.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.ImportWithH2);
				invHeader.Validation.ValidateJZ_ValuationCode();
				AssertNoMessageErrorContaining("Assert not mandatory JZ_ValuationCode has no value for Import with H2 entry instruction", invHeader.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				var invoiceLine1 = invHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction2.PK;
				invHeader.Validation.ValidateJZ_ValuationCode();
				AssertHasMessageErrorContaining("Assert mandatory JZ_ValuationCode has no value for Import with two entry instruction, one with H2 and other without", invHeader.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				invHeader.JZ_ValuationCode = "AC";
				invHeader.Validation.ValidateJZ_ValuationCode();
				AssertNoMessageErrorContaining("Assert not mandatory JZ_ValuationCode has value for Import with two entry instruction, one with H2 and other without", invHeader.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJZ_ValuationCode_ForT2C()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				var header = declaration.Invoices.AddNew();
				header.JZ_ValuationCode = "AH";
				AssertNoMessageErrorContaining("No error for no empty", header.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				header.JZ_ValuationCode = ZString.Empty;
				AssertHasMessageErrorContaining("No error for 0 entry instructions", header.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var invoiceLine1 = header.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = instruction.PK;

				instruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				header.Validation.ValidateJZ_ValuationCode();
				AssertHasMessageErrorContaining("Error for not T2C entry instructions", header.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				header.Validation.ValidateJZ_ValuationCode();
				AssertNoMessageErrorContaining("No error for T2C entry instructions", header.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				var instruction2 = declaration.CustomsEntryInstructions.AddNew();
				instruction2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				header.Validation.ValidateJZ_ValuationCode();
				AssertNoMessageErrorContaining("No error for A entry instruction not associated", header.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine1.JI_CEI = instruction2.PK;
				header.Validation.ValidateJZ_ValuationCode();
				AssertHasMessageErrorContaining("Error for not all entry instructions equals T2C", header.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);

				instruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				header.Validation.ValidateJZ_ValuationCode();
				AssertNoMessageErrorContaining("No error for all entry instructions equals T2C", header.JZ_ValuationCodeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJZ_InvoiceAmount()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				invHeader.JZ_InvoiceAmount = 22.2m;
				AssertNoMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has value for Import", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);

				invHeader.JZ_InvoiceAmount = ZDecimal.Zero;
				AssertHasMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has no value for Import", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import, ValidationModes.PDS);
				invHeader.JZ_InvoiceAmount = ZDecimal.Zero;
				AssertNoMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has value for Import with validationMode PDI or PDS", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LReception);
				invHeader.JZ_InvoiceAmount = ZDecimal.Zero;
				AssertNoMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has no value for T2LReception", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LClearanceImport);
				invHeader.JZ_InvoiceAmount = ZDecimal.Zero;
				AssertNoMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has no value for T2LClearance", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				invHeader.JZ_InvoiceAmount = 22.2m;
				AssertNoMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has value for Import with more than 1 entry instruction", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);

				invHeader.JZ_InvoiceAmount = ZDecimal.Zero;
				AssertHasMessageErrorContaining("Assert mandatory JZ_InvoiceAmount has no value for Import with more than 1 entry instruction", invHeader.JZ_InvoiceAmountInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJZ_OH_Supplier_Mandatory()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				invHeader.JZ_OH_Supplier = org.PK;
				AssertNoMessageErrorContaining("No Message Error when JZ_OH_Supplier is declared for Import", invHeader.JZ_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

				invHeader.JZ_OH_Supplier = ZGuid.Empty;
				AssertHasMessageErrorContaining("Message Error when JZ_OH_Supplier is not declared for Import", invHeader.JZ_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import, ValidationModes.PDI);
				invHeader.JZ_OH_Supplier = ZGuid.Empty;
				AssertNoMessageErrorContaining("No Message Error when JZ_OH_Supplier is declared for Import with validationMode PDI or PDS", invHeader.JZ_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LReception);
				invHeader.JZ_OH_Supplier = ZGuid.Empty;
				AssertNoMessageErrorContaining("No Message Error when JZ_OH_Supplier is not declared for T2L", invHeader.JZ_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LClearanceImport);
				invHeader.JZ_OH_Supplier = ZGuid.Empty;
				AssertNoMessageErrorContaining("No Message Error when JZ_OH_Supplier is not declared for T2C", invHeader.JZ_OH_SupplierInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestValidateHasAtLeastOneTransportSupportingDocument()
		{
			GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
			ImportJobComInvoiceHeaderValidation invoiceValidation = (ImportJobComInvoiceHeaderValidation)invHeader.Validation;
			var transportSupportingDocumentTypes = invHeader.SupportingDocuments.Helper.GetInvoiceTransportSupportingDocumentTypes();
			var expectedMessage = invoiceValidation.AtLeastOneTransportSupportingDocumentMessage + ZString.Join(", ", transportSupportingDocumentTypes.ToArray());
			var notifications = invHeader.Notifications.GetNotifications(CargoWise.EntityFramework.NotificationType.Warning);
			CombineAssertions(() =>
			{
				invoiceValidation.ValidateNeedAtLeastOneTransportSupportingDocument();
				AssertEquals("The count of AtLeastOneTransportSupportingDocumentMessage", 1, notifications.Count(x => x.Message.Contains(invoiceValidation.AtLeastOneTransportSupportingDocumentMessage)));
				AssertEquals("Invoice.Notifications should have transport sup doc validation for import when no sup docs are declared", true, notifications.Any(x => x.Message.Contains(expectedMessage)));
				
				var supportingDocument = invHeader.SupportingDocuments.AddNew();
				AssertValidateHasAtLeastOneTransportSupportingDocument(invoiceValidation, supportingDocument, expectedMessage, nameof(invHeader));

				invHeader.SupportingDocuments.RemoveAndDeleteAll();
				supportingDocument = entryInstruction.SupportingDocuments.AddNew();
				AssertValidateHasAtLeastOneTransportSupportingDocument(invoiceValidation, supportingDocument, expectedMessage, nameof(entryInstruction));

				entryInstruction.SupportingDocuments.RemoveAndDeleteAll();
				supportingDocument = invoiceLine.SupportingDocuments.AddNew();
				AssertValidateHasAtLeastOneTransportSupportingDocument(invoiceValidation, supportingDocument, expectedMessage, nameof(invoiceLine));

				invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
				supportingDocument = declaration.SupportingDocuments.AddNew();
				AssertValidateHasAtLeastOneTransportSupportingDocument(invoiceValidation, supportingDocument, expectedMessage, nameof(declaration));

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LReception);
				notifications = invHeader.Notifications.GetNotifications(CargoWise.EntityFramework.NotificationType.Warning);
				((ImportJobComInvoiceHeaderValidation)invHeader.Validation).ValidateNeedAtLeastOneTransportSupportingDocument();
				AssertEquals("Invoice.Notifications should not have transport sup doc validation for t2l reception", false, notifications.Any(x => x.Message.Contains(expectedMessage)));

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LClearanceImport);
				notifications = invHeader.Notifications.GetNotifications(CargoWise.EntityFramework.NotificationType.Warning);
				((ImportJobComInvoiceHeaderValidation)invHeader.Validation).ValidateNeedAtLeastOneTransportSupportingDocument();
				AssertEquals("Invoice.Notifications should not have transport sup doc validation for t2c import", false, notifications.Any(x => x.Message.Contains(expectedMessage)));

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import, ValidationModes.PDI);
				notifications = invHeader.Notifications.GetNotifications(CargoWise.EntityFramework.NotificationType.Warning);
				((ImportJobComInvoiceHeaderValidation)invHeader.Validation).ValidateNeedAtLeastOneTransportSupportingDocument();
				AssertEquals("Invoice.Notifications should not have transport sup doc validation for import with validation mode PDI", false, notifications.Any(x => x.Message.Contains(expectedMessage)));
			});

			void AssertValidateHasAtLeastOneTransportSupportingDocument(ImportJobComInvoiceHeaderValidation invoiceValidation, SupportingDocument supportingDocument, string expectedMessage, string type)
			{
				var notifications = invHeader.Notifications.GetNotifications(CargoWise.EntityFramework.NotificationType.Warning);

				supportingDocument.CSI_Code = "AAA";
				invoiceValidation.ValidateNeedAtLeastOneTransportSupportingDocument();
				AssertEquals($"{type}: Invoice.Notifications should have transport sup doc validation for import when wrong sup doc declared", true, notifications.Any(x => x.Message.Contains(expectedMessage)));

				foreach (var documentType in transportSupportingDocumentTypes)
				{
					supportingDocument.CSI_Code = documentType;
					invoiceValidation.ValidateNeedAtLeastOneTransportSupportingDocument();
					AssertEquals($"{type}: Invoice.Notifications should not have transport sup doc validation for import when correct sup doc declared", false, notifications.Any(x => x.Message.Contains(expectedMessage)));
				}

				supportingDocument.CSI_Code = "AAA";
				invHeader.RunPreSaveValidation();
				AssertEquals($"{type}: Invoice.Notifications should have transport sup doc validation for import when wrong sup doc declared (running the validation)", true, notifications.Any(x => x.Message.Contains(expectedMessage)));
			}
		}

		enum GetDeclarationTypeForTesting { Import, ImportWithH2, T2LReception, T2LClearanceImport }

		void GetDeclarationForTesting(GetDeclarationTypeForTesting declarationType, ValidationModes validationMode = ValidationModes.None)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invHeader = declaration.Invoices.AddNew();
			invoiceLine = invHeader.InvoiceLines.AddNew();

			switch (declarationType)
			{
				case GetDeclarationTypeForTesting.Import:
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
					invoiceLine.JI_CEI = entryInstruction.PK;
					break;
				case GetDeclarationTypeForTesting.ImportWithH2:
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
					entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
					invoiceLine.JI_CEI = entryInstruction.PK;
					break;
				case GetDeclarationTypeForTesting.T2LReception:
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
					invoiceLine.JI_CEI = entryInstruction.PK;
					break;
				case GetDeclarationTypeForTesting.T2LClearanceImport:
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
					invoiceLine.JI_CEI = entryInstruction.PK;
					break;
				default:
					break;
			}

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
