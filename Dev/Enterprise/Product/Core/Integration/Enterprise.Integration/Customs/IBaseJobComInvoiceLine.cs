using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IBaseJobComInvoiceLine
		{
			ZGuid PK { get; }
			object this[string propertyName] { get; set; }

			ZInt JI_ClusterKey { get; set; }
			ZString JI_AddInfo { get; set; }
			ZDecimal JI_BondedWhsQuantity { get; set; }
			ZString JI_BondedWhsUnitQty { get; set; }
			ZGuid JI_CC { get; set; }
			ZGuid JI_CL { get; set; }
			ZGuid JI_CO { get; set; }
			ZString JI_ConcessionOrder { get; set; }
			ZString JI_ContainerMode { get; set; }
			ZString JI_CustomAttrib1 { get; set; }
			ZString JI_CustomAttrib2 { get; set; }
			ZString JI_CustomAttrib3 { get; set; }
			ZString JI_CustomAttrib4 { get; set; }
			ZString JI_CustomAttrib5 { get; set; }
			ZString JI_CustomAttrib6 { get; set; }
			ZDateTime JI_CustomDate1 { get; set; }
			ZDateTime JI_CustomDate2 { get; set; }
			ZDateTime JI_CustomDate3 { get; set; }
			ZDateTime JI_CustomDate4 { get; set; }
			ZDateTime JI_CustomDate5 { get; set; }
			ZDecimal JI_CustomDecimal1 { get; set; }
			ZDecimal JI_CustomDecimal2 { get; set; }
			ZDecimal JI_CustomDecimal3 { get; set; }
			ZDecimal JI_CustomDecimal4 { get; set; }
			ZDecimal JI_CustomDecimal5 { get; set; }
			ZBool JI_CustomFlag1 { get; set; }
			ZBool JI_CustomFlag2 { get; set; }
			ZBool JI_CustomFlag3 { get; set; }
			ZBool JI_CustomFlag4 { get; set; }
			ZBool JI_CustomFlag5 { get; set; }
			ZDecimal JI_CustomsQuantity { get; set; }
			ZString JI_CustomsUnitQty { get; set; }
			ZString JI_CustomTextBlob1 { get; set; }
			ZString JI_Description { get; set; }
			ZString JI_NDescription { get; set; }
			ZString JI_ExtraInfoForClassification { get; set; }
			ZString JI_HazMatCode { get; set; }
			ZString JI_HazMatCodeQualifier { get; set; }
			ZDecimal JI_InvoiceQuantity { get; set; }
			ZString JI_InvoiceUQ { get; set; }
			ZGuid JI_JO { get; set; }
			ZGuid JI_JZ { get; set; }
			ZShort JI_LineNo { get; set; }
			ZDecimal JI_LinePrice { get; set; }
			ZDecimal JI_NetWeight { get; set; }
			ZString JI_NetWeightUQ { get; set; }
			ZGuid JI_OP { get; set; }
			ZString JI_OrderNumber { get; set; }
			ZGuid JI_ParentID { get; set; }
			ZShort JI_ParentLine { get; set; }
			ZString JI_ParentTableCode { get; set; }
			ZString JI_PartAttrib1 { get; set; }
			ZString JI_PartAttrib2 { get; set; }
			ZString JI_PartAttrib3 { get; set; }
			ZString JI_SerialNumber { get; set; }
			ZString JI_PartNo { get; set; }
			ZString JI_RH_NKCommodity_Code { get; set; }
			ZString JI_CountryOfOrigin { get; set; }
			ZString JI_Tariff { get; set; }
			ZString JI_TariffForComplianceWise { get; }
			ZDecimal JI_Volume { get; set; }
			ZString JI_VolumeUQ { get; set; }
			ZDecimal JI_Weight { get; set; }
			ZString JI_WeightUQ { get; set; }

			void SynchroniseFromForwardingOrderLine(Forwarding.IOrderLine orderLine, bool autoAddOrderNumberOnSet);
		}
	}
}
