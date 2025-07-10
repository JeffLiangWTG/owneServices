using System;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.Exceptions
{
	[Serializable]
	public class ExcelLimitationException : ExcelLimitationBaseException
	{
		internal ExcelLimitationException(ExcelLimitationsHelper.LimitationType limitationType, Exception inner = null)
			: base(limitationType, (NoResString)"Too many rows/columns in resulting report.", inner)
		{
		}

#if NETFRAMEWORK
		protected ExcelLimitationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
