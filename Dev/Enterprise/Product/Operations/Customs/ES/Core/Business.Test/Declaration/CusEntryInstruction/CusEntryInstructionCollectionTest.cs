using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>))]
	public class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>(declaration);
		}

		public void TestSetDefaultsForNewChild_ShouldNotSetValue()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals(ZString.Empty, entry.CEI_SubStyle);
		}
	}
}
