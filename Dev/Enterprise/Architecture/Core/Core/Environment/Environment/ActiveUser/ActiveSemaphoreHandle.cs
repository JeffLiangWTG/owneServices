using System;

namespace Enterprise.Core.Environment
{
	public interface IActiveSemaphoreHandle
	{
		string Category { get; }
		string LockInfo { get; }
		int UseCount { get; }
		DateTime CreateTimeUtc { get; }
	}

	class ActiveSemaphoreHandle : IActiveSemaphoreHandle
	{
		public ActiveSemaphoreHandle(string category, string lockInfo, int useCount, DateTime creteTimeUtc)
		{
			Category = category;
			LockInfo = lockInfo;
			UseCount = useCount;
			CreateTimeUtc = creteTimeUtc;
		}

		public string Category { get; private set; }
		public string LockInfo { get; private set; }
		public int UseCount { get; private set; }
		public DateTime CreateTimeUtc { get; private set; }
	}
}
