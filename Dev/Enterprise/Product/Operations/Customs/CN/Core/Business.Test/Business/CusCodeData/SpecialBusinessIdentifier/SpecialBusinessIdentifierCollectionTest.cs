using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(SpecialBusinessIdentifierCollection))]
	class SpecialBusinessIdentifierCollectionTest : CusCodeDataCollectionTest<SpecialBusinessIdentifier>
	{
		public void TestGetAllOptions()
		{
			var testCollection = GetCusCodeDataCollection() as ICodeDescriptionOptionStorage;
			AssertSame(Factory.GetCachedValue<SpecialBusinessList>(), testCollection.GetAllOptions());
			AssertSame((GetCusCodeDataCollection() as ICodeDescriptionOptionStorage).GetAllOptions(), testCollection.GetAllOptions());
			AssertEquals(Factory.GetCachedValue<SpecialBusinessList>().Count, testCollection.GetAllOptions().Count);
			Assert(testCollection.GetAllOptions().ContainsCode("C03"));
		}

		public void TestValidationModeProvider()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => Factory.Save());
			var instruction = testItems.EntryInstruction;
			var collection = new SpecialBusinessIdentifierCollection(instruction);
			ValidationExtensionsTest.AssertValidationModeProvider(instruction.JobDeclaration, collection.ValidationModeProvider);

			AssertNull((GetCusCodeDataCollection() as SpecialBusinessIdentifierCollection).ValidationModeProvider);
		}

		protected override CusCodeDataCollection<SpecialBusinessIdentifier> GetCusCodeDataCollection()
		{
			return new SpecialBusinessIdentifierCollection(Instruction);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<SpecialBusinessIdentifier>();
			result.CY_ParentID = Instruction.PK;
			result.CY_ParentTableCode = Instruction.TablePrefix;
			result.CY_Type = Constants.CusCodeDataTypes.Codes.SpecialBusinessIdentifier;
			return result;
		}

		CusEntryInstruction instruction;
		CusEntryInstruction Instruction => instruction ?? (instruction = Factory.New<CusEntryInstruction>());
	}
}
