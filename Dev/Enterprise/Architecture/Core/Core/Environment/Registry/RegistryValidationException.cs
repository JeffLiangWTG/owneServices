using System;

namespace Enterprise.ZArchitecture.Environment
{
	/// <summary>
	/// The reason for throwing exception rather than setting an error message property for validation error
	/// is so that if a developer sets an invalid value on a DataRegistry, an exception will be thrown.
	/// </summary>
	[Serializable]
	public class RegistryValidationException : OdysseyException
	{
		public RegistryValidationException(string message)
			: base(message)
		{
		}

		public RegistryValidationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected RegistryValidationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class RegistryParsingException : OdysseyException
	{
		public RegistryParsingException(string message)
			: base(message)
		{
		}

		public RegistryParsingException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

#if NETFRAMEWORK
		protected RegistryParsingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public class RegistryDuplicateException : OdysseyException
	{
		public RegistryDuplicateException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected RegistryDuplicateException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
