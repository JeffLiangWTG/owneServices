using System;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	class TemplateInUseException : Exception, System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		internal TemplateInUseException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected TemplateInUseException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		#region Constructor For IJsonSerializable

		internal TemplateInUseException(TemplateInUseExceptionJsonData data)
			: base(data.Message)
		{
		}

		#endregion

		public object GetJsonData() => new TemplateInUseExceptionJsonData()
		{
			Message = base.Message
		};
	}
}
