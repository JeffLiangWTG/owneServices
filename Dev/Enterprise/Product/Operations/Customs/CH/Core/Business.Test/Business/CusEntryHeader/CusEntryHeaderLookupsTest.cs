using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusEntryHeaderLookups))]
sealed class CusEntryHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCH_EntryStatusList()
	{
		RefCusCodeTestHelper.CreateCustomsStatusCodeListAndFrenchLanguage(Factory);

		AssertEquals(8, lookups.CH_EntryStatusList.Count);
		CombineAssertions("CH_EntryStatusList should contain these codes", () =>
		{
			Assert(lookups.CH_EntryStatusList.ContainsCode("123"));
			Assert(lookups.CH_EntryStatusList.ContainsCode("456"));
			foreach (var code in new AdditionalCHEntryStatusList().GetAllCodes())
			{
				Assert(lookups.CH_EntryStatusList.ContainsCode(code));
			}
		});
	}

	public void TestMessageStatusList() => AssertType<CHLogicalStatusList>(lookups.MessageStatusList);

	public void TestPhaseStatusList() => AssertType<PassarDeclarationPhaseList>(lookups.PhaseStatusList);

	public void TestSelectionResultList()
	{
		RefCusCodeTestHelper.CreateSelectionResultCodeListAndFrenchLanguage(Factory);

		AssertEquals(2, lookups.SelectionResultList.Count);
		CombineAssertions("SelectionResultList should contain these codes", () =>
		{
			Assert(lookups.SelectionResultList.ContainsCode("123"));
		});
	}

	public void TestCEI_LastEComStatusList()
	{
		AssertEquals("List Codes", "ACC, CLS, , RCV, REJ, SNT", lookups.EComplaintStatusList.CodesAsString);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entry = declaration.CustomsEntryHeaders.AddNew();
		lookups = entry.Lookups;
	}

	CusEntryHeaderLookups lookups;
}
