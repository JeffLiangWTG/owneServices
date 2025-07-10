using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Documents.DocDataObjects
{
	public class EntryHeaderDataObject : EU.Business.Documents.DocDataObjects.EntryHeaderDataObject
	{
		public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;
		public EntryHeaderDataObject(CusEntryHeader entry) : base(entry)
		{
			incoTerms = EntryHeader.InvoiceHeaders.Length == 1 ? EntryHeader.InvoiceHeaders.First().JZ_IncoTerm + entry.Declaration.ZG_AgreedPlaceCode + " " + EntryHeader.InvoiceHeaders.First().JZ_IncoTermPlace : string.Empty;
			invoiceNumber = EntryHeader.InvoiceHeaders.Length == 1 ? EntryHeader.InvoiceHeaders.First().JZ_InvoiceNumber + (NoResString)" del " + EntryHeader.InvoiceHeaders.First().JZ_InvoiceDate.ToCustomsFormatDateStringddMMyyyyWithDash() : string.Empty;
		}

		protected override ZString MRNTextCore
		{
			get
			{
				var stringEntryNumber = EntryHeader.EntryNumber.Length == 18 ? EntryHeader.EntryNumber.InsertSafe(17, " ").InsertSafe(11, " ").InsertSafe(10, " ").InsertSafe(2, " ") : EntryHeader.EntryNumber;
				var stringMRN = EntryHeader.MovementReferenceNumber.Length == 18 ? EntryHeader.MovementReferenceNumber.InsertSafe(17, " ").InsertSafe(11, " ").InsertSafe(10, " ").InsertSafe(2, " ") : EntryHeader.MovementReferenceNumber;
				return !stringEntryNumber.IsEmpty ? stringEntryNumber : stringMRN;
			}
		}

		protected override List<EU.Business.Documents.DocDataObjects.EntryLineGroup> EntryLineGroupsCore
		{
			get
			{
				var entryLineGroup = new List<EU.Business.Documents.DocDataObjects.EntryLineGroup>();
				var entryLines = EntryHeader.AllEntryLines;
				for (int i = 0; i < NumberOfCalculationSheets; i++)
				{
					var tempEntryLines = entryLines.Skip(i * 3).Take(3);

					if (tempEntryLines.Any())
					{
						entryLineGroup.Add(new EntryLineGroup(tempEntryLines.Cast<CusEntryLine>(), i + 1));
					}
				}

				return entryLineGroup;
			}
		}

		protected override EU.Business.Documents.DocDataObjects.EntryLineDataObject GroupedEntryLineDataObjCore
		{
			get
			{
				var entryLines = EntryHeader.AllEntryLines;
				return entryLines.Any() ? new GroupedEntryLineDataObject(null, entryLines.Cast<CusEntryLine>()) : null;
			}
		}
	}
}
