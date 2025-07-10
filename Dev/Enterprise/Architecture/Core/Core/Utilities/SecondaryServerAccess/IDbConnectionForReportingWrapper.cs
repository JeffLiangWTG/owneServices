using System;
using CargoWise.Data;

namespace Enterprise.ZArchitecture.Core
{
	public interface IDbConnectionForReportingWrapper : IDisposable
	{
		DbConnection Connection { get; }
		bool IsMainServer { get; }
	}
}
