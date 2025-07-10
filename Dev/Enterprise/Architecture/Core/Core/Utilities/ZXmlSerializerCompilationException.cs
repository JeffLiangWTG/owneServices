using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Xml
{
	[ExceptionVisibility(ExceptionVisibility.User), Serializable]
	public class ZXmlSerializerCompilationException : Exception
	{
		public ZXmlSerializerCompilationException(string message)
			: base(message)
		{
		}

#if NETFRAMEWORK
		protected ZXmlSerializerCompilationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
