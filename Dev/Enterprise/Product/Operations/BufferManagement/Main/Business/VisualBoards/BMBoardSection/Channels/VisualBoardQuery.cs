using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	class VisualBoardQuery : IVisualBoardQuery
	{
		internal VisualBoardQuery()
		{
			Staffs = new List<IStaffCode>();
			Capabilities = new List<ZGuid>();
			Groups = new List<ZGuid>();
			TagMagnitudes = new List<ZGuid>();
			StaffCapabilities = new List<ICapabilityScope>();
		}

		public IList<IStaffCode> Staffs { get; }
		public IList<ZGuid> Capabilities { get; }
		public IList<ZGuid> Groups { get; }
		public IList<ZGuid> TagMagnitudes { get; }
		public IList<ICapabilityScope> StaffCapabilities { get; }
	}
}
