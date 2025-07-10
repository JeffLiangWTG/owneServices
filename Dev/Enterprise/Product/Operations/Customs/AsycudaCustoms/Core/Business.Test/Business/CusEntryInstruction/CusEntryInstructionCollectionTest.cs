using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionCollection<CusEntryInstruction>))]
	class CusEntryInstructionCollectionTest : Customs.Business.Testing.CusEntryInstructionCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusEntryInstructionCollection<CusEntryInstruction>(Factory.NewWithValidTestData<JobDeclaration>());
	}
}
