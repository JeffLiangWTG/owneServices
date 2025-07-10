using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class ExportAddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_UsualReplacement()
		{
			var message = "For the selected Type (Procedure) and CPC – Code Previous Procedure 48 the Usual Replacement flag must be set.";
			var targetInfo = invoiceLine.ZG_UsualReplacementInfo;
			CombineAssertions(() =>
			{
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
				invoiceLine.JI_Procedure = CustomsProcedureCodeList.Export.ProcedureCode._10 + CustomsProcedureCodeList.Export.PreviousProcedureCode._48;
				invoiceLine.AddInfoValidation.ValidateZG_UsualReplacement();
				AssertHasMessageError("CEI_Style is '12****' and JI_Procedure is '**48'", targetInfo, message);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
				invoiceLine.AddInfoValidation.ValidateZG_UsualReplacement();
				AssertNoMessageError("CEI_Style isn't '12****' and JI_Procedure is '**48'", targetInfo, message);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
				invoiceLine.JI_Procedure = CustomsProcedureCodeList.Export.ProcedureCode._10 + CustomsProcedureCodeList.Export.PreviousProcedureCode._07;
				invoiceLine.AddInfoValidation.ValidateZG_UsualReplacement();
				AssertNoMessageError("CEI_Style is '12****' and JI_Procedure isn't '**48'", targetInfo, message);

				invoiceLine.JI_Procedure = CustomsProcedureCodeList.Export.ProcedureCode._10 + CustomsProcedureCodeList.Export.PreviousProcedureCode._48;
				invoiceLine.AddInfoValidation.ValidateZG_UsualReplacement();
				invoiceLine.ZG_UsualReplacement = ZBool.True;
				AssertNoMessageError("ZG_UsualReplacement is true", targetInfo, message);
			});
		}

		public void TestCheckZG_ReimportDate_Mandatory()
		{
			var targetInfo = invoiceLine.ZG_ReimportDateInfo;
			CombineAssertions(() =>
			{
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
				invoiceLine.AddInfoValidation.ValidateZG_ReimportDate();
				AssertHasMessageErrorContaining("CEI_Style is '12****'", targetInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
				invoiceLine.AddInfoValidation.ValidateZG_ReimportDate();
				AssertNoMessageErrorContaining("CEI_Style isn't '12****'", targetInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
				invoiceLine.ZG_ReimportDate = ZDateTime.Today;
				AssertNoMessageErrorContaining("ZG_ReimportDate isn't empty", targetInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckZG_ReimportDate_MustNotBeEarlierThanTheCurrentDate_ZG_UsualReplacementAndEntryHeaders()
		{
			var message = "The date of Re-Importation must not be earlier than the current date.";
			var targetInfo = invoiceLine.ZG_ReimportDateInfo;
			CombineAssertions(() =>
			{
				invoiceLine.ZG_ReimportDate = ZDateTime.Today.AddDays(-5);
				AssertHasMessageError("Reimport Date is earlier than current date", targetInfo, message);

				invoiceLine.ZG_ReimportDate = ZDateTime.Today.AddDays(5);
				AssertNoMessageError("Reimport Date is later than current date", targetInfo, message);

				invoiceLine.ZG_UsualReplacement = ZBool.True;
				invoiceLine.ZG_ReimportDate = ZDateTime.Today.AddDays(-5);
				AssertNoMessageError("ZG_UsualReplacement is true", targetInfo, message);

				invoiceLine.ZG_UsualReplacement = ZBool.False;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				invoiceLine.AddInfoValidation.ValidateZG_ReimportDate();
				AssertHasMessageError("All CH_EntryReleaseDates are empty", targetInfo, message);

				var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_EntryReleaseDate = ZDateTime.Today;
				invoiceLine.AddInfoValidation.ValidateZG_ReimportDate();
				AssertNoMessageError("There is a CH_EntryReleaseDate that isn't empty", targetInfo, message);
			});
		}

		public void TestCheckZG_ReimportDate_MustNotBeEarlierThanTheCurrentDate_JI_ProcedureAndInstruction()
		{
			var message = "The date of Re-Importation must not be earlier than the current date.";
			invoiceLine.ZG_UsualReplacement = ZBool.True;
			var targetInfo = invoiceLine.ZG_ReimportDateInfo;
			CombineAssertions(() =>
			{
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
				invoiceLine.JI_Procedure = CustomsProcedureCodeList.Export.ProcedureCode._10 + CustomsProcedureCodeList.Export.PreviousProcedureCode._00;
				invoiceLine.ZG_ReimportDate = ZDateTime.Today.AddDays(-5);
				AssertHasMessageError("CEI_Style is '12****' and JI_Procedure is '**00'", targetInfo, message);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
				invoiceLine.AddInfoValidation.ValidateZG_ReimportDate();
				AssertNoMessageError("CEI_Style isn't '12****' and JI_Procedure is '**00'", targetInfo, message);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
				invoiceLine.JI_Procedure = CustomsProcedureCodeList.Export.ProcedureCode._10 + CustomsProcedureCodeList.Export.PreviousProcedureCode._07;
				invoiceLine.AddInfoValidation.ValidateZG_ReimportDate();
				AssertNoMessageError("CEI_Style is '12****' and JI_Procedure isn't '**00'", targetInfo, message);

				invoiceLine.JI_Procedure = CustomsProcedureCodeList.Export.ProcedureCode._10 + CustomsProcedureCodeList.Export.PreviousProcedureCode._00;
				invoiceLine.ZG_ReimportDate = ZDateTime.Today.AddDays(5);
				AssertNoMessageError("ZG_ReimportDate is Today", targetInfo, message);
			});
		}

		public void TestCheckZG_ReimportDate_MustBeEarlierThanTheCurrentDate()
		{
			var message = "The date of Re-Importation must be earlier than the current date.";
			var targetInfo = invoiceLine.ZG_ReimportDateInfo;
			CombineAssertions(() =>
			{
				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
				invoiceLine.JI_Procedure = CustomsProcedureCodeList.Export.ProcedureCode._10 + "46";
				invoiceLine.ZG_ReimportDate = ZDateTime.Today.AddDays(5);
				AssertHasMessageError("CEI_Style is '12****' and JI_Procedure is '**46'", targetInfo, message);

				invoiceLine.JI_Procedure = CustomsProcedureCodeList.Export.ProcedureCode._10 + CustomsProcedureCodeList.Export.PreviousProcedureCode._48;
				invoiceLine.AddInfoValidation.ValidateZG_ReimportDate();
				AssertHasMessageError("CEI_Style is '12****' and JI_Procedure is '**48'", targetInfo, message);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._000000;
				invoiceLine.AddInfoValidation.ValidateZG_ReimportDate();
				AssertNoMessageError("CEI_Style isn't '12****' and JI_Procedure is '**48'", targetInfo, message);

				instruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._120000;
				invoiceLine.JI_Procedure = CustomsProcedureCodeList.Export.ProcedureCode._10 + CustomsProcedureCodeList.Export.PreviousProcedureCode._07;
				invoiceLine.AddInfoValidation.ValidateZG_ReimportDate();
				AssertNoMessageError("CEI_Style is '12****' and JI_Procedure isn't '**46' or '**48'", targetInfo, message);

				invoiceLine.JI_Procedure = CustomsProcedureCodeList.Export.ProcedureCode._10 + CustomsProcedureCodeList.Export.PreviousProcedureCode._48;
				invoiceLine.ZG_ReimportDate = ZDateTime.Today.AddDays(-5);
				AssertNoMessageError("ZG_ReimportDate is Today", targetInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
			instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
		}
		JobDeclaration declaration;
		CusEntryInstruction instruction;
		JobComInvoiceLine invoiceLine;
	}
}
