using System;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	[Serializable]
	public class DbBackupAndRestoreException : Exception
	{
		public DbBackupAndRestoreException(string message)
			: base(message)
		{
		}

		public DbBackupAndRestoreException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected DbBackupAndRestoreException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
