using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(SubsetCusEntryInstructionCollection))]
	class SubsetCusEntryInstructionCollectionTest : SubsetBusinessObjectCollectionTestCase<SubsetCusEntryInstructionCollection, CusEntryInstruction>
	{
		public void TestBuild()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();
			instruction2.CEI_LegalDocument = LegalDocumentList.Codes.ElectronicLogisticInvoice;
			var instruction3 = declaration.CustomsEntryInstructions.AddNew();
			instruction3.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
			AssertContainsExactElementsInAnyOrder(new[] { instruction1, instruction2 }, new SubsetCusEntryInstructionCollection(declaration.CustomsEntryInstructions, x => x.CEI_LegalDocument == LegalDocumentList.Codes.ElectronicLogisticInvoice));
			AssertEquals(0, new SubsetCusEntryInstructionCollection(declaration.CustomsEntryInstructions, x => false).Count);
		}

		protected override SubsetCusEntryInstructionCollection GetCollectionToTest() => new SubsetCusEntryInstructionCollection(Declaration.CustomsEntryInstructions, x => true);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<CusEntryInstruction>();
			result.CEI_JE = Declaration.PK;
			return result;
		}

		JobDeclaration Declaration => fDeclaration ?? (fDeclaration = Factory.New<JobDeclaration>());
		JobDeclaration fDeclaration;
	}
}
