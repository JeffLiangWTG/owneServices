using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;

namespace Enterprise.Dash.Business.Entities
{
	public sealed class EntityDashCommercialInvoiceLineItem : IDashCommercialInvoiceLineItem
	{
		public Guid DLI_PK { get; set; }

		public Guid? DLI_CI_MatchedHSCodeID { get; set; }
		public Guid DLI_DCI_HeaderID { get; set; }
		public string DLI_EditedHSCode { get; set; }
		public string DLI_EditedProductCode { get; set; }
		public string DLI_EditedProductDescription { get; set; }
		public string DLI_F3_NKUnitType { get; set; }
		public string DLI_HSCode { get; set; }
		public int DLI_Index { get; set; }
		public bool DLI_IsActive { get; set; }
		public decimal DLI_LineTotal { get; set; }
		public string DLI_MatchedType { get; set; }
		public Guid? DLI_OP_MatchedProductCodeID { get; set; }
		public string DLI_ParsedHSCode { get; set; }
		public string DLI_ParsedProductCode { get; set; }
		public string DLI_ParsedProductDescription { get; set; }
		public decimal DLI_PricePerUnit { get; set; }
		public string DLI_ProductCode { get; set; }
		public string DLI_ProductDescription { get; set; }
		public string DLI_ParsedRawText { get; set; }
		public decimal DLI_Quantity { get; set; }
		public string DLI_RN_NKOriginCountry { get; set; }
		public DateTime DLI_SystemCreateTimeUtc { get; set; }
		public string DLI_SystemCreateUser { get; set; }
		public DateTime DLI_SystemLastEditTimeUtc { get; set; }
		public string DLI_SystemLastEditUser { get; set; }

		#region Not Used Properties

		public IRefPackTypeInfo UnitType { get; set; }
		public IRefCountryInfo OriginCountry { get; set; }
		public IGlbStaffInfo CreatedByStaff { get; set; }
		public IGlbStaffInfo LastEditedByStaff { get; set; }
		public ICusClassPartPivotInfo MatchedHSCodeID { get; set; }
		public IDashCommercialInvoice HeaderID { get; set; }
		public IOrgSupplierPartInfo MatchedProductCodeID { get; set; }
		public ICollection<IAcknowledgement<IDashCommercialInvoiceLineItem>> Acknowledgements { get; set; }

		#endregion
	}
}
