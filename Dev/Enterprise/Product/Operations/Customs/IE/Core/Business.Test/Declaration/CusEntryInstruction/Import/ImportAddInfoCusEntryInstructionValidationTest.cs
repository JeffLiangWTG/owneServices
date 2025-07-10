using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Testing;
using static Enterprise.Customs.IE.Business.Constants;
using static Enterprise.Customs.IE.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ImportAddInfoCusEntryInstructionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_RateOfYield_RuleBR8F0003()
		{
			var messageError = "[BR8F0003] Rate of Yield is required.";
			var targetInfo = entryInstruction.ZG_RateOfYieldInfo;
			entryInstruction.CEI_Style = "H1";
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = "44";
			var additionalInfo1 = entryInstruction.AdditionalInfos.AddNew();
			additionalInfo1.CSI_SubType = "INF";
			additionalInfo1.CSI_Code = "00100";

			CombineAssertions(() =>
			{
				entryInstruction.ZG_RateOfYield = "Rate1";
				AssertNoMessageError("style is H1, procedure code is 44 and Rate of Yield is not empty, has valid additional info", targetInfo, messageError);

				entryInstruction.ZG_RateOfYield = ZString.Empty;
				AssertHasMessageError("style is H1, procedure code is 44 and Rate of Yield is empty, has valid additional info", targetInfo, messageError);

				invoiceLine.JI_Procedure = "51";
				entryInstruction.AddInfoValidation.ValidateZG_RateOfYield();
				AssertNoMessageError("style is H1, procedure code is 51 and Rate of Yield is empty, has valid additional info", targetInfo, messageError);

				entryInstruction.CEI_Style = "H4";
				entryInstruction.AddInfoValidation.ValidateZG_RateOfYield();
				AssertHasMessageError("style is H4, procedure code is 51 and Rate of Yield is empty, has valid additional info", targetInfo, messageError);

				additionalInfo1.CSI_Code = "123456";
				entryInstruction.AddInfoValidation.ValidateZG_RateOfYield();
				AssertNoMessageError("style is H4, procedure code is 51 and Rate of Yield is empty, has invalid additional info", targetInfo, messageError);
			});
		}

		public void TestCheckZG_IdOfGoodCode_R8F0013()
		{
			var message = "[BR8F00013] Identification of Goods > Code is required.";
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = ProcedureCodes.ProcedureCode._44;
			var addInfo = invoiceLine.AdditionalInfos.AddNew();
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addInfo.CSI_Code = AdditionalInformationCodes._00100;
			AssertEquals("Precondition", true, entryInstruction.IsPlacesOfUsageAndDetailsOfPlannedActivitiesRequired);

			entryInstruction.ZG_IdOfGoodCode = ZString.Empty;
			AssertHasMessageError(entryInstruction.ZG_IdOfGoodCodeInfo, message);

			entryInstruction.ZG_IdOfGoodCode = "1";
			AssertNoMessageError(entryInstruction.ZG_IdOfGoodCodeInfo, message);
		}

		public void TestCheckCEI_Style_BR8F0011() => CombineAssertions(() =>
		{
			AssertBR8F0011(ImportDeclarationTypeList.Codes.H1, ProcedureCodes.ProcedureCode._44, CusAuthorizationHeaderTypeList.Codes.EndUse);
			AssertBR8F0011(ImportDeclarationTypeList.Codes.H3, ProcedureCodes.ProcedureCode._53, CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission);
			AssertBR8F0011(ImportDeclarationTypeList.Codes.H4, ProcedureCodes.ProcedureCode._51, CusAuthorizationHeaderTypeList.Codes.InwardProcessing);
		});

		public void TestCheckZG_PeriodForDischarge()
		{
			AssertRuleBR8074();
		}

		void AssertBR8F0011(string style, string procedure, string authCode)
		{
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryInstruction.AdditionalInfos.RemoveAndDeleteAll();
			entryInstruction.ZG_PeriodForDischarge = 0;

			entryInstruction.CEI_Style = style;
			invoiceLine.JI_Procedure = procedure;
			entryInstruction.AddInfoValidation.ValidateZG_PeriodForDischarge();
			AssertNoMessageError($"When no 'INF', style is {style}, procedure is {procedure}, should have no error message", entryInstruction.ZG_PeriodForDischargeInfo, MessageError_BR8F0011);

			var addInfo = entryInstruction.AdditionalInfos.AddNew();
			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			addInfo.CSI_Code = AdditionalInformationCodes._00100;

			entryInstruction.AddInfoValidation.ValidateZG_PeriodForDischarge();
			AssertHasMessageError($"When has 'INF', style is {style}, procedure is {procedure}, should have error message", entryInstruction.ZG_PeriodForDischargeInfo, MessageError_BR8F0011);

			entryInstruction.ZG_PeriodForDischarge = 1;

			entryInstruction.AddInfoValidation.ValidateZG_PeriodForDischarge();
			AssertNoMessageError($"When has 'INF', style is {style}, procedure is {procedure}, should have no error message", entryInstruction.ZG_PeriodForDischargeInfo, MessageError_BR8F0011);
		}

		void AssertRuleBR8074()
		{
			var targetPropertyInfo = entryInstruction.ZG_PeriodForDischargeInfo;
			var humanReadableName = targetPropertyInfo.HumanReadableName;
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var messageGreaterThan6 = $"[BR8074] If Requested Procedure is 51 or 44, {humanReadableName} cannot be greater than 6.";
			var messageGreaterThan24 = $"[BR8074] {humanReadableName} cannot be greater than 24.";

			entryInstruction.ZG_PeriodForDischarge = 5;
			entryInstruction.AddInfoValidation.ValidateZG_PeriodForDischarge();
			AssertNoMessageErrorContaining(targetPropertyInfo, messageGreaterThan6);
			AssertNoMessageErrorContaining(targetPropertyInfo, messageGreaterThan24);

			entryInstruction.ZG_PeriodForDischarge = 25;
			entryInstruction.AddInfoValidation.ValidateZG_PeriodForDischarge();
			AssertNoMessageErrorContaining(targetPropertyInfo, messageGreaterThan6);
			AssertHasMessageErrorContaining(targetPropertyInfo, messageGreaterThan24);

			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._44;
			entryInstruction.AddInfoValidation.ValidateZG_PeriodForDischarge();
			AssertHasMessageErrorContaining(targetPropertyInfo, messageGreaterThan6);
			AssertHasMessageErrorContaining(targetPropertyInfo, messageGreaterThan24);

			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._51;
			entryInstruction.AddInfoValidation.ValidateZG_PeriodForDischarge();
			AssertHasMessageErrorContaining(targetPropertyInfo, messageGreaterThan6);
			AssertHasMessageErrorContaining(targetPropertyInfo, messageGreaterThan24);
		}

		public void TestCheckZG_BillOfDischargeIsNecessary_RuleBR8F0002()
		{
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var messageError = "[BR8F0002] Bill of Discharge is necessary.";
			entryInstruction.CEI_Style = "H1";
			invoiceLine.JI_Procedure = "44";
			var additionalInfo1 = entryInstruction.AdditionalInfos.AddNew();
			additionalInfo1.CSI_SubType = "INF";
			additionalInfo1.CSI_Code = "00100";

			CombineAssertions(() =>
			{
				entryInstruction.ZG_BillOfDischargeIsNecessary = true;
				AssertNoMessageError("style is H1, procedure code is 44 and bill of discharge is necessary is true, has valid additional info", entryInstruction.ZG_BillOfDischargeIsNecessaryInfo, messageError);

				entryInstruction.ZG_BillOfDischargeIsNecessary = false;
				AssertHasMessageError("style is H1, procedure code is 44 and bill of discharge is necessary is false, has valid additional info", entryInstruction.ZG_BillOfDischargeIsNecessaryInfo, messageError);

				invoiceLine.JI_Procedure = "51";
				entryInstruction.ZG_BillOfDischargeIsNecessary = false;
				entryInstruction.AddInfoValidation.ValidateZG_BillOfDischargeIsNecessary();
				AssertNoMessageError("style is H1, procedure code is 51 and bill of discharge is necessary is false, has valid additional info", entryInstruction.ZG_BillOfDischargeIsNecessaryInfo, messageError);

				entryInstruction.CEI_Style = "H4";
				entryInstruction.AddInfoValidation.ValidateZG_BillOfDischargeIsNecessary();
				AssertHasMessageError("style is H4, procedure code is 51 and bill of discharge is necessary is false, has valid additional info", entryInstruction.ZG_BillOfDischargeIsNecessaryInfo, messageError);

				additionalInfo1.CSI_Code = "123456";
				entryInstruction.AddInfoValidation.ValidateZG_BillOfDischargeIsNecessary();
				AssertNoMessageError("style is H4, procedure code is 51 and bill of discharge is necessary is false, has invalid additional info", entryInstruction.ZG_BillOfDischargeIsNecessaryInfo, messageError);
			});
		}

		public void TestCheckZG_ProcessingProcedureCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(
				entryInstruction.ZG_ProcessingProcedureCodeInfo,
				invalidCode: "XX",
				validCode: "1"
			);
		}

		public void TestCheckZG_ProcessingProcedureCode_BR8F0004()
		{
			var message = "[BR8F0004] Processing Procedures > Procedure Code is required.";
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H4;
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._51;
			var additionalInfo = entryInstruction.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			additionalInfo.CSI_Code = Constants.TransportDocumentCodes._N705;
			entryInstruction.AddInfoValidation.ValidateZG_ProcessingProcedureCode();
			AssertNoMessageError("Import, Style equal to H4, procedure equal to 51, there is not an additional Info where subtype is INF and code is 00100, no procedure code", entryInstruction.ZG_ProcessingProcedureCodeInfo, message);

			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			entryInstruction.AddInfoValidation.ValidateZG_ProcessingProcedureCode();
			AssertNoMessageError("Import, Style equal to H4, procedure equal to 51, there is an additional Info where subtype is INF but code is not 00100, no procedure code", entryInstruction.ZG_ProcessingProcedureCodeInfo, message);

			additionalInfo.CSI_Code = Constants.AdditionalInformationCodes._00100;
			entryInstruction.AddInfoValidation.ValidateZG_ProcessingProcedureCode();
			AssertHasMessageError("Import, Style equal to H4, procedure equal to 51, there is an additional Info where subtype is INF and code is 00100, no procedure code", entryInstruction.ZG_ProcessingProcedureCodeInfo, message);

			entryInstruction.ZG_ProcessingProcedureCode = "1";
			AssertNoMessageError("Has procedure code", entryInstruction.ZG_ProcessingProcedureCodeInfo, message);
		}

		public void TestCheckZG_Article86_3_UCC_RuleBR8F0005()
		{
			var message = "[BR8F0005] Calculate the Amount of Import Duty in Accordance with Article 86(3) of the Code is required when Dataset is 'H4' and Requested Procedure is '51'.";
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H4;
			var invoiceLine = declaration.Invoices.FirstOrAddNew<JobComInvoiceHeader>().InvoiceLines.FirstOrAddNew<JobComInvoiceLine>();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._51;
			var additionalInfo = entryInstruction.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalInfo.CSI_Code = AdditionalInformationCodes._00100;
			var validation = entryInstruction.AddInfoValidation;

			entryInstruction.ZG_Article86_3_UCC = ZString.Empty;
			CombineAssertions("ZG_Article86_3_UCC validations.", () =>
			{
				AssertHasMessageError("When Empty, H4, 51, INF, 00100", entryInstruction.ZG_Article86_3_UCCInfo, message);

				entryInstruction.ZG_Article86_3_UCC = YesNoList.Codes.Yes;
				AssertNoMessageError("When has value, H4, 51, INF, 00100", entryInstruction.ZG_Article86_3_UCCInfo, message);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
				entryInstruction.ZG_Article86_3_UCC = ZString.Empty;
				AssertNoMessageError("When Empty, H1, 51, INF, 00100", entryInstruction.ZG_Article86_3_UCCInfo, message);

				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H4;
				invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._44;
				validation.ValidateZG_Article86_3_UCC();
				AssertNoMessageError("When Empty, H4, 44, INF, 00100", entryInstruction.ZG_Article86_3_UCCInfo, message);

				invoiceLine.JI_Procedure = Core.Constants.Customs.Universal.RefCusProcedure.Codes._51;
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
				validation.ValidateZG_Article86_3_UCC();
				AssertNoMessageError("When Empty, H4, 51, TRA, 00100", entryInstruction.ZG_Article86_3_UCCInfo, message);

				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.CSI_Code = AdditionalInformationCodes._00500;
				validation.ValidateZG_Article86_3_UCC();
				AssertNoMessageError("When Empty, H4, 51, INF, 00500", entryInstruction.ZG_Article86_3_UCCInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;

		const string MessageError_BR8F0011 = "[BR8F0011] Period for Discharge > Period (Month) is required.";
	}
}
