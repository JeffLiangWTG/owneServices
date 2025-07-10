using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

public static class PreviousDocumentHelperTest
{
	public static void TestSetDefaultAndEmptyField(IPreviousDocumentForTesting previousDocument)
	{
		PopulateFields(previousDocument, "X");
		previousDocument.CSI_Procedure = "LC";
		Assertion.CombineAssertions("Checking Previous Document after selecting CSI_Procedure as LC", () =>
		{
			Assertion.AssertEquals("CSI_Code", "270", previousDocument.CSI_Code);
			Assertion.AssertEquals("CSI_SubType", "X", previousDocument.CSI_SubType);
			Assertion.AssertEquals("CSI_ReferenceNumber", "REF", previousDocument.CSI_ReferenceNumber);
			Assertion.AssertEquals("CSI_ReferenceNumber2", ZString.Empty, previousDocument.CSI_ReferenceNumber2);
			Assertion.AssertEquals("CSI_DateOfIssue", new ZDate(2019, 08, 23), previousDocument.CSI_DateOfIssue);
			Assertion.AssertEquals("CSI_CustomsOffice", ZString.Empty, previousDocument.CSI_CustomsOffice);
			Assertion.AssertEquals("CSI_LineNo", (ZShort)0, previousDocument.CSI_LineNo);
			Assertion.AssertEquals("CSI_Status", ZString.Empty, previousDocument.CSI_Status);
		});

		PopulateFields(previousDocument, "Z");
		previousDocument.CSI_Procedure = "MRN";
		Assertion.CombineAssertions("Checking Previous Document after selecting CSI_Procedure as MRN", () =>
		{
			Assertion.AssertEquals("CSI_Code", ZString.Empty, previousDocument.CSI_Code);
			Assertion.AssertEquals("CSI_SubType", "Z", previousDocument.CSI_SubType);
			Assertion.AssertEquals("CSI_ReferenceNumber2", "REF2", previousDocument.CSI_ReferenceNumber2);
			Assertion.AssertEquals("CSI_LineNo", (ZShort)1, previousDocument.CSI_LineNo);
			Assertion.AssertEquals("CSI_ReferenceNumber", ZString.Empty, previousDocument.CSI_ReferenceNumber);
			Assertion.AssertEquals("CSI_DateOfIssue", ZDate.Empty, previousDocument.CSI_DateOfIssue);
			Assertion.AssertEquals("CSI_CustomsOffice", ZString.Empty, previousDocument.CSI_CustomsOffice);
			Assertion.AssertEquals("CSI_Status", ZString.Empty, previousDocument.CSI_Status);
		});

		PopulateFields(previousDocument, "Z");
		previousDocument.CSI_Procedure = "2T";
		Assertion.CombineAssertions("Checking Previous Document after selecting CSI_Procedure as 2T", () =>
		{
			Assertion.AssertEquals("CSI_Code", ZString.Empty, previousDocument.CSI_Code);
			Assertion.AssertEquals("CSI_SubType", "Z", previousDocument.CSI_SubType);
			Assertion.AssertEquals("CSI_ReferenceNumber", "REF", previousDocument.CSI_ReferenceNumber);
			Assertion.AssertEquals("CSI_DateOfIssue", new ZDate(2019, 08, 23), previousDocument.CSI_DateOfIssue);
			Assertion.AssertEquals("CSI_CustomsOffice", "CUSOFFICE", previousDocument.CSI_CustomsOffice);
			Assertion.AssertEquals("CSI_LineNo", (ZShort)1, previousDocument.CSI_LineNo);
			Assertion.AssertEquals("CSI_ReferenceNumber2", ZString.Empty, previousDocument.CSI_ReferenceNumber2);
			Assertion.AssertEquals("CSI_Status", ZString.Empty, previousDocument.CSI_Status);
		});

		PopulateFields(previousDocument, "Z");
		previousDocument.CSI_Procedure = "A3";
		Assertion.CombineAssertions("Checking Previous Document after selecting CSI_Procedure as A3", () =>
		{
			Assertion.AssertEquals("CSI_Code", "ZZZ", previousDocument.CSI_Code);
			Assertion.AssertEquals("CSI_SubType", "Z", previousDocument.CSI_SubType);
			Assertion.AssertEquals("CSI_ReferenceNumber", "REF", previousDocument.CSI_ReferenceNumber);
			Assertion.AssertEquals("CSI_DateOfIssue", new ZDate(2019, 08, 23), previousDocument.CSI_DateOfIssue);
			Assertion.AssertEquals("CSI_CustomsOffice", "CUSOFFICE", previousDocument.CSI_CustomsOffice);
			Assertion.AssertEquals("CSI_Status", "ST", previousDocument.CSI_Status);
			Assertion.AssertEquals("CSI_LineNo", (ZShort)0, previousDocument.CSI_LineNo);
			Assertion.AssertEquals("CSI_ReferenceNumber2", ZString.Empty, previousDocument.CSI_ReferenceNumber2);
		});

		previousDocument.CSI_Code = "270";
		previousDocument.CSI_SubType = "Z";
		previousDocument.CSI_Procedure = "AWB";
		Assertion.CombineAssertions("Checking Previous Document after selecting CSI_Procedure as AWB", () =>
		{
			Assertion.AssertEquals("CSI_Code", "740", previousDocument.CSI_Code);
			Assertion.AssertEquals("CSI_SubType", "X", previousDocument.CSI_SubType);
		});
	}

	static void PopulateFields(IPreviousDocumentForTesting previousDocument, ZString subType)
	{
		previousDocument.CSI_Code = "270";
		previousDocument.CSI_SubType = subType;
		previousDocument.CSI_ReferenceNumber = "REF";
		previousDocument.CSI_DateOfIssue = new ZDate(2019, 08, 23);
		previousDocument.CSI_CustomsOffice = "CUSOFFICE";
		previousDocument.CSI_LineNo = 1;
		previousDocument.CSI_Status = "ST";
		previousDocument.CSI_ReferenceNumber2 = "REF2";
	}

	public static void TestReadOnlyFields(IPreviousDocumentForTesting previousDocument)
	{
		previousDocument.CSI_Procedure = ZString.Empty;
		Assertion.AssertEquals("Is ReferenceNumber readonly when CSI_Procedure is Empty?", ZBool.True, previousDocument.ReferenceNumberReadOnly);
		previousDocument.CSI_Procedure = "AWB";
		Assertion.AssertEquals("Is ReferenceNumber readonly when CSI_Procedure is AWB?", ZBool.False, previousDocument.ReferenceNumberReadOnly);
		previousDocument.CSI_Procedure = "MRN";
		Assertion.AssertEquals("Is ReferenceNumber readonly when CSI_Procedure is MRN?", ZBool.True, previousDocument.ReferenceNumberReadOnly);
		previousDocument.CSI_Procedure = "2T";
		Assertion.AssertEquals("Is ReferenceNumber readonly when CSI_Procedure is 2T?", ZBool.False, previousDocument.ReferenceNumberReadOnly);
		previousDocument.CSI_Procedure = "A3";
		Assertion.AssertEquals("Is ReferenceNumber readonly when CSI_Procedure is A3?", ZBool.False, previousDocument.ReferenceNumberReadOnly);

		previousDocument.CSI_Procedure = ZString.Empty;
		Assertion.AssertEquals("Is ReferenceNumber2 readonly when CSI_Procedure is Empty?", ZBool.True, previousDocument.ReferenceNumber2ReadOnly);
		previousDocument.CSI_Procedure = "AWB";
		Assertion.AssertEquals("Is ReferenceNumber2 readonly when CSI_Procedure is AWB?", ZBool.True, previousDocument.ReferenceNumber2ReadOnly);
		previousDocument.CSI_Procedure = "MRN";
		Assertion.AssertEquals("Is ReferenceNumber2 readonly when CSI_Procedure is MRN?", ZBool.False, previousDocument.ReferenceNumber2ReadOnly);
		previousDocument.CSI_Procedure = "2T";
		Assertion.AssertEquals("Is ReferenceNumber2 readonly when CSI_Procedure is 2T?", ZBool.True, previousDocument.ReferenceNumber2ReadOnly);
		previousDocument.CSI_Procedure = "A3";
		Assertion.AssertEquals("Is ReferenceNumber2 readonly when CSI_Procedure is A3?", ZBool.True, previousDocument.ReferenceNumber2ReadOnly);

		previousDocument.CSI_Procedure = ZString.Empty;
		Assertion.AssertEquals("Is Date Of Issue readonly when CSI_Procedure is Empty?", ZBool.True, previousDocument.DateOfIssueReadOnly);
		previousDocument.CSI_Procedure = "AWB";
		Assertion.AssertEquals("Is Date Of Issue readonly when CSI_Procedure is AWB?", ZBool.False, previousDocument.DateOfIssueReadOnly);
		previousDocument.CSI_Procedure = "MRN";
		Assertion.AssertEquals("Is Date Of Issue readonly when CSI_Procedure is MRN?", ZBool.True, previousDocument.DateOfIssueReadOnly);
		previousDocument.CSI_Procedure = "2T";
		Assertion.AssertEquals("Is Date Of Issue readonly when CSI_Procedure is 2T?", ZBool.False, previousDocument.DateOfIssueReadOnly);
		previousDocument.CSI_Procedure = "A3";
		Assertion.AssertEquals("Is Date Of Issue readonly when CSI_Procedure is A3?", ZBool.False, previousDocument.DateOfIssueReadOnly);

		previousDocument.CSI_Procedure = ZString.Empty;
		Assertion.AssertEquals("Is Customs Office readonly when CSI_Procedure is Empty?", ZBool.True, previousDocument.CustomsOfficeReadOnly);
		previousDocument.CSI_Procedure = "AWB";
		Assertion.AssertEquals("Is Customs Office readonly when CSI_Procedure is AWB?", ZBool.True, previousDocument.CustomsOfficeReadOnly);
		previousDocument.CSI_Procedure = "MRN";
		Assertion.AssertEquals("Is Customs Office readonly when CSI_Procedure is MRN?", ZBool.True, previousDocument.CustomsOfficeReadOnly);
		previousDocument.CSI_Procedure = "2T";
		Assertion.AssertEquals("Is Customs Office readonly when CSI_Procedure is 2T?", ZBool.False, previousDocument.CustomsOfficeReadOnly);
		previousDocument.CSI_Procedure = "A3";
		Assertion.AssertEquals("Is Customs Office readonly when CSI_Procedure is A3?", ZBool.False, previousDocument.CustomsOfficeReadOnly);

		previousDocument.CSI_Procedure = ZString.Empty;
		Assertion.AssertEquals("Is Line No readonly when CSI_Procedure is Empty?", ZBool.True, previousDocument.LineNoReadOnly);
		previousDocument.CSI_Procedure = "AWB";
		Assertion.AssertEquals("Is Line No readonly when CSI_Procedure is AWB?", ZBool.True, previousDocument.LineNoReadOnly);
		previousDocument.CSI_Procedure = "MRN";
		Assertion.AssertEquals("Is Line No readonly when CSI_Procedure is MRN?", ZBool.False, previousDocument.LineNoReadOnly);
		previousDocument.CSI_Procedure = "2T";
		Assertion.AssertEquals("Is Line No readonly when CSI_Procedure is 2T?", ZBool.False, previousDocument.LineNoReadOnly);
		previousDocument.CSI_Procedure = "A3";
		Assertion.AssertEquals("Is Line No readonly when CSI_Procedure is A3?", ZBool.True, previousDocument.LineNoReadOnly);

		previousDocument.CSI_Procedure = ZString.Empty;
		Assertion.AssertEquals("Is Status readonly when CSI_Procedure is Empty?", ZBool.True, previousDocument.StatusReadOnly);
		previousDocument.CSI_Procedure = "AWB";
		Assertion.AssertEquals("Is Status readonly when CSI_Procedure is AWB?", ZBool.True, previousDocument.StatusReadOnly);
		previousDocument.CSI_Procedure = "MRN";
		Assertion.AssertEquals("Is Status readonly when CSI_Procedure is MRN?", ZBool.True, previousDocument.StatusReadOnly);
		previousDocument.CSI_Procedure = "2T";
		Assertion.AssertEquals("Is Status readonly when CSI_Procedure is 2T?", ZBool.True, previousDocument.StatusReadOnly);
		previousDocument.CSI_Procedure = "A3";
		Assertion.AssertEquals("Is Status readonly when CSI_Procedure is A3?", ZBool.False, previousDocument.StatusReadOnly);
	}

	public static void AddPreviousDocumentsInOrderToGetNbMessages(JobComInvoiceLine invoiceLine)
	{
		var paDocument1 = invoiceLine.PreviousDocuments.AddNew();
		paDocument1.CSI_Procedure = "A3";
		paDocument1.CSI_ReferenceNumber = "1A";
		paDocument1.CSI_DateOfIssue = new ZDate(2020, 01, 01);
		paDocument1.CSI_Status = "X";
		paDocument1.CSI_CustomsOffice = "IT137100";
		paDocument1.CSI_LineNo = 1;

		var paDocument2 = invoiceLine.PreviousDocuments.AddNew();
		paDocument2.CSI_Procedure = "A3";
		paDocument2.CSI_ReferenceNumber = "2";

		var rpDocument1 = invoiceLine.PreviousDocuments.AddNew();
		rpDocument1.CSI_Procedure = "2";
		rpDocument1.CSI_ReferenceNumber = "1";
	}
}

public interface IPreviousDocumentForTesting
{
	ZString CSI_Procedure { get; set; }
	ZString CSI_SubType { get; set; }
	ZPropertyInfo CSI_SubTypeInfo { get; }
	ZDateTime CSI_DateOfIssue { get; set; }
	bool DateOfIssueReadOnly { get; }
	ZPropertyInfo CSI_DateOfIssueInfo { get; }
	ZString CSI_Code { get; set; }
	ZPropertyInfo CSI_CodeInfo { get; }
	ZPropertyInfo CSI_ProcedureInfo { get; }
	ZString CSI_ReferenceNumber { get; set; }
	bool ReferenceNumberReadOnly { get; }
	ZPropertyInfo CSI_ReferenceNumberInfo { get; }
	ZInt CSI_LineNo { get; set; }
	ZPropertyInfo CSI_LineNoInfo { get; }
	bool LineNoReadOnly { get; }
	ZString CSI_ReferenceNumber2 { get; set; }
	ZPropertyInfo CSI_ReferenceNumber2Info { get; }
	bool ReferenceNumber2ReadOnly { get; }
	ZString CSI_CustomsOffice { get; set; }
	bool CustomsOfficeReadOnly { get; }
	ZString CSI_Status { get; set; }
	ZPropertyInfo CSI_StatusInfo { get; }
	bool StatusReadOnly { get; }
	ZPropertyInfo CSI_CustomsOfficeInfo { get; }
	BusinessObjectFactory Factory { get; }
	JobComInvoiceLine ParentLine { get; }
}
