using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class PreviousDocumentProcedureCodeListProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When previousDocument is null", () => new PreviousDocumentProcedureCodeListProvider(previousDocument: null));
	}

	public void TestGetProcedureCodeListWhenImport()
	{
		declaration.JE_MessageType = "IMP";

		CombineAssertions(() =>
		{
			var procedureCodeList = procedureCodeListProvider.GetProcedureCodeList();
			AssertType<ImportPreviousDocumentProcedureList>("Type", procedureCodeList);

			var expectedOrderedCodesAsString = new ImportPreviousDocumentProcedureList();
			expectedOrderedCodesAsString.Sort();
			AssertEquals("Sorted CodesAsString", expectedOrderedCodesAsString.CodesAsString, procedureCodeList.CodesAsString);
		});
	}

	public void TestGetProcedureCodeListWhenNotImport()
	{
		declaration.JE_MessageType = "EXP";

		CombineAssertions(() =>
		{
			var procedureCodeList = procedureCodeListProvider.GetProcedureCodeList();
			AssertType<PreviousDocumentProcedureList>("Type", procedureCodeList);

			var expectedOrderedCodesAsString = new PreviousDocumentProcedureList();
			expectedOrderedCodesAsString.Sort();
			AssertEquals("Sorted CodesAsString", expectedOrderedCodesAsString.CodesAsString, procedureCodeList.CodesAsString);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		previousDocument = declaration.PreviousDocuments.AddNew();
		procedureCodeListProvider = new PreviousDocumentProcedureCodeListProvider(previousDocument);
	}

	JobDeclaration declaration;
	PreviousDocument previousDocument;
	IPreviousDocumentProcedureCodeListProvider procedureCodeListProvider;
}
