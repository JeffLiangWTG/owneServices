using System;
using System.Text;

namespace Enterprise.DataTransfer.Native.Common.Exceptions
{
	[Serializable]
	public class ParentReferenceMismatchException : NativeXMLUserVisibleException
	{
		public ParentReferenceMismatchException(IEntity entity, IEntity parent, object dbParentKey, string extraMessage = null)
			: base(GenerateMessage(entity, parent, dbParentKey, extraMessage))
		{
		}

#if NETFRAMEWORK
		protected ParentReferenceMismatchException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{ }
#endif

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		static string GenerateMessage(IEntity entity, IEntity parent, object dbParentKey, string extraMessage)
		{
			var sb = new StringBuilder();

			sb.AppendLine("Database parent does not match entity parent.");
			if (extraMessage != null)
			{
				sb.AppendLine(extraMessage);
			}
			sb.Append("Entity: ").Append(entity).AppendLine();
			sb.Append("Parent: ").Append(parent).AppendLine();
			sb.Append("DB Parent Key: ").Append(dbParentKey);

			return sb.ToString();
		}
	}
}
