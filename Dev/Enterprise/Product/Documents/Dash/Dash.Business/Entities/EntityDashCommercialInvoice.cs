using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;

namespace Enterprise.Dash.Business.Entities
{
	public sealed class EntityDashCommercialInvoice : IDashCommercialInvoice
	{
		public Guid DCI_PK { get; set; }

		public Guid DCI_DDD_DashDocID { get; set; }
		public decimal DCI_GrossTotal { get; set; }
		public string DCI_ImporterParsedRawText { get; set; }
		public string DCI_ImporterParsedAddressRawText { get; set; }
		public string DCI_ImporterParsedNameRawText { get; set; }
		public string DCI_Incoterm { get; set; }
		public DateTime? DCI_InvoiceDate { get; set; }
		public string DCI_InvoiceNumber { get; set; }
		public Guid? DCI_OA_MatchedImporterAddressID { get; set; }
		public Guid? DCI_OA_MatchedSupplierAddressID { get; set; }
		public Guid? DCI_OH_MatchedImporterID { get; set; }
		public Guid? DCI_OH_MatchedSupplierID { get; set; }
		public string DCI_RX_NKInvoiceCurrency { get; set; }
		public string DCI_SupplierParsedRawText { get; set; }
		public string DCI_SupplierParsedAddressRawText { get; set; }
		public string DCI_SupplierParsedNameRawText { get; set; }
		public DateTime DCI_SystemCreateTimeUtc { get; set; }
		public string DCI_SystemCreateUser { get; set; }
		public DateTime DCI_SystemLastEditTimeUtc { get; set; }
		public string DCI_SystemLastEditUser { get; set; }

		public IOrgAddressInfo MatchedImporterAddressID { get; set; }
		public IOrgAddressInfo MatchedSupplierAddressID { get; set; }
		public IOrgHeaderInfo MatchedImporterID { get; set; }
		public IOrgHeaderInfo MatchedSupplierID { get; set; }

		public ICollection<IDashCommercialInvoiceLineItem> DashCommercialInvoiceLineItems { get; set; } = new List<IDashCommercialInvoiceLineItem>();

		#region Not Used Properties

		public IRefCurrencyInfo InvoiceCurrency { get; set; }
		public IGlbStaffInfo CreatedByStaff { get; set; }
		public IGlbStaffInfo LastEditedByStaff { get; set; }
		public IDashDocument DashDocID { get; set; }
		public ICollection<IAcknowledgement<IDashCommercialInvoice>> Acknowledgements { get; set; }

		#endregion
	}
}
