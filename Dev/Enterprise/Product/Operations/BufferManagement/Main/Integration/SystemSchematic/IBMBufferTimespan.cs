using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMBufferTimespan : IBusiness
	{
		ZGuid PK { get; }
		ZBool BMT_IsActive { get; set; }
		ZString BMT_Name { get; set; }
		ZInt BMT_BufferTimespanInMinutes { get; set; }

		ZDateTime BMT_SystemCreateTimeUtc { get; set; }
		ZString BMT_SystemCreateUser { get; set; }
		ZDateTime BMT_SystemLastEditTimeUtc { get; set; }
		ZString BMT_SystemLastEditUser { get; set; }
	}
}
