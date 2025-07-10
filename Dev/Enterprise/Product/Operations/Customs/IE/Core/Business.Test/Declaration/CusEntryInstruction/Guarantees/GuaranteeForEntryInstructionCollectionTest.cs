using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(GuaranteeForEntryInstructionCollection))]
	sealed class GuaranteeForEntryInstructionCollectionTest : GuaranteeForEntryInstructionCollectionAbstractTest<GuaranteeForEntryInstructionCollection>
	{
		protected override GuaranteeForEntryInstructionCollection GetNewGuaranteeCollection() => Factory.New<CusEntryInstruction>().Guarantees;
	}
}
