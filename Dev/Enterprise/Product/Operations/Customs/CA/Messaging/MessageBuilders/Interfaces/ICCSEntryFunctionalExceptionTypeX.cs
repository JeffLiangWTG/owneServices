using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Messaging
{
	public interface ICCSEntryFunctionalExceptionTypeX : ICAEDIFACTMessageAttachee
	{
		ZString InboundB3TransactionNumber { get; }
		IEnumerable<IErrorFtx> ErrorFtxs { get; }
	}

	public interface IErrorFtx
	{
		ZString ErrorText1 { get; }
		ZString ErrorText2 { get; }
	}
}
