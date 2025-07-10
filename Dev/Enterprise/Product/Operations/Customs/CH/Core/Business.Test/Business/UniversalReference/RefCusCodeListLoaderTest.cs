using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business.Testing;

public class RefCusCodeListLoaderTest : TestCaseWithFactory
{
	public void TestGetDirectTransportationCountryList()
	{
		RefCusCodeTestHelper.CreateDirectTransportationCodeList(Factory);

		CombineAssertions(() =>
		{
			var list = RefCusCodeListLoader.GetDirectTransportationCountryList(Factory, ZDateTime.Today);
			AssertEquals("country in list", true, list.ContainsCode(RefCusCodeTestHelper.ValidDirectTransportationCountry));
			AssertEquals("country not in list", false, list.ContainsCode(RefCusCodeTestHelper.InvalidDirectTransportationCountry));

			AssertSame("cached", list, RefCusCodeListLoader.GetDirectTransportationCountryList(Factory, ZDateTime.Today));
		});
	}

	public void TestIsDirectTransportationCountry()
	{
		RefCusCodeTestHelper.CreateDirectTransportationCodeList(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("country in list", true, RefCusCodeListLoader.IsDirectTransportationCountry(Factory, RefCusCodeTestHelper.ValidDirectTransportationCountry, ZDateTime.Today));
			AssertEquals("country not in list", false, RefCusCodeListLoader.IsDirectTransportationCountry(Factory, RefCusCodeTestHelper.InvalidDirectTransportationCountry, ZDateTime.Today));
		});
	}

	public void TestGetGSPCertificateCodes()
	{
		RefCusCodeTestHelper.CreateSupportingDocumentsList(Factory);

		CombineAssertions(() =>
		{
			var list = RefCusCodeListLoader.GetGSPCertificateCodes(Factory, ZDateTime.Today);
			AssertEquals("codes in list", $"{RefCusCodeTestHelper.ValidSupportingDocument101}, {RefCusCodeTestHelper.ValidSupportingDocument102}", list.CodesAsString);
			AssertSame("cached", list, RefCusCodeListLoader.GetGSPCertificateCodes(Factory, ZDateTime.Today));
		});
	}

	public void TestIsGSPCertificate()
	{
		RefCusCodeTestHelper.CreateSupportingDocumentsList(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("GSP Certificate", true, RefCusCodeListLoader.IsGSPCertificate(Factory, RefCusCodeTestHelper.ValidSupportingDocument101, ZDateTime.Today));
			AssertEquals("any other valid document code", false, RefCusCodeListLoader.IsGSPCertificate(Factory, RefCusCodeTestHelper.ValidSupportingDocument100, ZDateTime.Today));
			AssertEquals("any other valid document code", false, RefCusCodeListLoader.IsGSPCertificate(Factory, RefCusCodeTestHelper.ValidSupportingDocument103, ZDateTime.Today));
			AssertEquals("invalid document code", false, RefCusCodeListLoader.IsGSPCertificate(Factory, RefCusCodeTestHelper.InvalidSupportingDocument, ZDateTime.Today));
			AssertEquals("empty code", false, RefCusCodeListLoader.IsGSPCertificate(Factory, ZString.Empty, ZDateTime.Today));
		});
	}

	public void TestIsAnyGSPCertificate()
	{
		RefCusCodeTestHelper.CreateSupportingDocumentsList(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("has GSP Certificate", true, RefCusCodeListLoader.IsAnyGSPCertificate(Factory, new ZString[] { RefCusCodeTestHelper.ValidSupportingDocument100, RefCusCodeTestHelper.ValidSupportingDocument101 }, ZDateTime.Today));
			AssertEquals("no GSP Certificate", false, RefCusCodeListLoader.IsAnyGSPCertificate(Factory, new ZString[] { RefCusCodeTestHelper.ValidSupportingDocument100 }, ZDateTime.Today));
			AssertEquals("empty code list", false, RefCusCodeListLoader.IsAnyGSPCertificate(Factory, Array.Empty<ZString>(), ZDateTime.Today));
			AssertEquals("null code list", false, RefCusCodeListLoader.IsAnyGSPCertificate(Factory, null, ZDateTime.Today));
		});
	}

	public void TestGetBulkPackTypeList()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		CombineAssertions(() =>
		{
			var list = RefCusCodeListLoader.GetBulkPackTypeList(Factory);
			AssertEquals("count", 1, list.Count);
			AssertEquals(true, list.ContainsCode(RefCusCodeTestHelper.UNPKGCodeWithBulkYes));
			AssertSame("cached", list, RefCusCodeListLoader.GetBulkPackTypeList(Factory));
		});
	}

	public void TestGetBreakBulkPackTypeList()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		CombineAssertions(() =>
		{
			var list = RefCusCodeListLoader.GetBreakBulkPackTypeList(Factory);
			AssertEquals("count", 1, list.Count);
			AssertEquals(true, list.ContainsCode(RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes));
			AssertSame("cached", list, RefCusCodeListLoader.GetBreakBulkPackTypeList(Factory));
		});
	}

	public void TestIsBulkPackType()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("bulk", true, RefCusCodeListLoader.IsBulkPackType(Factory, RefCusCodeTestHelper.UNPKGCodeWithBulkYes));
			AssertEquals("break bulk", false, RefCusCodeListLoader.IsBulkPackType(Factory, RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes));
			AssertEquals("no bulk", false, RefCusCodeListLoader.IsBulkPackType(Factory, RefCusCodeTestHelper.UNPKGCodeNoBulk));
		});
	}

	public void TestIsBreakBulkPackType()
	{
		RefCusCodeTestHelper.CreateUNPKGCodeList(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("bulk", false, RefCusCodeListLoader.IsBreakBulkPackType(Factory, RefCusCodeTestHelper.UNPKGCodeWithBulkYes));
			AssertEquals("break bulk", true, RefCusCodeListLoader.IsBreakBulkPackType(Factory, RefCusCodeTestHelper.UNPKGCodeWithBreakBulkYes));
			AssertEquals("no bulk", false, RefCusCodeListLoader.IsBreakBulkPackType(Factory, RefCusCodeTestHelper.UNPKGCodeNoBulk));
		});
	}

	public void TestGetSupportingExportDocumentsWithAttribute()
	{
		RefCusCodeTestHelper.CreateSupportingDocumentCodes(Factory);

		CombineAssertions(() =>
		{
			var list = RefCusCodeListLoader.GetExportSupportingDocumentsWithAttribute(Factory, ZDateTime.Today, UniversalReferenceConstants.RefCusCodeList.Attributes.Reference);
			AssertEquals("With attribute Reference=Y", true, list.ContainsCode(RefCusCodeTestHelper.ExportSupportingDocumentCodeWithReferenceY));
			AssertEquals("With attribute Reference=N", false, list.ContainsCode(RefCusCodeTestHelper.ExportSupportingDocumentCodeWithReferenceN));
			AssertEquals("With no Reference attribute", false, list.ContainsCode(RefCusCodeTestHelper.ValidExportSupportingDocumentCode));
			list = RefCusCodeListLoader.GetExportSupportingDocumentsWithAttribute(Factory, ZDateTime.Today, UniversalReferenceConstants.RefCusCodeList.Attributes.IssuingDate);
			AssertEquals("With attribute IssuingDate=Y", true, list.ContainsCode(RefCusCodeTestHelper.ExportSupportingDocumentCodeWithIssuingDateY));
			AssertEquals("With attribute IssuingDate=N", false, list.ContainsCode(RefCusCodeTestHelper.ExportSupportingDocumentCodeWithIssuingDateN));
			AssertEquals("With no IssuingDate attribute", false, list.ContainsCode(RefCusCodeTestHelper.ValidExportSupportingDocumentCode));
		});
	}

	public void TestIsSupportingExportDocumentRequiringReference()
	{
		RefCusCodeTestHelper.CreateSupportingDocumentCodes(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("With attribute Reference=Y", true, RefCusCodeListLoader.IsExportSupportingDocumentRequiringReference(Factory, RefCusCodeTestHelper.ExportSupportingDocumentCodeWithReferenceY, ZDateTime.Today));
			AssertEquals("With attribute Reference=N", false, RefCusCodeListLoader.IsExportSupportingDocumentRequiringReference(Factory, RefCusCodeTestHelper.ExportSupportingDocumentCodeWithReferenceN, ZDateTime.Today));
			AssertEquals("With no Reference attribute", false, RefCusCodeListLoader.IsExportSupportingDocumentRequiringReference(Factory, RefCusCodeTestHelper.ValidExportSupportingDocumentCode, ZDateTime.Today));
		});
	}

	public void TestIsSupportingExportDocumentRequiringIssueDate()
	{
		RefCusCodeTestHelper.CreateSupportingDocumentCodes(Factory);

		CombineAssertions(() =>
		{
			AssertEquals("With attribute IssuingDate=Y", true, RefCusCodeListLoader.IsExportSupportingDocumentRequiringIssueDate(Factory, RefCusCodeTestHelper.ExportSupportingDocumentCodeWithIssuingDateY, ZDateTime.Today));
			AssertEquals("With attribute IssuingDate=N", false, RefCusCodeListLoader.IsExportSupportingDocumentRequiringIssueDate(Factory, RefCusCodeTestHelper.ExportSupportingDocumentCodeWithIssuingDateN, ZDateTime.Today));
			AssertEquals("With no IssuingDate attribute", false, RefCusCodeListLoader.IsExportSupportingDocumentRequiringIssueDate(Factory, RefCusCodeTestHelper.ValidExportSupportingDocumentCode, ZDateTime.Today));
		});
	}
}
