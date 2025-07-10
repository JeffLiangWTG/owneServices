using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(GuaranteeForDeclarationCollection))]
	public class GuaranteeForDeclarationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestPivotIsCreatedWhenGuranteeIsAdded()
		{
			var declaration = Factory.New<JobDeclaration>();
			var gurantee = declaration.Guarantees.AddNew();
			var pivots = Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, gurantee.PK));
			AssertEquals(1, pivots.Length);
			var entryInstructionPivot = pivots[0];
			AssertEquals("entryInstructionPivot.XX_RelationType", GenPivotTypeDecider.Types.CusBondDetailRelatedEntryInstructionPivot, entryInstructionPivot.XX_RelationType);
			AssertEquals("entryInstructionPivot.XX_Relation1TableCode", CusBondDetailSchema.Constants.Prefix, entryInstructionPivot.XX_Relation1TableCode);
			AssertEquals("entryInstructionPivot.XX_Relation2TableCode", CusEntryInstructionSchema.Constants.Prefix, entryInstructionPivot.XX_Relation2TableCode);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.Guarantees;
		}
	}
}
