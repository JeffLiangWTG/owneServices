using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SupportingDocumentExtensionTest : TestCaseWithFactory
{
	public void TestHasDocument()
	{
		CombineAssertions(() =>
		{
			IEnumerable<EU.Business.Declaration.MultiLineAddInfos.SupportingDocument> nullSupportingDocumentCollection = null;
			AssertNoExceptionThrown("No exception expected when supporting Documents param is null", () => nullSupportingDocumentCollection.HasDocument(""));
			AssertEquals("When supporting documents param is null, return value", false, nullSupportingDocumentCollection.HasDocument(""));
		});

		var supportingDocument1 = Factory.New<SupportingDocument>();
		supportingDocument1.CSI_Code = "XXX";

		var supportingDocument2 = Factory.New<SupportingDocument>();
		supportingDocument2.CSI_Code = "YYY";

		var supportingDocumentCollecton = new EU.Business.Declaration.MultiLineAddInfos.SupportingDocument[] { supportingDocument1, supportingDocument2 };

		AssertEquals("supportingDocumentCollecton should have a sup of code 'XXX'", true, supportingDocumentCollecton.HasDocument("XXX"));
		AssertEquals("supportingDocumentCollecton should not have a sup of code 'ZZZ'", false, supportingDocumentCollecton.HasDocument("ZZZ"));
	}

	public void TestGetReferenceNumberWithYearOfIssueAndCountry()
	{
		CombineAssertions(() =>
		{
			AssertEquals("When supportingDocument is null, return value", null, ((SupportingDocument)null).GetReferenceNumberWithYearOfIssueAndCountry());

			var supportingDocument = Factory.New<SupportingDocument>();
			supportingDocument.CSI_YearOfIssue = "2021";
			supportingDocument.CSI_RN_NKCountryCode = "GB";
			supportingDocument.CSI_ReferenceNumber = "123456";

			AssertEquals("Reference number with year of issue and country", "2021-GB-123456", supportingDocument.GetReferenceNumberWithYearOfIssueAndCountry());
		});
	}

	public void TestSetYearOfIssue()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		CombineAssertions("SetYearOfIssue", () =>
		{
			AssertEquals("To make sure CSI_DateOfIssue is empty before SetYearOfIssue", ZDateTime.Empty, supportingDocument.CSI_DateOfIssue);
			supportingDocument.SetYearOfIssue("ASD");
			AssertEquals("Empty for unrecognized input.", ZDateTime.Empty, supportingDocument.CSI_DateOfIssue);
			supportingDocument.SetYearOfIssue("111");
			AssertEquals("Empty for unrecognized input.", ZDateTime.Empty, supportingDocument.CSI_DateOfIssue);
			supportingDocument.SetYearOfIssue("1111");
			AssertEquals("Valid input year 1111", new ZDateTime(1111, 1, 1), supportingDocument.CSI_DateOfIssue);
			supportingDocument.SetYearOfIssue("11111");
			AssertEquals("Empty for unrecognized input.", ZDateTime.Empty, supportingDocument.CSI_DateOfIssue);
			supportingDocument.SetYearOfIssue("2010");
			AssertEquals("Valid input year 2010", new ZDateTime(2010, 1, 1), supportingDocument.CSI_DateOfIssue);
		});
	}

	public void TestGetYearOfIssue()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		CombineAssertions("GetYearOfIssue", () =>
		{
			supportingDocument.CSI_DateOfIssue = new ZDateTime(2017, 11, 14, 2, 3, 4);
			AssertEquals("Valid CSI_DateOfIssue", "2017", supportingDocument.GetYearOfIssue());
			supportingDocument.CSI_DateOfIssue = ZDateTime.Invalid;
			AssertEquals("Invalid CSI_DateOfIssue", ZString.Empty, supportingDocument.GetYearOfIssue());
			supportingDocument.CSI_DateOfIssue = ZDateTime.Empty;
			AssertEquals("Empty CSI_DateOfIssue", ZString.Empty, supportingDocument.GetYearOfIssue());
		});
	}
}
