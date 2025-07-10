using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMBoard : IVisualBoardProvider
	{
		ZString MB_Name { get; set; }
		ZString MB_Description { get; set; }
		ZString MB_GS_NKStaffCode { get; set; }

		ZGuid MB_FS_System { get; set; }
		ZGuid MB_GG_ReleaseGroup { get; set; }
		ZGuid MB_GB_AgingBranch { get; set; }
		ZGuid MB_GE_AgingDepartment { get; set; }

		ZBool MB_IsPublished { get; set; }

		ZDateTime MB_SystemLastEditTimeUtc { get; }

		IBMSystem System { get; }
		IEnumerable<IBMBoardSection> Sections { get; }

		ZDateTime CustomisationLastEditTimeUTC { get; }
	}
}
