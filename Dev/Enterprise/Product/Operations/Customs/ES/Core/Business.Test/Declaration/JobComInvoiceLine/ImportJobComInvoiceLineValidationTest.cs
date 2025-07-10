using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class ImportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		public void TestCheckNoPreviousDocuments()
		{
			var messageWarningText = "If you don't send [40] Previous Documents, the declaration will be a Complete Import Pre-declaration.";
			GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);

			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateAll();
				AssertHasRowWarningContaining(invoiceLine, messageWarningText);

				var previousDoc = invoiceLine.PreviousDocuments.AddNew();
				SetDataForPreviousDocument(previousDoc, "AA", 1);
				invoiceLine.Validation.ValidateAll();
				AssertNoRowWarningContaining(invoiceLine, messageWarningText);

				previousDoc.Delete();
				invoiceLine.Validation.ValidateAll();
				AssertHasRowWarningContaining(invoiceLine, messageWarningText);

				var previousDoc2 = invoiceHeader.PreviousDocuments.AddNew();
				SetDataForPreviousDocument(previousDoc2, "AA", 1);
				invoiceLine.Validation.ValidateAll();
				AssertNoRowWarningContaining(invoiceLine, messageWarningText);

				previousDoc2.Delete();
				invoiceLine.Validation.ValidateAll();
				AssertHasRowWarningContaining(invoiceLine, messageWarningText);

				var previousDoc3 = declaration.PreviousDocuments.AddNew();
				SetDataForPreviousDocument(previousDoc3, "AA", 1);
				invoiceLine.Validation.ValidateAll();
				AssertNoRowWarningContaining(invoiceLine, messageWarningText);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import, ValidationModes.PDI);
				invoiceLine.Validation.ValidateAll();
				AssertNoRowWarningContaining(invoiceLine, messageWarningText);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LReception);
				invoiceLine.Validation.ValidateAll();
				AssertNoRowWarningContaining(invoiceLine, messageWarningText);
			});
		}

		void SetDataForPreviousDocument(PreviousDocument doc, string code, ZShort lineNo)
		{
			doc.CSI_Code = code;
			doc.CSI_DateOfIssue = ZDateTime.BrettsBirthday;
			doc.CSI_LineNo = lineNo;
		}

		public void TestCheckJI_DescriptionImport()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				invoiceLine.JI_Description = "Fish";
				AssertNoMessageErrorContaining("Assert mandatory JI_Description has value for Import", invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_Description = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JI_Description has no value for Import", invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_DescriptionImport_Length()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				invoiceLine.JI_Description = "goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaafbbb";
				invoiceLine.Validation.ValidateAll();
				AssertHasWarningContaining("Warns if JI_Description length is longer than 250 characters for import", invoiceLine.JI_DescriptionInfo, "When sending Import declarations, the maximum length accepted for the description is 250, so it will be trimmed");

				invoiceLine.JI_Description = "goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaf";
				invoiceLine.Validation.ValidateAll();
				AssertNoWarningContaining("No warning if JI_Description length is not longer than 250 characters for import", invoiceLine.JI_DescriptionInfo, "When sending Import declarations, the maximum length accepted for the description is 250, so it will be trimmed");

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LReception);
				invoiceLine.JI_Description = "goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaafbbb";
				invoiceLine.Validation.ValidateAll();
				AssertHasWarningContaining("Warns if JI_Description length is longer than 250 characters for T2LReception", invoiceLine.JI_DescriptionInfo, "When sending T2L Reception declarations, the maximum length accepted for the description is 250, so it will be trimmed");

				invoiceLine.JI_Description = "goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaf";
				invoiceLine.Validation.ValidateAll();
				AssertNoWarningContaining("No warning if JI_Description length is not longer than 250 characters for T2LReception", invoiceLine.JI_DescriptionInfo, "When sending T2L Reception declarations, the maximum length accepted for the description is 250, so it will be trimmed");
			});
		}

		public void TestCheckJI_PrimaryPreference()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import);
				invoiceLine.JI_PrimaryPreference = "100";
				AssertNoMessageErrorContaining("No Message Error when JI_PrimaryPreference is declared for Import", invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_PrimaryPreference = ZString.Empty;
				AssertHasMessageErrorContaining("Message Error when JI_PrimaryPreference is not declared for Import", invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Import, ValidationModes.PDI);
				invoiceLine.JI_PrimaryPreference = ZString.Empty;
				AssertNoMessageErrorContaining("No Message Error when JI_PrimaryPreference is not declared for Import and validation mode is PDI", invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LReception);
				invoiceLine.JI_PrimaryPreference = ZString.Empty;
				AssertNoMessageErrorContaining("No Message Error when JI_PrimaryPreference is not declared for T2L", invoiceLine.JI_PrimaryPreferenceInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_Procedure_Mandatory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Spain, "", "12", "34", "001", "AB DESC 1", "IMP", group: "AAA");
			Factory.Save();

			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LReception);
				entryInstruction.CEI_Style = "AAA";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("No mandatory message error for Import T2L", invoiceLine.JI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageErrorContaining("Mandatory message error for Import non T2L/T2C", invoiceLine.JI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("No mandatory message error for Import T2C", invoiceLine.JI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_CountryOfOrigin()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LReception);
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoMessageErrorContaining("No mandatory message error for Import T2L", invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertHasMessageErrorContaining("Mandatory message error for Import non T2L/T2C", invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoMessageErrorContaining("No mandatory message error for Import T2C", invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		enum GetDeclarationTypeForTesting { Import, T2LReception, T2LClearanceImport }

		void GetDeclarationForTesting(GetDeclarationTypeForTesting declarationType, ValidationModes validationMode = ValidationModes.None)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			switch (declarationType)
			{
				case GetDeclarationTypeForTesting.Import:
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
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

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
