using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(GuaranteeForEntryInstructionCollection))]
	public class GuaranteeForEntryInstructionCollectionTest : GuaranteeForEntryInstructionCollectionAbstractTest<GuaranteeForEntryInstructionCollection>
	{
		protected override GuaranteeForEntryInstructionCollection GetNewGuaranteeCollection() => Factory.New<CusEntryInstruction>().Guarantees;

		protected override int MaxCountForValidation
		{
			get => maxCountForValidation;
		}
		const int maxCountForValidation = 9;
	}
}
