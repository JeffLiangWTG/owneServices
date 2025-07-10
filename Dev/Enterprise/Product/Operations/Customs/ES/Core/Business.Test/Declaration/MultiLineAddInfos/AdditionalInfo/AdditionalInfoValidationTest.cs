using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCode_IsNotMandatoryForEntryEXS_ParentInvoiceHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invHeader = declaration.Invoices.AddNew();
			var additionalInfo = invHeader.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = ZString.Empty;
			CombineAssertions(() =>
			{
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
				var invoiceLine = invHeader.InvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				additionalInfo.Validation.ValidateCSI_Code();
				AssertHasMessageErrorContaining("CSI_Code is mandatory in ES, with entry NOT EXS", additionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				var invoiceLine1 = invHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction1.PK;
				additionalInfo.Validation.ValidateCSI_Code();
				AssertNoMessageErrors("CSI_Code is not mandatory in ES, with entry EXS.", additionalInfo.CSI_CodeInfo);
			});
		}

		public void TestIsOtherFieldsEnabledForExport()
		{
			var declarataion = Factory.New<JobDeclaration>();
			var additionalInfo = declarataion.AdditionalInfos.AddNew();

			CombineAssertions(() =>
			{
				declarataion.JE_MessageType = MessageTypeList.Codes.Export;
				additionalInfo.Validation.ValidateCSI_Status();
				AssertNoMessageErrorContaining("In Export has no message.", additionalInfo.CSI_StatusInfo, MandatoryValidation.YouHaveNotEntered);

				declarataion.JE_MessageType = MessageTypeList.Codes.Import;
				additionalInfo.Validation.ValidateCSI_Status();
				AssertHasMessageErrorContaining("In Import has message.", additionalInfo.CSI_StatusInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCode_IsNotMandatoryForEntryEXS_ParentInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invHeader = declaration.Invoices.AddNew();
			var invoiceLine = invHeader.InvoiceLines.AddNew();
			var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = ZString.Empty;
			CombineAssertions(() =>
			{
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;
				invoiceLine.JI_CEI = entryInstruction.PK;
				additionalInfo.Validation.ValidateCSI_Code();
				AssertHasMessageErrorContaining("CSI_Code is mandatory in ES, with entry NOT EXS", additionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction1.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
				invoiceLine.JI_CEI = entryInstruction1.PK;
				additionalInfo.Validation.ValidateCSI_Code();
				AssertNoMessageErrors("CSI_Code is not mandatory in ES, with entry EXS.", additionalInfo.CSI_CodeInfo);
			});
		}
	}
}
