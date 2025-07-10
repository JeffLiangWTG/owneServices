using System;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
namespace Enterprise.Client.EDI;

[Serializable]
class SubmissionCannotBeProcessedException : Exception
{
	public SubmissionCannotBeProcessedException()
	{
	}

	public SubmissionCannotBeProcessedException(string message, Exception innerException = null)
		: base(message, innerException)
	{
	}

#if NETFRAMEWORK
	protected SubmissionCannotBeProcessedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
#endif
}
