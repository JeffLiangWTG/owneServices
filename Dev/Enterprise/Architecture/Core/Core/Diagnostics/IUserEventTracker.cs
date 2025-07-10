using System;

namespace Enterprise.ZArchitecture.Core
{
	public interface IUserEventTracker
	{
		IDisposable TemporarilyDisable();
		void ClearAll();
		bool IsEnabled { get; }
		string UserEventDescription { get; }
		string LastSqlQuery { get; }
		string SqlEventDescription { get; }
		string SqlFailedEventDescription { get; }
		string DatabaseConnectionEventDescription { get; }
	}
}
