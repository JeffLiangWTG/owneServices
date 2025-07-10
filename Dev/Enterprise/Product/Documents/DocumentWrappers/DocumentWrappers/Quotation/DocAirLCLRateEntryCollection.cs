using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocAirLCLRateEntryCollection : DocRateEntryCollection
	{
		public DocAirLCLRateEntryCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public DocAirLCLRateEntryCollection(DocTableQuotation tableQuotation, IEnumerable<RateEntry> entryCollection, BusinessObjectFactory factory)
			: base(factory)
		{
			Parent = tableQuotation;
			List<DocRateEntry> masterEntries = new List<DocRateEntry>();

			foreach (RateEntry entry in DocRateEntry.UniqueFreightEntriesIgnoringMode(entryCollection))
			{
				if (Parent != null && entry.HasVisibleLines(Parent.Page))
				{
					DocRateEntry newEntry = DocRateEntry.New(entry, Factory);
					Add(newEntry);
					masterEntries.Add(newEntry);
				}
			}

			Sort(new Quotation.EntryComparer());

			if (Parent != null && Parent.HasAirLCLMarkup)
			{
				SplitIntoMatrix(masterEntries);
			}
		}

		void SplitIntoMatrix(List<DocRateEntry> masterEntriesKeys)
		{
			int groupIndex = 0;

			foreach (DocRateEntry rateEntry in masterEntriesKeys)
			{
				groupIndex = GroupAndSplitMatrixCompatibleLines(groupIndex, rateEntry);

				if (rateEntry.MatrixCompatibleRateLines.Count > 0)
				{
					DocRateEntry entryToShowMatrixIncompatibleRateLines = DocRateEntry.New(rateEntry.Entry, Factory);
					Add(entryToShowMatrixIncompatibleRateLines);
					entryToShowMatrixIncompatibleRateLines.LCLHeaderGroupIndex = groupIndex - 1;
					entryToShowMatrixIncompatibleRateLines.AirLCLType = DocRateEntry.AirLCLDocRateEntryTypes.OtherChargesLine;
				}
				else
				{
					rateEntry.LCLHeaderGroupIndex = groupIndex - 1;
					rateEntry.AirLCLType = DocRateEntry.AirLCLDocRateEntryTypes.ComboLine;
				}
			}
		}

		int GroupAndSplitMatrixCompatibleLines(int groupIndex, DocRateEntry masterEntry)
		{
			List<RateLine> linesToProcess = masterEntry.MatrixCompatibleRateLines;

			do
			{
				RateLine headerLine;
				List<RateLine> headerCompatibleLines;

				using (IEnumerator<RateLine> enumerator = linesToProcess.GetEnumerator())
				{
					if (!enumerator.MoveNext())
					{
						break;
					}

					headerLine = enumerator.Current;
					headerCompatibleLines = GetCompatibleLines(headerLine, linesToProcess);
					int headerWeight = GetVisibleRateLineItemsCount(headerLine) * headerCompatibleLines.Count;

					while (enumerator.MoveNext())
					{
						RateLine candidateLine = enumerator.Current;

						if (candidateLine == headerLine)
						{
							continue;
						}

						List<RateLine> candidateCompatibleLines = GetCompatibleLines(candidateLine, linesToProcess);
						int candidateWeight = GetVisibleRateLineItemsCount(candidateLine) * candidateCompatibleLines.Count;

						if (headerWeight < candidateWeight)
						{
							headerLine = candidateLine;
							headerCompatibleLines = candidateCompatibleLines;
							headerWeight = candidateWeight;
						}
					}
				}

				foreach (RateLine line in headerCompatibleLines)
				{
					PlaceRateLineToDocRateEntryMatrix(masterEntry, line, groupIndex, headerLine);
				}

				linesToProcess = linesToProcess.Where(line => !headerCompatibleLines.Contains(line)).ToList();
				groupIndex++;
			}
			while (linesToProcess.Count > 0);

			return groupIndex;
		}

		void PlaceRateLineToDocRateEntryMatrix(DocRateEntry masterEntry, RateLine lineToMatrix, int groupIndex, RateLine headerLine)
		{
			DocRateEntry rateEntry;

			if (masterEntry.MatrixedRateLine == null)
			{
				rateEntry = masterEntry;
			}
			else
			{
				rateEntry = DocRateEntry.New(masterEntry.Entry, Factory);
				Add(rateEntry);
			}

			rateEntry.MatrixedRateLine = lineToMatrix;
			rateEntry.LCLHeaderGroupIndex = groupIndex;
			rateEntry.LCLHeaderGroupRateLine = headerLine;
			rateEntry.AirLCLType = DocRateEntry.AirLCLDocRateEntryTypes.WeightBreaksMatrixLine;
		}

		static List<RateLine> GetCompatibleLines(RateLine sampleRateLine, List<RateLine> lines)
		{
			List<RateLine> result = new List<RateLine>();

			if (sampleRateLine != null && lines != null && lines.Count > 0)
			{
				foreach (RateLine rateLine in lines)
				{
					if (HeadersAreOneWayCompatible(sampleRateLine, rateLine))
					{
						result.Add(rateLine);
					}
				}
			}

			return result;
		}

		static bool HeadersAreOneWayCompatible(RateLine headerLine, RateLine line2)
		{
			List<RateLineItem> headerItems = GetSupportedRateLineItems(headerLine.RateLineItems);
			List<RateLineItem> line2Items = GetSupportedRateLineItems(line2.RateLineItems);

			if (headerItems.Count < line2Items.Count)
			{
				return false;
			}

			for (int i = 0; i < line2Items.Count; i++)
			{
				bool hasCompatibleHeaderItem = false;

				foreach (RateLineItem headerItem in headerItems)
				{
					if (RateLineItemsCorrespondOneWay(headerItem, line2Items[i]))
					{
						hasCompatibleHeaderItem = true;
						break;
					}
				}

				if (!hasCompatibleHeaderItem)
				{
					return false;
				}
			}

			return true;
		}

		static bool IsSupportedOperator(string type)
		{
			switch (type)
			{
				case Calculator.Items.Operator.Minus:
				case Calculator.Items.Operator.Plus:
				case Calculator.Items.Operator.UNT:
				case Calculator.Items.Operator.BAS:
				case Calculator.Items.Operator.MIN:
				case Calculator.Items.Operator.MAX:
					return true;

				default:
					return false;
			}
		}

		static bool IsUnitOperator(string type)
		{
			switch (type)
			{
				case Calculator.Items.Operator.Minus:
				case Calculator.Items.Operator.Plus:
				case Calculator.Items.Operator.UNT:
					return true;

				default:
					return false;
			}
		}

		static List<RateLineItem> GetSupportedRateLineItems(RateLineItemsCollection items)
		{
			List<RateLineItem> result = new List<RateLineItem>();

			foreach (RateLineItem item in items)
			{
				if (IsSupportedOperator(item.TM_Type))
				{
					result.Add(item);
				}
			}

			return result;
		}

		static int GetVisibleRateLineItemsCount(RateLine line)
		{
			int count = 0;

			foreach (RateLineItem item in line.RateLineItems)
			{
				if (IsSupportedOperator(item.TM_Type))
				{
					count++;
				}
			}

			return count;
		}

		internal static bool RateLineItemsCorrespondOneWay(RateLineItem headerItem, RateLineItem item2)
		{
			if (headerItem != null && item2 != null)
			{
				bool weightVlomeMultipleIsCompatible = true;

				if (IsUnitOperator(headerItem.TM_Type) && IsUnitOperator(item2.TM_Type))
				{
					string weightVolume1 = headerItem.TM_BreakWeightVolume.IsEmpty ? headerItem.Parent.TL_WeightVolume : headerItem.TM_BreakWeightVolume;
					string weightVolume2 = item2.TM_BreakWeightVolume.IsEmpty ? item2.Parent.TL_WeightVolume : item2.TM_BreakWeightVolume;

					weightVlomeMultipleIsCompatible = weightVolume1 == weightVolume2 && headerItem.Parent.TL_WeightVolumeMultiple == item2.Parent.TL_WeightVolumeMultiple;
				}

				return weightVlomeMultipleIsCompatible
					&&
					(
						(
							headerItem.TM_Type == item2.TM_Type
							&&
							headerItem.TM_Break == item2.TM_Break
						)
						||
						(
							item2.RateOperatorIsUNT()
							&&
							(
								headerItem.RateOperatorIsPlus()
								||
								headerItem.RateOperatorIsMinus()
							)
						)
					);
			}

			return false;
		}
	}
}
