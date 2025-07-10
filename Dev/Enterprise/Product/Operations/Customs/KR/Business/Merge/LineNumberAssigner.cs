using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class LineNumberAssigner : Customs.Business.LineNumberAssigner
	{
		public LineNumberAssigner(CusEntryHeader entry) : base(entry)
		{ }

		new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;
		const string Separator = "|";

		protected override void ExecuteCore()
		{
			base.ExecuteCore();
			if (EntryHeader.Declaration != null)
			{
				var transportMeansNoToStartWith = EntryHeader.CH_HighestTransportMeansNo;
				foreach (TransportMeans transportMean in EntryHeader.Declaration.TransportMeans)
				{
					if (transportMean.CY_Order.IsEmpty || transportMean.CY_Order > EntryHeader.CH_HighestTransportMeansNo)
					{
						transportMeansNoToStartWith++;
						transportMean.CY_Order = transportMeansNoToStartWith;
					}
				}
			}

			foreach (CusEntryLine entryLine in EntryHeader.MergedLines)
			{
				var invoiceLines = entryLine.InvoiceLines.Cast<JobComInvoiceLine>();

				var invLineImmediateDeliveries = invoiceLines.SelectMany(x => x.ImmediateDeliveries.Cast<ImmediateDelivery>());
				SetCollectionOrder(entryLine.ImmediateDeliveries, invLineImmediateDeliveries, entryLine.KR_HighestImmediateDeliveryNo, x => x.CY_OrderInfo, x => x.CY_Data);

				var invoiceLineNonGADetails = invoiceLines.SelectMany(x => x.NonGADetailCollection.Cast<NonGADetail>());
				SetCollectionOrder(entryLine.NonGADetailCollection, invoiceLineNonGADetails, entryLine.KR_HighestNonGADetailNo, x => x.CSI_LineNoInfo, x => string.Join(Separator, x.KeyFields));

				var invoiceLinePreviousExpDecLines = invoiceLines.SelectMany(x => x.PreviousExpDecLineCollection.Cast<PreviousExpDecLine>());
				SetCollectionOrder(entryLine.PreviousExpDecLineCollection, invoiceLinePreviousExpDecLines, entryLine.KR_HighestPreviousExpDecLineNo, x => x.CSI_LineNoInfo, x => string.Join(Separator, x.KeyFields));

				foreach (JobComInvoiceLine invoiceLine in entryLine.InvoiceLines)
				{
					var highestVehicleSeqNoToStartWith = invoiceLine.KR_HighestVehicleSeqNo;
					var highestHighestGAApprovalSeqNoToStartWith = invoiceLine.KR_HighestGAApprovalSeqNo;
					foreach (VehicleNumber vehicleNumber in invoiceLine.VehicleNumbers)
					{
						if (vehicleNumber.CY_Order.IsEmpty
						|| vehicleNumber.CY_Order > invoiceLine.KR_HighestVehicleSeqNo)
						{
							highestVehicleSeqNoToStartWith++;
							vehicleNumber.CY_Order = highestVehicleSeqNoToStartWith;
						}
					}
					foreach (GAApproval gaApproval in invoiceLine.GAApprovalDataCollection)
					{
						if (gaApproval.CSI_LineNo.IsEmpty
						|| gaApproval.CSI_LineNo > invoiceLine.KR_HighestGAApprovalSeqNo)
						{
							highestHighestGAApprovalSeqNoToStartWith++;
							gaApproval.CSI_LineNo = highestHighestGAApprovalSeqNoToStartWith;
						}
					}
				}

				if (!entryLine.RandomLine.IsFTAPreference)
				{
					entryLine.CL_FTASequenceNumber = 0;
				}
				else if (entryLine.CL_FTASequenceNumber.IsEmpty)
				{
					entryLine.CL_FTASequenceNumber = Math.Max(EntryHeader.MergedLines.MaxOrDefault(x => x.CL_FTASequenceNumber), EntryHeader.CH_HighestFTASequenceNumber) + (ZShort)1;
				}
			}

			var orderedPivots = EntryHeader.PivotsToContainers.Cast<CusContainerEntryHeaderPivot>().Where(x => x.Container != null).OrderBy(x => x.Container.CO_ContainerNumber);
			var nextSequenceNo = EntryHeader.CH_HighestContainerNumber;

			foreach (var pivot in orderedPivots)
			{
				if (pivot.CCE_SequenceNumber.IsEmpty || pivot.CCE_SequenceNumber > EntryHeader.CH_HighestContainerNumber)
				{
					nextSequenceNo++;
					pivot.CCE_SequenceNumber = nextSequenceNo;
				}
			}
		}

		void SetCollectionOrder<T>(IEnumerable<T> collectionFromEntryLine, IEnumerable<T> collectionFromInvoiceLines, short startsWith, Func<T, ZPropertyInfo> getLineNoInfo, Func<T, string> getKeys) where T : BusinessObject
		{
			var lineNumbersByKey = new Dictionary<string, INumericZType>();
			var keyList = collectionFromInvoiceLines.Select(getKeys).ToHashSet();
			foreach (T item in collectionFromEntryLine)
			{
				var key = getKeys(item);
				if (keyList.Contains(key))
				{
					var lineNumberInfo = getLineNoInfo(item);
					switch (lineNumberInfo.Value)
					{
						case ZShort s:
							if (s.IsEmpty || s > (ZShort)startsWith)
							{
								startsWith++;
								lineNumberInfo.Value = (ZShort)startsWith;
							}
							break;
						case ZInt i:
							if (i.IsEmpty || i > (ZInt)startsWith)
							{
								startsWith++;
								lineNumberInfo.Value = (ZInt)startsWith;
							}
							break;
						default:
							throw new ArgumentException(typeof(T).ToString());
					}

					lineNumbersByKey.Add(key, (INumericZType)lineNumberInfo.Value);
				}
			}

			foreach (T item in collectionFromInvoiceLines)
			{
				var key = getKeys(item);
				getLineNoInfo(item).Value = lineNumbersByKey[key];
			}
		}
	}
}
