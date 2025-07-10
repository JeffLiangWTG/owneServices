using System;
using CargoWise.Common;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	[Serializable]
	[ExceptionVisibility(ExceptionVisibility.User)]
	public class WorkflowValidationException : Exception
	{
#if NETFRAMEWORK
		public WorkflowValidationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		internal WorkflowValidationException(string validationMessage)
			: base(validationMessage)
		{
		}
	}
}
