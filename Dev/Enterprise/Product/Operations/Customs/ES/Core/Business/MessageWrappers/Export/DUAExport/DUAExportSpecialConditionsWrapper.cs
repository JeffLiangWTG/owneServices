using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DUAExportSpecialConditionsWrapper : IDUAExportSpecialConditions
	{
		public DUAExportSpecialConditionsWrapper(CusEntryLine cusEntryLine)
		{
			entryLine = Argument.NotNull(cusEntryLine, "CusEntryLine cannot be null");
			Argument.GreaterThan(entryLine.InvoiceLines.Count, 0, nameof(entryLine.InvoiceLines));
		}

		readonly CusEntryLine entryLine;

		public ZString Code1 => SpecialConditionsCodes.ElementAtOrDefault(0)?.Item1 ?? ZString.Empty;

		public ZString Code2 => SpecialConditionsCodes.ElementAtOrDefault(1)?.Item1 ?? ZString.Empty;

		public ZString Code3 => SpecialConditionsCodes.ElementAtOrDefault(2)?.Item1 ?? ZString.Empty;

		public ZString Code4 => SpecialConditionsCodes.ElementAtOrDefault(3)?.Item1 ?? ZString.Empty;

		public ZString Text => string.Join(" ", SpecialConditionsCodes.Select(t => t.Item2));

		List<Tuple<ZString, ZString>> SpecialConditionsCodes
		{
			get
			{
				if (conditionsCodesAndText == null)
				{
					conditionsCodesAndText = new List<Tuple<ZString, ZString>>();

					if (entryLine.Declaration.AdditionalInfos.Any())
					{
						foreach (EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo supDoc in entryLine.Declaration.AdditionalInfos)
						{
							conditionsCodesAndText.Add(Tuple.Create(supDoc.CSI_Code, supDoc.CSI_Description));
						}
					}
					if (entryLine.Header.RandomHeader.AdditionalInfos.Any())
					{
						foreach (EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo supDoc in entryLine.Header.RandomHeader.AdditionalInfos)
						{
							conditionsCodesAndText.Add(Tuple.Create(supDoc.CSI_Code, supDoc.CSI_Description));
						}
					}
					if (entryLine.AdditionalInfos.Any())
					{
						foreach (EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo supDoc in entryLine.AdditionalInfos)
						{
							conditionsCodesAndText.Add(Tuple.Create(supDoc.CSI_Code, supDoc.CSI_Description));
						}
					}

					if (conditionsCodesAndText.Count > 4)
					{
						conditionsCodesAndText = conditionsCodesAndText.Take(4).ToList();
					}
				}

				return conditionsCodesAndText;
			}
		}
		List<Tuple<ZString, ZString>> conditionsCodesAndText;
	}
}
