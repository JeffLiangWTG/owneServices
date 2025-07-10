using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusAuthorisation.Testing
{
	[TestedType(typeof(CusAuthorizationUsageEntryInstructionCollection))]
	class CusAuthorizationUsageEntryInstructionCollectionTest : EU.Business.Testing.CusAuthorizationUsageCollectionCusEntryInstructionAbstractTest<CusAuthorizationUsageEntryInstructionCollection, CusAuthorizationUsage, CusEntryInstruction>
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusAuthorizationUsageEntryInstructionCollection(Factory.New<CusEntryInstruction>(), Factory);

		public void TestDefaultAGC_Code()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			entryInstruction.CEI_Style = DeltaIEImportDeclarationTypeList.Codes.H1;
			var cusAuthorizationUsage1 = entryInstruction.CusAuthorizationUsages.AddNew();
			AssertEquals("When the CEI_Style is not I1, the default value of AGC_Code for the newly created CusAuthorizationUsage is empty.", ZString.Empty, cusAuthorizationUsage1.AGC_Code);
			entryInstruction.CEI_Style = DeltaIEImportDeclarationTypeList.Codes.I1;
			var cusAuthorizationUsage2 = entryInstruction.CusAuthorizationUsages.AddNew();
			AssertEquals("When the CEI_Style is I1, the default value of AGC_Code for the newly created CusAuthorizationUsage is SDE.", Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, cusAuthorizationUsage2.AGC_Code);
		}
	}
}
