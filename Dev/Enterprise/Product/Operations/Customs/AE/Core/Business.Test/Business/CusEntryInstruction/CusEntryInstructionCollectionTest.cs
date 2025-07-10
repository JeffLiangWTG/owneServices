using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(CusEntryInstructionCollection))]
sealed class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestChildType()
		=> AssertType<CusEntryInstruction>(Collection.AddNew());

	protected override BusinessObjectCollection GetCollectionToTest() => new CusEntryInstructionCollection(Factory.New<JobDeclaration>());
}
