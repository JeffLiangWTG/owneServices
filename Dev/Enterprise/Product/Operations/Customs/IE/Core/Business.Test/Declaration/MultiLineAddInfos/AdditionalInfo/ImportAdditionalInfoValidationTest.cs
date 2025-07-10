using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Testing;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ImportAdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_Code_BR2041_NotOnInvoiceLine()
		{
			var message = "[BR2041] N9001 can only be entered on the invoice line level.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var additionalInfo = instruction.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Code = "n9001";
			var targetInfo = additionalInfo.CSI_CodeInfo;
			AssertHasMessageError(targetInfo, message);

			additionalInfo.CSI_Code = "n9000";
			AssertNoMessageError(targetInfo, message);
		}

		public void TestCheckCSI_Code_BR2041_OnInvoiceLine()
		{
			var procedureMessage = "[BR2041] N9001 can only be declared for the following customs procedures: '4051', '4053', '4054', '4071', '5151', '5153', '5154', '5171', '5353', '7151', '7153', '7171'.";
			var previousDocumentMessage = "[BR2041] N9001 can only be declared when the MRN of the original SAD document is declared at the invoice line level.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Code = Constants.AdditionalInformationCodes.N9001;
			var targetInfo = additionalInfo.CSI_CodeInfo;
			AssertHasMessageError(targetInfo, procedureMessage);
			AssertHasMessageError(targetInfo, previousDocumentMessage);

			var validation = additionalInfo.Validation;
			foreach (var code in new[] { "4051", "4053", "4054", "4071", "5151", "5153", "5154", "5171", "5353", "7151", "7153", "7171" })
			{
				invoiceLine.JI_Procedure = code + "C01";
				validation.ValidateCSI_Code();
				AssertNoMessageError(code, targetInfo, procedureMessage);
			}

			var previousDocument = invoiceLine.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "sad";
			validation.ValidateCSI_Code();
			AssertNoMessageError(targetInfo, previousDocumentMessage);
		}

		public void TestCheckCSI_Code_BR600013()
		{
			var necessaryErrorMessage = "[BR600013] Additional reference 1A06 can only be declared when the declaration dataset is H1.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.FirstOrAddNew();
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var addInfoInstruction = instruction.AdditionalInfos.AddNew();
			var addInfoInvoiceHeader = invHeader.AdditionalInfos.AddNew();
			var addInfoInvoiceLine = invLine.AdditionalInfos.AddNew();

			instruction.CEI_Style = "L1";

			addInfoInstruction.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addInfoInstruction.CSI_Code = "1A06";

			addInfoInvoiceHeader.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addInfoInvoiceHeader.CSI_Code = "1A06";

			addInfoInvoiceLine.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addInfoInvoiceLine.CSI_Code = "1A06";

			CombineAssertions("Test for getting message error for related validation rule", () =>
			{
				AssertHasMessageError("Style is different, subType and Code are correct for Instruction", addInfoInstruction.CSI_CodeInfo, necessaryErrorMessage);
				AssertHasMessageError("Style is same, but subType and Code are wrong for Instruction", addInfoInstruction.CSI_CodeInfo, necessaryErrorMessage);
				AssertHasMessageError("Style is different, subType and Code are wrong also for Instruction", addInfoInstruction.CSI_CodeInfo, necessaryErrorMessage);

				AssertHasMessageError("Style is different, subType and Code are correct for InvoiceHeader", addInfoInvoiceHeader.CSI_CodeInfo, necessaryErrorMessage);
				AssertHasMessageError("Style is same, but subType and Code are wrong for InvoiceHeader", addInfoInvoiceHeader.CSI_CodeInfo, necessaryErrorMessage);
				AssertHasMessageError("Style is different, subType and Code are wrong also for InvoiceHeader", addInfoInvoiceHeader.CSI_CodeInfo, necessaryErrorMessage);

				AssertHasMessageError("Style is different, subType and Code are correct for InvoiceLine", addInfoInvoiceLine.CSI_CodeInfo, necessaryErrorMessage);
				AssertHasMessageError("Style is same, but subType and Code are wrong for InvoiceLine", addInfoInvoiceLine.CSI_CodeInfo, necessaryErrorMessage);
				AssertHasMessageError("Style is different, subType and Code are wrong also for InvoiceLine", addInfoInvoiceLine.CSI_CodeInfo, necessaryErrorMessage);
			});

			instruction.CEI_Style = "H1";

			addInfoInstruction.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addInfoInstruction.CSI_Code = "1A06";

			addInfoInvoiceHeader.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addInfoInvoiceHeader.CSI_Code = "1A06";

			addInfoInvoiceLine.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			addInfoInvoiceLine.CSI_Code = "1A06";

			CombineAssertions("Test for getting no message error for related validation rule", () =>
			{
				AssertNoMessageError("Style is same, subType and Code are correct for Instruction", addInfoInstruction.CSI_CodeInfo, necessaryErrorMessage);
				AssertNoMessageError("Style is same, subType and Code are correct for InvoiceHeader", addInfoInvoiceHeader.CSI_CodeInfo, necessaryErrorMessage);
				AssertNoMessageError("Style is same, subType and Code are correct for InvoiceLine", addInfoInvoiceLine.CSI_CodeInfo, necessaryErrorMessage);
			});
		}

		public void TestCheckCSI_Code_BR1109()
		{
			const string message = "[BR1109] If Requested Procedure is not '76' nor '77', and Transport Mode is 'SEA', then 'N740' or 'N741' Transport Document is not allowed.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invHeader = declaration.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			invLine.JI_CEI = instruction.PK;
			var addInfoInstruction = instruction.AdditionalInfos.AddNew();
			var addInfoInvoiceHeader = invHeader.AdditionalInfos.AddNew();
			var addInfoInvoiceLine = invLine.AdditionalInfos.AddNew();
			var addInfoInstructionValidation = addInfoInstruction.Validation;
			var addInfoInvoiceHeaderValidation = addInfoInvoiceHeader.Validation;
			var addInfoInvoiceLineValidation = addInfoInvoiceLine.Validation;
			
			AssertCSI_Code_BR1109(SupportingDocumentCodes._N740);
			AssertCSI_Code_BR1109(SupportingDocumentCodes._N741);
			
			void AssertCSI_Code_BR1109(string supportingDocumentCode)
			{
				addInfoInstruction.CSI_Code = supportingDocumentCode;
				addInfoInstruction.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfoInvoiceHeader.CSI_Code = supportingDocumentCode;
				addInfoInvoiceHeader.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				addInfoInvoiceLine.CSI_Code = supportingDocumentCode;
				addInfoInvoiceLine.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

				foreach (var procedureCode in new[] { UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76, UniversalReferenceConstants.ProcedureCodes.ProcedureCode._77 })
				{
					invLine.JI_Procedure = procedureCode;
					addInfoInstructionValidation.ValidateCSI_Code();
					addInfoInvoiceHeaderValidation.ValidateCSI_Code();
					addInfoInvoiceLineValidation.ValidateCSI_Code();
					AssertNoMessageError($"When addInfoInstruction ProcedureCode:{procedureCode},TransportMode:Sea, CSI_SubType:{AdditionalInfoSubTypeList.Codes.TransportDocument}, CSI_Code:{supportingDocumentCode}", addInfoInstruction.CSI_CodeInfo, message);
					AssertNoMessageError($"When addInfoInvoiceHeader ProcedureCode:{procedureCode},TransportMode:Sea, CSI_SubType:{AdditionalInfoSubTypeList.Codes.TransportDocument}, CSI_Code:{supportingDocumentCode}", addInfoInvoiceHeader.CSI_CodeInfo, message);
					AssertNoMessageError($"When addInfoInvoiceLine ProcedureCode:{procedureCode},TransportMode:Sea, CSI_SubType:{AdditionalInfoSubTypeList.Codes.TransportDocument}, CSI_Code:{supportingDocumentCode}", addInfoInvoiceLine.CSI_CodeInfo, message);
				}

				invLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._78;
				addInfoInstructionValidation.ValidateCSI_Code();
				addInfoInvoiceHeaderValidation.ValidateCSI_Code();
				addInfoInvoiceLineValidation.ValidateCSI_Code();
				CombineAssertions(() =>
				{
					AssertHasMessageError($"When addInfoInstruction ProcedureCode:{UniversalReferenceConstants.ProcedureCodes.ProcedureCode._78},TransportMode:Sea, CSI_SubType:{AdditionalInfoSubTypeList.Codes.TransportDocument}, CSI_Code:{supportingDocumentCode}", addInfoInstruction.CSI_CodeInfo, message);
					AssertHasMessageError($"When addInfoInvoiceHeader ProcedureCode:{UniversalReferenceConstants.ProcedureCodes.ProcedureCode._78},TransportMode:Sea, CSI_SubType:{AdditionalInfoSubTypeList.Codes.TransportDocument}, CSI_Code:{supportingDocumentCode}", addInfoInvoiceHeader.CSI_CodeInfo, message);
					AssertHasMessageError($"When addInfoInvoiceLine ProcedureCode:{UniversalReferenceConstants.ProcedureCodes.ProcedureCode._78},TransportMode:Sea, CSI_SubType:{AdditionalInfoSubTypeList.Codes.TransportDocument}, CSI_Code:{supportingDocumentCode}", addInfoInvoiceLine.CSI_CodeInfo, message);
				});

				foreach (var code in new[] { SupportingDocumentCodes._N864, SupportingDocumentCodes._N853 })
				{
					addInfoInstruction.CSI_Code = code;
					addInfoInvoiceHeader.CSI_Code = code;
					addInfoInvoiceLine.CSI_Code = code;
					AssertNoMessageError($"When addInfoInstruction ProcedureCode:{UniversalReferenceConstants.ProcedureCodes.ProcedureCode._78},TransportMode:Sea, CSI_SubType:{AdditionalInfoSubTypeList.Codes.TransportDocument}, CSI_Code:{code}", addInfoInstruction.CSI_CodeInfo, message);
					AssertNoMessageError($"When addInfoInvoiceHeader ProcedureCode:{UniversalReferenceConstants.ProcedureCodes.ProcedureCode._78},TransportMode:Sea, CSI_SubType:{AdditionalInfoSubTypeList.Codes.TransportDocument}, CSI_Code:{code}", addInfoInvoiceHeader.CSI_CodeInfo, message);
					AssertNoMessageError($"When addInfoInvoiceLine ProcedureCode:{UniversalReferenceConstants.ProcedureCodes.ProcedureCode._78},TransportMode:Sea, CSI_SubType:{AdditionalInfoSubTypeList.Codes.TransportDocument}, CSI_Code:{code}", addInfoInvoiceLine.CSI_CodeInfo, message);
				}

				declaration.JE_TransportMode = TransportTypeList.Codes.Air;
				addInfoInstruction.CSI_Code = supportingDocumentCode;
				addInfoInvoiceHeader.CSI_Code = supportingDocumentCode;
				addInfoInvoiceLine.CSI_Code = supportingDocumentCode;
				AssertNoMessageError($"When addInfoInstruction ProcedureCode:{UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76},TransportMode:Air, CSI_SubType:{AdditionalInfoSubTypeList.Codes.TransportDocument}, CSI_Code:{supportingDocumentCode}", addInfoInstruction.CSI_CodeInfo, message);
				AssertNoMessageError($"When addInfoInvoiceHeader ProcedureCode:{UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76},TransportMode:Air, CSI_SubType:{AdditionalInfoSubTypeList.Codes.TransportDocument}, CSI_Code:{supportingDocumentCode}", addInfoInvoiceHeader.CSI_CodeInfo, message);
				AssertNoMessageError($"When addInfoInvoiceLine ProcedureCode:{UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76},TransportMode:Air, CSI_SubType:{AdditionalInfoSubTypeList.Codes.TransportDocument}, CSI_Code:{supportingDocumentCode}", addInfoInvoiceLine.CSI_CodeInfo, message);

				declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
				addInfoInstruction.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				addInfoInvoiceHeader.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				addInfoInvoiceLine.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				addInfoInstructionValidation.ValidateCSI_Code();
				addInfoInvoiceHeaderValidation.ValidateCSI_Code();
				addInfoInvoiceLineValidation.ValidateCSI_Code();
				AssertNoMessageError($"When addInfoInstruction ProcedureCode:{UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76},TransportMode:Sea, CSI_SubType:{AdditionalInfoSubTypeList.Codes.AdditionalInformation}, CSI_Code:{supportingDocumentCode}", addInfoInstruction.CSI_CodeInfo, message);
				AssertNoMessageError($"When addInfoInvoiceHeader ProcedureCode:{UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76},TransportMode:Sea, CSI_SubType:{AdditionalInfoSubTypeList.Codes.AdditionalInformation}, CSI_Code:{supportingDocumentCode}", addInfoInvoiceHeader.CSI_CodeInfo, message);
				AssertNoMessageError($"When addInfoInvoiceLine ProcedureCode:{UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76},TransportMode:Sea, CSI_SubType:{AdditionalInfoSubTypeList.Codes.AdditionalInformation}, CSI_Code:{supportingDocumentCode}", addInfoInvoiceLine.CSI_CodeInfo, message);
			}
		}

		public void TestValidateCSI_Code_BR5153_InvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew().JI_CEI = instruction.PK;
			var addInfoInvoiceHeader = invoiceHeader.AdditionalInfos.AddNew();
			var addInfoInvoiceHeaderValidation = addInfoInvoiceHeader.Validation;

			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._51;
			addInfoInvoiceHeaderValidation.ValidateCSI_Code();
			var targetInfo = addInfoInvoiceHeader.CSI_CodeInfo;

			CombineAssertions("CSI_Code_BR5153", () =>
			{
				addInfoInvoiceHeader.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				addInfoInvoiceHeaderValidation.ValidateCSI_Code();
				AssertNoMessageError("BR5153: pass for single INF.", targetInfo, MessageError_BR5153);

				addInfoInvoiceHeader.CSI_Code = AdditionalInformationCodes._00100;
				AssertNoMessageError("BR5153: pass for single INF 00100.", targetInfo, MessageError_BR5153);

				instruction.SupportingDocuments.AddNew(SupportingDocumentCodes.AuthorisationInwardProcessingProcedure, string.Empty);
				addInfoInvoiceHeaderValidation.ValidateCSI_Code();
				AssertHasMessageError("BR5153: warn for INF 00100 if C601 exists.", targetInfo, MessageError_BR5153);

				addInfoInvoiceHeader.CSI_Code = AdditionalReferenceCodes.RoRoShipID;
				AssertNoMessageError("BR5153: invalid for non-00100 INF.", targetInfo, MessageError_BR5153);
				addInfoInvoiceHeader.CSI_Code = AdditionalInformationCodes._00100;

				instruction.SupportingDocuments.RemoveAndDeleteAll();

				invoiceHeader.SupportingDocuments.AddNew(SupportingDocumentCodes.AuthorisationInwardProcessingProcedure, string.Empty);
				addInfoInvoiceHeaderValidation.ValidateCSI_Code();
				AssertHasMessageError("BR5153: warn for INF 00100 if C601 exists on JZ.", targetInfo, MessageError_BR5153);

				addInfoInvoiceHeader.CSI_Code = AdditionalReferenceCodes.Prefix_6;
				AssertNoMessageError("BR5153: invalid with non-00100 add-info.", targetInfo, MessageError_BR5153);
				addInfoInvoiceHeader.CSI_Code = AdditionalInformationCodes._00100;
				AssertHasMessageError("BR5153: to make sure the validation is valid back when CSI_Code back to 00100.", targetInfo, MessageError_BR5153);

				invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._42;
				addInfoInvoiceHeaderValidation.ValidateCSI_Code();
				AssertNoMessageError("BR5153: invalid with non-51 JI_Procedure.", targetInfo, MessageError_BR5153);
			});
		}

		const string MessageError_BR5153 = "[BR5153] If Requested Procedure is '51', '00100' Additional Information should not be declared when there is a 'C601' Supporting Document entered under the Entry Instructions > Supporting Documents tab or the Invoice Headers > Supporting Documents tab.";
	}
}
