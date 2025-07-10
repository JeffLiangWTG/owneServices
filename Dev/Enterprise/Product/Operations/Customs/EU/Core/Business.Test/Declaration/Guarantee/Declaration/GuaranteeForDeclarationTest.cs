using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(GuaranteeForDeclaration))]
	public class GuaranteeForDeclarationTest : CommonGuaranteeTest
	{
		protected override BusinessObject GetNewBusinessObject() => Factory.New<JobDeclaration>().Guarantees.AddNew();

		public void TestEntryInstructionID()
		{
			var guarantee = (GuaranteeForDeclaration)GetNewBusinessObject();
			var declaration = guarantee.Declaration;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "A";

			guarantee.EntryInstructionID = entryInstruction.PK;
			AssertEquals(entryInstruction, guarantee.EntryInstruction);

			var genPivot = Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, guarantee.PK)).FirstOrDefault();

			AssertNotNull(genPivot);
			AssertEquals(entryInstruction.PK, genPivot.XX_Relation2ID);

			guarantee.EntryInstructionID = Guid.Empty;
			genPivot = Factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation1ID, guarantee.PK)).FirstOrDefault();
			AssertNull("The pivot should be deleted when set entry instruction to blank", genPivot);
		}

		public override void TestLookups()
		{
			var guarantee = (GuaranteeForDeclaration)GetNewBusinessObject();
			AssertType<GuaranteeForDeclarationLookups>(guarantee.Lookups);
		}
	}
}
