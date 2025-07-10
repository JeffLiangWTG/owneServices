using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Data.Mutex
{
	public class LockInfo
	{
		internal LockInfo(ZDateTime lockStartTime, MutexID mutexID, ZString recordID, string userID, ZString hostName, ZInt processId)
		{
			this.LockStartTime = lockStartTime;
			this.MutexID = mutexID;
			this.RecordID = recordID;
			this.UserWithLock = new BusinessObjectFactory().LoadTop1<IGlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, userID));
			this.HostName = hostName;
			this.ProcessId = processId;
		}

		public ZDateTime LockStartTime { get; private set; }
		public MutexID MutexID { get; private set; }
		public ZString RecordID { get; private set; }
		public IGlbStaff UserWithLock { get; private set; }
		public ZString HostName { get; private set; }
		public ZInt ProcessId { get; private set; }
	}
}
