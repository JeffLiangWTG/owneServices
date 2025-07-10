using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(NonPersistentCusDV1DetailPivot))]
	sealed class NonPersistentCusDV1DetailPivotTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeleteWhenOtherSelected()
		{
			AssertNull(LoadGenPivot(dv1Detail));
			AssertNull(LoadGenPivot(dv1Detail2));
			AssertEquals(false, dv1DetailPivot2.IsForEntryInstruction);
			AssertEquals(false, dv1DetailPivot.IsForEntryInstruction);

			dv1DetailPivot.IsForEntryInstruction = true;

			AssertNotNull(LoadGenPivot(dv1Detail));
			AssertNull(LoadGenPivot(dv1Detail2));
			AssertEquals(false, dv1DetailPivot2.IsForEntryInstruction);

			dv1DetailPivot2.IsForEntryInstruction = true;

			AssertNull(LoadGenPivot(dv1Detail));
			AssertNotNull(LoadGenPivot(dv1Detail2));
			AssertEquals(false, dv1DetailPivot.IsForEntryInstruction);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			dv1DetailPivot.IsForEntryInstruction = true;
			return dv1DetailPivot;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			dv1Detail = declaration.DV1Details.AddNew();
			dv1DetailPivot = entryInstruction.DV1DetailsPivots[0];
			dv1Detail2 = declaration.DV1Details.AddNew();
			dv1DetailPivot2 = entryInstruction.DV1DetailsPivots.First(e => e != dv1DetailPivot);
		}

		CusDV1Detail dv1Detail;
		CusDV1Detail dv1Detail2;
		NonPersistentCusDV1DetailPivot dv1DetailPivot;
		NonPersistentCusDV1DetailPivot dv1DetailPivot2;
		CusEntryInstruction entryInstruction;

		GenPivot LoadGenPivot(CusDV1Detail dv1Detail)
		{
			var pivotQuery = new ZQuery(GenPivotSchema.XX_RelationType, Core.Constants.GenPivotTypes.CusDV1Detail);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, entryInstruction.PK);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2ID, dv1Detail.PK);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, CusEntryInstructionSchema.Constants.Prefix);
			pivotQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, CusDV1DetailSchema.Constants.Prefix);

			return entryInstruction.Factory.LoadTop1<GenPivot>(pivotQuery);
		}
	}
}
