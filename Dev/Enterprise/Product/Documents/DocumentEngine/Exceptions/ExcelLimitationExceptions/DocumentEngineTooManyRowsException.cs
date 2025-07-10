using System;
using Enterprise.DocumentEngine.ReportErrorManagement;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class DocumentEngineTooManyRowsException : ExcelLimitationException
	{
		internal DocumentEngineTooManyRowsException(Exception inner = null)
			: base(ExcelLimitationsHelper.LimitationType.Row, inner)
		{
		}

#if NETFRAMEWORK
		protected DocumentEngineTooManyRowsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
