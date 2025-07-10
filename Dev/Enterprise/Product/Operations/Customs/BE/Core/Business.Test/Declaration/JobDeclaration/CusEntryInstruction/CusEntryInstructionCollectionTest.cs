using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(CusEntryInstructionCollection))]
class CusEntryInstructionCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest() => new CusEntryInstructionCollection(Factory.New<JobDeclaration>());
}
