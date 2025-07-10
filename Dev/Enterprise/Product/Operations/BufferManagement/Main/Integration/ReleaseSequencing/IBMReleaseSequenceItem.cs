using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMReleaseSequenceItem
	{
		ZInt BMI_Position { get; }
		ZInt BMI_Investment { get; }
		ZInt BMI_Value { get; }
	}
}
