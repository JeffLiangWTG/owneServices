using System;

namespace CargoWise.Bi.Maintenance
{
	[Serializable]
	public class BiMaintenanceException : Exception
	{
		public BiMaintenanceException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected BiMaintenanceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
