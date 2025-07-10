using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class Import5SGHeaderCreator
	{
		public Import5SGHeader Create(FinalPriceReportByDateExtensionHeader finalPriceReport)
		{
			var import5SGData = new Import5SGHeader();
			import5SGData.DeclarationCustomsOffice = finalPriceReport.CustomsOffice;
			import5SGData.UnipassDeclarantID = finalPriceReport.Branch != null ? KRCustomsRegistry.Instance.UNIPASSDeclarantID.GetValueWithFallbackDefault(finalPriceReport.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty).ToString() : String.Empty;
			import5SGData.ApplicationNumber = EDIMessage.EntryNumberPlaceHolder;
			import5SGData.SequenceNo = 1;
			PopulateEntry(finalPriceReport, import5SGData);

			return import5SGData;
		}

		void PopulateEntry(FinalPriceReportByDateExtensionHeader finalPriceReport, Import5SGHeader import5SGData)
		{
			var finalPriceReportLines = finalPriceReport.FinalPriceReportByDateExtensionLines;
			var entryList = new List<Import5SGEntry>();

			foreach (FinalPriceReportByDateExtensionLine line in finalPriceReportLines)
			{
				var entry = new Import5SGEntry()
				{
					ImportDeclarationNumber = line.ImportDeclarationNumber,
					ApplicationReason = line.ApplicationReason
				};

				if (!line.ExtensionDate.IsEmpty)
				{
					entry.ExtensionDate = line.ExtensionDate.ToDateTime();
				}

				entryList.Add(entry);
			}
			import5SGData.Declarations = entryList.Count > 0 ? entryList.ToArray() : null;
		}
	}
}
