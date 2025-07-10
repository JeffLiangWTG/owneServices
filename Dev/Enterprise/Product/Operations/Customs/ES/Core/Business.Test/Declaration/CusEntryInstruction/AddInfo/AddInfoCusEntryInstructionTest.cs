using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusEntryInstruction))]
	class AddInfoCusEntryInstructionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<AddInfoCusEntryInstructionLookups>(Factory.New<CusEntryInstruction>().AddInfo.Lookups);
		}

		public void TestValidation()
		{
			AssertType<AddInfoCusEntryInstructionValidation>(Factory.New<CusEntryInstruction>().AddInfo.Validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			return new AddInfoCusEntryInstruction(instruction.CEI_AddInfoInfo);
		}
	}
}
