using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Messaging
{
	public interface ICCSEntryResponseTypeX : ICAEDIFACTMessageAttachee
	{
		ZString BatchNumber { get; }
		ZString AccountSecurityNumber { get; }
		ZInt TotalNumberOfEntries { get; }
		ZInt TotalNumberOfValidEntries { get; }
		ZInt TotalNumberOfInvalidEntries { get; }

		IEnumerable<IResponseCodes> ResponseCodes { get; }
	}

	public interface IResponseCodes
	{
		ZString MessageItemNumber { get; }
		ZString ApplicableReferenceNumber { get; }
		ZString CSSErrorMessageNumber { get; }
	}
}
