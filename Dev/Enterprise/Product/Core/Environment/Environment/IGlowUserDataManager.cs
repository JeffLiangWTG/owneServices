using System;

namespace Enterprise.Environment
{
	public interface IGlowUserDataManager
	{
		void ClearUserData();
		IDisposable IncreaseTempUserCount();
	}
}
