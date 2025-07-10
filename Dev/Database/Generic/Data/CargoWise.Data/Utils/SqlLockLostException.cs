using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace CargoWise.Data.Utils
{
	[Serializable]
	public class SqlLockLostException : InvalidOperationException, ICriticalException
	{
		public SqlLockLostException() { }
#if NETFRAMEWORK
		protected SqlLockLostException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
#endif

		public SqlLockLostException(string message, IEnumerable<string> lockKeys)
			: base(message)
		{
			LockKeys = lockKeys;
		}

		public string KeysAsCSV
		{
			get
			{
				return (LockKeys != null && LockKeys.Any()) ?
					"'" + string.Join("', '", LockKeys) + "'" : // This is for debugging information
					"Unknown"; // This is for debugging information
			}
		}

		public IEnumerable<string> LockKeys { get; private set; }

		public override string ToString()
		{
			return Message + "\r\nLocks: " + KeysAsCSV + $"\n{base.ToString()}"; // This is for debugging information
		}

		public bool IsCriticalException => true;
	}
}
