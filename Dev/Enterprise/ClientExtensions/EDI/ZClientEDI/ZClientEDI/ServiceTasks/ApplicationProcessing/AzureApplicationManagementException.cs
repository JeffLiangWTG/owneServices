using System;
using System.Collections.Generic;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
namespace Enterprise.Client.EDI.ServiceTasks.AzureApplicationProcessing
{
	[Serializable]
	public class AzureApplicationManagementException : Exception
	{
		public AzureApplicationManagementException(string message) : base(message)
		{
		}

		public AzureApplicationManagementException(Exception innerException) : this(string.Empty, innerException)
		{
		}

		public AzureApplicationManagementException(string message, Exception innerException) : base(message)
		{
			InnerExceptions.Add(innerException);
		}

#if NETFRAMEWORK
		protected AzureApplicationManagementException(SerializationInfo info, StreamingContext context) : base(info, context) { }
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
