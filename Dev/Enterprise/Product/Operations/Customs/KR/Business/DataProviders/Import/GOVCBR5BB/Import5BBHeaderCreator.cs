using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5BBHeaderCreator
	{
		public Import5BBHeader Create(CusEntryHeader entry, Import5BAHeader current5BAHeader, AmendedItemCollection amendedItems)
		{
			var result = new Import5BBHeader();
			var declaration = entry.Declaration;
			result.DeclarationCustomsOffice = declaration.JE_CustomsOffice;
			result.DeclarationCustomsDivision = declaration.JE_CustomsDivision;
			result.ImportDeclarationNumber = entry.EntryNumber;
			var instruction = entry.EntryInstruction;
			if (instruction != null)
			{
				result.TariffRateClassification = instruction.CEI_AgreedDutyRatePreferenceCode;
				result.TariffRate = instruction.CEI_AgreedDutyRate;
			}
			if (declaration.BrokerAddress != null)
			{
				result.Declarant = new Organisation(RoleType.Declarant) { CompanyName = declaration.BrokerAddress.CompanyName };
			}
			result.EntryLines = Get5BBLines(current5BAHeader, amendedItems);
			return result;
		}

		static Import5BBLine[] Get5BBLines(Import5BAHeader current5BAHeader, AmendedItemCollection amendedItems)
		{
			var result = new List<Import5BBLine>();
			var currentEntryLineNumbers = current5BAHeader.EntryLines.Select(x => x.EntryLineNo).ToArray();

			foreach (AmendedItem amendedItem in amendedItems)
			{
				var value = amendedItem.IDsInList?.FirstOrDefault(x => x.IDType == nameof(IImport5BALine))?.IDValue ?? "0";
				var lineNo = int.Parse(value);

				switch (amendedItem.AmendType)
				{
					case EntityAmendType.Add:
						if (!result.Any(x => x.EntryLineNo == lineNo))
						{
							var newLine = GetImport5BBLineToAdd(lineNo, current5BAHeader);
							if (newLine != null)
							{
								result.Add(newLine);
							}
						}
						break;

					case EntityAmendType.Delete:
						result.Add(CreateImport5BBLine(lineNo));
						break;

					case EntityAmendType.Update:
						if (amendedItem.IDsInList == null)
						{
							result.AddRange(GetImport5BBLinesForRateChanges(currentEntryLineNumbers, amendedItem));
						}
						else
						{
							result.Add(CreateImport5BBLine(lineNo, amendedItem));
						}
						break;
				}
			}
			return result.OrderBy(x => x.EntryLineNo).ToArray();
		}

		static IEnumerable<Import5BBLine> GetImport5BBLinesForRateChanges(int[] lineNumbers, AmendedItem rateAmendedItem)
		{
			foreach (var lineNo in lineNumbers)
			{
				yield return CreateImport5BBLine(lineNo, rateAmendedItem);
			}
		}
		static Import5BBLine CreateImport5BBLine(int lineNo, AmendedItem item = null)
		{
			var result = new Import5BBLine() { EntryLineNo = lineNo };
			if (item != null)
			{
				result.AmendDataItemID = item.DataItemID;
				result.BeforeDescription = item.BeforeValue;
				result.AfterDescription = item.AfterValue;
			}
			return result;
		}

		static Import5BBLine GetImport5BBLineToAdd(int lineNo, Import5BAHeader current5BAHeader)
		{
			Import5BBLine result = null;
			var line5BA = current5BAHeader.EntryLines.FirstOrDefault(x => x.EntryLineNo == lineNo);
			if (line5BA != null)
			{
				result = CreateImport5BBLine(lineNo);
				result.HSDescription = line5BA.HSDescription;
				result.InvoiceDescription = line5BA.InvoiceDescription;
				result.HSCode = line5BA.HSCode;
			}
			return result;
		}
	}
}
