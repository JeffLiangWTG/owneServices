using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(SubsetCusEntryInstructionCollection))]
	class SubsetCusEntryInstructionCollectionTest : SubsetBusinessObjectCollectionTestCase<SubsetCusEntryInstructionCollection, CusEntryInstruction>
	{
		public void TestBuild()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			var instruction3 = declaration.CustomsEntryInstructions.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { instruction1, instruction2, instruction3 }, new SubsetCusEntryInstructionCollection(declaration, x => true));
			AssertEquals(0, new SubsetCusEntryInstructionCollection(declaration, x => false).Count);
		}

		protected override SubsetCusEntryInstructionCollection GetCollectionToTest() => new SubsetCusEntryInstructionCollection(Declaration, x => true);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<CusEntryInstruction>();
			result.CEI_JE = Declaration.PK;
			return result;
		}

		JobDeclaration Declaration => fDeclaration ?? (fDeclaration = Factory.New<JobDeclaration>());
		JobDeclaration fDeclaration;
	}
}
