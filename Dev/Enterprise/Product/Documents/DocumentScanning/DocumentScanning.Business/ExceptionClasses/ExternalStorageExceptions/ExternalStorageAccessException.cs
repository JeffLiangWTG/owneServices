using System;
using Enterprise.DocumentScanning.Integration;

namespace Enterprise.DocumentScanning.Business
{
	[Serializable]
	public class ExternalStorageAccessException : ExternalStorageException
	{
		public ExternalStorageAccessException(string account, string message, string provider, Exception innerException)
			: base(message, provider, innerException)
		{
			Account = account;
		}

#if NETFRAMEWORK
		protected ExternalStorageAccessException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public string Account { get; private set; }

#if NET
		[Obsolete]
#endif
		public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue(nameof(Account), Account);
			base.GetObjectData(info, context);
		}
	}
}
