using CargoWise.Types;

namespace Enterprise.Customs.CN.Business;

public class ChildEntryCreationStrategy : EntryCreationStrategy
{
	public ChildEntryCreationStrategy(JobDeclaration declaration, ZString entryHeaderMessageTypeToNewEntryHeader)
		: base(declaration, entryHeaderMessageTypeToNewEntryHeader, true)
	{
	}

	protected override CusEntryInstruction GetEntryInstruction(JobComInvoiceLine invoiceLine)
	{
		return invoiceLine.JI_CEI.IsValid ? invoiceLine.ChildInstruction : null;
	}

	protected override bool IsActiveCore => Declaration.WillGenerateBothEntries;
}
