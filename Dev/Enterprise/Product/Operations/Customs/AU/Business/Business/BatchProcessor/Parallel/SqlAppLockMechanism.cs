using System;
using CargoWise.Data;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SqlAppLockMechanism : ILockMechanism
	{
		readonly string prefix;

		public SqlAppLockMechanism(string prefix)
		{
			if (string.IsNullOrWhiteSpace(prefix))
			{
				throw new ArgumentException("Lock key prefix cannot be null, empty or white-space only.", nameof(prefix));
			}

			if (prefix.Length > LockMechanismConstants.MaxPrefixLength)
			{
				throw new ArgumentException(
					FormattableString.Invariant($"Lock key prefix cannot be longer than {LockMechanismConstants.MaxPrefixLength} characters."),
					nameof(prefix)
				);
			}

			this.prefix = prefix;
		}

		public bool TryGetLock(string key, out IDisposable appLock)
		{
			var result = Db.Connection.TryGetLock(prefix + key, out var sqlAppLock);
			appLock = sqlAppLock;
			return result;
		}
	}
}
