using Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	sealed class LinehaulManifestPickupInstructionTest : LinehaulManifestCommonInstructionStrategyTest
	{
		#region TestProperties

		public void TestPropertiesOnPickupInstruction()
		{
			CombineAssertions(() =>
			{
				AssertNotNull(Instruction);
				AssertEquals(depot1.OA_Address1, Instruction.GetAddress().AddressLine1);
				AssertEquals(1, Instruction.GetSequence());
				AssertEquals(InstructionTypes.Descriptions.PickUp, Instruction.GetInstructionType());
			});
		}

		#endregion

		#region Implementation

		public override LinehaulManifestCommonInstructionStrategy Instruction
		{
			get
			{
				var manifest = GetManifest();
				return new LinehaulManifestPickupInstructionStrategy(manifest, Factory);
			}
		}

		#endregion
	}
}
