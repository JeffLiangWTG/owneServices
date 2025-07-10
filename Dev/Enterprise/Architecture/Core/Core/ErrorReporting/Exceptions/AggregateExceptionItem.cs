using System;
using System.Diagnostics;
using CargoWise.Common.ErrorManagement;

namespace Enterprise.ZArchitecture.Core
{
	[Serializable]
	public class AggregateExceptionItem : Exception, IWithRootCauseStackTrace
	{
		public AggregateExceptionItem(AggregateException parent, Exception innerException)
			: base(parent.Message, innerException)
		{
			rootCauseStackTrace = new StackTrace(parent);
		}

#if NETFRAMEWORK
		protected AggregateExceptionItem(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public StackTrace RootCauseStackTrace => rootCauseStackTrace;

		[NonSerialized]
		readonly StackTrace rootCauseStackTrace;
	}
}
