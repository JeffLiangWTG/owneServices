using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public abstract class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
	{
		protected EntryCreationStrategy(JobDeclaration declaration, ZString entryHeaderMessageTypeToNewEntryHeader, bool useAdditionalEntryLineLinks = false)
			: base(declaration, entryHeaderMessageTypeToNewEntryHeader)
		{
			UseAdditionalEntryLineLinks = useAdditionalEntryLineLinks;
		}
		protected readonly bool UseAdditionalEntryLineLinks;

		protected new JobDeclaration Declaration => base.Declaration as JobDeclaration;

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetExistingEntriesCreatedThroughThisStrategy()
		{
			return Declaration.ActiveEntryHeaders.Cast<Customs.Business.CusEntryHeader>().Where(entry => entry.CH_MessageType == CH_MessageTypeToNewEntryHeader);
		}

		protected override Customs.Business.CusEntryLine GetExistingEntryLine(BaseJobComInvoiceLine invoiceLine)
		{
			return UseAdditionalEntryLineLinks
				? invoiceLine.AdditionalEntryLineLinks
					.Select(link => link.EntryLine)
					.FirstOrDefault(line => line.Header != null && line.Header.CH_MessageType == CH_MessageTypeToNewEntryHeader)
				: base.GetExistingEntryLine(invoiceLine);
		}

		protected override AdditionalInvoiceLineEntryLineLink LinkInvoiceLineEntryLineAndReturnPivotIfUsed(Customs.Business.CusEntryLine entryLine, BaseJobComInvoiceLine baseInvoiceLine)
		{
			return UseAdditionalEntryLineLinks
				? baseInvoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine)
				: base.LinkInvoiceLineEntryLineAndReturnPivotIfUsed(entryLine, baseInvoiceLine);
		}

		protected override void ClearReferenceToEntryLineWhenLineIsNotValidForMerge(BaseJobComInvoiceLine invoiceLine)
		{
			if (UseAdditionalEntryLineLinks)
			{
				foreach (var entryLine in invoiceLine.AdditionalEntryLineLinks.GetEntryLineFor(CH_MessageTypeToNewEntryHeader))
				{
					invoiceLine.AdditionalEntryLineLinks.DeleteLinkIfExistsFor(entryLine);
				}
			}
			else
			{
				base.ClearReferenceToEntryLineWhenLineIsNotValidForMerge(invoiceLine);
			}
		}
	}
}
