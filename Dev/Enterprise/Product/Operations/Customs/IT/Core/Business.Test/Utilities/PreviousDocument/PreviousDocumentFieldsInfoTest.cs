using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class PreviousDocumentFieldsInfoTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		var fieldsInfoTemplate1 = new PreviousDocumentFieldsInfo(PreviousDocumentCombinationTemplate._1);
		CombineAssertions("Checking properties PreviousDocumentFieldsInfo instantiated with template 1", () =>
		{
			AssertEquals("IsReferenceNumberEditable", true, fieldsInfoTemplate1.IsReferenceNumberEditable);
			AssertEquals("IsDateOfIssueEditable", true, fieldsInfoTemplate1.IsDateOfIssueEditable);
			AssertEquals("IsReferenceNumber2Editable", false, fieldsInfoTemplate1.IsReferenceNumber2Editable);
			AssertEquals("IsLineNoEditable", false, fieldsInfoTemplate1.IsLineNoEditable);
			AssertEquals("IsStatusEditable", false, fieldsInfoTemplate1.IsStatusEditable);
			AssertEquals("IsCustomsOfficeEditable", false, fieldsInfoTemplate1.IsCustomsOfficeEditable);
		});
		var fieldsInfoTemplate2 = new PreviousDocumentFieldsInfo(PreviousDocumentCombinationTemplate._2);
		CombineAssertions("Checking properties PreviousDocumentFieldsInfo instantiated with template 2", () =>
		{
			AssertEquals("IsReferenceNumber2Editable", true, fieldsInfoTemplate2.IsReferenceNumber2Editable);
			AssertEquals("IsLineNoEditable", true, fieldsInfoTemplate2.IsLineNoEditable);
			AssertEquals("IsReferenceNumberEditable", false, fieldsInfoTemplate2.IsReferenceNumberEditable);
			AssertEquals("IsDateOfIssueEditable", false, fieldsInfoTemplate2.IsDateOfIssueEditable);
			AssertEquals("IsStatusEditable", false, fieldsInfoTemplate2.IsStatusEditable);
			AssertEquals("IsCustomsOfficeEditable", false, fieldsInfoTemplate2.IsCustomsOfficeEditable);
		});
		var fieldsInfoTemplate3 = new PreviousDocumentFieldsInfo(PreviousDocumentCombinationTemplate._3);
		CombineAssertions("Checking properties PreviousDocumentFieldsInfo instantiated with template 3", () =>
		{
			AssertEquals("IsReferenceNumberEditable", true, fieldsInfoTemplate3.IsReferenceNumberEditable);
			AssertEquals("IsDateOfIssueEditable", true, fieldsInfoTemplate3.IsDateOfIssueEditable);
			AssertEquals("IsCustomsOfficeEditable", true, fieldsInfoTemplate3.IsCustomsOfficeEditable);
			AssertEquals("IsLineNoEditable", true, fieldsInfoTemplate3.IsLineNoEditable);
			AssertEquals("IsReferenceNumber2Editable", false, fieldsInfoTemplate3.IsReferenceNumber2Editable);
			AssertEquals("IsStatusEditable", false, fieldsInfoTemplate3.IsStatusEditable);
		});
		var fieldsInfoTemplate4 = new PreviousDocumentFieldsInfo(PreviousDocumentCombinationTemplate._4);
		CombineAssertions("Checking properties PreviousDocumentFieldsInfo instantiated with template 4", () =>
		{
			AssertEquals("IsReferenceNumberEditable", true, fieldsInfoTemplate4.IsReferenceNumberEditable);
			AssertEquals("IsDateOfIssueEditable", true, fieldsInfoTemplate4.IsDateOfIssueEditable);
			AssertEquals("IsCustomsOfficeEditable", true, fieldsInfoTemplate4.IsCustomsOfficeEditable);
			AssertEquals("IsStatusEditable", true, fieldsInfoTemplate4.IsStatusEditable);
			AssertEquals("IsLineNoEditable", false, fieldsInfoTemplate4.IsLineNoEditable);
			AssertEquals("IsReferenceNumber2Editable", false, fieldsInfoTemplate4.IsReferenceNumber2Editable);
		});
		var fieldsInfoTemplate5 = new PreviousDocumentFieldsInfo(PreviousDocumentCombinationTemplate._5);
		CombineAssertions("Checking properties PreviousDocumentFieldsInfo instantiated with template 5", () =>
		{
			AssertEquals("IsReferenceNumberEditable", true, fieldsInfoTemplate5.IsReferenceNumberEditable);
			AssertEquals("IsDateOfIssueEditable", true, fieldsInfoTemplate5.IsDateOfIssueEditable);
			AssertEquals("IsCustomsOfficeEditable", true, fieldsInfoTemplate5.IsCustomsOfficeEditable);
			AssertEquals("IsStatusEditable", false, fieldsInfoTemplate5.IsStatusEditable);
			AssertEquals("IsLineNoEditable", false, fieldsInfoTemplate5.IsLineNoEditable);
			AssertEquals("IsReferenceNumber2Editable", false, fieldsInfoTemplate5.IsReferenceNumber2Editable);
		});
		var fieldsInfo = new PreviousDocumentFieldsInfo();
		CombineAssertions("Checking properties PreviousDocumentFieldsInfo instantiated with no template", () =>
		{
			AssertEquals("IsReferenceNumberEditable", false, fieldsInfo.IsReferenceNumberEditable);
			AssertEquals("IsDateOfIssueEditable", false, fieldsInfo.IsDateOfIssueEditable);
			AssertEquals("IsCustomsOfficeEditable", false, fieldsInfo.IsCustomsOfficeEditable);
			AssertEquals("IsStatusEditable", false, fieldsInfo.IsStatusEditable);
			AssertEquals("IsLineNoEditable", false, fieldsInfo.IsLineNoEditable);
			AssertEquals("IsReferenceNumber2Editable", false, fieldsInfo.IsReferenceNumber2Editable);
		});
	}
}
