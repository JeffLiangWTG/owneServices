using System;
using Enterprise.DocumentEngine.ReportErrorManagement;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class DocumentEngineTooManyColumnsException : ExcelLimitationException
	{
		internal DocumentEngineTooManyColumnsException(Exception inner = null)
			: base(ExcelLimitationsHelper.LimitationType.Column, inner)
		{
		}

#if NETFRAMEWORK
		protected DocumentEngineTooManyColumnsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
