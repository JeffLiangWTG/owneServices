using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(JobDeclarationDocumentSupporter))]
sealed class JobDeclarationDocumentSupporterTest : EU.Business.Declaration.Testing.JobDeclarationDocumentSupporterTest
{
	public void TestGetDocumentWrappersInternal_UTBDataContext()
	{
		AssertGetDocumentWrappersInternal_DataContext("UTB");
	}

	public void TestGetDocumentWrappersInternal_ReleaseDocumentDataContext()
	{
		AssertGetDocumentWrappersInternal_DataContext("Release");
	}

	public void TestDocumentSupporter()
	{
		var declarationDocumentSupporter = (JobDeclarationDocumentSupporter)Factory.New<JobDeclaration>().DocumentSupporter;
		AssertType<JobDeclarationDocumentSupporter>("JobDeclarationDocumentSupporter for NL expected", declarationDocumentSupporter);
	}

	public void TestGetSupportedBODataSources()
	{
		var declarationDocumentSupporter = (JobDeclarationDocumentSupporter)Factory.New<JobDeclaration>().DocumentSupporter;
		AssertContains(".UTBDocument, .ReleaseDocument", declarationDocumentSupporter.CommaSeparatedListOfSupportedDataContexts);
	}

	public void AssertGetDocumentWrappersInternal_DataContext(string menuName)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		var menu = Factory.New<IStmMenuItem>();
		menu.SU_MenuName = menuName;

		DocumentWrapper[] result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, menu);
		AssertEquals(0, result.Length);

		declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MergedLines.AddNew();

		result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusEntryHeader, menu);
		AssertEquals(1, result.Length);
	}

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.GoodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
		var inv = declaration.Invoices.AddNew();
		inv.FillWithValidTestData();
		var invLine = inv.InvoiceLines.AddNew();
		invLine.FillWithValidTestData();

		var merger = new EU.Business.Declaration.LineMerger(declaration);
		merger.DoMerge();

		return declaration;
	}

	protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
	{
		return documentCommand.SU_MenuName.StartsWith("Landed Costing") || base.ExcludeDocumentCommandTest(documentCommand);
	}
}
