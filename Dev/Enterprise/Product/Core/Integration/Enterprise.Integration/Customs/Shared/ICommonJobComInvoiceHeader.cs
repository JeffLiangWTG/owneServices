using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class Shared
		{
			public interface ICommonJobComInvoiceHeader
			{
				ZGuid PK { get; }
				object this[string propertyName] { get; set; }
				ZInt JZ_ClusterKey { get; set; }

				#region JZ_AddInfo

				ZString JZ_AddInfo { get; set; }
				ZPropertyInfo JZ_AddInfoInfo { get; }

				#endregion

				#region JZ_CU_RelatedHouseBill

				ZGuid JZ_CU_RelatedHouseBill { get; set; }
				ZPropertyInfo JZ_CU_RelatedHouseBillInfo { get; }

				#endregion

				#region JZ_FOBValue

				ZDecimal JZ_FOBValue { get; set; }
				ZPropertyInfo JZ_FOBValueInfo { get; }

				#endregion

				#region JZ_GB

				ZGuid JZ_GB { get; set; }
				ZPropertyInfo JZ_GBInfo { get; }

				#endregion

				#region JZ_GroupInvoice

				ZBool JZ_GroupInvoice { get; set; }
				ZPropertyInfo JZ_GroupInvoiceInfo { get; }

				#endregion

				#region JZ_IncoTerm

				ZString JZ_IncoTerm { get; set; }
				ZPropertyInfo JZ_IncoTermInfo { get; }

				#endregion

				#region JZ_InvoiceAmount

				ZDecimal JZ_InvoiceAmount { get; set; }
				ZPropertyInfo JZ_InvoiceAmountInfo { get; }

				#endregion

				#region JZ_InvoiceCurrExRate

				ZDecimal JZ_InvoiceCurrExRate { get; set; }
				ZPropertyInfo JZ_InvoiceCurrExRateInfo { get; }

				#endregion

				#region JZ_InvoiceCurrExRateType

				ZString JZ_InvoiceCurrExRateType { get; set; }
				ZPropertyInfo JZ_InvoiceCurrExRateTypeInfo { get; }

				#endregion

				#region JZ_InvoiceCurrLandedCostExRate

				ZDecimal JZ_InvoiceCurrLandedCostExRate { get; set; }
				ZPropertyInfo JZ_InvoiceCurrLandedCostExRateInfo { get; }

				#endregion

				#region JZ_InvoiceDate

				ZDateTime JZ_InvoiceDate { get; set; }
				ZPropertyInfo JZ_InvoiceDateInfo { get; }

				#endregion

				#region JZ_InvoiceDisplaySequence

				ZShort JZ_InvoiceDisplaySequence { get; set; }
				ZPropertyInfo JZ_InvoiceDisplaySequenceInfo { get; }

				#endregion

				#region JZ_InvoiceNumber

				ZString JZ_InvoiceNumber { get; set; }
				ZPropertyInfo JZ_InvoiceNumberInfo { get; }

				#endregion

				#region JZ_JE

				ZGuid JZ_JE { get; set; }
				ZPropertyInfo JZ_JEInfo { get; }

				#endregion

				#region JZ_JZ_GroupInvoiceFK

				ZGuid JZ_JZ_GroupInvoiceFK { get; set; }
				ZPropertyInfo JZ_JZ_GroupInvoiceFKInfo { get; }

				#endregion

				#region JZ_MessageStatus

				ZString JZ_MessageStatus { get; set; }
				ZPropertyInfo JZ_MessageStatusInfo { get; }

				#endregion

				#region JZ_NoOfPacks

				ZDecimal JZ_NoOfPacks { get; set; }
				ZPropertyInfo JZ_NoOfPacksInfo { get; }

				#endregion

				#region JZ_OH_Buyer

				ZGuid JZ_OH_Buyer { get; set; }
				ZPropertyInfo JZ_OH_BuyerInfo { get; }

				#endregion

				#region JZ_OH_Supplier

				ZGuid JZ_OH_Supplier { get; set; }
				ZPropertyInfo JZ_OH_SupplierInfo { get; }

				#endregion

				#region JZ_OverrideFOB

				ZBool JZ_OverrideFOB { get; set; }
				ZPropertyInfo JZ_OverrideFOBInfo { get; }

				#endregion

				#region JZ_PaymentAmount

				ZDecimal JZ_PaymentAmount { get; set; }
				ZPropertyInfo JZ_PaymentAmountInfo { get; }

				#endregion

				#region JZ_PaymentDate

				ZDateTime JZ_PaymentDate { get; set; }
				ZPropertyInfo JZ_PaymentDateInfo { get; }

				#endregion

				#region JZ_PaymentExRate

				ZDecimal JZ_PaymentExRate { get; set; }
				ZPropertyInfo JZ_PaymentExRateInfo { get; }

				#endregion

				#region JZ_PaymentNo

				ZString JZ_PaymentNo { get; set; }
				ZPropertyInfo JZ_PaymentNoInfo { get; }

				#endregion

				#region JZ_IncoTermPlace

				ZString JZ_IncoTermPlace { get; set; }
				ZPropertyInfo JZ_IncoTermPlaceInfo { get; }

				#endregion

				#region JZ_RN_NKDefaultOrigin

				ZString JZ_RN_NKDefaultOrigin { get; set; }
				ZPropertyInfo JZ_RN_NKDefaultOriginInfo { get; }

				#endregion

				#region JZ_RX_NKInvoice_Currency

				ZString JZ_RX_NKInvoice_Currency { get; set; }
				ZPropertyInfo JZ_RX_NKInvoice_CurrencyInfo { get; }

				#endregion

				#region JZ_StandAloneInvoiceDirection

				ZString JZ_StandAloneInvoiceDirection { get; set; }
				ZPropertyInfo JZ_StandAloneInvoiceDirectionInfo { get; }

				#endregion

				#region JZ_ValuationDateOverride

				ZDateTime JZ_ValuationDateOverride { get; set; }
				ZPropertyInfo JZ_ValuationDateOverrideInfo { get; }

				#endregion

				#region JZ_Volume

				ZDecimal JZ_Volume { get; set; }
				ZPropertyInfo JZ_VolumeInfo { get; }

				#endregion

				#region JZ_VolumeUQ

				ZString JZ_VolumeUQ { get; set; }
				ZPropertyInfo JZ_VolumeUQInfo { get; }

				#endregion

				#region JZ_Weight

				ZDecimal JZ_Weight { get; set; }
				ZPropertyInfo JZ_WeightInfo { get; }

				#endregion

				#region JZ_WeightUQ

				ZString JZ_WeightUQ { get; set; }
				ZPropertyInfo JZ_WeightUQInfo { get; }

				#endregion
			}
		}
	}
}
