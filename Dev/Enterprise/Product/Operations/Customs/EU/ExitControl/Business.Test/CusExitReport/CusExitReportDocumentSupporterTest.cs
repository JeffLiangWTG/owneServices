using System;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

[TestedType(typeof(CusExitReportDocumentSupporter))]
class CusExitReportDocumentSupporterTest : DocumentSupporterTest
{
	public void TestConstructor()
	{
		_ = AssertExceptionThrown<ArgumentNullException>("Exception expected when report parameter is null", () => new CusExitReportDocumentSupporter(null));
	}

	public void TestBusinessContext()
	{
		AssertEquals("BusinessContext", BusinessContext.CusExitReport, documentSupporter.BusinessContext);
	}

	public void TestStorageDocsAreEditableIfInRelated()
	{
		AssertEquals("StorageDocsAreEditableIfInRelated enabled", expected: true, documentSupporter.StorageDocsAreEditableIfInRelated);
	}

	protected override void SetUp()
	{
		base.SetUp();
		documentSupporter = (CusExitReportDocumentSupporter)Factory.New<CusExitReport>().DocumentSupporter;
	}
	CusExitReportDocumentSupporter documentSupporter;

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.New<CusExitReport>();
}
