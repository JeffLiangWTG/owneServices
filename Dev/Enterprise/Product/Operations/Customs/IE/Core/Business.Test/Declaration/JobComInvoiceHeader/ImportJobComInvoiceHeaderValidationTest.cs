using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportJobComInvoiceHeaderValidation))]
	sealed class ImportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest<ImportJobComInvoiceHeaderValidation>
	{
		public void TestCheckJZ_OA_ExporterAddress_BR3013() => CombineAssertions(() =>
		{
			var errorMessage = "[BR3013] Please enter an Additional Information code where Kind is 'INF' and Full Type is '00200' to the Invoice Lines > Additional Documents grid for all invoice lines under this invoice.";
			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			var additionalDocument1 = invoiceLine1.AdditionalInfos.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var additionalDocument2 = invoiceLine2.AdditionalInfos.AddNew();
			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			var addressPK = exporter.MainAddress.PK;

			invoiceHeader.JZ_OA_ExporterAddress = addressPK;
			AssertHasMessageError("No INF AddInfos at all", invoiceHeader.JZ_OA_ExporterAddressInfo, errorMessage);

			invoiceHeader.JZ_OA_ExporterAddress = ZGuid.Empty;
			AssertNoMessageError("Exporter Address is empty", invoiceHeader.JZ_OA_ExporterAddressInfo, errorMessage);

			additionalDocument1.CSI_SubType = "INF";
			additionalDocument1.CSI_Code = "00200";
			invoiceHeader.JZ_OA_ExporterAddress = addressPK;
			AssertHasMessageError("Only one invoice line has a valid add doc", invoiceHeader.JZ_OA_ExporterAddressInfo, errorMessage);

			additionalDocument2.CSI_SubType = "INF";
			additionalDocument2.CSI_Code = "00200";
			invoiceHeader.Validation.ValidateJZ_OA_ExporterAddress();
			AssertNoMessageError("Both invoice lines have a valid add doc", invoiceHeader.JZ_OA_ExporterAddressInfo, errorMessage);
		});

		public void TestCheckJZ_IncoTerm_RuleBR4010_AKCharges()
		{
			const string messageError = "[BR4010] When Incoterm is EXW, FCA, FAS or FOB, a charge of type AK is required and must be greater than 0.";
			var incotermsWithError = new string[]
			{
				Core.Constants.IncoTerms.ExWorks,
				Core.Constants.IncoTerms.FreeCarrier,
				Core.Constants.IncoTerms.FreeAlongsideShip,
				Core.Constants.IncoTerms.FreeOnBoard,
			};

			var instrction = declaration.CustomsEntryInstructions.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_CEI = instrction.PK;
			declaration.JE_ApplicationCode = "V1";
			instrction.CEI_Style = "H1";

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "When neither an invoice charge nor an invoice apportion charge of type AK is present.",
				assertPrerequisite: () => { },
				incotermsThatShoulHaveMessageError: incotermsWithError,
				messageError: messageError);

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "When an invoice charge of type AK is present but having value 0.",
				assertPrerequisite: () => invoiceHeader.Charges.AddNew(AISChargeCodeList.Codes.AK),
				incotermsThatShoulHaveMessageError: incotermsWithError,
				messageError: messageError);
			invoiceHeader.Charges.RemoveAndDeleteAll();

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "When an invoice apportion charge of type AK is present but having value 0.",
				assertPrerequisite: () => invoiceHeader.GroupCharges.AddNew(AISChargeCodeList.Codes.AK),
				incotermsThatShoulHaveMessageError: incotermsWithError,
				messageError: messageError);
			invoiceHeader.GroupCharges.RemoveAndDeleteAll();

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "When an invoice charge of type AK is present with value greater than 0.",
				assertPrerequisite: () =>
				{
					var akCharge = invoiceHeader.Charges.AddNew(AISChargeCodeList.Codes.AK);
					akCharge.J7_Amount = 1;
				},
				incotermsThatShoulHaveMessageError: Array.Empty<string>(),
				messageError: messageError);
			invoiceHeader.Charges.RemoveAndDeleteAll();

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "When an invoice apportion charge of type AK is present with value greater than 0.",
				 assertPrerequisite: () =>
				 {
					 var akApportionCharge = invoiceHeader.GroupCharges.AddNew(AISChargeCodeList.Codes.AK);
					 akApportionCharge.J7_Amount = 1;
				 },
				 incotermsThatShoulHaveMessageError: Array.Empty<string>(),
				 messageError: messageError);
			invoiceHeader.GroupCharges.RemoveAndDeleteAll();

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "Should not check charge type when UCC5 and Declaration type equal to H2, H3, H4 or I1.",
				assertPrerequisite: () => instrction.CEI_Style = "H2",
				incotermsThatShoulHaveMessageError: Array.Empty<string>(),
				messageError: messageError);
		}

		public void TestCheckJZ_IncoTerm_RuleBR4010_1XCharges()
		{
			const string messageError = "[BR4010] When Incoterm is EXW, FCA, FAS or FOB, a charge of type 1X is required and the amount for 1X must be equal to the amount for BA.";
			var targetPropertyInfo = invoiceHeader.JZ_IncoTermInfo;
			var incotermsWithError = new string[]
			{
				Core.Constants.IncoTerms.ExWorks,
				Core.Constants.IncoTerms.FreeCarrier,
				Core.Constants.IncoTerms.FreeAlongsideShip,
				Core.Constants.IncoTerms.FreeOnBoard,
			};

			var instrction = declaration.CustomsEntryInstructions.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_CEI = instrction.PK;
			declaration.JE_ApplicationCode = "V1";
			instrction.CEI_Style = "H1";

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "When an invoice charge of type 1X is not present.",
				assertPrerequisite: () =>
				{
					invoiceHeader.Charges.RemoveAndDeleteAll();
					invoiceHeader.GroupCharges.RemoveAndDeleteAll();
				},
				incotermsThatShoulHaveMessageError: incotermsWithError,
				messageError: messageError);

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "When an invoice charge of type 1X is present but having value 0.",
				assertPrerequisite: () =>
				{
					invoiceHeader.Charges.AddNew(AISChargeCodeList.Codes._1X);
				},
				incotermsThatShoulHaveMessageError: Array.Empty<string>(),
				messageError: messageError);

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "When an invoice apportion charge of type 1X is present but having value 0.",
				assertPrerequisite: () =>
				{
					invoiceHeader.Charges.RemoveAndDeleteAll();
					invoiceHeader.GroupCharges.AddNew(AISChargeCodeList.Codes._1X);
				},
				incotermsThatShoulHaveMessageError: Array.Empty<string>(),
				messageError: messageError);

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "When an invoice charge of type 1X is present with value greater than 0.",
				assertPrerequisite: () =>
				{
					invoiceHeader.GroupCharges.RemoveAndDeleteAll();
					var akCharge = invoiceHeader.Charges.AddNew(AISChargeCodeList.Codes._1X);
					akCharge.J7_Amount = 1;
				},
				incotermsThatShoulHaveMessageError: Array.Empty<string>(),
				messageError: messageError);

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "When an invoice apportion charge of type 1X is present with value greater than 0.",
				assertPrerequisite: () =>
				{
					invoiceHeader.Charges.RemoveAndDeleteAll();
					var akApportionCharge = invoiceHeader.GroupCharges.AddNew(AISChargeCodeList.Codes._1X);
					akApportionCharge.J7_Amount = 1;
				},
				incotermsThatShoulHaveMessageError: Array.Empty<string>(),
				messageError: messageError);

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "Should not check charge type when UCC5 and Declaration type equal to H2, H3, H4 or I1.",
				assertPrerequisite: () =>
				{
					invoiceHeader.GroupCharges.RemoveAndDeleteAll();
					instrction.CEI_Style = "H2";
				},
				incotermsThatShoulHaveMessageError: Array.Empty<string>(),
				messageError: messageError);
		}

		public void TestCheckJZ_IncoTerm_RuleBR4010_BACharges()
		{
			const string messageError = "[BR4010] When Incoterm is EXW, FCA, FAS or FOB, a charge of type BA is required.";
			var targetPropertyInfo = invoiceHeader.JZ_IncoTermInfo;
			var incotermsWithError = new string[]
			{
				Core.Constants.IncoTerms.ExWorks,
				Core.Constants.IncoTerms.FreeCarrier,
				Core.Constants.IncoTerms.FreeAlongsideShip,
				Core.Constants.IncoTerms.FreeOnBoard,
			};

			var instrction = declaration.CustomsEntryInstructions.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_CEI = instrction.PK;
			declaration.JE_ApplicationCode = "V1";
			instrction.CEI_Style = "H1";

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "When no Charges are present.",
				assertPrerequisite: () =>
				{
					invoiceHeader.Charges.RemoveAndDeleteAll();
				},
				incotermsThatShoulHaveMessageError: incotermsWithError,
				messageError: messageError);

			AssertJZ_IncoTermMessageErrorsForIncoterms("When an invoice charge of type BA is present.",
				assertPrerequisite: () => invoiceHeader.Charges.AddNew(AISChargeCodeList.Codes.BA),
				incotermsThatShoulHaveMessageError: Array.Empty<string>(),
				messageError: messageError);
			invoiceHeader.Charges.RemoveAndDeleteAll();

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "When an invoice apportion charge of type BA is present.",
				assertPrerequisite: () => invoiceHeader.GroupCharges.AddNew(AISChargeCodeList.Codes.BA),
				incotermsThatShoulHaveMessageError: Array.Empty<string>(),
				messageError: messageError);
			invoiceHeader.GroupCharges.RemoveAndDeleteAll();

			AssertJZ_IncoTermMessageErrorsForIncoterms(
				assertMessage: "Should not check charge type when UCC5 and Declaration type equal to H2, H3, H4 or I1.",
				assertPrerequisite: () => instrction.CEI_Style = "H2",
				incotermsThatShoulHaveMessageError: Array.Empty<string>(),
				messageError: messageError);
		}

		void AssertJZ_IncoTermMessageErrorsForIncoterms(string assertMessage, Action assertPrerequisite, string[] incotermsThatShoulHaveMessageError, string messageError)
		{
			var targetPropertyInfo = invoiceHeader.JZ_IncoTermInfo;
			var allIncoterms = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms).GetAllCodes();
			var incotermsWithNoError = allIncoterms.Except(incotermsThatShoulHaveMessageError);
			CombineAssertions(assertMessage, () =>
			{
				incotermsThatShoulHaveMessageError.ForEach(incoterm =>
				{
					invoiceHeader.JZ_IncoTerm = incoterm;
					assertPrerequisite.Invoke();
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertHasMessageErrorContaining(targetPropertyInfo, messageError);
				});
				incotermsWithNoError.ForEach(incoterm =>
				{
					invoiceHeader.JZ_IncoTerm = incoterm;
					assertPrerequisite.Invoke();
					invoiceHeader.Validation.ValidateJZ_IncoTerm();
					AssertNoMessageErrorContaining(targetPropertyInfo, messageError);
				});
			});
		}

		public void TestValidationForMissingMandatoryCharges()
		{
			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CEI = entryinstruction.PK;
			declaration.JE_ApplicationCode = "V1";
			entryinstruction.CEI_Style = "H2";
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoiceHeader.Charges.RemoveAndDeleteAll();
			invoiceHeader.Validation.ValidateJZ_IncoTerm();
			AssertNoMessageErrorContaining("No validation when UCC5 and CEI_Style is not H1 or H5", invoiceHeader.JZ_IncoTermInfo, "The following charges are missing");

			entryinstruction.CEI_Style = "H1";
			invoiceHeader.Validation.ValidateJZ_IncoTerm();
			AssertHasMessageErrorContaining("Has validation when UCC5 and CEI_Style is H1 or H5", invoiceHeader.JZ_IncoTermInfo, "The following charges are missing");
		}

		public void TestCheckJZ_IncoTerm_MandatoryValidation()
		{
			declaration.JE_ApplicationCode = "V1";
			var instrction = declaration.CustomsEntryInstructions.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_CEI = instrction.PK;
			instrction.CEI_Style = "H2";
			invoiceHeader.JZ_IncoTerm = "";

			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(invoiceHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);

				instrction.CEI_Style = "H1";
				invoiceHeader.Validation.ValidateJZ_IncoTerm();
				AssertHasMessageErrorContaining(invoiceHeader.JZ_IncoTermInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckJZ_IncoTerm_RuleBR4014()
		{
			const string messageError = "[BR4014] When Incoterm is CIF, Agreed Place Code must be inside the EU.";
			var targetPropertyInfo = invoiceHeader.JZ_IncoTermInfo;
			var allIncoterms = new IncoTermsCodeDescriptionPairList(IncoTermsListType.ActiveIncoTerms).GetAllCodes();
			var cifIncoterm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			invoiceHeader.ZG_AgreedPlaceCode = "US122";
			CombineAssertions("When Agreed Place Code first two characters do not start with an EU Country Code", () =>
			{
				invoiceHeader.JZ_IncoTerm = cifIncoterm;
				AssertHasMessageErrorContaining(targetPropertyInfo, messageError);

				var incotermsWithNoError = allIncoterms.Except(cifIncoterm);
				incotermsWithNoError.ForEach(incoterm =>
				{
					invoiceHeader.JZ_IncoTerm = incoterm;
					AssertNoMessageErrorContaining(targetPropertyInfo, messageError);
				});
			});

			invoiceHeader.ZG_AgreedPlaceCode = "AT123";
			CombineAssertions("When Agreed Place Code first two characters starts with an EU Country Code", () =>
			{
				allIncoterms.ForEach(incoterm =>
				{
					invoiceHeader.JZ_IncoTerm = incoterm;
					AssertNoMessageErrorContaining(targetPropertyInfo, messageError);
				});
			});
		}

		public void TestCheckJZ_IncoTerm_RuleBR4016()
		{
			const string expectedMessageError = "[BR4016] Incoterm cannot be DDP when Requested Procedure is 51 or 53.";
			var targetPropertyInfo = invoiceHeader.JZ_IncoTermInfo;

			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			AssertNoMessageErrorContaining("When IncoTerm = DDP and No Entered Procedure Code", targetPropertyInfo, expectedMessageError);

			var entryinstruction = declaration.CustomsEntryInstructions.AddNew();
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CEI = entryinstruction.PK;
			line1.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._31;
			invoiceHeader.Validation.ValidateJZ_IncoTerm();
			AssertNoMessageErrorContaining("When IncoTerm = DDP and Procedure Code isn't 51 or 53", targetPropertyInfo, expectedMessageError);

			line1.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._51;
			invoiceHeader.Validation.ValidateJZ_IncoTerm();
			AssertHasMessageErrorContaining("When IncoTerm = DDP and Procedure Code is 51 or 53", targetPropertyInfo, expectedMessageError);

			line1.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._53;
			invoiceHeader.Validation.ValidateJZ_IncoTerm();
			AssertHasMessageErrorContaining("When IncoTerm = DDP and Procedure Code is 51 or 53", targetPropertyInfo, expectedMessageError);
		}

		public void TestCheckJZ_ValuationCode_MandatoryValidation()
		{
			declaration.JE_ApplicationCode = "V1";
			var instrction = declaration.CustomsEntryInstructions.AddNew();
			var line = invoiceHeader.InvoiceLines.AddNew();
			line.JI_CEI = instrction.PK;
			instrction.CEI_Style = "H2";
			invoiceHeader.JZ_ValuationCode = "";
			
			CombineAssertions(() =>
			{
				AssertEquals("UCC5 and H2", 0, invoiceHeader.JZ_ValuationCodeInfo.Notifications.Count());

				instrction.CEI_Style = "H1";
				invoiceHeader.Validation.ValidateJZ_ValuationCode();
				AssertEquals("UCC5 and H1", 0, invoiceHeader.JZ_ValuationCodeInfo.Notifications.Count());
			});
		}

		public void TestCheckJZ_ValuationCode_RuleBR8051()
		{
			const string messageError = "[BR8051] Nature of Transaction must be 51 (same original export country) or 52 (different original export country) when Requested Procedure is 61 and Previous Procedure is 21 or 22.";
			var targetPropertyInfo = invoiceHeader.JZ_ValuationCodeInfo;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var invalidValuationCodes = new NatureOfTransactionList().ToArray().Select(x => x.Code)
				.Except(NatureOfTransactionList.Codes._51)
				.Except(NatureOfTransactionList.Codes._52);

			CombineAssertions(() =>
			{
				invalidValuationCodes.ForEach(invalidValuationCode =>
				{
					assertBR8051(invalidValuationCode, "61", "21");
					assertBR8051(invalidValuationCode, "61", "22");

					invoiceLine.JI_Procedure = "6123";
					invoiceHeader.JZ_ValuationCode = invalidValuationCode;
					AssertNoMessageError($"JZ_ValuationCode '{invalidValuationCode}' JI_Procedure '6123'", targetPropertyInfo, messageError);

					invoiceLine.JI_Procedure = "6221";
					invoiceHeader.JZ_ValuationCode = invalidValuationCode;
					AssertNoMessageError($"JZ_ValuationCode '{invalidValuationCode}' JI_Procedure '6221'", targetPropertyInfo, messageError);
				});
			});

			void assertBR8051(ZString invalidValuationCode, ZString procedure, ZString previousProcedure)
			{
				var invoiceLineProcedure = procedure + previousProcedure;
				invoiceLine.JI_Procedure = invoiceLineProcedure;
				invoiceHeader.JZ_ValuationCode = NatureOfTransactionList.Codes._51;
				AssertNoMessageError($"JZ_ValuationCode '51' Procedure '{invoiceLineProcedure}'", targetPropertyInfo, messageError);

				invoiceHeader.JZ_ValuationCode = invalidValuationCode;
				AssertHasMessageError($"JZ_ValuationCode '{invalidValuationCode}' Procedure '{invoiceLineProcedure}'", targetPropertyInfo, messageError);

				invoiceHeader.JZ_ValuationCode = NatureOfTransactionList.Codes._52;
				AssertNoMessageError($"JZ_ValuationCode '52' Procedure '{invoiceLineProcedure}'", targetPropertyInfo, messageError);
			}
		}

		public void TestCheckJZ_ValuationCode_RuleCD8051()
		{
			const string expectedMessageError = "[CD8051] Nature of Transaction is required when Requested Procedure is 51 or 61.";
			var targetPropertyInfo = invoiceHeader.JZ_ValuationCodeInfo;

			declaration.JE_ApplicationCode = "V1";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "H1";
			var line1 = invoiceHeader.InvoiceLines.AddNew();
			line1.JI_CEI = entryInstruction.PK;

			line1.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._31;
			invoiceHeader.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageErrorContaining("When JZ_ValuationCode blank and Procedure Code isn't 51 or 61", targetPropertyInfo, expectedMessageError);

			line1.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._51;
			invoiceHeader.Validation.ValidateJZ_ValuationCode();
			AssertHasMessageErrorContaining("When JZ_ValuationCode blank and Procedure Code = 51", targetPropertyInfo, expectedMessageError);

			line1.JI_Procedure = UniversalReferenceConstants.ProcedureCodes.ProcedureCode._61;
			invoiceHeader.Validation.ValidateJZ_ValuationCode();
			AssertHasMessageErrorContaining("When JZ_ValuationCode blank and Procedure Code = 61", targetPropertyInfo, expectedMessageError);

			entryInstruction.CEI_Style = "H2";
			invoiceHeader.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageErrorContaining("When UCC5 and Declaration type is H2", targetPropertyInfo, expectedMessageError);

			entryInstruction.CEI_Style = "H3";
			invoiceHeader.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageErrorContaining("When UCC5 and Declaration type is H3", targetPropertyInfo, expectedMessageError);

			entryInstruction.CEI_Style = "H4";
			invoiceHeader.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageErrorContaining("When UCC5 and Declaration type is H4", targetPropertyInfo, expectedMessageError);

			entryInstruction.CEI_Style = "H6";
			invoiceHeader.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageErrorContaining("When UCC5 and Declaration type is H6", targetPropertyInfo, expectedMessageError);

			entryInstruction.CEI_Style = "I1";
			invoiceHeader.Validation.ValidateJZ_ValuationCode();
			AssertNoMessageErrorContaining("When UCC5 and Declaration type is I1", targetPropertyInfo, expectedMessageError);

			entryInstruction.CEI_Style = "H5";
			invoiceHeader.Validation.ValidateJZ_ValuationCode();
			AssertHasMessageErrorContaining("When UCC5 and Declaration type is H5", targetPropertyInfo, expectedMessageError);

			entryInstruction.CEI_Style = "H1";
			invoiceHeader.JZ_ValuationCode = NatureOfTransactionList.Codes._11;
			AssertNoMessageErrorContaining("When JZ_ValuationCode is set and Procedure Code = 61", targetPropertyInfo, expectedMessageError);
		}

		public void TestCheckJZ_RX_NKInvoice_Currency_RuleBR6142()
		{
			const string messageError = "[BR6142] Invoice Currency must be 'EUR' when Declaration Type is I1 and Lines Price sharing same instruction less than €22.";

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = instruction.PK;
			invoiceLine1.JI_LinePrice = 10m;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_LinePrice = 10m;

			var targetPropertyInfo = invoiceHeader.JZ_RX_NKInvoice_CurrencyInfo;

			AssertNoMessageError("Invoice currency is EUR, total lines price is 20, instruction CE_Style is I1", targetPropertyInfo, messageError);

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertHasMessageError("Invoice currency is USD, total lines price is 20, instruction CE_Style is I1", targetPropertyInfo, messageError);

			invoiceLine2.JI_LinePrice = 20m;
			invoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertNoMessageError("Invoice currency is USD, total lines price is 30, instruction CE_Style is I1", targetPropertyInfo, messageError);

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			AssertNoMessageError("Invoice currency is EUR, total lines price is 30, instruction CE_Style is I1", targetPropertyInfo, messageError);

			invoiceLine2.JI_LinePrice = 10m;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			invoiceHeader.Validation.ValidateJZ_RX_NKInvoice_Currency();
			AssertNoMessageError("Invoice currency is EUR, total lines price is 20, instruction CE_Style is H1", targetPropertyInfo, messageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			invoiceHeader = declaration.Invoices.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;

		protected override string MessageType => IEJobMessageTypeList.Codes.Import;
	}
}
