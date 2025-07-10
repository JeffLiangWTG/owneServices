using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMComponent : IBusiness
	{
		ZGuid PK { get; }
		ZGuid FC_FS_System { get; set; }
		ZGuid FC_FC_ParentComponent { get; set; }

		ZString FC_Name { get; set; }
		ZString FC_Type { get; set; }

		ZInt FC_BufferTimespanInMinutes { get; set; }
		ZInt FC_OffsetInMinutes { get; set; }
		ZInt FC_DisplaySequence { get; set; }

		ZBool FC_IsActive { get; set; }

		ZDateTime FC_SystemCreateTimeUtc { get; set; }
		ZString FC_SystemCreateUser { get; set; }
		ZDateTime FC_SystemLastEditTimeUtc { get; set; }
		ZString FC_SystemLastEditUser { get; set; }

		bool IsBuffer { get; }

		IBusinessObjectCollection FromMeToOthersLinks { get; }
		IBusinessObjectCollection FromOthersToMeLinks { get; }
		IBMComponentReleaseGroupLinkCollection ReleaseGroupLinks { get; }

		IBMComponent ParentComponent { get; }
	}
}
