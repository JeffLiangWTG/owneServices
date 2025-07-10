using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(ESGuaranteeCollection))]
	class ESGuaranteeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.Guarantees;
		}

		public void TestGetReferenceForType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

			ZString guaranteeType = "A";
			ZString guaranteeReference = "Guarantee";

			GuaranteesTestHelper.CreateGuaranteeForEntryInstruction(declaration, (entryInstruction2.PK, "Guarantee"));
			declaration.Guarantees[0].PW_BondType = guaranteeType;

			AssertEquals(1, declaration.Guarantees.Count);

			CombineAssertions(() =>
			{
				AssertEquals("Expected reference is empty when guarantee entry instruction is not the same as the entry instruction given", ZString.Empty, declaration.Guarantees.GetReferenceForType(entryInstruction1.PK, guaranteeType));

				AssertEquals("Expected reference is empty when guarantee entry instruction is the same as the entry instruction given but bond type is not correct", ZString.Empty, declaration.Guarantees.GetReferenceForType(entryInstruction2.PK, "B"));

				AssertEquals("Expected reference is filled when guarantee entry instruction is the same as the entry instruction given and bond type is correct", guaranteeReference, declaration.Guarantees.GetReferenceForType(entryInstruction2.PK, guaranteeType));
			});
		}

		public void TestGetGRNReferencesForCharacter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_SubStyle = EntrySubStyleList.Codes.B;

			var guaranteeCharacter = 'A';

			var grnGuaranteesCodes = new ZString[]
			{
				"1234A1", "1234A2", "1234A3", "1234A4", "1234A5", "1234A6", "1234A7", "1234A8", "1234A9", "1234A10"
			};

			GuaranteesTestHelper.CreateMultipleGuaranteesForEntryInstruction(declaration, entryInstruction2.PK, grnGuaranteesCodes);

			CombineAssertions(() =>
			{
				AssertEquals("Expected reference ist is empty when guarantees entry instruction is not the same as the entry instruction given", 0, declaration.Guarantees.GetGRNReferencesForCharacter(entryInstruction1.PK, guaranteeCharacter).Count);

				AssertEquals("Expected reference list is empty when guarantee entry instruction is the same as the entry instruction given but character is not correct", 0, declaration.Guarantees.GetGRNReferencesForCharacter(entryInstruction2.PK, 'B').Count);

				AssertArrayEqualsByElements("Expected reference list is filled when guarantee entry instruction is the same as the entry instruction given and the given character is correct", grnGuaranteesCodes, declaration.Guarantees.GetGRNReferencesForCharacter(entryInstruction2.PK, guaranteeCharacter).ToArray());
			});
		}
	}
}
