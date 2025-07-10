using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryInstructionCollection))]
	class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
	{
		[TestDate(2019, 4, 22)]
		public void TestSetDefaultsForNewChild_ShouldSetDateAsToday()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(new ZDateTime(2019, 4, 22), entry.CEI_DateForDuty);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new CusEntryInstructionCollection(declaration);
		}

		public void TestSetSubStyleDefaultToPreviousLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.CEI_SubStyle = EntrySubstyleCodePairList.Codes.D;
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();

			AssertEquals("Sub Style should match", entryInstruction1.CEI_SubStyle, entryInstruction2.CEI_SubStyle);
		}
	}
}
