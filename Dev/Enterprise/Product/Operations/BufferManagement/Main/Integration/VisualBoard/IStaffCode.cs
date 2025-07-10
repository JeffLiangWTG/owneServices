using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IStaffCode
	{
		ZGuid PK { get; }
		ZString Code { get; }
	}
}
