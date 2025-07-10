using System;

namespace CargoWise.EntityFramework
{
	[Serializable]
	class ObjectNotFoundInDatabaseException : Exception
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "exception message")]
		public ObjectNotFoundInDatabaseException() : base("Object not found in database") { }

#if NETFRAMEWORK
		protected ObjectNotFoundInDatabaseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
