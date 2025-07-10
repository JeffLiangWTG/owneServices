using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business.Testing
{
	internal class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQualifierList()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "AH";

			var lookups = entryInstruction.GoodsLocation.Lookups;
			CombineAssertions(() =>
			{
				AssertEquals("Base list when IMP declaration and CEI_Style != H2", "T, U, V, W, X, Y, Z", lookups.QualifierList.CodesAsString);

				entryInstruction.CEI_Style = "H2";
				AssertEquals("ES list when IMP declaration and CEI_Style == H2", "Y, Z", lookups.QualifierList.CodesAsString);

				declaration.JE_MessageType = "EXP";
				AssertEquals("Base list when EXP declaration", "T, U, V, W, X, Y, Z", lookups.QualifierList.CodesAsString);
			});
		}
	}
}
