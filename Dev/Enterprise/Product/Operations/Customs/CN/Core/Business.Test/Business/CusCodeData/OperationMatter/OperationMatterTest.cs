using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(OperationMatter))]
	class OperationMatterTest : Customs.Business.Testing.CusCodeDataTest<OperationMatter>
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, ((OperationMatter)BusinessObject).SupportsNotes);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			return declaration.CustomsEntryInstructions.AddNew().OperationMatters.AddNew();
		}

		public void TestDefaultValue()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var operationMatter = instruction.OperationMatters.AddNew();
			AssertEquals(Constants.CusCodeDataTypes.Codes.OperationMatter, operationMatter.CY_Type);
		}
	}
}
