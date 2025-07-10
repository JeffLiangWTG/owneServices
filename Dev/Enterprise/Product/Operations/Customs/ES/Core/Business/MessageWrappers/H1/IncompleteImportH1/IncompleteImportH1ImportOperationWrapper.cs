using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class IncompleteImportH1ImportOperationWrapper : ImportH1CommonImportOperationWrapper, IIncompleteImportH1ImportOperation
{
	public IncompleteImportH1ImportOperationWrapper(CusEntryHeader entryHeader) : base(entryHeader)
	{
	}

	public ZString CustomsRegistrationNumber => entryHeader.MovementReferenceNumber;
}
