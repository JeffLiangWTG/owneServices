using System;
using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	class MetadataHasChangedException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "The string literal is safe to use in this context and does not need to be externalized.")]
		const string MessageText = "Metadata has been changed (e.g. ISU service task just ran). Retry report query";

		internal MetadataHasChangedException()
			: base(MessageText)
		{
		}

#if NETFRAMEWORK
		protected MetadataHasChangedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal MetadataHasChangedException(MetadataHasChangedExceptionJsonData data)
			: base(data.Message)
		{
		}

		#endregion

		public object GetJsonData() => new MetadataHasChangedExceptionJsonData()
		{
			Message = base.Message
		};
	}

	[SuppressMessage("Microsoft.Design", "CA1064")]
	[Serializable]
	class GhostRecordsBeingDeletedException : Exception
	{
		internal GhostRecordsBeingDeletedException()
			: base((NoResString)"The transaction was terminated because of the availability replica config/state change or because ghost records are being deleted on the primary and the secondary availability replica that might be needed by queries running under snapshot isolation. Retry the transaction")
		{
		}

#if NETFRAMEWORK
		protected GhostRecordsBeingDeletedException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
