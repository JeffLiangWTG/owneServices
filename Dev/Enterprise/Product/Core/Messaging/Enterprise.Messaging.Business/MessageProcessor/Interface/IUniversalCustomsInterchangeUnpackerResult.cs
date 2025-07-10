using System.Collections.Generic;

namespace Enterprise.Messaging.Business.MessageProcessor
{
	public interface IUniversalCustomsInterchangeUnpackerResult
	{
		ICollection<EDIMessage> EdiMessages { get; }
		string ErrorReason { get; }
		bool IsSuccess { get; }
	}
}
