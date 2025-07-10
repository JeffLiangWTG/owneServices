using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IVisualBoardQuery
	{
		IList<ZGuid> Capabilities { get; }
		IList<ZGuid> Groups { get; }
		IList<IStaffCode> Staffs { get; }
		IList<ICapabilityScope> StaffCapabilities { get; }
		IList<ZGuid> TagMagnitudes { get; }
	}
}
