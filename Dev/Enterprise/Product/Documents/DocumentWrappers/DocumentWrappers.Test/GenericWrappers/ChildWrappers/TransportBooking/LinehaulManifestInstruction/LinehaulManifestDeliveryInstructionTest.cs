using Enterprise.TransportCommon.Shared;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class LinehaulManifestDeliveryInstructionTest : LinehaulManifestCommonInstructionStrategyTest
	{
		#region TestProperties

		public void TestPropertiesOnDeliveryInstruction()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(Instruction);
				AssertEquals(depot2.OA_Address1, Instruction.GetAddress().AddressLine1);
				AssertEquals(2, Instruction.GetSequence());
				AssertEquals(InstructionTypes.Descriptions.Delivery, Instruction.GetInstructionType());
			});
		}

		#endregion

		#region Implementation

		public override LinehaulManifestCommonInstructionStrategy Instruction
		{
			get
			{
				var manifest = GetManifest();
				return new LinehaulManifestDeliveryInstructionStrategy(manifest, Factory);
			}
		}

		#endregion
	}
}
