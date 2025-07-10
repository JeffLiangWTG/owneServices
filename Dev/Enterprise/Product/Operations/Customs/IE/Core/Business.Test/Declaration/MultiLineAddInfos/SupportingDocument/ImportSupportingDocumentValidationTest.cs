using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IE.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportSupportingDocumentValidation))]
	class ImportSupportingDocumentValidationTest : SupportingDocumentValidationAbstractTest
	{
		public void TestCheckReferenceNumber_BR20326()
		{
			AssertInvoiceHeaderReferenceNumber_BR20326();
			AssertInvoiceLineReferenceNumber_BR20326();
			AssertEntryInstructionReferenceNumber_BR20326();
		}

		void AssertInvoiceHeaderReferenceNumber_BR20326()
		{
			var supportingDoc = declaration.Invoices.AddNew().SupportingDocuments.AddNew();
			AssertReferenceNumber_BR20326(supportingDoc);
		}

		void AssertInvoiceLineReferenceNumber_BR20326()
		{
			_ = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertReferenceNumber_BR20326(supportingDocument);
		}

		void AssertEntryInstructionReferenceNumber_BR20326()
		{
			var supportingDoc = declaration.CustomsEntryInstructions.AddNew().SupportingDocuments.AddNew();
			AssertReferenceNumber_BR20326(supportingDoc);
		}

		void AssertReferenceNumber_BR20326(SupportingDocument supDoc)
		{
			var errorMessage = "[BR20326] The first 2 characters of a 'C501', 'C502', or 'C503' Supporting Document Reference Number must be a valid country code";

			supDoc.CSI_Code = "C501";
			supDoc.CSI_ReferenceNumber = "WA123";

			CombineAssertions(() =>
			{
				AssertHasMessageError(supDoc.CSI_ReferenceNumberInfo, errorMessage);
				supDoc.CSI_ReferenceNumber = "IE123";
				AssertNoMessageError(supDoc.CSI_ReferenceNumberInfo, errorMessage);
			});
		}

		public void TestCheckCSI_Code_BR20312()
		{
			var message = "[BR20312] Supporting Document Type 'U164', 'U165', 'U166' and 'U167' are mutually exclusive, i.e. only one of them is allowed per item. The exception is the combination of 'U165' and 'U167', which is allowed.";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var supportingDoc = entryInstruction.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = "U164";
			var supportingDoc2 = entryInstruction.SupportingDocuments.AddNew();
			supportingDoc2.CSI_Code = "U165";
			AssertHasMessageError("U164 and U165 are muatually exclusive", supportingDoc2.CSI_CodeInfo, message);
			supportingDoc.CSI_Code = "U167";
			var validation = supportingDoc2.Validation;
			validation.ValidateCSI_Code();
			AssertNoMessageError("U165 and U167 are not muatually exclusive", supportingDoc2.CSI_CodeInfo, message);

			var invoiceHeader = declaration.Invoices.AddNew();
			var supportingDoc3 = invoiceHeader.SupportingDocuments.AddNew();
			supportingDoc3.CSI_Code = "U164";
			var supportingDoc4 = invoiceHeader.SupportingDocuments.AddNew();
			supportingDoc4.CSI_Code = "U165";
			AssertHasMessageError("U164 and U165 are muatually exclusive", supportingDoc4.CSI_CodeInfo, message);
			supportingDoc3.CSI_Code = "U167";
			var validation2 = supportingDoc4.Validation;
			validation2.ValidateCSI_Code();
			AssertNoMessageError("U165 and U167 are not muatually exclusive", supportingDoc4.CSI_CodeInfo, message);

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var supportingDoc_InvoiceLine = invoiceLine.SupportingDocuments.AddNew();
			supportingDoc_InvoiceLine.CSI_Code = "U164";
			var supportingDoc2_InvoiceLine = invoiceLine.SupportingDocuments.AddNew();
			supportingDoc2_InvoiceLine.CSI_Code = "U165";
			AssertHasMessageError("U164 and U165 are muatually exclusive", supportingDoc2_InvoiceLine.CSI_CodeInfo, message);
			supportingDoc_InvoiceLine.CSI_Code = "U167";
			var validation3 = supportingDoc2_InvoiceLine.Validation;
			validation3.ValidateCSI_Code();
			AssertNoMessageError("U165 and U167 are not muatually exclusive", supportingDoc2_InvoiceLine.CSI_CodeInfo, message);
		}

		public void TestCheckCSI_Code_BR5149()
		{
			var errorMessage = "[BR5149] 'N990' or 'C990' Supporting Document can only be declared when Requested Procedure is '44'.";
			var instruction = declaration.CustomsEntryInstructions.FirstOrAddNew();

			var invoice = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>();
			var invoiceLine = invoice.InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			supportingDocument = invoice.SupportingDocuments.AddNew();
			AssertBR5149(supportingDocument, "Invoice Supporting Document");
			supportingDocument.Delete();

			supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			AssertBR5149(supportingDocument, "Invoice Line Supporting Document");
			supportingDocument.Delete();

			void AssertBR5149(SupportingDocument supportingDocument, string message)
			{
				AssertBR5149(supportingDocument, SupportingDocumentCodes._C990);
				AssertBR5149(supportingDocument, SupportingDocumentCodes._N990);

				void AssertBR5149(SupportingDocument supportingDocument, ZString code)
				{
					CombineAssertions($"{message}, Code: {code}", () =>
					{
						supportingDocument.CSI_Code = code;
						invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._76;
						supportingDocument.Validation.ValidateCSI_Code();
						AssertHasMessageError("Procedure is not 44", supportingDocument.CSI_CodeInfo, errorMessage);
						invoiceLine.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._44;
						supportingDocument.Validation.ValidateCSI_Code();
						AssertNoMessageError("Procedure is 44", supportingDocument.CSI_CodeInfo, errorMessage);
					});
				}
			}
		}

		public void TestCheckReferenceNumber_BR20329()
		{
			AssertInvoiceHeaderReferenceNumber_BR20329();
			AssertInvoiceLineReferenceNumber_BR20329();
			AssertEntryInstructionReferenceNumber_BR20329();
		}

		void AssertInvoiceHeaderReferenceNumber_BR20329()
		{
			var supportingDoc = declaration.Invoices.AddNew().SupportingDocuments.AddNew();
			AssertReferenceNumber_BR20329(supportingDoc);
		}

		void AssertInvoiceLineReferenceNumber_BR20329()
		{
			var supportingDoc = declaration.Invoices.AddNew().InvoiceLines.AddNew().SupportingDocuments.AddNew();
			AssertReferenceNumber_BR20329(supportingDoc);
		}

		void AssertEntryInstructionReferenceNumber_BR20329()
		{
			var supportingDoc = declaration.CustomsEntryInstructions.AddNew().SupportingDocuments.AddNew();
			AssertReferenceNumber_BR20329(supportingDoc);
		}

		void AssertReferenceNumber_BR20329(SupportingDocument supDoc)
		{
			var info = supDoc.CSI_ReferenceNumberInfo;
			AssertReferenceNumberBR20329_C644();
			AssertReferenceNumberBR20329_L100();
			AssertReferenceNumberBR20329_C085_C678_N853_C640();
			AssertReferenceNumberBR20329_C057_C082_C079();
			AssertReferenceNumberBR20329_Y120_Y123_Y124_Y125_Y951_Y986();

			void AssertReferenceNumberBR20329_C644()
			{
				var errorMessage = "[BR20329] The format for a COI Certificate Number should be either COI.XX.YYYY.nnnnnnn or COI.XX.YYYY.nnnnnnn/mm, where XX is the country code of issuance, YYYY is the year, nnnnnnn is the 7-digit number, and mm is the 2-digit extract number.";
				supDoc.CSI_Code = SupportingDocumentCodes._C644;

				supDoc.CSI_ReferenceNumber = "COI.AU.2023.1234657";
				AssertNoMessageError(info, errorMessage);

				supDoc.CSI_ReferenceNumber = "COI.AU.2023.1234567/89";
				AssertNoMessageError(info, errorMessage);

				supDoc.CSI_ReferenceNumber = "XXX.AU.2023.1234567/89";
				AssertHasMessageError("Specific code is invalid", info, errorMessage);

				supDoc.CSI_ReferenceNumber = "COI.XX.2023.1234567/89";
				AssertHasMessageError("Country code is invalid", info, errorMessage);

				supDoc.CSI_ReferenceNumber = "COI.AU.2XX3.1234567/89";
				AssertHasMessageError("Year is invalid", info, errorMessage);

				supDoc.CSI_ReferenceNumber = "COI.AU.2023.123X567/89";
				AssertHasMessageError("Sequence number is invalid", info, errorMessage);

				supDoc.CSI_ReferenceNumber = "COI.AU.2023.12345678/89";
				AssertHasMessageError("Sequence number is invalid", info, errorMessage);

				supDoc.CSI_ReferenceNumber = "COI.AU.2023.1234567/X9";
				AssertHasMessageError("Extract number is invalid", info, errorMessage);
			}

			void AssertReferenceNumberBR20329_L100()
			{
				var errorMessage = "[BR20329] The format for a ODS License should be ZZZ-XXXX-KKKK-YYYY-NNNNNNNN, where ZZZ is 'EXP' or 'IMP', XXXX is the ID composed of 2 letters followed by a 2-digit number, KKKK is the license type, YYYY is the license year, NNNNNNNN is the 8-digit number.";
				supDoc.CSI_Code = SupportingDocumentCodes._L100;

				supDoc.CSI_ReferenceNumber = "IMP-AA01-ICUH-2023-12345678";
				AssertNoMessageError(info, errorMessage);

				supDoc.CSI_ReferenceNumber = "EXP-AA01-EHCO-2023-12345678";
				AssertNoMessageError(info, errorMessage);

				supDoc.CSI_ReferenceNumber = "XXX-AA01-ICUH-2023-12345678";
				AssertHasMessageError("Specific code is invalid", info, errorMessage);

				supDoc.CSI_ReferenceNumber = "IMP-AXX1-ICUH-2023-12345678";
				AssertHasMessageError("ID is invalid", info, errorMessage);

				supDoc.CSI_ReferenceNumber = "IMP-AA01-XXXX-2023-12345678";
				AssertHasMessageError("license is invalid", info, errorMessage);

				supDoc.CSI_ReferenceNumber = "IMP-AA01-ICUH-2XX3-12345678";
				AssertHasMessageError("Year is invalid", info, errorMessage);

				supDoc.CSI_ReferenceNumber = "IMP-AA01-ICUH-2023-1234X678";
				AssertHasMessageError("Sequence number is invalid", info, errorMessage);
			}

			void AssertReferenceNumberBR20329_C085_C678_N853_C640()
			{
				var codeMessagePairs = new Tuple<string, string>[]
				{
					Tuple.Create(SupportingDocumentCodes._C085, "[BR20329] The format for a CHED_PP Certificate should be either CHEDPP.XX.YYYY.nnnnnnn or CHEDPP.XX.YYYY.nnnnnnnR or CHEDPP.XX.YYYY.nnnnnnnV, where XX is the country code of issuance, YYYY is the year, nnnnnnn is the 7-digit number, can end with 'R' is partially Rejected, 'V' is partially Validated."),
					Tuple.Create(SupportingDocumentCodes._C678, "[BR20329] The format for a CHED_D Certificate should be either CHEDD.XX.YYYY.nnnnnnn or CHEDD.XX.YYYY.nnnnnnnR or CHEDD.XX.YYYY.nnnnnnnV, where XX is the country code of issuance, YYYY is the year, nnnnnnnn is the 7-digit number, can end with 'R' is partially Rejected, 'V' is partially Validated."),
					Tuple.Create(SupportingDocumentCodes._N853, "[BR20329] The format for a CHED_P/CVED_P Certificate should be either CHEDP/CVEDP.XX.YYYY.nnnnnnnR or CHEDP/CVEDP.XX.YYYY.nnnnnnnV, CHEDP/CVEDP.XX.YYYY.nnnnnnn where XX is the country code of issuance, YYYY is the year, nnnnnnn is the 7-digit number, can end with 'R' is partially Rejected, 'V' is partially Validated."),
					Tuple.Create(SupportingDocumentCodes._C640, "[BR20329] The format for a CHED_A Certificate should be either CHEDA.XX.YYYY.nnnnnnn or CHEDA.XX.YYYY.nnnnnnnR or CHEDA.XX.YYYY.nnnnnnnV, where XX is the country code of issuance, YYYY is the year, nnnnnnn is the 7-digit number, can end with 'R' is partially Rejected, 'V' is partially Validated."),
				};

				foreach (var codeMessagePair in codeMessagePairs)
				{
					supDoc.CSI_Code = codeMessagePair.Item1;
					var errorMessage = codeMessagePair.Item2;

					var targetSpecificCodes = SupportingDocumentCodes.BR20239SpecificCode[supDoc.CSI_Code];

					supDoc.CSI_ReferenceNumber = $"{targetSpecificCodes.FirstOrDefault()}.US.2023.1234567V";
					AssertNoMessageError(info, errorMessage);

					supDoc.CSI_ReferenceNumber = "CHEDX.US.2023.1234567V";
					AssertHasMessageError("Specific code is invalid", info, errorMessage);

					supDoc.CSI_ReferenceNumber = $"{targetSpecificCodes.FirstOrDefault()}.XX.2023.1234567V";
					AssertHasMessageError("Country is invalid", info, errorMessage);

					supDoc.CSI_ReferenceNumber = $"{targetSpecificCodes.FirstOrDefault()}.US.20213.1234567V";
					AssertHasMessageError("Year is invalid", info, errorMessage);

					supDoc.CSI_ReferenceNumber = $"{targetSpecificCodes.FirstOrDefault()}.US.2023.1234567X";
					AssertHasMessageError("Sequence number is invalid", info, errorMessage);

					supDoc.CSI_ReferenceNumber = $"{targetSpecificCodes.FirstOrDefault()}.US.2023.12345678V";
					AssertHasMessageError("Sequence number is invalid", info, errorMessage);
				}
			}

			void AssertReferenceNumberBR20329_C057_C082_C079()
			{
				List<string> codeList = new List<string>() { SupportingDocumentCodes._C057, SupportingDocumentCodes._C082, SupportingDocumentCodes._C079 };
				var docValidation = supDoc.Validation;

				supDoc.CSI_ReferenceNumber = "XXX-AA01-ICUH-2023-12345678";

				foreach (var code in codeList)
				{
					supDoc.CSI_Code = SupportingDocumentCodes._L100;

					docValidation.ValidateCSI_ReferenceNumber();
					AssertEquals(true, info.HasMessageErrors());

					supDoc.CSI_Code = code;
					docValidation.ValidateCSI_ReferenceNumber();
					AssertEquals(false, info.HasMessageErrors());
				}
			}

			void AssertReferenceNumberBR20329_Y120_Y123_Y124_Y125_Y951_Y986()
			{
				List<string> codeList = new List<string>() { SupportingDocumentCodes._Y120, SupportingDocumentCodes._Y123, SupportingDocumentCodes._Y124, SupportingDocumentCodes._Y125, SupportingDocumentCodes._Y951, SupportingDocumentCodes._Y986 };
				var docValidation = supDoc.Validation;
				supDoc.CSI_ReferenceNumber = "XXX-AA01-ICUH-2023-12345678";

				foreach (var code in codeList)
				{
					supDoc.CSI_Code = SupportingDocumentCodes._L100;

					docValidation.ValidateCSI_ReferenceNumber();
					AssertEquals(true, info.HasMessageErrors());

					supDoc.CSI_Code = code;
					supDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
					docValidation.ValidateCSI_ReferenceNumber();
					AssertEquals(false, info.HasMessageErrors());
				}
			}
		}

		public void TestCheckCSI_ReferenceNumber_EntryInstructionAndInvoice_BR20314()
		{
			var message = "[BR20314] Supporting document type C100 (REX Registered Exporter Number) must be unique across this entry instruction and all its related invoice headers.";
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var supDoc1 = instruction1.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = SupportingDocumentCodes._C100;
			supDoc1.CSI_ReferenceNumber = "S1";

			var supDoc2 = instruction1.SupportingDocuments.AddNew();
			supDoc2.CSI_Code = SupportingDocumentCodes._C100;
			supDoc2.CSI_ReferenceNumber = "S2";

			var supDoc3 = instruction1.SupportingDocuments.AddNew();
			supDoc3.CSI_Code = SupportingDocumentCodes._C502;
			supDoc3.CSI_ReferenceNumber = "S2";

			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction1.PK;

			var supDoc4 = invoice1.SupportingDocuments.AddNew();
			supDoc4.CSI_Code = SupportingDocumentCodes._C100;
			supDoc4.CSI_ReferenceNumber = "S2";
			CombineAssertions(() =>
			{
				declaration.RunPreSaveValidation();
				AssertNoMessageError("supDoc1", supDoc1.CSI_ReferenceNumberInfo, message);
				AssertHasMessageError("supDoc2", supDoc2.CSI_ReferenceNumberInfo, message);
				AssertNoMessageError("supDoc3", supDoc3.CSI_ReferenceNumberInfo, message);
				AssertHasMessageError("supDoc4", supDoc4.CSI_ReferenceNumberInfo, message);

				var invoice2 = declaration.Invoices.AddNew();
				var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = instruction1.PK;
				var supDoc5 = invoice2.SupportingDocuments.AddNew();
				supDoc5.CSI_Code = SupportingDocumentCodes._C100;
				supDoc5.CSI_ReferenceNumber = "S1";
				declaration.RunPreSaveValidation();
				AssertHasMessageError("supDoc1", supDoc1.CSI_ReferenceNumberInfo, message);
				AssertHasMessageError("supDoc2", supDoc2.CSI_ReferenceNumberInfo, message);
				AssertNoMessageError("supDoc3", supDoc3.CSI_ReferenceNumberInfo, message);
				AssertHasMessageError("supDoc4", supDoc4.CSI_ReferenceNumberInfo, message);
				AssertHasMessageError("supDoc5", supDoc5.CSI_ReferenceNumberInfo, message);

				var instruction2 = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine2.JI_CEI = instruction2.PK;
				declaration.RunPreSaveValidation();
				AssertNoMessageError("supDoc1", supDoc1.CSI_ReferenceNumberInfo, message);
				AssertHasMessageError("supDoc2", supDoc2.CSI_ReferenceNumberInfo, message);
				AssertNoMessageError("supDoc3", supDoc3.CSI_ReferenceNumberInfo, message);
				AssertHasMessageError("supDoc4", supDoc4.CSI_ReferenceNumberInfo, message);
				AssertNoMessageError("supDoc5", supDoc5.CSI_ReferenceNumberInfo, message);
			});
		}

		public void TestCheckCSI_ReferenceNumber_InvoiceLine_BR20314()
		{
			var message = "[BR20314] Supporting document type C100 (REX Registered Exporter Number) must be unique across all invoice lines under the same entry instruction.";
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction1.PK;

			var supDoc1 = invoiceLine1.SupportingDocuments.AddNew();
			supDoc1.CSI_Code = SupportingDocumentCodes._C100;
			supDoc1.CSI_ReferenceNumber = "S1";

			var supDoc2 = invoiceLine1.SupportingDocuments.AddNew();
			supDoc2.CSI_Code = SupportingDocumentCodes._C100;
			supDoc2.CSI_ReferenceNumber = "S1";

			var supDoc3 = invoiceLine1.SupportingDocuments.AddNew();
			supDoc3.CSI_Code = SupportingDocumentCodes._C502;
			supDoc3.CSI_ReferenceNumber = "S1";

			var supDoc4 = invoiceLine1.SupportingDocuments.AddNew();
			supDoc4.CSI_Code = SupportingDocumentCodes._C100;
			supDoc4.CSI_ReferenceNumber = "S2";
			CombineAssertions(() =>
			{
				declaration.RunPreSaveValidation();
				AssertHasMessageError("supDoc1", supDoc1.CSI_ReferenceNumberInfo, message);
				AssertHasMessageError("supDoc2", supDoc2.CSI_ReferenceNumberInfo, message);
				AssertNoMessageError("supDoc3", supDoc3.CSI_ReferenceNumberInfo, message);
				AssertNoMessageError("supDoc4", supDoc4.CSI_ReferenceNumberInfo, message);

				var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CEI = instruction1.PK;
				var supDoc5 = invoiceLine2.SupportingDocuments.AddNew();
				supDoc5.CSI_Code = SupportingDocumentCodes._C100;
				supDoc5.CSI_ReferenceNumber = "S2";
				declaration.RunPreSaveValidation();
				AssertHasMessageError("supDoc1", supDoc1.CSI_ReferenceNumberInfo, message);
				AssertHasMessageError("supDoc2", supDoc2.CSI_ReferenceNumberInfo, message);
				AssertNoMessageError("supDoc3", supDoc3.CSI_ReferenceNumberInfo, message);
				AssertHasMessageError("supDoc4", supDoc4.CSI_ReferenceNumberInfo, message);
				AssertHasMessageError("supDoc5", supDoc5.CSI_ReferenceNumberInfo, message);

				var instruction2 = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine2.JI_CEI = instruction2.PK;
				declaration.RunPreSaveValidation();
				AssertHasMessageError("supDoc1", supDoc1.CSI_ReferenceNumberInfo, message);
				AssertHasMessageError("supDoc2", supDoc2.CSI_ReferenceNumberInfo, message);
				AssertNoMessageError("supDoc3", supDoc3.CSI_ReferenceNumberInfo, message);
				AssertNoMessageError("supDoc4", supDoc4.CSI_ReferenceNumberInfo, message);
				AssertNoMessageError("supDoc5", supDoc5.CSI_ReferenceNumberInfo, message);
			});
		}

		public void TestValidateCSI_ReferenceNumber_1D24()
		{
			var message = "1D24 Reference is mandatory and must be in the correct format: 'yyyyMMddHHmm'";
			declaration.JE_DateAtFinalDestination = ZDateTime.BrettsBirthday;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var document = instruction.SupportingDocuments.AddNew();
			document.CSI_Code = SupportingDocumentCodes._1D24;
			var targetInfo = document.CSI_ReferenceNumberInfo;
			document.Validation.ValidateCSI_ReferenceNumber();
			CombineAssertions("1D24 Reference", () =>
			{
				AssertHasMessageError("Empty.", targetInfo, message);

				document.CSI_ReferenceNumber = "X";
				AssertHasMessageError("Unrecognized input.", targetInfo, message);

				document.CSI_ReferenceNumber = "202405231559";
				AssertNoMessageError("Valid input.", targetInfo, message);
			});
		}

		public void TestCSI_ReferenceNumber()
		{
			string message = "Supporting Document Reference can have up to 35 alpha numeric characters.";

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			{
				supportingDocument.CSI_ReferenceNumber = new ZString('A', 36);
				AssertHasMessageError("UCC5, 36 characters", supportingDocument.CSI_ReferenceNumberInfo, message);
				supportingDocument.CSI_ReferenceNumber = new ZString('A', 35);
				AssertNoMessageError("UCC5, 35 characters", supportingDocument.CSI_ReferenceNumberInfo, message);
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				supportingDocument.CSI_ReferenceNumber = new ZString('A', 36);
				AssertNoMessageError("UCC6, 36 characters", supportingDocument.CSI_ReferenceNumberInfo, message);
			}
		}

		public void TestValidateCSI_Code_BR5153_Instruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>();
			var invoiceLine = invoiceHeader.InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._51;
			var supportingDocument = instruction.SupportingDocuments.AddNew();
			var supportingDocumentValidation = supportingDocument.Validation;

			supportingDocumentValidation.ValidateCSI_Code();
			var targetInfo = supportingDocument.CSI_CodeInfo;

			CombineAssertions("CSI_Code_BR5153", () =>
			{
				supportingDocument.CSI_Code = SupportingDocumentCodes.AuthorisationInwardProcessingProcedure;
				AssertNoMessageError("BR5153: pass for single C601.", targetInfo, MessageError_BR5153);

				instruction.AdditionalInfos.AddNew(AdditionalInformationCodes._00100, string.Empty).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				supportingDocumentValidation.ValidateCSI_Code();
				AssertHasMessageError("BR5153: warn for C601 when INF 0010 exists.", targetInfo, MessageError_BR5153);
				instruction.AdditionalInfos.RemoveAndDeleteAll();

				invoiceHeader.AdditionalInfos.AddNew(AdditionalInformationCodes._00100, string.Empty).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				supportingDocumentValidation.ValidateCSI_Code();
				AssertHasMessageError("BR5153: warn for C601 when INF 0010 exists on JZ.", targetInfo, MessageError_BR5153);

				supportingDocument.CSI_Code = SupportingDocumentCodes._L100;
				AssertNoMessageError("BR5153: invalid with non-C601.", targetInfo, MessageError_BR5153);
				supportingDocument.CSI_Code = SupportingDocumentCodes.AuthorisationInwardProcessingProcedure;
				AssertHasMessageError("BR5153: to make sure the validation is valid back when CSI_Code back to C601.", targetInfo, MessageError_BR5153);

				invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._42;
				supportingDocumentValidation.ValidateCSI_Code();
				AssertNoMessageError("BR5153: invalid with non-51 Requested Procedure.", targetInfo, MessageError_BR5153);
			});
		}

		public void TestValidateCSI_Code_BR5153_InvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			var supportingDocumentValidation = supportingDocument.Validation;

			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._51;
			supportingDocumentValidation.ValidateCSI_Code();
			var targetInfo = supportingDocument.CSI_CodeInfo;

			CombineAssertions("CSI_Code_BR5153", () =>
			{
				supportingDocument.CSI_Code = SupportingDocumentCodes.AuthorisationInwardProcessingProcedure;
				supportingDocumentValidation.ValidateCSI_Code();
				AssertNoMessageError("BR5153: pass for single C601.", targetInfo, MessageError_BR5153);

				instruction.AdditionalInfos.AddNew(AdditionalInformationCodes._00100, string.Empty).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				supportingDocumentValidation.ValidateCSI_Code();
				AssertHasMessageError("BR5153: warn for C601 when INF 00100 exists.", targetInfo, MessageError_BR5153);
				instruction.AdditionalInfos.RemoveAndDeleteAll();

				invoiceHeader.AdditionalInfos.AddNew(AdditionalInformationCodes._00100, string.Empty).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				supportingDocumentValidation.ValidateCSI_Code();
				AssertHasMessageError("BR5153: warn for C601 when INF 00100 exists on JZ.", targetInfo, MessageError_BR5153);

				supportingDocument.CSI_Code = SupportingDocumentCodes._L100;
				AssertNoMessageError("BR5153: invalid with non-C601.", targetInfo, MessageError_BR5153);
				supportingDocument.CSI_Code = SupportingDocumentCodes.AuthorisationInwardProcessingProcedure;
				AssertHasMessageError("BR5153: to make sure the validation is valid back when CSI_Code back to C601.", targetInfo, MessageError_BR5153);

				invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._42;
				supportingDocumentValidation.ValidateCSI_Code();
				AssertNoMessageError("BR5153: invalid with non-51 Requested Procedure.", targetInfo, MessageError_BR5153);
			});
		}

		const string MessageError_BR5153 = "[BR5153] If Requested Procedure is '51', 'C601' Supporting Document should not be declared when there is a '00100' Additional Information entered under the Entry Instructions > Additional Documents tab or the Invoice Headers > Additional Documents tab.";

		protected override string MessageType => MessageTypeList.Codes.Import;

		protected override SupportingDocument SetupSupportingDocument() => declaration.SupportingDocuments.AddNew();
	}
}
