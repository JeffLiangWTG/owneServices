using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusEntryInstructionLookups))]
sealed class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestWeightUQList()
	{
		AssertSame(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), Lookups.WeightUQList);
	}

	public void TestStyleList()
	{
		var lookedUpList = Lookups.StyleList;
		CombineAssertions(() =>
		{
			AssertSame(Factory.GetCachedValue<DeclarationTypeList>(), lookedUpList);
			AssertEquals("StyleList values", "FEI, NFEI", lookedUpList.CodesAsString);
		});
	}

	public void TestMessageStatusList()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var messageStatusList = instruction.Lookups.MessageStatusList;
		AssertEquals("MessageStatusList empty when header is null", 0, messageStatusList.Count);

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		messageStatusList = instruction.Lookups.MessageStatusList;
		AssertContainsExactElementsInExactOrder("MessageStatusList have values in lookup", new[] { "INV", "ERR", "ACC", "FAL", "QUE", "SNT" }, messageStatusList.GetAllCodes());
		AssertSame("cached", messageStatusList, instruction.Lookups.MessageStatusList);
	}

	public void TestCustomsStatusList()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var customsStatusList = instruction.Lookups.CustomsStatusList;
		AssertEquals("CustomsStatusList empty when header is null", 0, customsStatusList.Count);

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		customsStatusList = instruction.Lookups.CustomsStatusList;
		AssertContainsExactElementsInExactOrder("CustomsStatusList have values in lookup", new[] { "FSC", "GPR", "LEO", "SBA" }, customsStatusList.GetAllCodes());
		AssertSame("cached", customsStatusList, instruction.Lookups.CustomsStatusList);
	}

	public void TestEntrySubStyleList()
	{
		RefDataSetupTestHelper.SetupNFEICategoryCodes(Factory);

		var lookedUpList = Lookups.EntrySubStyleList;
		CombineAssertions(() =>
		{
			AssertSame(Lookups.EntrySubStyleList, lookedUpList);
			AssertContains("AA, BB", lookedUpList.CodesAsString);
		});
	}

	public void TestPackageUQList()
	{
		var lookedUpList = Lookups.PackageUQList;
		CombineAssertions(() =>
		{
			AssertSame(Lookups.PackageUQList, lookedUpList);
			AssertEquals("PackageUQList values", "PKG", lookedUpList.CodesAsString);
		});
	}

	CusEntryInstructionLookups Lookups => lookups ??= Factory.New<CusEntryInstruction>().Lookups;
	CusEntryInstructionLookups lookups;
}
