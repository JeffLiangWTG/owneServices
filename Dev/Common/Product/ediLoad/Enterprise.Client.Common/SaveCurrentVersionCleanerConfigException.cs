using System;
using CargoWise.ApplicationManager.Common;
using static System.FormattableString;

namespace Enterprise.Client.Common
{
	[Serializable]
	public class SaveCurrentVersionCleanerConfigException : Exception
	{
		public SaveCurrentVersionCleanerConfigException()
			: this("Save CurrentVersionConfig file exception.")
		{
		}

		public SaveCurrentVersionCleanerConfigException(string baseInstallationPath, AppManagerResult result)
			: this(Invariant($"Save CurrentVersionConfig file exception at path: '{baseInstallationPath}', result: {result}."))
		{
		}

		public SaveCurrentVersionCleanerConfigException(string message)
			: base(message)
		{
		}

		public SaveCurrentVersionCleanerConfigException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected SaveCurrentVersionCleanerConfigException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
