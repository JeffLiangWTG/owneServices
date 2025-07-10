using CargoWise.Types;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public abstract class LinehaulManifestCommonInstructionStrategy
	{
		public abstract AddressWrapper GetAddress();
		public abstract ZInt GetSequence();
		public abstract ZString GetInstructionType();
	}
}
