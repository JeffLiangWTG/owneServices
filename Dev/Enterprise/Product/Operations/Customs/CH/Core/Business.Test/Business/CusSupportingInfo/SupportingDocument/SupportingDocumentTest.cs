using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(SupportingDocument))]
public class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
{
	public void TestParentIsLine()
	{
		CombineAssertions(() =>
		{
			AssertEquals("supportingDocumentForLine parent is InvoiceLine", true, lineSupportingDocument.Parent is JobComInvoiceLine);
		});
	}

	public void TestReferenceNumber()
	{
		AssertEquals("MaxLength", 35, lineSupportingDocument.CSI_ReferenceNumberInfo.MaxLength);
		AssertEquals("Caption", "Reference", lineSupportingDocument.CSI_ReferenceNumberInfo.Description);
	}

	public void TestReferenceNumber2()
	{
		AssertEquals("MaxLength", 70, lineSupportingDocument.CSI_ReferenceNumber2Info.MaxLength);
		AssertEquals("Caption", "Additional Information", lineSupportingDocument.CSI_ReferenceNumber2Info.Description);
	}

	public void TestCSI_Code()
	{
		AssertEquals("Caption", "Type", lineSupportingDocument.CSI_CodeInfo.Description);
	}

	public void TestCSI_DateOfIssue()
	{
		AssertEquals("Caption", "Date Of Issue", lineSupportingDocument.CSI_DateOfIssueInfo.Description);
	}

	public void TestSuppportingDocumentHumanReadableName() => AssertEquals("Supporting Document", Factory.New<SupportingDocument>().HumanReadableName);

	public void TestIsGSPCertificate()
	{
		RefCusCodeTestHelper.CreateSupportingDocumentsList(Factory);

		CombineAssertions(() =>
		{
			lineSupportingDocument.CSI_Code = RefCusCodeTestHelper.ValidSupportingDocument101;
			AssertEquals("GSP Certificate", true, lineSupportingDocument.IsGSPCertificate);

			lineSupportingDocument.CSI_Code = RefCusCodeTestHelper.ValidSupportingDocument100;
			AssertEquals("other document", false, lineSupportingDocument.IsGSPCertificate);

			lineSupportingDocument.CSI_Code = RefCusCodeTestHelper.InvalidSupportingDocument;
			AssertEquals("invalid document", false, lineSupportingDocument.IsGSPCertificate);

			lineSupportingDocument.CSI_Code = ZString.Empty;
			AssertEquals("empty code", false, lineSupportingDocument.IsGSPCertificate);
		});
	}

	#region Implementation

	protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		yield return invoiceLine.SupportingDocuments.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.JobComInvoiceLines.AddNew();
		return invoiceLine.SupportingDocuments.AddNew();
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		lineSupportingDocument = invoiceLine.SupportingDocuments.AddNew();
	}

	SupportingDocument lineSupportingDocument;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	JobDeclaration declaration;

	#endregion
}
