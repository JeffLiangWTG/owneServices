using System;
using System.Collections.Generic;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
namespace Enterprise.Client.EDI.ServiceTasks.EDICertProcessing
{
	[Serializable]
	public class AwsCertManagementServiceException : Exception
	{
		public AwsCertManagementServiceException(string message) : base(message)
		{
		}

		public AwsCertManagementServiceException(Exception innerException) : this(string.Empty, innerException)
		{
		}

		public AwsCertManagementServiceException(string message, Exception innerException) : base(message)
		{
			InnerExceptions.Add(innerException);
		}

#if NETFRAMEWORK
		protected AwsCertManagementServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif

		public List<Exception> InnerExceptions { get; private set; } = new List<Exception>();

		public override string ToString()
		{
			var message = base.ToString();
			foreach (var exception in InnerExceptions)
			{
				message += string.Format("{0}{1}{2}{3}", System.Environment.NewLine, "------------", System.Environment.NewLine, exception.ToString());
			}
			return message;
		}
	}
}
