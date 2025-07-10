using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>))]
	class CusAuthorizationUsageEntryInstructionCollectionTest : CusAuthorizationUsageCollectionCusEntryInstructionAbstractTest<CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>, CusAuthorizationUsage, CusEntryInstruction>
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, CusEntryInstruction>(Factory.New<CusEntryInstruction>(), Factory);
	}
}
