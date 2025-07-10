using System;

namespace CargoWise.BuildTools
{
	#region SourceControlException

	[Serializable]
	public class SourceControlException : Exception
	{
		public SourceControlException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public SourceControlException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected SourceControlException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	#endregion
}
