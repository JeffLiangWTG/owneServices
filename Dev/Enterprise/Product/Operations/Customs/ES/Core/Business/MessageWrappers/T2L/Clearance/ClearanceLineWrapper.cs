using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ClearanceLineWrapper : IClearanceLine
	{
		public ClearanceLineWrapper(CusEntryLine cusEntryLine)
		{
			entryLine = Argument.NotNull(cusEntryLine, nameof(cusEntryLine));
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));

			randomLine = entryLine.RandomLine;
			previousDocuments = randomLine.GetPreviousDocumentsFromSelfOrParentsForMergeKeyOnly();
		}

		readonly CusEntryLine entryLine;
		readonly JobComInvoiceLine randomLine;
		readonly PreviousDocumentCollection previousDocuments;

		public ZInt LineNumber => entryLine.CL_LineNumber;

		public ZString SummaryDeclaration => previousDocuments != null && previousDocuments.Count > 0 ? PreviousDocumentHelper.GetSUMReferenceNumberToSend(previousDocuments[0]) : ZString.Empty;

		public ZInt LineNumberReferenced => randomLine.EntryInstruction != null && randomLine.EntryInstruction.IsT2C ? randomLine.ZG_T2LItemNumber : entryLine.CL_LineNumber;

		public ZDecimal GrossWeightInKG
		{
			get
			{
				var grossWeight = entryLine.EffectiveGrossWeight.InKilogramsSafe;
				return grossWeight > 1 ? (ZDecimal)Math.Ceiling(grossWeight) : grossWeight;
			}
		}

		public ZInt PackageQty
		{
			get
			{
				if (packageQty == null)
				{
					packageQty = new CachedProperty<ZInt>(entryLine.Factory, () =>
					{
						var vehicleAndPackageQty = ZInt.Zero;

						vehicleAndPackageQty += entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.Vehicles.Count);

						vehicleAndPackageQty += entryLine.PackagingDetails.Sum(pack => pack.CHC_NumberOfPacks);

						return vehicleAndPackageQty;
					});
				}
				return packageQty.Value;
			}
		}
		CachedProperty<ZInt> packageQty;

		public IReadOnlyCollection<ZString> Containers => containers ?? (containers = entryLine.Containers.ToList().AsReadOnly());
		IReadOnlyCollection<ZString> containers;
	}
}
