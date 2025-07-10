using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IE.Business.Constants;
using static Enterprise.Customs.IE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	abstract class CommonImportCusEntryInstructionValidationTest<T> : CusEntryInstructionValidationAbstractTest<T> where T : CommonImportCusEntryInstructionValidation
	{
		public void TestValidateBR0339()
		{
			var message = "[BR0339] At least one [3/39] Authorizations have to be filled in.";
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("When H1, no CusAuthorizationUsages", instruction, message);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("When I1, no CusAuthorizationUsages", instruction, message);

			instruction.CusAuthorizationUsages.AddNew();
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("When I1, has CusAuthorizationUsages", instruction, message);
		}

		public void TestCheckIdentificationofGoodsDetails_R8F0013()
		{
			var message = "[BR8F00013] Identification of Goods > Detail is required.";
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			var invoiceLine = jobDeclaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addInfo.CSI_Code = AdditionalInformationCodes._00100;
			AssertEquals("Precondition", true, instruction.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired);

			instruction.IdentificationofGoodsDetails = ZString.Empty;
			AssertHasMessageError(instruction.IdentificationofGoodsDetailsInfo, message);

			instruction.IdentificationofGoodsDetails = "GoodsId";
			AssertNoMessageError(instruction.IdentificationofGoodsDetailsInfo, message);
		}

		public abstract void TestValidateBR2037();

		protected void TestValidateBR2037(string[] supportingDocumentCodes, string message)
		{
			var invoice = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "01511A1";
			var additionalProcedureCode = invoiceLine.AdditionalProcedureCodes.AddNew();
			additionalProcedureCode.CY_Code = "0151C39";
			var instructionSupportingDocument = instruction.SupportingDocuments.AddNew();
			instructionSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes._C057;
			var invoiceSupportingDocument = invoice.SupportingDocuments.AddNew();
			invoiceSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes._N740;
			var invoiceLineSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
			invoiceLineSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes._N740;
			validation.ValidateAll();
			AssertHasRowMessageError("No C08, No valid support document", instruction, message);

			invoiceLine.JI_Procedure = "0151C08";
			validation.ValidateAll();
			AssertNoRowMessageError("Procedure C08, No valid support document", instruction, message);

			invoiceLine.JI_Procedure = "01511A1";
			additionalProcedureCode.CY_Code = "0151C08";
			validation.ValidateAll();
			AssertNoRowMessageError("Additional procedure C08, No valid support document", instruction, message);

			additionalProcedureCode.CY_Code = "0151C39";
			foreach (var supportingDocumentCode in supportingDocumentCodes)
			{
				instructionSupportingDocument.CSI_Code = supportingDocumentCode;
				validation.ValidateAll();
				AssertNoRowMessageError($"No C08, Has {supportingDocumentCode} instruction support document", instruction, message);

				instructionSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes._C057;
				invoiceSupportingDocument.CSI_Code = supportingDocumentCode;
				validation.ValidateAll();
				AssertNoRowMessageError($"No C08, Has {supportingDocumentCode} invoice support document", instruction, message);

				invoiceSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes._N740;
				invoiceLineSupportingDocument.CSI_Code = supportingDocumentCode;
				validation.ValidateAll();
				AssertNoRowMessageError($"No C08, Has {supportingDocumentCode} invoice line support document", instruction, message);
				invoiceLineSupportingDocument.CSI_Code = Constants.SupportingDocumentCodes._N740;
			}
		}

		public void TestValidateBR8F0012()
		{
			var message = "[BR8F0012] Please enter at least one Processed Product under the Entry Instructions > Special Procedures > Processed Products grid.";
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H4;
			var invoiceLine = jobDeclaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "51";
			var instructionInfo = instruction.AdditionalInfos.AddNew();
			instructionInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			instructionInfo.CSI_Code = Constants.TransportDocumentCodes._N705;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("Import, Style equal to H4, procedure equal to 51, there is not an additional Info where subtype is INF and code is 00100, no processed product", instruction, message);

			instructionInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("Import, Style equal to H4, procedure equal to 51, there is an additional Info where subtype is INF but code is not 00100, no processed product", instruction, message);

			instructionInfo.CSI_Code = Constants.AdditionalInformationCodes._00100;
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("Import, Style equal to H4, procedure equal to 51, there is an additional Info where subtype is INF and code is 00100, no processed product", instruction, message);

			instruction.ZG_ProcessedProductsCommodityCode = "1";
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("Has processed product", instruction, message);
		}

		public abstract void TestCheckCEI_Style_BR3005();

		public void TestCheckBR8076()
		{
			var invoiceLine = jobDeclaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._51;

			AssertBR8076(ProcessingProcedureCodeList.Codes._1, false);
			AssertBR8076(ProcessingProcedureCodeList.Codes._6, true);
			AssertBR8076(ProcessingProcedureCodeList.Codes._7, true);
			AssertBR8076(ProcessingProcedureCodeList.Codes._8, true);
			AssertBR8076(ProcessingProcedureCodeList.Codes._9, true);
			AssertBR8076(ProcessingProcedureCodeList.Codes._10, false);
			AssertBR8076(ProcessingProcedureCodeList.Codes._22, true);

			void AssertBR8076(string processingProcedureCode, bool shouldHaveMessageError)
			{
				instruction.ZG_ProcessingProcedureCode = processingProcedureCode;
				validation.ValidateAll();
				if (shouldHaveMessageError)
				{
					AssertHasRowMessageError($"{processingProcedureCode} should be invalid for BR8076", instruction, MessageError_BR8076);
				}
				else
				{
					AssertNoRowMessageError($"{processingProcedureCode} should be valid for BR8076", instruction, MessageError_BR8076);
				}
			}
		}

		public void TestCheckRuleC0626_ToWarehouseType() => CombineAssertions(() =>
		{
			const string message = "[C0626] Warehouse Type & ID are required for Declaration Type 'H2' if Sub Style is either 'A' or 'D'.";
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			instruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertEquals("Precondition: ToWarehouseType is empty", ZString.Empty, instruction.ToWarehouseType);
			AssertHasMessageError("CEI_Style = 'H2', CEI_SubStyle = 'A'", instruction.ToWarehouseTypeInfo, message);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
			instruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertHasMessageError("CEI_Style = 'H2', CEI_SubStyle = 'D'", instruction.ToWarehouseTypeInfo, message);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
			instruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageError("CEI_Style = 'H2', CEI_SubStyle not in ('A', 'D')", instruction.ToWarehouseTypeInfo, message);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			instruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageError("CEI_Style <> 'H2', CEI_SubStyle = 'A'", instruction.ToWarehouseTypeInfo, message);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var authorizationUsage = instruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1;
			authorizationUsage.AGC_OH_Owner = warehouse.PK;
			instruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			AssertEquals("Precondition: ToWarehouseType is not empty", WarehouseTypeList.Codes.CustomsWarehousingCW1, instruction.ToWarehouseType);
			AssertNoMessageError("ToWarehouseType not empty", instruction.ToWarehouseTypeInfo, message);
		});

		public void TestCheckRuleC0626_ToWarehouseCode() => CombineAssertions(() =>
		{
			const string message = "[C0626] Warehouse Type & ID are required for Declaration Type 'H2' if Sub Style is either 'A' or 'D'.";
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			instruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertEquals("Precondition: ToWarehouseCode is empty", ZString.Empty, instruction.ToWarehouseCode);
			AssertHasMessageError("CEI_Style = 'H2', CEI_SubStyle = 'A'", instruction.ToWarehouseCodeInfo, message);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA;
			instruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertHasMessageError("CEI_Style = 'H2', CEI_SubStyle = 'D'", instruction.ToWarehouseCodeInfo, message);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
			instruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageError("CEI_Style = 'H2', CEI_SubStyle not in ('A', 'D')", instruction.ToWarehouseCodeInfo, message);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			instruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageError("CEI_Style <> 'H2', CEI_SubStyle = 'A'", instruction.ToWarehouseCodeInfo, message);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			warehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "123", Core.Constants.CountryCodes.Ireland);
			instruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			AssertEquals("Precondition: ToWarehouseCode is not empty", "123", instruction.ToWarehouseCode);
			AssertNoMessageError("ToWarehouseCode not empty", instruction.ToWarehouseCodeInfo, message);
		});

		public void TestCheckRuleBR2075_ToWarehouseType() => CombineAssertions(() =>
		{
			const string message = "[BR2075] Warehouse ID is required for To Warehouse when Requested Procedure is '07' and at least one Previous Procedure is '00'. Please enter a To Warehouse with an EORI number.";
			var invoiceLine = jobDeclaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = "0700";
			instruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertEquals("Precondition: ToWarehouseType is empty", ZString.Empty, instruction.ToWarehouseType);
			AssertHasMessageError("Requested Procedure = '07', Previous Procedure = '00'", instruction.ToWarehouseCodeInfo, message);

			invoiceLine.JI_Procedure = "0701";
			instruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageError("Requested Procedure = '07', Previous Procedure = '01'", instruction.ToWarehouseCodeInfo, message);

			invoiceLine.JI_Procedure = "0708";
			invoiceLine.JI_Procedure = "08";
			instruction.Validation.ValidateCEI_OA_Warehouse2();
			AssertNoMessageError("Requested Procedure = '08', Previous Procedure = '00'", instruction.ToWarehouseCodeInfo, message);

			invoiceLine.JI_Procedure = "0700";
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			warehouse.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "123", Core.Constants.CountryCodes.Ireland);
			instruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;
			AssertEquals("Precondition: ToWarehouseCode is not empty", "123", instruction.ToWarehouseCode);
			AssertNoMessageError("ToWarehouseCode not empty", instruction.ToWarehouseCodeInfo, message);
		});

		public void TestCheckCEI_SubStyle_ApplicationTypeIsEmpty()
		{
			instruction.CEI_Style = string.Empty;
			ValidationTestHelper.AssertFieldIsNotMandatory(instruction.CEI_SubStyleInfo, MessageError_BR1021);
		}

		public void TestCheckCEI_SubStyle_ApplicationTypeIsH1()
		{
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			AssertCEI_SubStyle_BR1021();
			AssertCEI_SubStyle_BR1010();
			AssertCEI_SubStyleBR2027_NotI1();
		}

		public void TestCheckCEI_SubStyle_ApplicationTypeIsH2()
		{
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			AssertCEI_SubStyle_BR1021();
			AssertCEI_SubStyle_BR1010();
			AssertCEI_SubStyleBR2027_NotI1();
		}

		public void TestCheckCEI_SubStyle_ApplicationTypeIsH3()
		{
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H3;
			AssertCEI_SubStyle_BR1021();
			AssertCEI_SubStyle_BR1010();
			AssertCEI_SubStyleBR2027_NotI1();
		}

		public void TestCheckCEI_SubStyle_ApplicationTypeIsH4()
		{
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H4;
			AssertCEI_SubStyle_BR1021();
			AssertCEI_SubStyle_BR1010();
			AssertCEI_SubStyleBR2027_NotI1();
		}

		public void TestCheckCEI_SubStyle_ApplicationTypeIsH6()
		{
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H6;
			AssertCEI_SubStyle_BR1021();
			AssertCEI_SubStyle_BR1010();
			AssertCEI_SubStyleBR2027_NotI1();
		}

		public void TestCheckCEI_SubStyle_ApplicationTypeIsI1()
		{
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.I1;
			AssertCEI_SubStyle_BR1021();
			AssertCEI_SubStyle_BR1010();
			AssertCEI_SubStyle_BR2027_I1();
			AssertCEI_SubStyle_BR6142();
		}

		public void TestCheckCEI_SubStyle_OtherApplicationType()
		{
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H5;
			ValidationTestHelper.AssertFieldIsNotMandatory(instruction.CEI_SubStyleInfo, MessageError_BR1021);
		}

		void AssertCEI_SubStyle_BR1021()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(instruction.CEI_SubStyleInfo, MessageError_BR1021);
		}

		void AssertCEI_SubStyle_BR1010()
		{
			var arr = new ZString[] { EntrySubStyleList.Codes.IncompleteDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE };
			foreach (var code in arr)
			{
				instruction.CEI_SubStyle = code;
				AssertHasMessageError(instruction.CEI_SubStyleInfo, MessageError_BR1010);
			}
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
			AssertNoMessageError(instruction.CEI_SubStyleInfo, MessageError_BR1010);
		}

		void AssertCEI_SubStyle_BR2027_I1()
		{
			var arr = new ZString[] { EntrySubStyleList.Codes.SimplifiedDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic };
			foreach (var code in arr)
			{
				instruction.CEI_SubStyle = code;
				AssertNoMessageError(instruction.CEI_SubStyleInfo, MessageError_BR2027_I1);
			}
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
			AssertHasMessageError(instruction.CEI_SubStyleInfo, MessageError_BR2027_I1);
		}

		void AssertCEI_SubStyle_BR6142()
		{
			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_LinePrice = 10m;

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.NormalDeclaration;
			AssertNoMessageError(instruction.CEI_SubStyleInfo, MessageError_BR6142);

			var entrySubStyleListBR6142 = new ZString[] {   EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF,
										EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationUnderTheProcedureCoveredUnderArticle182OfTheCode,
										EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE,
										EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF,
										EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic };

			foreach (var code in entrySubStyleListBR6142)
			{
				instruction.CEI_SubStyle = code;
				AssertHasMessageError(instruction.CEI_SubStyleInfo, MessageError_BR6142);
			}

			invoiceLine.JI_LinePrice = 30m;
			validation.ValidateCEI_SubStyle();
			AssertNoMessageError(instruction.CEI_SubStyleInfo, MessageError_BR6142);
		}

		void AssertCEI_SubStyleBR2027_NotI1()
		{
			var arr = new ZString[] { EntrySubStyleList.Codes.NormalDeclaration, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA,
				EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic };
			foreach (var code in arr)
			{
				instruction.CEI_SubStyle = code;
				AssertNoMessageError(instruction.CEI_SubStyleInfo, MessageError_BR2027_NotI1);
			}
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
			AssertHasMessageError(instruction.CEI_SubStyleInfo, MessageError_BR2027_NotI1);
		}

		public void TestCheckCEI_SubStyle_BR8063_NoAmend()
		{
			var entry = jobDeclaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			entry.MovementReferenceNumberSetter("MRN0000001");
			entry.CH_EntryStatus = AISEntryStatusList.Codes.Accepted;
			instruction.CusAuthorizationUsages.RemoveAndDeleteAll();
			instruction.CEI_SubStyle = "A";
			Factory.Save();

			instruction = NewFactory().Load<CusEntryInstruction>(instruction.PK);
			instruction.CEI_SubStyle = "B";
			AssertHasMessageErrorContaining("Amend check.", instruction.CEI_SubStyleInfo, CommonResStrings.ShouldNotAmendThisValue);
			instruction.ValidationModes = EU.Business.Declaration.ValidationModes.None;
			instruction.Validation.ValidateCEI_SubStyle();
			AssertNoMessageErrorContaining("Skip Amend check when ValidationModes is NONE.", instruction.CEI_SubStyleInfo, CommonResStrings.ShouldNotAmendThisValue);
			instruction.ValidationModes = EU.Business.Declaration.ValidationModes.Amendment;

			instruction.CEI_SubStyle = "A";
			AssertNoMessageErrorContaining("Amend check(changed back to same as in the ouggoing message, validation passes).", instruction.CEI_SubStyleInfo, CommonResStrings.ShouldNotAmendThisValue);
		}

		public void TestCheckCEI_SubStyle_BR8077()
		{
			var message = "[BR8077] Please enter a MRN number under Entry Instructions > Previous Documents grid with type 'MRN' when declaring a supplementary declaration.";
			instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
			AssertHasMessageError("Import, SubStyle equal to Y, previous documents are empty, there should be a message error.", instruction.CEI_SubStyleInfo, message);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
			AssertNoMessageError("SubStyle not equal to Y, there should be no message error.", instruction.CEI_SubStyleInfo, message);

			instruction.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF;
			var previousDoc = instruction.PreviousDocuments.AddNew();
			previousDoc.CSI_Code = "MRN";
			previousDoc.CSI_ReferenceNumber = "MRN123451234512345";
			instruction.Validation.ValidateCEI_SubStyle();
			AssertNoMessageError("Only one valid previous document added, there should be no message error.", instruction.CEI_SubStyleInfo, message);

			var previousDoc2 = instruction.PreviousDocuments.AddNew();
			previousDoc2.CSI_Code = "MRN";
			previousDoc2.CSI_ReferenceNumber = "MRN123451234512344";
			instruction.Validation.ValidateCEI_SubStyle();
			AssertHasMessageError("Have valid previous document but not only one, there should be a message error.", instruction.CEI_SubStyleInfo, message);
		}

		public void TestCheckCEI_Style_BR1118()
		{
			var message = "[BR1118] Declaration must be H5 when at least one invoice line has Additional Procedure Code F15";
			var styleInfo = instruction.CEI_StyleInfo;
			var invoice = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			CombineAssertions(() =>
			{
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				AssertNoMessageError("No error for H1", styleInfo, message);
				invoiceLine.JI_Procedure = "4051F15";
				instruction.Validation.ValidateCEI_Style();
				AssertHasMessageError("Has error on special procedure for H1", styleInfo, message);
				invoiceLine.JI_Procedure = "5555F99";
				invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "4051F15";
				instruction.Validation.ValidateCEI_Style();
				AssertHasMessageError("Has error with additional procedure for H1", styleInfo, message);
				invoiceLine.JI_Procedure = "4051F15";
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.H5;
				AssertNoMessageError("No error for H5", styleInfo, message);
			});
		}

		public void TestCheckBR1030()
		{
			var message = "[BR1030] Authorization code 'TEA' is required when declaration type is H3 and procedure code '53'.";
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "H3";
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "53";

			var validation = entryInstruction.Validation;
			validation.ValidateAll();
			CombineAssertions("BR1030 check on Row.", () =>
			{
				AssertHasRowMessageError("No Authorisations", entryInstruction, message);

				var authUsage = entryInstruction.CusAuthorizationUsages.AddNew();
				authUsage.AGC_Code = "TEA";
				validation.ValidateAll();
				AssertNoRowMessageError("CusAuthorizationUsage with AGC_Code: TEA", entryInstruction, message);

				authUsage.AGC_Code = "ZZZ";
				validation.ValidateAll();
				AssertHasRowMessageError("CusAuthorizationUsage with AGC_Code: ZZZ", entryInstruction, message);

				entryInstruction.CEI_Style = "H1";
				validation.ValidateAll();
				AssertNoRowMessageError("Entry Instruction != H3", entryInstruction, message);
			});
		}

		public void TestCheckCEI_Style_BR2011()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "44";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var validStyles = new ZString[] {
				ImportDeclarationTypeList.Codes.H1, ImportDeclarationTypeList.Codes.H2, ImportDeclarationTypeList.Codes.H3,
				ImportDeclarationTypeList.Codes.H4, ImportDeclarationTypeList.Codes.H6, ImportDeclarationTypeList.Codes.H7,
				ImportDeclarationTypeList.Codes.I1
			};
			var nonValidStyles = new ZString[] {
				ImportDeclarationTypeList.Codes.H5, "ZZ", ZString.Empty,
			};
			var validSubStyles = new ZString[] { EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeA, EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeC, };
			var nonValidSubStyles = new ZString[] {
				EntrySubStyleList.Codes.NormalDeclaration, EntrySubStyleList.Codes.IncompleteDeclaration, EntrySubStyleList.Codes.SimplifiedDeclaration,
				EntrySubStyleList.Codes.PreliminaryDeclarationUnderCodeB, EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationOfSimplifiedDeclarationsCoveredByCAndF,
				EntrySubStyleList.Codes.SupplementaryRecapitulativeDeclarationUnderTheProcedureCoveredUnderArticle182OfTheCode, EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE,
				EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF, EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic, ZString.Empty,
			};

			CombineAssertions(() =>
			{
				AssertCheckCEI_Style_BR2011(entryInstruction, validStyles, validSubStyles, hasError: false);
				AssertCheckCEI_Style_BR2011(entryInstruction, nonValidStyles, validSubStyles, hasError: true);
				AssertCheckCEI_Style_BR2011(entryInstruction, validStyles, nonValidSubStyles, hasError: true);
				AssertCheckCEI_Style_BR2011(entryInstruction, nonValidStyles, nonValidSubStyles, hasError: true);

				entryInstruction.PreviousDocuments.AddNew();
				AssertCheckCEI_Style_BR2011(entryInstruction, nonValidStyles, validSubStyles, hasError: false);
				AssertCheckCEI_Style_BR2011(entryInstruction, validStyles, nonValidSubStyles, hasError: false);

				entryInstruction.PreviousDocuments.RemoveAndDeleteAll();
				invoiceLine.InvoiceHeader.PreviousDocuments.AddNew();
				AssertCheckCEI_Style_BR2011(entryInstruction, nonValidStyles, validSubStyles, hasError: false);
				AssertCheckCEI_Style_BR2011(entryInstruction, validStyles, nonValidSubStyles, hasError: false);
			});
		}

		static void AssertCheckCEI_Style_BR2011(CusEntryInstruction instruction, ZString[] styles, ZString[] subStyles, bool hasError)
		{
			foreach (var style in styles)
			{
				foreach (var subStyle in subStyles)
				{
					instruction.CEI_SubStyle = subStyle;
					instruction.CEI_Style = style;
					if (hasError)
					{
						AssertHasMessageErrorContaining($"Has Error: CEI_Style = {style}, CEI_SubStyle = {subStyle}", instruction.CEI_StyleInfo, MessageError_BR2011);
					}
					else
					{
						AssertNoMessageErrorContaining($"No Error: CEI_Style = {style}, CEI_SubStyle = {subStyle}", instruction.CEI_StyleInfo, MessageError_BR2011);
					}
				}
			}
		}

		public void TestCheckCEI_Style_BR3241()
		{
			const string messageError1 = "[BR3241] Please enter an Additional Document where Kind is 'INF' and Full Type is '00200' under Invoice Line: {1} > Additional Documents tab.";
			const string messageError2 = "[BR3241] Please enter an Additional Document where Kind is 'INF' and Full Type is '00200' under Invoice Line: {2} > Additional Documents tab.";

			var seller = Factory.New<OrgHeader>();
			jobDeclaration.SellerOrgPK = seller.PK;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			var invoice = jobDeclaration.Invoices.AddNew();
			invoice.SellerOrgPK = seller.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addInfo.CSI_Code = "00200";

			var validation = instruction.Validation;
			CombineAssertions(() =>
			{
				validation.ValidateCEI_Style();
				AssertNoMessageError("IsSellerOrBuyerDifferentFromDeclaration false, HasAddtionalInfoINF00200 true", instruction.CEI_StyleInfo, messageError1);

				invoice.SellerOrgPK = Factory.New<OrgHeader>().PK;
				validation.ValidateCEI_Style();
				AssertNoMessageError("IsSellerOrBuyerDifferentFromDeclaration true, HasAddtionalInfoINF00200 true", instruction.CEI_StyleInfo, messageError1);

				addInfo.CSI_Code = "00000";
				validation.ValidateCEI_Style();
				AssertHasMessageError("IsSellerOrBuyerDifferentFromDeclaration true, HasAddtionalInfoINF00200 false", instruction.CEI_StyleInfo, messageError1);

				instruction.CEI_Style = ImportDeclarationTypeList.Codes.H3;
				validation.ValidateCEI_Style();
				AssertNoMessageError("CEI_Style isn't H1", instruction.CEI_StyleInfo, messageError1);

				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_CEI = instruction.PK;
				var addInfo_Line2 = invoiceLine2.AdditionalInfos.AddNew();
				addInfo_Line2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				addInfo_Line2.CSI_Code = "00000";
				instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				AssertHasMessageError("Multiple lines-Line1", instruction.CEI_StyleInfo, messageError1);
				AssertHasMessageError("Multiple lines-Line2", instruction.CEI_StyleInfo, messageError2);
			});
		}

		public void TestCheckCEI_OH_Owner_BR8F0001()
		{
			var message = "[BR8F0001] Primary Owner of Goods is required.";
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H3;
			var invoiceLine = jobDeclaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;
			var info = instruction.AdditionalInfos.AddNew();
			info.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			info.CSI_Code = Constants.AdditionalInformationCodes._00100;

			instruction.Validation.ValidateCEI_OH_Owner();
			AssertHasMessageError("CEI_Style is H3, Requested Procedure is 53, there is a AdditionalInfo whose CSI_SubType is INF and CSI_Code is 00100, Primary Owner of Goods is empty and Owners of Goods is empty , there should be a message error.", instruction.CEI_OH_OwnerInfo, message);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			instruction.Validation.ValidateCEI_OH_Owner();
			AssertNoMessageError("CEI_Style is not H3, there should be no message error.", instruction.CEI_OH_OwnerInfo, message);
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H3;

			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._01;
			instruction.Validation.ValidateCEI_OH_Owner();
			AssertNoMessageError("Requested Procedure is not 53, there should be no message error.", instruction.CEI_OH_OwnerInfo, message);
			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._53;

			info.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			instruction.Validation.ValidateCEI_OH_Owner();
			AssertNoMessageError("info CSI_SubType is not INF, there should be no message error.", instruction.CEI_OH_OwnerInfo, message);
			info.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

			instruction.CEI_OH_Owner = Factory.New<OrgHeader>().PK;
			instruction.Validation.ValidateCEI_OH_Owner();
			AssertNoMessageError("Primary Owner of Goods is not empty, there should be no message error.", instruction.CEI_OH_OwnerInfo, message);
			instruction.CEI_OH_Owner = ZGuid.Empty;

			var goods = instruction.OwnerOfGoodsCollection.AddNew();
			goods.OrganisationPK = Factory.New<OrgHeader>().PK;
			instruction.Validation.ValidateCEI_OH_Owner();
			AssertNoMessageError("Owner of Goods is not empty, there should be no message error.", instruction.CEI_OH_OwnerInfo, message);
			goods.Delete();

			instruction.Validation.ValidateCEI_OH_Owner();
			AssertHasMessageError("CEI_Style is H3, Requested Procedure is 53, there is a AdditionalInfo whose CSI_SubType is INF and CSI_Code is 00100, Primary Owner of Goods is empty and Owners of Goods is empty , there should be a message error.", instruction.CEI_OH_OwnerInfo, message);
		}

		public void TestValidateRuleBR8F0007()
		{
			AssertRuleBR8F0007(ImportDeclarationTypeList.Codes.H1, ProcedureCodes.ProcedureCode._44);
			AssertRuleBR8F0007(ImportDeclarationTypeList.Codes.H3, ProcedureCodes.ProcedureCode._53);
			AssertRuleBR8F0007(ImportDeclarationTypeList.Codes.H4, ProcedureCodes.ProcedureCode._51);
		}

		void AssertRuleBR8F0007(string style, string procedure)
		{
			var expectedMessageErrorMessage = "[BR8F0007] First Place of Use or Processing is required.";
			instruction.CEI_Style = style;

			var invoiceLine = instruction.JobDeclaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = procedure;

			var additionalInfo = invoiceLine.AdditionalInfos.FirstOrAddNew<AdditionalInfo>();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Code = AdditionalReferenceCodes._1A06;

			var firstPlaceOfUseOrProcessing = instruction.FirstPlaceOfUseOrProcessing;
			firstPlaceOfUseOrProcessing.CGL_Qualifier = "";
			firstPlaceOfUseOrProcessing.CGL_AdditionalIdentifier = "";

			var info = instruction.FirstPlaceOfUseOrProcessingDescriptionInfo;
			var validation = (T)instruction.Validation;
			CombineAssertions($"When CEI_Style: {style}, Requested Procedure: {procedure}", () =>
			{
				validation.ValidateFirstPlaceOfUseOrProcessingDescription();
				AssertNoMessageError("No 00100 INF, no BR8F0007 check.", info, expectedMessageErrorMessage);

				additionalInfo.CSI_Code = AdditionalInformationCodes._00100;
				validation.ValidateFirstPlaceOfUseOrProcessingDescription();
				AssertHasMessageError("BR8F0007 check.", info, expectedMessageErrorMessage);

				firstPlaceOfUseOrProcessing.CGL_Qualifier = "U";
				firstPlaceOfUseOrProcessing.CGL_AdditionalIdentifier = "U";
				validation.ValidateFirstPlaceOfUseOrProcessingDescription();
				AssertNoMessageError("BR8F0007 check, passes.", info, expectedMessageErrorMessage);
			});
		}

		public void TestValidateRuleBR8F0008()
		{
			AssertRuleBR8F0008(ImportDeclarationTypeList.Codes.H1, ProcedureCodes.ProcedureCode._44);
			AssertRuleBR8F0008(ImportDeclarationTypeList.Codes.H3, ProcedureCodes.ProcedureCode._53);
			AssertRuleBR8F0008(ImportDeclarationTypeList.Codes.H4, ProcedureCodes.ProcedureCode._51);
		}

		void AssertRuleBR8F0008(string style, string procedure)
		{
			var expectedMessageErrorMessage = "[BR8F0008] Please enter at least one row in the below Places of Use or Processing grid.";
			instruction.CEI_Style = style;
			var invoiceLine = jobDeclaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = procedure;

			var additionalInfo = invoiceLine.AdditionalInfos.FirstOrAddNew<AdditionalInfo>();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Code = AdditionalReferenceCodes._1A06;

			instruction.PlaceOfUseOrProcessingCollection.RemoveAll();

			var info = instruction.FirstPlaceOfUseOrProcessingDescriptionInfo;
			var validation = (T)instruction.Validation;
			CombineAssertions($"When CEI_Style: {style}, Requested Procedure: {procedure}", () =>
			{
				validation.ValidateFirstPlaceOfUseOrProcessingDescription();
				AssertNoMessageError("No 00100 INF, no BR8F0008 check.", info, expectedMessageErrorMessage);

				additionalInfo.CSI_Code = AdditionalInformationCodes._00100;
				validation.ValidateFirstPlaceOfUseOrProcessingDescription();
				AssertHasMessageError("BR8F0008 check.", info, expectedMessageErrorMessage);

				instruction.PlaceOfUseOrProcessingCollection.AddNew();
				validation.ValidateFirstPlaceOfUseOrProcessingDescription();
				AssertNoMessageError("BR8F0008 check, passes.", info, expectedMessageErrorMessage);
			});
		}

		public void TestValidateRuleBR8F0014()
		{
			AssertRuleBR8F0014(ImportDeclarationTypeList.Codes.H1, ProcedureCodes.ProcedureCode._44);
			AssertRuleBR8F0014(ImportDeclarationTypeList.Codes.H3, ProcedureCodes.ProcedureCode._53);
			AssertRuleBR8F0014(ImportDeclarationTypeList.Codes.H4, ProcedureCodes.ProcedureCode._51);
		}

		void AssertRuleBR8F0014(string style, string procedure)
		{
			var expectedMessageErrorMessage = "[BR8F0014] Details of Planned Activities is required.";
			instruction.CEI_Style = style;
			var invoiceLine = jobDeclaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = procedure;

			var additionalInfo = invoiceLine.AdditionalInfos.FirstOrAddNew<AdditionalInfo>();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Code = AdditionalReferenceCodes._1A06;

			instruction.DetailsOfPlannedActivities = ZString.Empty;

			var info = instruction.DetailsOfPlannedActivitiesInfo;
			var validation = (T)instruction.Validation;
			CombineAssertions($"When CEI_Style: {style}, Requested Procedure: {procedure}", () =>
			{
				validation.ValidateDetailsOfPlannedActivities();
				AssertNoMessageError("No 00100 INF, no BR8F0014 check.", info, expectedMessageErrorMessage);

				additionalInfo.CSI_Code = AdditionalInformationCodes._00100;
				validation.ValidateDetailsOfPlannedActivities();
				AssertHasMessageError("BR8F0014 check.", info, expectedMessageErrorMessage);

				instruction.DetailsOfPlannedActivities = "Planned Activities";
				validation.ValidateDetailsOfPlannedActivities();
				AssertNoMessageError("BR8F0014 check, passes.", info, expectedMessageErrorMessage);
			});
		}

		public void TestValidateBR5153()
		{
			jobDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceLine = jobDeclaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._51;
			validation.ValidateAll();

			CombineAssertions("Validate BR515", () =>
			{
				AssertHasRowMessageError("BR5153: warning if 51 and neither 00100 nor C601.", instruction, MessageError_BR5153);

				var ceiAddInfo = instruction.AdditionalInfos.AddNew();
				ceiAddInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				validation.ValidateAll();
				AssertHasRowMessageError("BR5153: warning with empty INF.", instruction, MessageError_BR5153);

				ceiAddInfo.CSI_Code = AdditionalInformationCodes._00100;
				validation.ValidateAll();
				AssertNoRowMessageError("BR5153: pass when having INF 00100.", instruction, MessageError_BR5153);
				instruction.AdditionalInfos.RemoveAndDelete(ceiAddInfo);

				var ceiSupportingDocument = instruction.SupportingDocuments.AddNew();
				validation.ValidateAll();
				AssertHasRowMessageError("BR5153: warning when having empty SupportingDocument.", instruction, MessageError_BR5153);

				ceiSupportingDocument.CSI_Code = SupportingDocumentCodes.AuthorisationInwardProcessingProcedure;
				validation.ValidateAll();
				AssertNoRowMessageError("BR5153: pass when having C601.", instruction, MessageError_BR5153);
				instruction.SupportingDocuments.RemoveAndDelete(ceiSupportingDocument);

				var invoiceHeader = jobDeclaration.Invoices.AddNew();
				invoiceHeader.AdditionalInfos.AddNew(AdditionalInformationCodes._00100, string.Empty).CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				invoiceHeader.SupportingDocuments.AddNew(SupportingDocumentCodes.AuthorisationInwardProcessingProcedure, string.Empty);
				validation.ValidateAll();
				AssertHasRowMessageError("BR5153: warning when having INF 00100 and C601 exists on an Invoice not linked to CEI.", instruction, MessageError_BR5153);

				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				validation.ValidateAll();
				AssertNoRowMessageError("BR5153: pass when having 00100 and C601 exists on an Invoice not linked to CEI.", instruction, MessageError_BR5153);

				invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._42;
				validation.ValidateAll();
				AssertNoRowMessageError("BR5153: invalid with non-51 JI_Procedure.", instruction, MessageError_BR5153);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, "Procedure Code");

			CreateProcedureCode(ProcedureCodes.ProcedureCode._21);
			CreateProcedureCode(ProcedureCodes.ProcedureCode._44);
			CreateProcedureCode(ProcedureCodes.ProcedureCode._51);
			CreateProcedureCode(ProcedureCodes.ProcedureCode._53);
			CreateProcedureCode(ProcedureCodes.ProcedureCode._71);
			CreateProcedureCode(ProcedureCodes.ProcedureCode._76);
			CreateProcedureCode(ProcedureCodes.ProcedureCode._77);

			Factory.Save();
		}

		void CreateProcedureCode(string code, string group = ImportDeclarationTypeList.Codes.H1)
		{
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Ireland, "A", code, code, "CPC1", "CPC1 Desc", MessageTypeList.Codes.Import, group: group);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ProcedureCode, code, "CPC1 Desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		protected override string MessageType => MessageTypeList.Codes.Import;

		UniversalReferenceTestDataHelper helper;

		const string MessageError_BR1010 = "[BR1010] Additional Declaration Type cannot be B, E, or X.";
		const string MessageError_BR1021 = "[BR1021] Additional Declaration Type is required when Declaration Type is H1, H2, H3, H4, H6, or I1.";
		const string MessageError_BR2011 = "[BR2011] Please enter at least one previous document under Entry Instructions > Previous Documents.";
		const string MessageError_BR2027_I1 = "[BR2027] Additional Declaration Type can only contain one of the following values: C, F, Z.";
		const string MessageError_BR2027_NotI1 = "[BR2027] Additional Declaration Type can only contain one of the following values: A, D, Y, Z.";
		const string MessageError_BR6142 = "[BR6142] Supplementary additional declaration types are not allowed when the previous dataset is 'I1', and the total invoice amount is less than or equal to €22";
		const string MessageError_BR5153 = "[BR5153] If Requested Procedure is '51', then please either provide either a 'C601' Supporting Document under the Entry Instructions > Supporting Documents tab, or a '00100' Additional Information under the Entry Instructions > Additional Documents tab. Do not provide both.";
		const string MessageError_BR8076 = "[BR8076] If [11 09 001 000] Requested Procedure is '51', then [Annex A 6/2] Conditions and Terms > Economic Conditions > Processing Procedure Code can't be 6, 7, 8, 9 or 22.";
	}
}
