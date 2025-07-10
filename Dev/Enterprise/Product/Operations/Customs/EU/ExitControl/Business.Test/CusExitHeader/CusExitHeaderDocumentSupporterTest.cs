using System;
using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing;

[TestedType(typeof(CusExitHeaderDocumentSupporter))]
class CusExitHeaderDocumentSupporterTest : DocumentSupporterTest
{
	public void TestConstructor()
	{
		_ = AssertExceptionThrown<ArgumentNullException>("Exception expected when cusExitHeader parameter is null", () => new CusExitHeaderDocumentSupporter(null));
	}

	public void TestBusinessContext()
	{
		AssertEquals("BusinessContext", BusinessContext.CusExitHeader, documentSupporter.BusinessContext);
	}

	public void TestSupportedChildBusinessContexts()
	{
		AssertArrayEqualsByElements("SupportedChildBusinessContexts", [BusinessContext.CusExitHeader, BusinessContext.CusExitReport], documentSupporter.SupportedChildBusinessContexts);
	}

	public void TestGetChildCollection() => CombineAssertions(() =>
	{
		var menu = Factory.New<StmMenuItem>();

		AssertEquals(1, documentSupporter.GetChildCollection(menu, BusinessContext.CusExitHeader, null).Length);
		AssertEquals(header, documentSupporter.GetChildCollection(menu, BusinessContext.CusExitHeader, null)[0]);
	});

	public void TestGetChildCollection_CusExitReport() => CombineAssertions(() =>
	{
		var menu = Factory.New<StmMenuItem>();

		AssertEquals(0, documentSupporter.GetChildCollection(menu, BusinessContext.CusExitReport, null).Length);

		_ = header.CusExitReports.AddNew();

		AssertEquals(1, documentSupporter.GetChildCollection(menu, BusinessContext.CusExitReport, null).Length);
	});

	protected override void SetUp()
	{
		base.SetUp();

		header = Factory.New<CusExitHeader>();
		documentSupporter = (CusExitHeaderDocumentSupporter)header.DocumentSupporter;
	}
	CusExitHeaderDocumentSupporter documentSupporter;
	CusExitHeader header;

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Factory.New<CusExitHeader>();
}
