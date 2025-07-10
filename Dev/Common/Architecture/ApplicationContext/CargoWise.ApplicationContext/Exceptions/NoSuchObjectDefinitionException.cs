using System;
using CargoWise.Common;

namespace CargoWise.Application.Exceptions
{
	/// <summary>
	/// Represents an exception thrown when a specific object definition does not exist.
	/// </summary>
	[Serializable]
	public class NoSuchObjectDefinitionException : ObjectFactoryException
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose message")]
		static string GenerateMessageForName(string name)
		{
			return string.Format("The object definition named \"{0}\" does not exist.", name);
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose message")]
		static string GenerateMessageForType(Type type)
		{
			Argument.NotNull(type, nameof(type));
			return string.Format("The object definition for type \"{0}\" does not exist.", type.FullName);
		}
		public NoSuchObjectDefinitionException()
		{
		}
		public NoSuchObjectDefinitionException(string name) : base(GenerateMessageForName(name))
		{
		}
		public NoSuchObjectDefinitionException(string name, Exception rootCause) : base(GenerateMessageForName(name), rootCause)
		{
		}
		public NoSuchObjectDefinitionException(Type type) : base(GenerateMessageForType(type))
		{
			Argument.NotNull(type, nameof(type));
		}

#if NETFRAMEWORK
		protected NoSuchObjectDefinitionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
