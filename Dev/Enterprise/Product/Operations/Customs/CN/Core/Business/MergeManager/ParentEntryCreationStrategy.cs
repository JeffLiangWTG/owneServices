using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

public class ParentEntryCreationStrategy : EntryCreationStrategy
{
	public ParentEntryCreationStrategy(JobDeclaration declaration, ZString entryHeaderMessageTypeToNewEntryHeader)
		: base(declaration, entryHeaderMessageTypeToNewEntryHeader, false)
	{
	}

	protected override CusEntryInstruction GetEntryInstruction(JobComInvoiceLine invoiceLine)
	{
		return invoiceLine.JI_CEI.IsValid ? invoiceLine.EntryInstruction : null;
	}
}
