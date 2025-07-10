using System;
using CargoWise.Data;
using ServiceManager.Runner.Abstractions;
using static System.FormattableString;

namespace Enterprise.ServiceManager.Runner
{
	class SqlApplicationLocker : ISqlApplicationLocker
	{
		public bool TryAcquireLock(string code, out IDisposable disposableLock)
		{
			disposableLock = null;
			var resourceKey = Invariant($"ServiceTaskInitializationLock:{code ?? throw new ArgumentNullException(nameof(code))}");
			if (Db.Connection.TryGetLock(resourceKey, TimeSpan.Zero, out var sqlApplicationLock))
			{
				disposableLock = sqlApplicationLock;
				return true;
			}

			return false;
		}
	}
}
