using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public abstract class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
	{
		public EntryCreationStrategy(JobDeclaration declaration, string messageType)
			: base(declaration, messageType)
		{
			unusedContainerEntryPivots = new List<CusContainerEntryHeaderPivot>();
		}
		protected override void AfterCreateOrGetEntryHeader(Customs.Business.CusEntryHeader baseEntryHeader, BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.AfterCreateOrGetEntryHeader(baseEntryHeader, baseInvoiceLine);

			var entryHeader = (CusEntryHeader)baseEntryHeader;
			//			var containers = baseInvoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().Where(x => x.Package?.PackingGroup?.Container != null).Select(x => x.Package.PackingGroup.Container).Distinct();
			var containers = baseInvoiceLine.ContainersPivot.Cast<CusContainerInvoiceLinePivot>().Select(x => x.Container).Distinct();

			foreach (var container in containers)
			{
				var pivot = entryHeader.PivotsToContainers.GetOrCreatePivotFor(container);
				unusedContainerEntryPivots.Remove(pivot);
			}
		}

		readonly List<CusContainerEntryHeaderPivot> unusedContainerEntryPivots;
		protected override bool IsValuationDatePartOfMergeKey => false;

		protected override void Initiate()
		{
			base.Initiate();

			foreach (CusEntryHeader entry in Declaration.CustomsEntryHeaders)
			{
				if (entry.CH_MessageType == CH_MessageTypeToNewEntryHeader)
				{
					unusedContainerEntryPivots.AddRange(entry.PivotsToContainers.Cast<CusContainerEntryHeaderPivot>());
				}
			}
		}
		protected override void DiscardUnusedObjectsCore()
		{
			base.DiscardUnusedObjectsCore();
			unusedContainerEntryPivots.ForEach(x => x.Delete());
			unusedContainerEntryPivots.Clear();
		}

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetExistingEntriesCreatedThroughThisStrategy()
		{
			return Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().Where(x => x.CH_MessageType == CH_MessageTypeToNewEntryHeader);
		}
		protected override Customs.Business.CusEntryHeader GetExistingEntryHeader(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetExistingEntryHeader(invoiceLine);

			if (!DoesMessageTypeMatch(result))
			{
				result = null;
			}
			return result;
		}

		protected override Customs.Business.CusEntryLine GetExistingEntryLine(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetExistingEntryLine(invoiceLine);
			if (!DoesMessageTypeMatch(result?.Header))
			{
				result = null;
			}

			return result;
		}

		bool DoesMessageTypeMatch(Customs.Business.CusEntryHeader entry) => (entry?.CH_MessageType ?? ZString.Empty) == CH_MessageTypeToNewEntryHeader;
		protected override void AfterCreateOrGetEntryLine(Customs.Business.CusEntryLine baseEntryLine, BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.AfterCreateOrGetEntryLine(baseEntryLine, baseInvoiceLine);

			if (IsSequenceNumberManaged)
			{
				var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
				if (invoiceLine.JI_SequenceNumber.IsEmpty)
				{
					var entryLine = (CusEntryLine)baseEntryLine;
					baseEntryLine.InvoiceLines.Load();
					invoiceLine.JI_SequenceNumber = Math.Max(baseEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Max(x => x.JI_SequenceNumber), entryLine.KR_HighestInvoiceLineSequenceNo) + (ZShort)1;
				}
			}
		}

		protected override AdditionalInvoiceLineEntryLineLink LinkInvoiceLineEntryLineAndReturnPivotIfUsed(Customs.Business.CusEntryLine entryLine, BaseJobComInvoiceLine baseInvoiceLine)
		{
			if (IsSequenceNumberManaged)
			{
				var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
				if (invoiceLine.JI_CL != entryLine.PK)
				{
					invoiceLine.JI_SequenceNumber = 0;
				}
			}
			return base.LinkInvoiceLineEntryLineAndReturnPivotIfUsed(entryLine, baseInvoiceLine);
		}

		protected override Customs.Business.CusEntryLine GetEntryLineFromSplitIfItCanBeReused(BaseJobComInvoiceLine baseInvoiceLine, List<Customs.Business.CusEntryLine> splitEntryLines)
		{
			if (!IsSequenceNumberManaged)
			{
				return null;
			}
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;
			if (!invoiceLine.JI_SequenceNumber.IsEmpty)
			{
				if (!splitEntryLines.Contains(invoiceLine.CusEntryLine))
				{
					splitEntryLines.Add(invoiceLine.CusEntryLine);
				}
				return invoiceLine.CusEntryLine;
			}
			else
			{
				foreach (var entryLine in splitEntryLines.Cast<CusEntryLine>())
				{
					if (CanEntryLineTakeOneMoreInvoiceLine(entryLine) && entryLine.KR_HighestInvoiceLineSequenceNo != MaximumInvoiceLinesToMerge)
					{
						return entryLine;
					}
				}
			}
			return null;
		}

		protected override bool EntryLineNeedsToBeSplit(Customs.Business.CusEntryLine baseEntryLine, BaseJobComInvoiceLine baseInvoiceLine)
		{
			var entryLine = (CusEntryLine)baseEntryLine;
			var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;

			return IsSequenceNumberManaged && !entryLine.PK.Equals(invoiceLine.JI_CL) && (entryLine.KR_HighestInvoiceLineSequenceNo == MaximumInvoiceLinesToMerge || !CanEntryLineTakeOneMoreInvoiceLine(entryLine));
		}

		bool CanEntryLineTakeOneMoreInvoiceLine(CusEntryLine entryLine)
		{
			entryLine.InvoiceLines.Load();
			return entryLine.InvoiceLines.Count > 0 && entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Max(x => x.JI_SequenceNumber) < MaximumInvoiceLinesToMerge;
		}

		protected virtual int MaximumInvoiceLinesToMerge => 99;
		bool IsSequenceNumberManaged => ((JobDeclaration)Declaration).IsInvoiceLineSequenceNumberUsed;
	}
}
