using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportAmendEntryHeader : ILocalExportAmendEntryHeader
	{
		public string DeclarationNumber { get; set; }
		public string CustomsReceiptNumber { get; set; }
		public string DeclarationCustomsOfficeAndDivision { get; set; }
		public DateTime LoadingDate { get; set; }
		public Organisation Supplier { get; set; }
		public LocalExportAmendItem[] AmendedItems { get; set; }
		public LocalExportStevedore[] Stevedores { get; set; }

		ZString ILocalExportAmendEntryHeader.DeclarationNumber => DeclarationNumber;
		ZString ILocalExportAmendEntryHeader.CustomsReceiptNumber => CustomsReceiptNumber;
		ZString ILocalExportAmendEntryHeader.DeclarationCustomsOfficeAndDivision => DeclarationCustomsOfficeAndDivision;
		ZDateTime ILocalExportAmendEntryHeader.LoadingDate => new ZDateTime(LoadingDate);
		IOrganization ILocalExportAmendEntryHeader.Supplier => Supplier;
		IEnumerable<ILocalExportAmendItem> ILocalExportAmendEntryHeader.AmendedItems => AmendedItems;
		IEnumerable<ILocalExportStevedore> ILocalExportAmendEntryHeader.Stevedores => Stevedores;
	}
}
