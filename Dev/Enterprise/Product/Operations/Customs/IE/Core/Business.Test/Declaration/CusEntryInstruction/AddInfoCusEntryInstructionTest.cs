using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusEntryInstruction))]
	class AddInfoCusEntryInstructionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParent()
		{
			AssertType<CusEntryInstruction>(((AddInfoCusEntryInstruction)GetNewBusinessObject()).Parent);
		}

		public void TestValidation()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			AssertType<ImportAddInfoCusEntryInstructionValidation>(entryInstruction.AddInfoValidation);
			jobDeclaration.JE_MessageType = "EXP";
			AssertType<AddInfoCusEntryInstructionValidation>(entryInstruction.AddInfoValidation);
		}

		public void TestLookups()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			AssertType<ImportAddInfoCusEntryInstructionLookups>(entryInstruction.AddInfoLookups);
			jobDeclaration.JE_MessageType = "EXP";
			AssertType<AddInfoCusEntryInstructionLookups>(jobDeclaration.CustomsEntryInstructions.AddNew().AddInfoLookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			return new AddInfoCusEntryInstruction(instruction.CEI_AddInfoInfo);
		}
	}
}
