using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface ICusSupportingInfo
		{
			ZGuid PK { get; }
			ZString CSI_UnitOfQuantity2 { get; set; }
			ZString CSI_UnitOfQuantity { get; set; }
			ZString CSI_Type { get; set; }
			ZString CSI_SubType { get; set; }
			ZString CSI_Status { get; set; }
			ZString CSI_RN_NKCountryCode { get; set; }
			ZString CSI_ReferenceNumber { get; set; }
			ZDecimal CSI_Quantity2 { get; set; }
			ZString CSI_Procedure { get; set; }
			ZString CSI_ParentTableCode { get; set; }
			ZGuid CSI_ParentID { get; set; }
			ZInt CSI_LineNo { get; set; }
			ZString CSI_Description { get; set; }
			ZDateTime CSI_DateOfIssue { get; set; }
			ZString CSI_CustomsOffice { get; set; }
			ZString CSI_Code { get; set; }
			ZDecimal CSI_Quantity { get; set; }
		}
	}
}
