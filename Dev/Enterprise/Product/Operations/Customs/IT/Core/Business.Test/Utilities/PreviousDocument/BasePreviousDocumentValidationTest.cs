using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class BasePreviousDocumentValidationTest : TestCaseWithFactory
{
	protected abstract IPreviousDocumentForTesting GetPreviousDocumentForTesting();

	public void TestCheckCSI_ReferenceNumber()
	{
		var previousDocument = GetPreviousDocumentForTesting();
		previousDocument.CSI_Procedure = ZString.Empty;
		CombineAssertions("Checking CSI_ReferenceNumber validation when CSI_Procedure is Empty", delegate
		{
			previousDocument.CSI_ReferenceNumber = "123";
			AssertNoMessageErrorContaining("CSI_ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("CSI_ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertNoMessageErrorContaining("CSI_ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.DoNotEntered);
		});

		previousDocument.CSI_Procedure = "LC";
		CombineAssertions("Checking CSI_ReferenceNumber validation when CSI_Procedure is LC", delegate
		{
			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining("CSI_ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_ReferenceNumber = "123";
			AssertNoMessageErrorContaining("CSI_ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.DoNotEntered);
		});

		previousDocument.CSI_Procedure = "MRN";
		CombineAssertions("Checking CSI_ReferenceNumber validation when CSI_Procedure is MRN", delegate
		{
			previousDocument.CSI_ReferenceNumber = "123";
			AssertNoMessageErrorContaining("CSI_ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("CSI_ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_ReferenceNumber = ZString.Empty;
			AssertNoMessageErrorContaining("CSI_ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_ReferenceNumberInfo", previousDocument.CSI_ReferenceNumberInfo, MandatoryValidation.DoNotEntered);
		});
	}

	public void TestCheckCSI_ReferenceNumber2()
	{
		var previousDocument = GetPreviousDocumentForTesting();
		previousDocument.CSI_Procedure = ZString.Empty;
		CombineAssertions("Checking CSI_ReferenceNumber2 validation when CSI_Procedure is Empty", delegate
		{
			previousDocument.CSI_ReferenceNumber2 = "123";
			AssertNoMessageErrorContaining("CSI_ReferenceNumber2Info", previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("CSI_ReferenceNumber2Info", previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_ReferenceNumber2 = ZString.Empty;
			AssertNoMessageErrorContaining("CSI_ReferenceNumber2Info", previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_ReferenceNumber2Info", previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.DoNotEntered);
		});

		previousDocument.CSI_Procedure = "MRN";
		CombineAssertions("Checking CSI_ReferenceNumber2 validation when CSI_Procedure is Empty", delegate
		{
			previousDocument.CSI_ReferenceNumber2 = ZString.Empty;
			AssertHasMessageErrorContaining("CSI_ReferenceNumber2Info", previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_ReferenceNumber2Info", previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_ReferenceNumber2 = "123";
			AssertNoMessageErrorContaining("CSI_ReferenceNumber2Info", previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_ReferenceNumber2Info", previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.DoNotEntered);
		});

		previousDocument.CSI_Procedure = "LC";
		CombineAssertions("Checking CSI_ReferenceNumber2 validation when CSI_Procedure is Empty", delegate
		{
			previousDocument.CSI_ReferenceNumber2 = "123";
			AssertNoMessageErrorContaining("CSI_ReferenceNumber2Info", previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("CSI_ReferenceNumber2Info", previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_ReferenceNumber2 = ZString.Empty;
			AssertNoMessageErrorContaining("CSI_ReferenceNumber2Info", previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_ReferenceNumber2Info", previousDocument.CSI_ReferenceNumber2Info, MandatoryValidation.DoNotEntered);
		});
	}

	public void TestCheckCSI_CustomsOffice()
	{
		var previousDocument = GetPreviousDocumentForTesting();

		previousDocument.CSI_Procedure = ZString.Empty;
		CombineAssertions("Checking CSI_CustomsOffice validation when CSI_Procedure is Empty", delegate
		{
			previousDocument.CSI_CustomsOffice = "IT03923";
			AssertHasMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			previousDocument.CSI_CustomsOffice = ZString.Empty;
			AssertNoMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		});

		previousDocument.CSI_Procedure = "2T";
		CombineAssertions("Checking CSI_CustomsOffice validation when CSI_Procedure is 2T", delegate
		{
			previousDocument.CSI_CustomsOffice = ZString.Empty;
			AssertHasMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_CustomsOffice = "~";
			AssertHasMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError.ToString());
			previousDocument.CSI_CustomsOffice = "01234";
			AssertNoMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError.ToString());
		});

		previousDocument.CSI_Procedure = "LC";
		CombineAssertions("Checking CSI_CustomsOffice validation when CSI_Procedure is LC", delegate
		{
			previousDocument.CSI_CustomsOffice = "IT03923";
			AssertHasMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			previousDocument.CSI_CustomsOffice = ZString.Empty;
			AssertNoMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining("CSI_CustomsOfficeInfo", previousDocument.CSI_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckCSI_SubType()
	{
		var previousDocument = GetPreviousDocumentForTesting();

		previousDocument.CSI_Procedure = "LC";
		CombineAssertions("Checking CSI_SubType validation when CSI_Procedure is LC and CSI_Code is Empty", delegate
		{
			previousDocument.CSI_SubType = ZString.Empty;
			AssertHasMessageErrorContaining("CSI_SubTypeInfo", previousDocument.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);
			previousDocument.CSI_SubType = "X";
			AssertNoMessageErrorContaining("CSI_SubTypeInfo", previousDocument.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);
		});

		previousDocument.CSI_Code = "270";
		CombineAssertions("Checking CSI_SubType validation when CSI_Procedure is LC and CSI_Code is 270", delegate
		{
			previousDocument.CSI_SubType = ZString.Empty;
			AssertHasMessageErrorContaining("CSI_SubTypeInfo", previousDocument.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);
			previousDocument.CSI_SubType = "X";
			AssertNoMessageErrorContaining("CSI_SubTypeInfo", previousDocument.CSI_SubTypeInfo, MandatoryValidation.YouHaveNotEntered);
		});

		previousDocument.CSI_Procedure = ZString.Empty;
		CombineAssertions("Checking CSI_SubType validation when CSI_Procedure is Empty", delegate
		{
			previousDocument.CSI_SubType = "X";
			AssertNoMessageErrorContaining("CSI_SubTypeInfo", previousDocument.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
		});

		previousDocument.CSI_Procedure = "LC";
		CombineAssertions("Checking CSI_SubType validation when CSI_Procedure is LC", delegate
		{
			previousDocument.CSI_SubType = "X";
			AssertNoMessageErrorContaining("CSI_SubTypeInfo", previousDocument.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
			previousDocument.CSI_SubType = "Z";
			AssertHasMessageErrorContaining("CSI_SubTypeInfo", previousDocument.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
		});

		previousDocument.CSI_Procedure = "T1";
		CombineAssertions("Checking CSI_SubType validation when CSI_Procedure is T1", delegate
		{
			previousDocument.CSI_SubType = "X";
			AssertNoMessageErrorContaining("CSI_SubTypeInfo", previousDocument.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
			previousDocument.CSI_SubType = "Z";
			AssertNoMessageErrorContaining("CSI_SubTypeInfo", previousDocument.CSI_SubTypeInfo, ListValidation.InvalidCodeMessageError.ToString());
		});
	}

	public void TestCheckCSI_DateOfIssue()
	{
		var previousDocument = GetPreviousDocumentForTesting();
		previousDocument.CSI_Procedure = ZString.Empty;
		CombineAssertions("Checking CSI_DateOfIssue validation when CSI_Procedure is Empty", delegate
		{
			previousDocument.CSI_DateOfIssue = ZDateTime.Now;
			AssertNoMessageErrorContaining("CSI_DateOfIssueInfo", previousDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("CSI_DateOfIssueInfo", previousDocument.CSI_DateOfIssueInfo, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_DateOfIssue = ZDateTime.Empty;
			AssertNoMessageErrorContaining("CSI_DateOfIssueInfo", previousDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_DateOfIssueInfo", previousDocument.CSI_DateOfIssueInfo, MandatoryValidation.DoNotEntered);
		});

		previousDocument.CSI_Procedure = "LC";
		CombineAssertions("Checking CSI_DateOfIssue validation when CSI_Procedure is LC", delegate
		{
			previousDocument.CSI_DateOfIssue = ZDateTime.Now;
			AssertNoMessageErrorContaining("CSI_DateOfIssueInfo", previousDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_DateOfIssueInfo", previousDocument.CSI_DateOfIssueInfo, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_DateOfIssue = ZDateTime.Empty;
			AssertHasMessageErrorContaining("CSI_DateOfIssueInfo", previousDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_DateOfIssueInfo", previousDocument.CSI_DateOfIssueInfo, MandatoryValidation.DoNotEntered);
		});

		previousDocument.CSI_Procedure = "MRN";
		CombineAssertions("Checking CSI_DateOfIssue validation when CSI_Procedure is LC", delegate
		{
			previousDocument.CSI_DateOfIssue = ZDateTime.Now;
			AssertNoMessageErrorContaining("CSI_DateOfIssueInfo", previousDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining("CSI_DateOfIssueInfo", previousDocument.CSI_DateOfIssueInfo, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_DateOfIssue = ZDateTime.Empty;
			AssertNoMessageErrorContaining("CSI_DateOfIssueInfo", previousDocument.CSI_DateOfIssueInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining("CSI_DateOfIssueInfo", previousDocument.CSI_DateOfIssueInfo, MandatoryValidation.DoNotEntered);
		});
	}

	public void TestCheckCSI_Status()
	{
		var previousDocument = GetPreviousDocumentForTesting();
		previousDocument.CSI_Procedure = ZString.Empty;
		CombineAssertions("Checking CSI_Status validation when CSI_Procedure is Empty", delegate
		{
			previousDocument.CSI_Status = "AA";
			AssertHasMessageErrorContaining("CSI_StatusInfo", previousDocument.CSI_StatusInfo, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_Status = ZString.Empty;
			AssertNoMessageErrorContaining("CSI_StatusInfo", previousDocument.CSI_StatusInfo, MandatoryValidation.DoNotEntered);
		});

		previousDocument.CSI_Procedure = "A3";
		CombineAssertions("Checking CSI_Status validation when CSI_Procedure is A3", delegate
		{
			previousDocument.CSI_Status = "ST";
			AssertNoMessageErrorContaining("CSI_StatusInfo", previousDocument.CSI_StatusInfo, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_Status = ZString.Empty;
			AssertNoMessageErrorContaining("CSI_StatusInfo", previousDocument.CSI_StatusInfo, MandatoryValidation.DoNotEntered);
		});

		previousDocument.CSI_Procedure = "AWB";
		CombineAssertions("Checking CSI_Status validation when CSI_Procedure is AWB", delegate
		{
			previousDocument.CSI_Status = "AA";
			AssertHasMessageErrorContaining("CSI_StatusInfo", previousDocument.CSI_StatusInfo, MandatoryValidation.DoNotEntered);
			previousDocument.CSI_Status = ZString.Empty;
			AssertNoMessageErrorContaining("CSI_StatusInfo", previousDocument.CSI_StatusInfo, MandatoryValidation.DoNotEntered);
		});
	}

	public void TestCheckCSI_Procedure()
	{
		var previousDocument = GetPreviousDocumentForTesting();
		CombineAssertions("Checking CSI_Procedure validation", delegate
		{
			previousDocument.CSI_Code = "270";
			previousDocument.CSI_Procedure = ZString.Empty;
			AssertHasMessageErrorContaining("CSI_ProcedureInfo", previousDocument.CSI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);
			previousDocument.CSI_Procedure = "LC";
			AssertNoMessageErrorContaining("CSI_ProcedureInfo", previousDocument.CSI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Procedure = "XX";
			AssertHasMessageErrorContaining("CSI_ProcedureInfo", previousDocument.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError.ToString());

			previousDocument.CSI_Procedure = PreviousDocumentProcedureList.Codes.PartitaDiTemporaneaCustodiaA3;
			AssertNoMessageErrorContaining("CSI_ProcedureInfo", previousDocument.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError.ToString());
		});

		var jobComInvoiceLine = previousDocument.ParentLine;
		if (jobComInvoiceLine != null)
		{
			jobComInvoiceLine.JI_FormattedProcedure = "4000";
			CombineAssertions("Checking CSI_Procedure validation when JI_FormattedProcedure is 4000", delegate
			{
				previousDocument.CSI_Procedure = "A3";
				AssertNoMessageErrorContaining("CSI_ProcedureInfo", previousDocument.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError.ToString());
				previousDocument.CSI_Procedure = "7S";
				AssertHasMessageErrorContaining("CSI_ProcedureInfo", previousDocument.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError.ToString());
			});

			jobComInvoiceLine.JI_FormattedProcedure = "0071";
			CombineAssertions("Checking CSI_Procedure validation when JI_FormattedProcedure is 0071", delegate
			{
				previousDocument.CSI_Procedure = "7S";
				AssertNoMessageErrorContaining("CSI_ProcedureInfo", previousDocument.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError.ToString());
				previousDocument.CSI_Procedure = "A3";
				AssertHasMessageErrorContaining("CSI_ProcedureInfo", previousDocument.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError.ToString());
			});

			jobComInvoiceLine.JI_FormattedProcedure = "0020";
			CombineAssertions("Checking CSI_Procedure validation when JI_FormattedProcedure is 0020", delegate
			{
				previousDocument.CSI_Procedure = "2";
				AssertNoMessageErrorContaining("CSI_ProcedureInfo", previousDocument.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError.ToString());
				previousDocument.CSI_Procedure = "7";
				AssertHasMessageErrorContaining("CSI_ProcedureInfo", previousDocument.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError.ToString());
			});

			jobComInvoiceLine.JI_FormattedProcedure = "0050";
			CombineAssertions("Checking CSI_Procedure validation when JI_FormattedProcedure is 0050", delegate
			{
				previousDocument.CSI_Procedure = "5";
				AssertNoMessageErrorContaining("CSI_ProcedureInfo", previousDocument.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError.ToString());
				previousDocument.CSI_Procedure = "A3";
				AssertHasMessageErrorContaining("CSI_ProcedureInfo", previousDocument.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError.ToString());
			});
		}
	}

	public void TestCheckCSI_Code()
	{
		var previousDocument = GetPreviousDocumentForTesting();
		previousDocument.CSI_Procedure = "LC";
		CombineAssertions("Checking CSI_Code validation when CSI_Procedure is LC", delegate
		{
			previousDocument.CSI_Code = ZString.Empty;
			AssertHasMessageErrorContaining("CSI_CodeInfo", previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			previousDocument.CSI_Code = "270";
			AssertNoMessageErrorContaining("CSI_CodeInfo", previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
		});
		previousDocument.CSI_Procedure = ZString.Empty;
		previousDocument.CSI_SubType = "X";
		CombineAssertions("Checking CSI_Code validation when CSI_Procedure is LC and SubType X", delegate
		{
			previousDocument.CSI_Code = ZString.Empty;
			AssertHasMessageErrorContaining("CSI_CodeInfo", previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			previousDocument.CSI_Code = "270";
			AssertNoMessageErrorContaining("CSI_CodeInfo", previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			previousDocument.CSI_Code = "270";
			AssertNoMessageErrorContaining("CSI_CodeInfo", previousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError.ToString());
			previousDocument.CSI_Code = "AAA";
			AssertHasMessageErrorContaining("CSI_CodeInfo", previousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError.ToString());
		});

		previousDocument.CSI_Procedure = "LC";
		CombineAssertions("Checking CSI_Code validation when CSI_Procedure is LC and SubType X", delegate
		{
			previousDocument.CSI_Code = "270";
			AssertNoMessageErrorContaining("CSI_CodeInfo", previousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError.ToString());
			previousDocument.CSI_Code = "720";
			AssertHasMessageErrorContaining("CSI_CodeInfo", previousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError.ToString());
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Enterprise.MasterFiles.Business.EconomicGroupList.Codes.EuropeanUnion);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eun);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "01234", "Test 0", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		Factory.Save();
	}
}
