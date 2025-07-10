using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class ExportEntryLineWrapper : NonPersistentBusinessObject
	{
		public ExportEntryLineWrapper(IExportEntryLine entryLine, ZDecimal uSDRate, ZBool isFirstItem, BusinessObjectFactory factory)
		{
			this.EntryLine = entryLine;
			this.uSDRate = uSDRate;
			this.isFirstItem = isFirstItem;
			PopulateGAApprovalDocuments(factory);
		}
		public IExportEntryLine EntryLine { get; }
		readonly ZDecimal uSDRate;
		readonly ZBool isFirstItem;
		public ZString GAApprovalDocumentContents1 { get; set; }
		public ZString GAApprovalDocumentContents2 { get; set; }
		public ZString GAApprovalDocumentContents3 { get; set; }
		public ZString GAApprovalDocumentContents4 { get; set; }

		public ZString FormattedHSCode => MessageFunctions.HSCodeFormat(EntryLine.HSCode);
		public ZDecimal CustomsValueUSD
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (EntryLine.CustomsValue > 0 && uSDRate > 0)
				{
					result = Utilities.Round(EntryLine.CustomsValue / uSDRate, 0);
				}
				return result;
			}
		}

		public ZString FormattedImportDeclarationNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (!EntryLine.ImportDeclarationNumber.IsEmpty)
				{
					result = MessageFunctions.DeclarationNumberFormat(EntryLine.ImportDeclarationNumber) + "(" + EntryLine.ImportEntryLineNo + ")";
				}
				return result;
			}
		}

		public ZString IsFirstEntryLineHavingOneInvoiceLine
		{
			get
			{
				if (!isFirstEntryLineHavingOneInvoiceLine.HasValue)
				{
					isFirstEntryLineHavingOneInvoiceLine = isFirstItem ? EntryLine.InvoiceLines.Count() == 1 ? "Y" : "N" : string.Empty;
				}
				return isFirstEntryLineHavingOneInvoiceLine.Value;
			}
		}
		ZString? isFirstEntryLineHavingOneInvoiceLine;

		void PopulateGAApprovalDocuments(BusinessObjectFactory factory)
		{
			var docsCount = 0;
			var documentDictionary = new Dictionary<string, string>();
			foreach (var invoiceLine in EntryLine.InvoiceLines)
			{
				foreach (var document in invoiceLine.GAApprovalDocuments)
				{
					if (!documentDictionary.ContainsKey(document.RegulationCategoryCode + document.RequirementApprovalNumber))
					{
						var result = document.RequirementApprovalNumber + "-" + document.RequirementDocumentType + "-" + factory.GetCachedValue<RequirementTypeCodeList>().GetDescriptionFromCode(document.RequirementType) + "\r\n";
						result += document.DocumentName;
						documentDictionary.Add(document.RegulationCategoryCode + document.RequirementApprovalNumber, result);

						docsCount++;
						switch (docsCount)
						{
							case 1:
								this.GAApprovalDocumentContents1 = result;
								break;
							case 2:
								this.GAApprovalDocumentContents2 = result;
								break;
							case 3:
								this.GAApprovalDocumentContents3 = result;
								break;
							case 4:
								this.GAApprovalDocumentContents4 = result;
								break;
						}

						if (docsCount == 4)
						{
							break;
						}
					}
				}
			}
		}

		public ExportInvoiceLineCollectionWrapper InvoiceLineItems
		{
			get
			{
				if (invoiceLineItems == null)
				{
					invoiceLineItems = new ExportInvoiceLineCollectionWrapper(EntryLine.InvoiceLines, base.Factory);
				}
				return invoiceLineItems;
			}
		}
		ExportInvoiceLineCollectionWrapper invoiceLineItems;
	}
}
