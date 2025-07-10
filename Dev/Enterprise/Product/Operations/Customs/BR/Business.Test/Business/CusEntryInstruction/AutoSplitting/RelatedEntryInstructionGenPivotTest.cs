using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(RelatedEntryInstructionGenPivot))]
	class RelatedEntryInstructionGenPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals(GenPivotTypeDecider.Types.RelatedEntryInstructionGenPivot, RelatedEntryInstructionGenPivot.XX_RelationType);
				AssertEquals(CusEntryInstructionSchema.Constants.Prefix, RelatedEntryInstructionGenPivot.XX_Relation1TableCode);
				AssertEquals(CusEntryInstructionSchema.Constants.Prefix, RelatedEntryInstructionGenPivot.XX_Relation2TableCode);
			});
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

			var instructionParent = declaration.CustomsEntryInstructions.AddNew();
			instructionParent.CEI_Description = "PARENT_TEST";
			var instructionSplit = declaration.CustomsEntryInstructions.AddNew();
			instructionSplit.CEI_Description = "SPLIT_TEST";

			var collection = instructionSplit.ParentEntryInstructionGenPivotCollection;
			collection.AddPivotFor(instructionParent);
			var pivot = collection.Cast<RelatedEntryInstructionGenPivot>().FirstOrDefault();
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("ParentEntryInstruction should be instructionOriginal.PK", instructionParent.PK, pivot.ParentEntryInstruction.PK);
				AssertEquals("Relation1ID should be instructionSplit.PK", instructionSplit.PK, pivot.Relation1ID);
				AssertEquals("Relation2ID should be instructionOriginal.PK", instructionParent.PK, pivot.Relation2ID);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return RelatedEntryInstructionGenPivot;
		}

		RelatedEntryInstructionGenPivot RelatedEntryInstructionGenPivot
		{
			get
			{
				return fRelatedEntryInstructionGenPivot ?? (fRelatedEntryInstructionGenPivot = Factory.New<RelatedEntryInstructionGenPivot>());
			}
		}

		RelatedEntryInstructionGenPivot fRelatedEntryInstructionGenPivot;
	}
}
