using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMReleaseSequence
	{
		ZGuid BMR_PK { get; }
		ZString BMR_Name { get; }
	}
}
