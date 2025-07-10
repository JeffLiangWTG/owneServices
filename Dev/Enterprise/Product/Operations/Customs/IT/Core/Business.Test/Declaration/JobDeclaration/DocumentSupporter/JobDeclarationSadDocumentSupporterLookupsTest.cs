using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationSadDocumentSupporterLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestEntryHeadersAvailableForPrinting()
	{
		var lookups = GetNewJobDeclarationSadDocumentSupporterLookups();

		AssertEquals("[PRE-CONDITION] No entries", 0, declaration.CustomsEntryHeaders.Count);

		AssertNotNull("EntryHeadersAvailableForPrinting", lookups.EntryHeadersAvailableForPrinting);
		AssertArrayEqualsByElements("Empty Lookups", System.Array.Empty<string>(), lookups.EntryHeadersAvailableForPrinting.GetAllCodes());

		var entry1 = declaration.CustomsEntryHeaders.AddNew();
		var entry2 = declaration.CustomsEntryHeaders.AddNew();
		var entry3 = declaration.CustomsEntryHeaders.AddNew();

		entry1.CH_EntryStatus = "";
		entry1.CH_BGMReference = "0001";
		entry2.CH_EntryStatus = "ICC";
		entry2.CH_BGMReference = "0002";
		entry3.CH_EntryStatus = "REG";
		entry3.CH_BGMReference = "0003";

		AssertArrayEqualsByElements("Lookups Codes", new string[] { "0001", "0002", "0003" }, lookups.EntryHeadersAvailableForPrinting.GetAllCodes());
		AssertArrayEqualsByElements("Lookups Description", new string[] { "[no status] 0001", "[ICC] 0002", "[REG] 0003" }, GetAllDescription(lookups.EntryHeadersAvailableForPrinting).ToArray());
	}

	public void TestLayoutStyleList()
	{
		var lookups = GetNewJobDeclarationSadDocumentSupporterLookups();
		AssertNotNull("LayoutStyleList", lookups.LayoutStyleList);

		declaration.JE_MessageType = "IMP";
		AssertType<SADImportCopyNumberList>("LayoutStyleList codes for import", lookups.LayoutStyleList);

		CombineAssertions("Assert LayoutStyleList for EXP", () =>
		{
			declaration.JE_MessageType = "EXP";
			AssertType<SADExportCopyNumberList>("LayoutStyleList codes for export", lookups.LayoutStyleList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declarationSadDocumentSupporter = new JobDeclarationSadDocumentSupporter(declaration);
	}
	JobDeclaration declaration;
	JobDeclarationSadDocumentSupporter declarationSadDocumentSupporter;

	JobDeclarationSadDocumentSupporterLookups GetNewJobDeclarationSadDocumentSupporterLookups() => new JobDeclarationSadDocumentSupporterLookups(declarationSadDocumentSupporter);

	IEnumerable<string> GetAllDescription(CodeDescriptionPairList lookups)
	{
		foreach (var code in lookups.GetAllCodes())
		{
			yield return lookups.GetDescriptionFromCode(code);
		}
	}
}
