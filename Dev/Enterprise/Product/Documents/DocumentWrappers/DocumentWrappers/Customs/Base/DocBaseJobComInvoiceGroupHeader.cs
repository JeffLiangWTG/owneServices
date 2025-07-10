using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBaseJobComInvoiceGroupHeader : DocBaseWrapper
	{
		protected DocBaseJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader baseJobComInvoiceGroupHeader, BusinessObjectFactory factoryToWrap)
			: base(baseJobComInvoiceGroupHeader, factoryToWrap)
		{
		}

		public override string ToString()
		{
			return InvoiceNumber;
		}

		#region Abstract

		protected abstract DocBaseJobComInvoiceHeaderCollection CreateJobComInvoiceHeaderCollection(InvoiceHeaderActiveCollection collectionToWrap);

		#endregion

		#region ZDecimal Fields

		public ZDecimal TNI
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_Calc_TNI; }
		}

		public ZDecimal CIFAmount
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_Calc_CIFAmount; }
		}

		public ZDecimal Commission
		{
			get { return BaseJobComInvoiceGroupHeader.CalcCommission; }
		}

		public ZDecimal Discount
		{
			get { return BaseJobComInvoiceGroupHeader.CalcDiscount; }
		}

		public ZDecimal ExWorksAmount
		{
			get { return BaseJobComInvoiceGroupHeader.CalcExWorks; }
		}

		public ZDecimal FOBAmount
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_Calc_FOBAmount; }
		}

		public ZDecimal ForeignInlandFreight
		{
			get { return BaseJobComInvoiceGroupHeader.CalcForeignInlandFreight; }
		}

		//Not valid
		//		public ZDecimal InvoiceAmount
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_InvoiceAmount; } 
		//		}
		//Not valid
		//		public ZDecimal InvoiceCurrExRate
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_InvoiceCurrExRate; } 
		//		}

		public ZDecimal LandingCharges
		{
			get { return BaseJobComInvoiceGroupHeader.CalcLandingCharges; }
		}

		//		public ZDecimal NonDutiablePreFOBCharges
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_NonDutiablePreFOBCharges; } 
		//		}

		public ZDecimal OtherCharges1
		{
			get { return BaseJobComInvoiceGroupHeader.CalcOtherCharges1; }
		}

		public ZDecimal OtherCharges2
		{
			get { return BaseJobComInvoiceGroupHeader.CalcOtherCharges2; }
		}

		public ZDecimal OverseasFreight
		{
			get { return BaseJobComInvoiceGroupHeader.CalcOverseasFreight; }
		}

		public ZDecimal OverseasInsurance
		{
			get { return BaseJobComInvoiceGroupHeader.CalcOverseasInsurance; }
		}

		public ZDecimal PackingCosts
		{
			get { return BaseJobComInvoiceGroupHeader.CalcPackingCosts; }
		}

		public ZDecimal PaymentAmount
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_PaymentAmount; }
		}

		public ZDecimal PaymentExRate
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_PaymentExRate; }
		}

		public ZDecimal Volume
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_Volume; }
		}

		public ZDecimal Weight
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_Weight; }
		}

		#endregion

		#region ZString Fields

		public ZString InvoiceHeaderNumbers
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (DocBaseJobComInvoiceHeader invoiceHeader in InvoiceHeadersInternal)
				{
					if (!invoiceHeader.InvoiceNumber.IsEmpty)
					{
						result += invoiceHeader.InvoiceNumber + ", ";
					}
				}

				return result;
			}
		}

		//Not valid
		public ZString IncoTerm
		{
			get
			{
				ZString result = DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.GetInternationalCode(BaseJobComInvoiceGroupHeader.JZ_IncoTerm);
				return result.IsEmpty ? BaseJobComInvoiceGroupHeader.JZ_IncoTerm : result;
			}
		}

		public ZString AddInfo
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_AddInfo; }
		}

		public ZString InvoiceNumber
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_InvoiceNumber; }
		}

		public ZString PaymentNo
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_PaymentNo; }
		}

		public ZString VolumeUQ
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_VolumeUQ; }
		}

		public ZString WeightUQ
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_WeightUQ; }
		}

		#endregion

		#region ZBool Fields

		//If you need these flags, please grab Joo. Need another function in Customs solution
		//		public ZBool IsCommissionIncludedInITOT
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_IsCommissionIncludedInITOT; } 
		//		}
		//
		//		public ZBool IsDiscountIncludedInITOT
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_IsDiscountIncludedInITOT; } 
		//		}
		//
		//		public ZBool IsForeignInlandFreightIncludedInITOT
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_IsForeignInlandFreightIncludedInITOT; } 
		//		}
		//
		//		public ZBool IsInsuranceIncludedInITOT
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_IsInsuranceIncludedInITOT; } 
		//		}
		//
		//		public ZBool IsInternationalFreightIncludedInITOT
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_IsInternationalFreightIncludedInITOT; } 
		//		}
		//
		//		public ZBool IsLandingChargeIncludedInITOT
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_IsLandingChargeIncludedInITOT; } 
		//		}
		//
		//		public ZBool IsNonDutiableFOBIncludedInITOT
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_IsNonDutiableFOBIncludedInITOT; } 
		//		}
		//
		//		public ZBool IsOtherCharges1IncludedInITOT
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_IsOtherCharges1IncludedInITOT; } 
		//		}
		//
		//		public ZBool IsOtherCharges2IncludedInITOT
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_IsOtherCharges2IncludedInITOT; } 
		//		}
		//
		//		public ZBool IsPackingChargeIncludedInITOT
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_IsPackingChargeIncludedInITOT; } 
		//		}

		#endregion

		#region Wrapper Fields

		public DocBranch Branch
		{
			get { return DocBranch.New(BaseJobComInvoiceGroupHeader.Branch, Factory); }
		}

		//		public DocComInvoiceHeader ComInvoiceHeader
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_JZ_GroupInvoiceFK.IsValid ? DocComInvoiceHeader.New(BaseJobComInvoiceGroupHeader.Factory, BaseJobComInvoiceGroupHeader.JZ_JZ_GroupInvoiceFK) : null; } 
		//		}

		public DocOrganisation Buyer
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_OH_Buyer.IsValid ? DocOrganisation.New(BaseJobComInvoiceGroupHeader.Factory, BaseJobComInvoiceGroupHeader.JZ_OH_Buyer) : null; }
		}

		public DocOrganisation Supplier
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_OH_Supplier.IsValid ? DocOrganisation.New(BaseJobComInvoiceGroupHeader.Factory, BaseJobComInvoiceGroupHeader.JZ_OH_Supplier) : null; }
		}

		public DocCountry NKDefaultOrigin
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_RN_NKDefaultOrigin.IsValid ? DocCountry.New(BaseJobComInvoiceGroupHeader.Factory, BaseJobComInvoiceGroupHeader.JZ_RN_NKDefaultOrigin) : null; }
		}

		public DocCurrency CIFCurrency
		{
			get { return DocCurrency.New(GetCurrecy(BaseJobComInvoiceGroupHeader.JZ_Calc_CIFCurrency), Factory); }
		}

		public DocCurrency CalcCommissionCurrency
		{
			get { return DocCurrency.New(GetCurrecy(BaseJobComInvoiceGroupHeader.CalcCommissionCurrency), Factory); }
		}

		public DocCurrency CalcDiscountCurrency
		{
			get { return DocCurrency.New(GetCurrecy(BaseJobComInvoiceGroupHeader.CalcDiscountCurrency), Factory); }
		}

		public DocCurrency CalcExWorksCurrency
		{
			get { return DocCurrency.New(GetCurrecy(BaseJobComInvoiceGroupHeader.CalcExWorksCurrency), Factory); }
		}

		public DocCurrency FOBCurrency
		{
			get { return DocCurrency.New(GetCurrecy(BaseJobComInvoiceGroupHeader.JZ_Calc_FOBCurrency), Factory); }
		}

		public DocCurrency CalcForeignInlandFreightCurrency
		{
			get { return DocCurrency.New(GetCurrecy(BaseJobComInvoiceGroupHeader.CalcForeignInlandFreightCurrency), Factory); }
		}

		//Not valid
		//		public DocCurrency Invoice_Currency
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_RX_NKInvoice_Currency.IsValid ? DocCurrency.New(BaseJobComInvoiceGroupHeader.Factory, BaseJobComInvoiceGroupHeader.JZ_RX_NKInvoice_Currency) : null; } 
		//		}

		public DocCurrency CalcLandingChargesCurrency
		{
			get { return DocCurrency.New(GetCurrecy(BaseJobComInvoiceGroupHeader.CalcLandingChargesCurrency), Factory); }
		}
		//
		//		public DocCurrency NonDutiablePreFOBChargeCurrency
		//		{
		//			get { return BaseJobComInvoiceGroupHeader.JZ_RX_NonDutiablePreFOBChargeCurrency.IsValid ? DocCurrency.New(BaseJobComInvoiceGroupHeader.Factory, BaseJobComInvoiceGroupHeader.JZ_RX_NonDutiablePreFOBChargeCurrency) : null; } 
		//		}

		public DocCurrency CalcOtherCharges1Currency
		{
			get { return DocCurrency.New(GetCurrecy(BaseJobComInvoiceGroupHeader.CalcOtherCharges1Currency), Factory); }
		}

		public DocCurrency CalcOtherCharges2Currency
		{
			get { return DocCurrency.New(GetCurrecy(BaseJobComInvoiceGroupHeader.CalcOtherCharges2Currency), Factory); }
		}

		public DocCurrency CalcOverseasFreightCurrency
		{
			get { return DocCurrency.New(GetCurrecy(BaseJobComInvoiceGroupHeader.CalcOverseasFreightCurrency), Factory); }
		}

		public DocCurrency CalcOverseasInsuranceCurrency
		{
			get { return DocCurrency.New(GetCurrecy(BaseJobComInvoiceGroupHeader.CalcOverseasInsuranceCurrency), Factory); }
		}

		public DocCurrency CalcPackingCostCurrency
		{
			get { return DocCurrency.New(GetCurrecy(BaseJobComInvoiceGroupHeader.CalcPackingCostsCurrency), Factory); }
		}

		protected RefCurrency GetCurrecy(ZGuid guid)
		{
			return Factory.Load<RefCurrency>(guid);
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime InvoiceDate
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_InvoiceDate; }
		}

		public ZDateTime PaymentDate
		{
			get { return BaseJobComInvoiceGroupHeader.JZ_PaymentDate; }
		}

		#endregion

		#region Implementation

		protected DocBaseJobComInvoiceHeaderCollection InvoiceHeadersInternal
		{
			get { return CreateJobComInvoiceHeaderCollection(BaseJobComInvoiceGroupHeader.JobComInvoiceHeaders); }
		}

		BaseJobComInvoiceGroupHeader BaseJobComInvoiceGroupHeader
		{
			get { return (BaseJobComInvoiceGroupHeader)WrappedObject; }
		}

		#endregion
	}
}
