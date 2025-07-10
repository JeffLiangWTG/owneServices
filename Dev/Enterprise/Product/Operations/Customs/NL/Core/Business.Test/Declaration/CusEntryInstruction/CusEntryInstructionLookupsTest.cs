using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

public class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCPCList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(CountryCodes.Netherlands, "", "71", "00", "", "DES1", "IMP", group: "B1");
		helper.CreateRefCusProcedure(CountryCodes.Netherlands, "", "71", "78", "", "DES2", "IMP", group: "B1");
		helper.CreateRefCusProcedure(CountryCodes.Netherlands, "", "49", "00", "C22", "DES3", "IMP", group: "C1");
		helper.CreateRefCusProcedure(CountryCodes.Netherlands, "", "42", "71", "C18", "DES4", "IMP", group: "C1");
		Factory.Save();

		CombineAssertions(() =>
		{
			instruction.CEI_Style = DeclarationTypeList.Codes.B1;
			AssertEquals("CEI_Style is B1", "71", lookups.CPCList.CodesAsString);

			instruction.CEI_Style = DeclarationTypeList.Codes.C1;
			var list = lookups.CPCList;
			AssertEquals("CEI_Style is C1", "49, 42", list.CodesAsString);
			AssertSame("Cached", list, lookups.CPCList);
		});
	}

	public void TestCEIStyleList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(CountryCodes.Netherlands, "", "20", "20", "200", "test2", "IMP", group: "H1,H2,H3,H4,H5,H6,I1,I2,TST");
		helper.CreateRefCusProcedure(CountryCodes.Netherlands, "", "30", "30", "300", "test2", "EXP", group: "B1,B2,B3,B4,C1,C2,TST");
		helper.CreateRefCusProcedure(CountryCodes.Netherlands, "", "40", "40", "400", "test3", "IMP, EXP", group: "H1, H2, B1, B2");
		Factory.Save();

		CombineAssertions(() =>
		{
			var list = lookups.StyleList;
			AssertEquals("CodesAsString", "H1, H2, H3, H4, H5, H6, I1, I2", list.CodesAsString);
			AssertSame("Cached", list, lookups.StyleList);
		});

		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			var list = lookups.StyleList;
			AssertEquals("CodesAsString", "B1, B2, B3, B4, C1, C2", list.CodesAsString);
			AssertSame("Cached", list, lookups.StyleList);
		});
	}

	public void TestCEISubStyleList()
	{
		CombineAssertions(() =>
		{
			var list = lookups.EntrySubStyleList;
			AssertEquals("CodesAsString", "A, B, C, D, E, F, U, V, X, Y, Z", list.CodesAsString);
			AssertSame("Cached", list, lookups.EntrySubStyleList);
		});

		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Export;
		CombineAssertions(() =>
		{
			var list = lookups.EntrySubStyleList;
			AssertEquals("CodesAsString", "A, B, C, D, E, F, R, U, V, X, Y, Z", list.CodesAsString);
			AssertSame("Cached", list, lookups.EntrySubStyleList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Customs.Common.EU.EUJobMessageTypeList.Codes.Import;
		instruction = declaration.CustomsEntryInstructions.AddNew();
		lookups = instruction.Lookups;
	}
	JobDeclaration declaration;
	CusEntryInstruction instruction;
	CusEntryInstructionLookups lookups;
}
