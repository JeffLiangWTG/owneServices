using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration) : base(declaration)
		{
		}
		new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
		protected override Customs.Business.EntryCreationStrategy[] GetEntryCreationStrategies()
		{
			var result = new List<Customs.Business.EntryCreationStrategy>();
			result.Add(new ExportEntryCreationStrategy(Declaration));
			result.Add(new ImportEntryCreationStrategy(Declaration));
			result.Add(new LocalExportEntryCreationStrategy(Declaration, ElectronicDocumentTypeList.Codes._5DP));
			result.Add(new LocalExportEntryCreationStrategy(Declaration, ElectronicDocumentTypeList.Codes._5DQ));
			result.Add(new PIDEntryCreationStrategy(Declaration));
			result.Add(new D87EntryCreationStrategy(Declaration));
			result.Add(new ValuationDeclarationEntryCreationStrategy(Declaration));

			return result.ToArray();
		}

		protected override void OnMerging()
		{
			base.OnMerging();
			if (Declaration.IsInvoiceLineSequenceNumberUsed)
			{
				foreach (var invoiceLine in Declaration.InvoiceLines.Cast<JobComInvoiceLine>())
				{
					if (invoiceLine.CusEntryLine == null || invoiceLine.JI_SequenceNumber > invoiceLine.CusEntryLine.KR_HighestInvoiceLineSequenceNo)
					{
						invoiceLine.JI_SequenceNumber = 0;
					}
				}
			}
			if (Declaration.IsImport)
			{
				foreach (CusEntryHeader entry in Declaration.ActiveEntryHeaders)
				{
					foreach (var entryLine in entry.MergedLines)
					{
						if (entryLine.CL_FTASequenceNumber > entry.CH_HighestFTASequenceNumber)
						{
							entryLine.CL_FTASequenceNumber = 0;
						}
						foreach (var p in entryLine.PreviousExpDecLineCollection)
						{
							p.CSI_Quantity = 0m;
							p.CSI_UnitOfQuantity = ZString.Empty;
						}
					}
				}
			}
		}

		protected override void OnMerged()
		{
			base.OnMerged();
			foreach (var entry in Declaration.CustomsEntryHeaders)
			{
				if (entry.IsImport && entry.IsActive)
				{
					foreach (var entryLine in entry.MergedLines)
					{
						var invoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>();

						var invoiceLineImmediateDeliveries = invoiceLines.SelectMany(x => x.ImmediateDeliveries.Cast<ImmediateDelivery>());
						var immediateDeliveryNumbers = invoiceLineImmediateDeliveries.Select(x => x.CY_Data).Distinct();
						var immediateDeliveriesToBeRemoved = entryLine.ImmediateDeliveries.Where(x => !immediateDeliveryNumbers.Contains(x.CY_Data)).ToList();

						foreach (var delivery in immediateDeliveriesToBeRemoved)
						{
							entryLine.ImmediateDeliveries.RemoveAndDelete(delivery);
						}

						var nonGADetails = invoiceLines.SelectMany(x => x.NonGADetailCollection.Cast<NonGADetail>()).Distinct().ToList();
						var nonGADetailsToBeRemoved = entryLine.NonGADetailCollection.Cast<NonGADetail>().Where(x => !nonGADetails.Any(y => y.HasSameKey(x))).ToList();
						foreach (var nonGADetail in nonGADetailsToBeRemoved)
						{
							entryLine.NonGADetailCollection.RemoveAndDelete(nonGADetail);
						}

						var previousExpDecLines = invoiceLines.SelectMany(x => x.PreviousExpDecLineCollection.Cast<PreviousExpDecLine>()).Distinct().ToList();
						var previousExpDecLinesToBeRemoved = entryLine.PreviousExpDecLineCollection.Where(x => !previousExpDecLines.Any(y => y.HasSameKey(x))).ToList();
						foreach (var previousExpDecLine in previousExpDecLinesToBeRemoved)
						{
							entryLine.PreviousExpDecLineCollection.RemoveAndDelete(previousExpDecLine);
						}
					}
				}
			}
		}

		protected override ILineNumberAssigner GetLineNumberAssigner(Customs.Business.CusEntryHeader entryHeader) => new LineNumberAssigner((CusEntryHeader)entryHeader);

		protected override IDutyCalculatorStrategy GetNewDutyCalculatorStrategy() => new DutyCalculatorStrategy(Declaration);
	}
}
