using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(GuaranteeForEntryInstruction))]
	public class GuaranteeForEntryInstructionTest : EU.Business.Declaration.Testing.GuaranteeForEntryInstructionAbstractTest<GuaranteeForEntryInstruction>
	{
		public override void TestLookups()
		{
			var guarantee = (GuaranteeForEntryInstruction)GetNewBusinessObject();
			AssertType<GuaranteeForEntryInstructionLookups>("LooksUps for GuaranteeForEntryInstruction should be FR.GuaranteeForEntryInstructionLookups", guarantee.Lookups);
		}

		protected override GuaranteeForEntryInstruction GetNewGuaranteeForEntryInstruction() => Factory.New<CusEntryInstruction>().Guarantees.AddNew();
	}
}
