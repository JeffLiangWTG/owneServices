using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
{
	public void TestConditionC075_FieldsForbidden_IMPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();

		additionalInfo.CSI_Code = "AA";
		additionalInfo.CSI_NctsExportFromEC = false;
		additionalInfo.CSI_RN_NKCountryCode = ZString.Empty;

		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);

		additionalInfo.CSI_NctsExportFromEC = true;
		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);

		additionalInfo.CSI_NctsExportFromEC = false;
		additionalInfo.CSI_RN_NKCountryCode = "IT";
		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);
	}

	public void TestConditionC075_FieldsForbidden_EXPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();

		additionalInfo.CSI_Code = "AA";
		additionalInfo.CSI_NctsExportFromEC = false;
		additionalInfo.CSI_RN_NKCountryCode = ZString.Empty;

		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);

		additionalInfo.CSI_NctsExportFromEC = true;
		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);

		additionalInfo.CSI_NctsExportFromEC = false;
		additionalInfo.CSI_RN_NKCountryCode = "IT";
		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);
	}

	public void TestConditionC075_FieldsRequired_IMPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();

		additionalInfo.CSI_Code = "AA";
		additionalInfo.CSI_NctsExportFromEC = false;
		additionalInfo.CSI_RN_NKCountryCode = ZString.Empty;

		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);

		additionalInfo.CSI_Code = "DG0";
		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);

		additionalInfo.CSI_NctsExportFromEC = true;
		additionalInfo.CSI_RN_NKCountryCode = "IT";
		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);

		additionalInfo.CSI_NctsExportFromEC = true;
		additionalInfo.CSI_RN_NKCountryCode = ZString.Empty;
		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);

		additionalInfo.CSI_NctsExportFromEC = false;
		additionalInfo.CSI_RN_NKCountryCode = "IT";
		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);
	}

	public void TestConditionC075_FieldsRequired_EXPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();

		additionalInfo.CSI_Code = "AA";
		additionalInfo.CSI_NctsExportFromEC = false;
		additionalInfo.CSI_RN_NKCountryCode = ZString.Empty;

		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);

		additionalInfo.CSI_Code = "DG0";
		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);

		additionalInfo.CSI_NctsExportFromEC = true;
		additionalInfo.CSI_RN_NKCountryCode = "IT";
		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);

		additionalInfo.CSI_NctsExportFromEC = true;
		additionalInfo.CSI_RN_NKCountryCode = ZString.Empty;
		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);

		additionalInfo.CSI_NctsExportFromEC = false;
		additionalInfo.CSI_RN_NKCountryCode = "IT";
		additionalInfo.Validation.ValidateAll();
		AssertNoMessageErrors(additionalInfo.CSI_RN_NKCountryCodeInfo);
	}

	public void TestValidateAdditionalInfoLineUniqueMaxCountInEntryLine_IMPDeclaration()
	{
		var expectedMessage = "Only 99 lines of Additional Info are allowed for an Entry Line.";
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();

		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;

		for (int i = 1; i <= 50; i++)
		{
			var additionalInfo1 = invoiceLine1.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "A" + i;
			additionalInfo1.CSI_Description = "A" + i;
		}

		for (int i = 1; i <= 49; i++)
		{
			var additionalInfo2 = invoiceLine2.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "B" + i;
			additionalInfo2.CSI_Description = "B" + i;
		}

		AssertEquals("AdditionalInfo from different invoice lines with same entry line", 99, GetAdditionalInfosInEntryLine().Count());
		AssertAdditionalInfosRowErrorMessage("When 99 lines of AdditionalInfo added for Entry line", false);

		var additionalInfoB1 = invoiceLine2.AdditionalInfos.AddNew();
		additionalInfoB1.CSI_Code = "B" + 1;
		additionalInfoB1.CSI_Description = "B" + 1;

		AssertEquals("AdditionalInfo from different invoice lines with same entry line, after adding duplicate", 100, GetAdditionalInfosInEntryLine().Count());
		AssertEquals("AdditionalInfo(Code + Description) from different invoice lines with same entry line, after adding duplicate", 99, GetAdditionalInfosDistinctCodeDescriptionInEntryLine().Count());
		AssertAdditionalInfosRowErrorMessage("When 100 lines of AdditionalInfo, 99 are distinct (Code + Description), added for Entry line", false);

		var additionalInfoB50 = invoiceLine2.AdditionalInfos.AddNew();
		additionalInfoB50.CSI_Code = "B" + 50;
		additionalInfoB50.CSI_Description = "B" + 50;

		AssertEquals("AdditionalInfo from different invoice lines with same entry line, after adding new additional info", 101, GetAdditionalInfosInEntryLine().Count());
		AssertEquals("AdditionalInfo(Code + Description) from different invoice lines with same entry line, after adding new additional info", 100, GetAdditionalInfosDistinctCodeDescriptionInEntryLine().Count());
		AssertAdditionalInfosRowErrorMessage("When 101 lines of AdditionalInfo, 100 are distinct (Code + Description), added for Entry line", true);

		IEnumerable<AdditionalInfo> GetAdditionalInfosInEntryLine() => entryLine.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(inv => inv.AdditionalInfos.Cast<AdditionalInfo>());
		IEnumerable<string> GetAdditionalInfosDistinctCodeDescriptionInEntryLine() => GetAdditionalInfosInEntryLine().Select(add => add.CSI_Code + add.CSI_Description).Distinct();

		void AssertAdditionalInfosRowErrorMessage(string message, bool hasMessage)
		{
			CombineAssertions(message, () =>
			{
				foreach (var additionalInfo in GetAdditionalInfosInEntryLine())
				{
					additionalInfo.Validation.ValidateAll();
					if (hasMessage)
					{
						AssertHasRowMessageErrorContaining(additionalInfo, expectedMessage);
					}
					else
					{
						AssertNoRowMessageErrorContaining(additionalInfo, expectedMessage);
					}
				}
			});
		}
	}

	public void TestValidateAdditionalInfoLineIsUniqueInEntryLine_IMPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();

		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;

		var additionalInfo1 = invoiceLine1.AdditionalInfos.AddNew();
		additionalInfo1.CSI_Description = "ABC1";

		additionalInfo1.Validation.ValidateAll();

		AssertNoRowMessageErrors(additionalInfo1);

		var additionalInfo2 = invoiceLine2.AdditionalInfos.AddNew();
		additionalInfo2.CSI_Description = "ABC1";

		additionalInfo1.Validation.ValidateAll();
		additionalInfo2.Validation.ValidateAll();
		AssertNoRowMessageErrors(additionalInfo1);
		AssertNoRowMessageErrors(additionalInfo2);
	}

	public void TestValidateAdditionalInfoLineIsUniqueInEntryLine_EXPDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var invoiceLine1 = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var invoiceLine2 = declaration.Invoices.AddNew().InvoiceLines.AddNew();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();

		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;

		var additionalInfo1 = invoiceLine1.AdditionalInfos.AddNew();

		additionalInfo1.Validation.ValidateAll();

		AssertNoRowMessageErrors(additionalInfo1);

		var additionalInfo2 = invoiceLine2.AdditionalInfos.AddNew();

		additionalInfo1.Validation.ValidateAll();
		additionalInfo2.Validation.ValidateAll();
		AssertNoRowMessageErrors(additionalInfo1);
		AssertNoRowMessageErrors(additionalInfo2);
	}

	public void TestDescriptionMaxLengthValidation()
	{
		var expectedErrorMessage = "Description exceeds the maximum allowed length in the declaration message (70 characters). Excess characters will be truncated.";
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();

		declaration.JE_MessageType = "EXP";
		additionalInfo.CSI_Description = new string('0', 70);
		AssertNoWarningContaining("When MessageType = EXP, Description length is 70", additionalInfo.CSI_DescriptionInfo, expectedErrorMessage);
		additionalInfo.CSI_Description = new string('0', 100);
		AssertHasWarningContaining("When MessageType = EXP, Description length is 100", additionalInfo.CSI_DescriptionInfo, expectedErrorMessage);

		declaration.JE_MessageType = "IMP";
		additionalInfo.CSI_Description = new string('0', 70);
		AssertNoWarningContaining("When MessageType = IMP, Description length is 70", additionalInfo.CSI_DescriptionInfo, expectedErrorMessage);
		additionalInfo.CSI_Description = new string('0', 100);
		AssertNoWarningContaining("When MessageType = IMP, Description length is 100", additionalInfo.CSI_DescriptionInfo, expectedErrorMessage);
	}

	public void TestCodeOrDescriptionMandatoryValidation()
	{
		var expectedMessage = "Code or Description must be filled.";
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();

		AssertEquals("Pre:CSI_Code", ZString.Empty, additionalInfo.CSI_Code);
		AssertEquals("Pre:CSI_Description", ZString.Empty, additionalInfo.CSI_Description);

		declaration.JE_MessageType = "EXP";
		additionalInfo.Validation.ValidateAll();
		AssertNoRowMessageErrorContaining(additionalInfo, expectedMessage);

		declaration.JE_MessageType = "IMP";
		additionalInfo.Validation.ValidateAll();
		AssertHasRowMessageErrorContaining(additionalInfo, expectedMessage);

		additionalInfo.CSI_Code = "ABC";
		additionalInfo.Validation.ValidateAll();
		AssertEquals("CSI_Description", ZString.Empty, additionalInfo.CSI_Description);
		AssertNoRowMessageErrorContaining(additionalInfo, expectedMessage);

		additionalInfo.CSI_Code = ZString.Empty;
		additionalInfo.CSI_Description = "XYZ";
		additionalInfo.Validation.ValidateAll();
		AssertEquals("CSI_Code", ZString.Empty, additionalInfo.CSI_Code);
		AssertNoRowMessageErrorContaining(additionalInfo, expectedMessage);
	}

	public void TestCheckCSI_CodeMandatory()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
		additionalInfo.CSI_Description = "XYZ";

		AssertEquals("Pre:CSI_Code", ZString.Empty, additionalInfo.CSI_Code);

		declaration.JE_MessageType = "EXP";
		additionalInfo.Validation.ValidateCSI_Code();
		AssertHasMessageErrorContaining("For EXP, CSI Code", additionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_MessageType = "IMP";
		additionalInfo.Validation.ValidateCSI_Code();
		AssertNoMessageErrorContaining("For IMP, CSI Code", additionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

		additionalInfo.CSI_Code = "ABC";
		declaration.JE_MessageType = "EXP";
		additionalInfo.Validation.ValidateCSI_Code();
		AssertNoMessageErrorContaining("For EXP, CSI Code not empty", additionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

		declaration.JE_MessageType = "IMP";
		additionalInfo.Validation.ValidateCSI_Code();
		AssertNoMessageErrorContaining("For IMP, CSI Code not empty", additionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCSI_CodeInList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
		additionalInfo.CSI_Code = "SJE";

		declaration.JE_MessageType = "EXP";
		additionalInfo.Validation.ValidateCSI_Code();
		AssertNoMessageErrorContaining("For EXP, CSI Code", additionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError.ToString());

		declaration.JE_MessageType = "IMP";
		additionalInfo.Validation.ValidateCSI_Code();
		AssertHasMessageErrorContaining("For IMP, CSI Code", additionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError.ToString());
	}
}
