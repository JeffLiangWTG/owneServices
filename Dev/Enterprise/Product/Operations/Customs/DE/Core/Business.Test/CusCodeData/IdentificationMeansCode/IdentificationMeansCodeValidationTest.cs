using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	class IdentificationMeansCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code_UniqueCode()
		{
			const string message = "Type must be unique.";
			identificationMeansCode.CY_Code = IdentificationMeansList.Codes.S;
			var identificationMeansCode2 = entryInstruction.IdentificationMeanCodes.AddNew();

			NUnit.Framework.Assert.Multiple(() =>
			{
				identificationMeansCode2.CY_Code = IdentificationMeansList.Codes.A;
				AssertNoMessageError("CEI_Style='1*****', unique code", identificationMeansCode2.CY_CodeInfo, message);

				identificationMeansCode2.CY_Code = IdentificationMeansList.Codes.S;
				AssertHasMessageError("CEI_Style='1*****', duplicate code", identificationMeansCode2.CY_CodeInfo, message);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._200000;
				identificationMeansCode2.Validation.ValidateCY_Code();
				AssertNoMessageError("CEI_Style!='1*****', duplicate code", identificationMeansCode2.CY_CodeInfo, message);
			});
		}

		public void TestCheckCY_Code_ListValidation()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				identificationMeansCode.CY_Code = "~";
				AssertHasMessageError("CEI_Style='1*****', invalid code", identificationMeansCode.CY_CodeInfo, ListValidation.InvalidCodeMessageError);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._200000;
				identificationMeansCode.CY_Code = "~";
				AssertNoMessageError("CEI_Style!='1*****', invalid code", identificationMeansCode.CY_CodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCY_Code_OnlyOneWithout()
		{
			const string onlyOneORecordAllowed = "Type 'O' allows only one record of Identification Means.";
			NUnit.Framework.Assert.Multiple(() =>
			{
				identificationMeansCode.CY_Code = IdentificationMeansList.Codes.O;
				AssertNoMessageError("CEI_Style='1*****', CY_Code='O', only one code", identificationMeansCode.CY_CodeInfo, onlyOneORecordAllowed);

				var identificationMeansCode2 = entryInstruction.IdentificationMeanCodes.AddNew();
				identificationMeansCode2.CY_Code = IdentificationMeansList.Codes.O;
				identificationMeansCode.Validation.ValidateCY_Code();
				AssertHasMessageError("CEI_Style='1*****', CY_Code='O', multiple codes", identificationMeansCode.CY_CodeInfo, onlyOneORecordAllowed);

				identificationMeansCode2.CY_Code = IdentificationMeansList.Codes.S;
				identificationMeansCode.Validation.ValidateCY_Code();
				AssertHasMessageError("CEI_Style='1*****', CY_Code='O', multiple codes", identificationMeansCode.CY_CodeInfo, onlyOneORecordAllowed);

				identificationMeansCode.CY_Code = IdentificationMeansList.Codes.A;
				AssertNoMessageError("CEI_Style='1*****', CY_Code!='O', multiple codes", identificationMeansCode.CY_CodeInfo, onlyOneORecordAllowed);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._200000;
				identificationMeansCode.CY_Code = IdentificationMeansList.Codes.O;
				AssertNoMessageError("CEI_Style!='1*****', CY_Code='O', multiple codes", identificationMeansCode.CY_CodeInfo, onlyOneORecordAllowed);
			});
		}

		public void TestCheckCY_Code_UsualReplacement()
		{
			const string usualReplacementFlagRequired = "Type 'O' requires the Usual Replacement Flag to be selected on all Invoice Lines linked to this Entry Instruction.";
			var invoice = entryInstruction.JobDeclaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.ZG_UsualReplacement = true;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			NUnit.Framework.Assert.Multiple(() =>
			{
				identificationMeansCode.CY_Code = IdentificationMeansList.Codes.A;
				AssertNoMessageError("CEI_Style='1*****', CY_Code!='O', ZG_UsualReplacement=False", identificationMeansCode.CY_CodeInfo, usualReplacementFlagRequired);

				identificationMeansCode.CY_Code = IdentificationMeansList.Codes.O;
				AssertHasMessageError("CEI_Style='1*****', CY_Code='O', ZG_UsualReplacement=False", identificationMeansCode.CY_CodeInfo, usualReplacementFlagRequired);

				invoiceLine2.ZG_UsualReplacement = true;
				identificationMeansCode.Validation.ValidateCY_Code();
				AssertNoMessageError("CEI_Style='1*****', CY_Code='O', ZG_UsualReplacement=True", identificationMeansCode.CY_CodeInfo, usualReplacementFlagRequired);

				invoiceLine2.ZG_UsualReplacement = false;
				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._200000;
				identificationMeansCode.CY_Code = IdentificationMeansList.Codes.O;
				AssertNoMessageError("CEI_Style!='1*****', CY_Code='O', ZG_UsualReplacement=False", identificationMeansCode.CY_CodeInfo, usualReplacementFlagRequired);
			});
		}

		public void TestCheckCY_Data()
		{
			const string message = "Description is mandatory for this Type.";
			NUnit.Framework.Assert.Multiple(() =>
			{
				identificationMeansCode.CY_Code = IdentificationMeansList.Codes.S;
				identificationMeansCode.Validation.ValidateCY_Data();
				AssertHasMessageError("CEI_Style='1*****', CY_Code='S'", identificationMeansCode.CY_DataInfo, message);

				identificationMeansCode.CY_Code = IdentificationMeansList.Codes.A;
				identificationMeansCode.Validation.ValidateCY_Data();
				AssertNoMessageError("CEI_Style='1*****', CY_Code!='S'", identificationMeansCode.CY_DataInfo, message);

				entryInstruction.CEI_Style = ExportDeclarationTypeProcedureList.Codes._200000;
				identificationMeansCode.CY_Code = IdentificationMeansList.Codes.S;
				identificationMeansCode.Validation.ValidateCY_Data();
				AssertNoMessageError("CEI_Style!='1*****', CY_Code='S'", identificationMeansCode.CY_DataInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			identificationMeansCode = Factory.CreateIdentificationMeansCode();
			entryInstruction = (CusEntryInstruction)identificationMeansCode.Parent;
		}

		CusEntryInstruction entryInstruction;
		IdentificationMeansCode identificationMeansCode;
	}
}
