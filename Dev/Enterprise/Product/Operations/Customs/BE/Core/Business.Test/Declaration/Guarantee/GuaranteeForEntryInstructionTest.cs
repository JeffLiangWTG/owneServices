using System;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(GuaranteeForEntryInstruction))]
sealed class GuaranteeForEntryInstructionTest : EU.Business.Declaration.Testing.GuaranteeForEntryInstructionAbstractTest<GuaranteeForEntryInstruction>
{
	public override void TestLookups()
	{
		var guarantee = (GuaranteeForEntryInstruction)GetNewBusinessObject();
		AssertType<GuaranteeForEntryInstructionLookups>(guarantee.Lookups);
	}

	protected override GuaranteeForEntryInstruction GetNewGuaranteeForEntryInstruction() => Factory.New<CusEntryInstruction>().Guarantees.AddNew();

	protected override Type GetGuaranteeForEntryInstructionValidationType() => typeof(GuaranteeForEntryInstructionValidation);
}
