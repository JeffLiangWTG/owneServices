using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[Serializable]
	class ZFormBashingExceptionWhereCallStackIsMeaningless : ApplicationException
	{
		public ZFormBashingExceptionWhereCallStackIsMeaningless(string title, string notificationMessage)
			: base("   >>> <B>" + title + "</B> <<<\r\n" + notificationMessage)
		{
		}

#if NETFRAMEWORK
		protected ZFormBashingExceptionWhereCallStackIsMeaningless(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
