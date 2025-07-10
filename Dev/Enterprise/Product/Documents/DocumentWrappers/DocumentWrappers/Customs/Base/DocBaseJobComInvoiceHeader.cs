using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBaseJobComInvoiceHeader : DocBaseWrapper
	{
		protected DocBaseJobComInvoiceHeader(BaseJobComInvoiceHeader baseJobComInvoiceHeader, BusinessObjectFactory factoryToWrap)
			: base(baseJobComInvoiceHeader, factoryToWrap)
		{
		}

		public override string ToString()
		{
			return InvoiceNumber;
		}

		#region Abstract

		protected abstract DocBaseJobDeclaration CreateJobDeclaration(BaseJobDeclaration declarationToWrap);
		protected abstract DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(BaseJobComInvoiceLineViewCollection collectionToWrap);

		#endregion

		#region Virtual

		public virtual ZString TariffHeading
		{
			get { return Res.GetString("B86EAE83-9C68-41B1-93AD-8370CB55335A", "Tariff"); }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal ConversionFactor
		{
			get { return BaseJobComInvoiceHeader.JZ_Calc_ConversionFactor; }
		}

		public ZDecimal TotalCIFLessFreightInLocalCurrency
		{
			get { return TotalCIFInLocalCurrency - TotalOverseasFreightInLocalCurrency; }
		}

		public ZDecimal TotalCIFLessFreightLessInsuranceInLocalCurrency
		{
			get { return TotalCIFLessFreightInLocalCurrency - TotalOverseasInsuranceInLocalCurrency; }
		}

		public ZDecimal TotalCIFLessAllNondutiableChargesExcludingFreightAndInsuranceInLocalCurrency
		{
			get { return TotalCIFLessFreightLessInsuranceInLocalCurrency - TotalNonDutiableChargesNotIncludedInLineExcludingFreightAndInsuranceInLocalCurrency; }
		}

		public ZDecimal TotalIncludedCosts
		{
			get { return BaseJobComInvoiceHeader.IncludedTotalInInvoiceCurr.Amount; }
		}

		public ZDecimal DiscountOrSurcharge
		{
			get { return -IncludedDiscount; }
		}

		public ZDecimal InvoiceLineTotal
		{
			get { return BaseJobComInvoiceHeader.InvoiceLineTotal; }
		}

		public ZDecimal TNI
		{
			get { return BaseJobComInvoiceHeader.JZ_Calc_TNI; }
		}

		public ZDecimal IncludedOverseasFreight
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.IncludedOverseasFreight); }
		}

		public ZDecimal ExcludedOverseasFreight
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.ExcludedOverseasFreight); }
		}

		public ZDecimal IncludedOverseasInsurance
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.IncludedOverseasInsurance); }
		}

		public ZDecimal ExcludedOverseasInsurance
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.ExcludedOverseasInsurance); }
		}

		public ZDecimal IncludedExWorks
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.IncludedExWorks); }
		}

		public ZDecimal ExcludedExWorks
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.ExcludedExWorks); }
		}

		public ZDecimal IncludedForeignInlandFreight
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.IncludedForeignInlandFreight); }
		}

		public ZDecimal ExcludedForeignInlandFreight
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.ExcludedForeignInlandFreight); }
		}

		public ZDecimal IncludedPackingCosts
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.IncludedPackingCosts); }
		}

		public ZDecimal ExcludedPackingCosts
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.ExcludedPackingCosts); }
		}

		public ZDecimal IncludedLandingCharges
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.IncludedLandingCharges); }
		}

		public ZDecimal ExcludedLandingCharges
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.ExcludedLandingCharges); }
		}

		public ZDecimal IncludedOtherCharges1
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.IncludedOtherCharges1); }
		}

		public ZDecimal ExcludedOtherCharges1
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.ExcludedOtherCharges1); }
		}

		public ZDecimal IncludedOtherCharges2
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.IncludedOtherCharges2); }
		}

		public ZDecimal ExcludedOtherCharges2
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.ExcludedOtherCharges2); }
		}

		public ZDecimal IncludedDiscount
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.IncludedDiscount); }
		}

		public ZDecimal ExcludedDiscount
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.ExcludedDiscount); }
		}

		public ZDecimal IncludedCommission
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.IncludedCommission); }
		}

		public ZDecimal ExcludedCommission
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.ExcludedCommission); }
		}

		public ZDecimal Balance
		{
			get { return BaseJobComInvoiceHeader.JZ_Calc_Balance; }
		}

		public ZDecimal CIFAmount
		{
			get { return BaseJobComInvoiceHeader.JZ_Calc_CIFAmount; }
		}

		public ZDecimal FOBAmount
		{
			get { return BaseJobComInvoiceHeader.JZ_Calc_FOBAmount; }
		}

		public ZDecimal InvoiceAmount
		{
			get { return BaseJobComInvoiceHeader.JZ_InvoiceAmount; }
		}

		public ZDecimal InvoiceCurrExRate
		{
			get { return BaseJobComInvoiceHeader.JZ_InvoiceCurrExRate; }
		}

		public ZDecimal TotalFreightExRate
		{
			get
			{
				CurrencyConverter converter = BaseJobComInvoiceHeader.CurrencyConverter;
				return converter.GetExchangeRate(BaseJobComInvoiceHeader.OverseasFreight.Currency);
			}
		}

		public ZDecimal TotalInsuranceExRate
		{
			get
			{
				CurrencyConverter converter = BaseJobComInvoiceHeader.CurrencyConverter;
				return converter.GetExchangeRate(BaseJobComInvoiceHeader.OverseasInsurance.Currency);
			}
		}

		public ZDecimal DutiableChargesNotIncludedInLinesExRate
		{
			get
			{
				CurrencyConverter converter = BaseJobComInvoiceHeader.CurrencyConverter;
				return converter.GetExchangeRate(BaseJobComInvoiceHeader.DutiableChargesNotIncludedInLines.Currency);
			}
		}

		public ZDecimal NonDutiableChargesNotIncludedInLinesExRate
		{
			get
			{
				CurrencyConverter converter = BaseJobComInvoiceHeader.CurrencyConverter;
				return converter.GetExchangeRate(BaseJobComInvoiceHeader.NonDutiableChargesNotIncludedInLines.Currency);
			}
		}

		public ZDecimal PaymentAmount
		{
			get { return BaseJobComInvoiceHeader.JZ_PaymentAmount; }
		}

		public ZDecimal PaymentExRate
		{
			get { return BaseJobComInvoiceHeader.JZ_PaymentExRate; }
		}

		public ZDecimal Volume
		{
			get { return BaseJobComInvoiceHeader.JZ_Volume; }
		}

		public ZDecimal Weight
		{
			get { return BaseJobComInvoiceHeader.JZ_Weight; }
		}

		public ZDecimal LinesEntered
		{
			get { return BaseJobComInvoiceHeader.JZ_Calc_LinesEntered; }
		}

		public ZDecimal InvoiceCurrExRateFallBackToJobExRate
		{
			get
			{
				ZDecimal result = BaseJobComInvoiceHeader.JZ_PaymentExRate;
				if (result == 0m)
				{
					result = BaseJobComInvoiceHeader.JZ_InvoiceCurrExRate;
				}
				if (result == 0m && Env.Registry.LandedCostingFallbackExRatesToJobInvoicing)
				{
					result = BaseJobComInvoiceHeader.GetExRateFromJobInvoicing();
				}
				if (result == 0m)
				{
					result = 1;
				}
				return result;
			}
		}

		public ZDecimal LandedCostingExRateFallBackToJobExRate
		{
			get { return BaseJobComInvoiceHeader.LandedCostingExRateFallBackToJobExRate; }
		}

		public ZDecimal TotalCIFInLocalCurrency
		{
			get { return BaseJobComInvoiceHeader.JZ_Calc_CIFAmount_InLocalCurrency; }
		}

		public ZDecimal InvoiceAmountInLocalCurrency
		{
			get { return BaseJobComInvoiceHeader.JZ_InvoiceAmountInLocalCurrency; }
		}

		public ZDecimal InvoiceLineTotalInLocalCurrency
		{
			get { return BaseJobComInvoiceHeader.InvoiceLineTotalInLocalCurrency; }
		}

		public ZDecimal FOBInLocalCurrency
		{
			get { return BaseJobComInvoiceHeader.JZ_Calc_FOBAmountInLocalCurrency; }
		}

		public ZDecimal FOBInLocalCurrencyRounded
		{
			get { return BaseJobComInvoiceHeader.JZ_Calc_FOBAmountInLocalCurrencyRounded; }
		}

		public ZDecimal TotalOverseasFreight
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.OverseasFreight); }
		}

		public ZDecimal TotalOverseasFreightInLocalCurrency
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.OverseasFreightInLocalCurrency); }
		}

		public ZDecimal TotalOverseasInsurance
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.OverseasInsurance); }
		}

		public ZDecimal TotalOverseasInsuranceInLocalCurrency
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.OverseasInsuranceInLocalCurrency); }
		}

		public ZDecimal TotalDutiableChargesNotIncludedInLines
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.DutiableChargesNotIncludedInLines); }
		}

		public ZDecimal TotalDutiableChargesNotIncludedInLinesInLocalCurrency
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.DutiableChargesNotIncludedInLinesInLocalCurrency); }
		}

		public ZDecimal TotalNonDutiableChargesNotIncludedInLineExcludingFreightAndInsurance
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsurance); }
		}

		public ZDecimal TotalNonDutiableChargesNotIncludedInLineExcludingFreightAndInsuranceInLocalCurrency
		{
			get { return ReturnMoneyAmountButZeroIfNull(BaseJobComInvoiceHeader.NonDutiableChargesNotIncludedInLinesExcludingFreightAndInsuranceInLocalCurrency); }
		}

		#endregion

		#region ZGuid Fields

		public ZGuid InvoiceLineTotalCurrency
		{
			get { return BaseJobComInvoiceHeader.InvoiceLineTotalCurrency; }
		}

		#endregion

		#region Wrapper Fields

		public DocCurrency Calc_TNICurrency
		{
			get
			{
				var currency = Factory.Load<RefCurrency>(BaseJobComInvoiceHeader.JZ_RX_Calc_TNICurrency);
				return DocCurrency.New(currency, Factory);
			}
		}

		public DocBranch Branch
		{
			get { return DocBranch.New(BaseJobComInvoiceHeader.Branch, Factory); }
		}

		public DocOrganisation Buyer
		{
			get { return BaseJobComInvoiceHeader.JZ_OH_Buyer.IsValid ? DocOrganisation.New(BaseJobComInvoiceHeader.Factory, BaseJobComInvoiceHeader.JZ_OH_Buyer) : null; }
		}

		public virtual DocOrganisation Supplier
		{
			get { return BaseJobComInvoiceHeader.JZ_OH_Supplier.IsValid ? GetNewSupplier() : null; }
		}

		protected virtual DocOrganisation GetNewSupplier()
		{
			return DocOrganisation.New(BaseJobComInvoiceHeader.Factory, BaseJobComInvoiceHeader.JZ_OH_Supplier);
		}

		public DocAddress PhysicalAddressForSupplier
		{
			get
			{
				DocAddress result = null;

				if (BaseJobComInvoiceHeader.Supplier != null)
				{
					OrgAddress physicalAddress = BaseJobComInvoiceHeader.Supplier.MainAddress;
					if (physicalAddress != null)
					{
						result = DocAddress.New(physicalAddress, Factory);
					}
				}

				return result;
			}
		}

		public DocCountry NKDefaultOrigin
		{
			get { return BaseJobComInvoiceHeader.JZ_RN_NKDefaultOrigin.IsValid ? DocCountry.New(BaseJobComInvoiceHeader.Factory, BaseJobComInvoiceHeader.JZ_RN_NKDefaultOrigin) : null; }
		}

		public DocCurrency CIFCurrency
		{
			get
			{
				var currency = Factory.Load<RefCurrency>(BaseJobComInvoiceHeader.JZ_Calc_CIFCurrency);
				return DocCurrency.New(currency, Factory);
			}
		}

		public DocCurrency IncludedCommissionCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.IncludedCommission.Currency); }
		}

		public DocCurrency ExcludedCommissionCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.ExcludedCommission.Currency); }
		}

		public DocCurrency IncludedDiscountCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.IncludedDiscount.Currency); }
		}

		public DocCurrency ExcludedDiscountCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.ExcludedDiscount.Currency); }
		}

		public DocCurrency IncludedExWorksCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.IncludedExWorks.Currency); }
		}

		public DocCurrency ExcludedExWorksCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.ExcludedExWorks.Currency); }
		}

		public DocCurrency FOBCurrency
		{
			get
			{
				var currency = Factory.Load<RefCurrency>(BaseJobComInvoiceHeader.JZ_Calc_FOBCurrency);
				return DocCurrency.New(currency, Factory);
			}
		}

		public DocCurrency IncludedForeignInlandFreightCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.IncludedForeignInlandFreight.Currency); }
		}

		public DocCurrency ExcludedForeignInlandFreightCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.ExcludedForeignInlandFreight.Currency); }
		}

		public DocCurrency InvoiceCurr
		{
			get { return DocCurrency.New(BaseJobComInvoiceHeader.Invoice_Currency, Factory); }
		}

		public DocCurrency IncludedLandingChargesCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.IncludedLandingCharges.Currency); }
		}

		public DocCurrency ExcludedLandingChargesCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.ExcludedLandingCharges.Currency); }
		}

		public DocCurrency IncludedOtherCharges1Currency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.IncludedOtherCharges1.Currency); }
		}

		public DocCurrency ExcludedOtherCharges1Currency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.ExcludedOtherCharges1.Currency); }
		}

		public DocCurrency IncludedOtherCharges2Currency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.IncludedOtherCharges2.Currency); }
		}

		public DocCurrency ExcludedOtherCharges2Currency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.ExcludedOtherCharges2.Currency); }
		}

		public DocCurrency IncludedOverseasFreightCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.IncludedOverseasFreight.Currency); }
		}

		public DocCurrency ExcludedOverseasFreightCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.ExcludedOverseasFreight.Currency); }
		}

		public DocCurrency IncludedOverseasInsuranceCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.IncludedOverseasInsurance.Currency); }
		}

		public DocCurrency ExcludedOverseasInsuranceCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.ExcludedOverseasInsurance.Currency); }
		}

		public DocCurrency IncludedPackingCostsCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.IncludedPackingCosts.Currency); }
		}

		public DocCurrency ExcludedPackingCostsCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.ExcludedPackingCosts.Currency); }
		}

		public DocCurrency OverseasFreightCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.OverseasFreight.Currency); }
		}

		public DocCurrency OverseasInsuranceCurrency
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.OverseasInsurance.Currency); }
		}

		public DocCurrency DutiableChargesNotIncludedInLines
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.DutiableChargesNotIncludedInLines.Currency); }
		}

		public DocCurrency NonDutiableChargesNotIncludedInLines
		{
			get { return DocCurrency.New(Factory, BaseJobComInvoiceHeader.NonDutiableChargesNotIncludedInLines.Currency); }
		}

		public DocOrganisation OSParty
		{
			get
			{
				DocOrganisation result = null;
				if (BaseJobComInvoiceHeader != null && BaseJobComInvoiceHeader.JobDeclaration != null && BaseJobComInvoiceHeader.JobDeclaration.IsExport)
				{
					result = Buyer;
				}
				else
				{
					result = Supplier;
				}
				return result;
			}
		}
		#endregion

		#region ZString Fields

		public ZString SupplierAndInvoiceNumberGroupByString
		{
			get { return ((Supplier == null) ? "" : (string)Supplier.Name) + "__" + InvoiceNumber; }
		}

		public ZString DiscountOrSurchargeText
		{
			get { return IncludedDiscount < 0 ? Res.GetString("E4F10E68-760D-41A0-8267-613B1818EAEC", "Surcharge") : Res.GetString("9A91192F-805D-4478-B91F-A215CD619151", "Discount"); }
		}

		public ZString CountryOfOrigin
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocBaseJobComInvoiceLine line in InvoiceLinesInternal)
				{
					if (line.Origin != null)
					{
						if (result.IsEmpty)
						{
							result = line.Origin.Name.GetUnresolvedString();
						}
						else
						{
							if (line.Origin.Name.GetUnresolvedString() != result)
							{
								result = "VARIOUS";
								break;
							}
						}
					}
				}
				return result;
			}
		}

		public ZString IncoTermDescription
		{
			get
			{
				if (IncoTermDescriptionCached == null)
				{
					IncoTermDescriptionCached = new CachedProperty<ZString>(BaseJobComInvoiceHeader.Factory, delegate
						{
							ZString incoTerm = DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.GetInternationalCode(BaseJobComInvoiceHeader.JZ_IncoTerm);
							if (incoTerm.IsEmpty)
							{
								return BaseJobComInvoiceHeader.Lookups.JZ_IncoTerm_List.GetDescriptionFromCode(IncoTerm);
							}
							else
							{
								return BaseJobComInvoiceHeader.Factory.GetCachedValue<IncoTermsCodeDescriptionPairList>().GetDescriptionFromCode(incoTerm);
							}
						});
				}
				return IncoTermDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> IncoTermDescriptionCached;

		public ZString CalcGroupInvoice
		{
			get { return BaseJobComInvoiceHeader.JZ_Calc_GroupInvoice; }
		}

		public ZString AddInfo
		{
			get { return BaseJobComInvoiceHeader.JZ_AddInfo; }
		}

		public ZString InvoiceNumber
		{
			get { return BaseJobComInvoiceHeader.JZ_InvoiceNumber; }
		}

		public ZString IncoTerm
		{
			get
			{
				ZString result = DocumentsDataRegistry.Instance.CustomsIncoTermsOverride.GetInternationalCode(BaseJobComInvoiceHeader.JZ_IncoTerm);
				return result.IsEmpty ? BaseJobComInvoiceHeader.JZ_IncoTerm : result;
			}
		}

		public ZString PaymentNo
		{
			get { return BaseJobComInvoiceHeader.JZ_PaymentNo; }
		}

		public ZString VolumeUQ
		{
			get { return BaseJobComInvoiceHeader.JZ_VolumeUQ; }
		}

		public ZString WeightUQ
		{
			get { return BaseJobComInvoiceHeader.JZ_WeightUQ; }
		}

		public ZString InvoiceAmt
		{
			get { return FormatNumber(InvoiceAmount, 2); }
		}

		public ZString MasterBill
		{
			//This is for Commercial Invoice Reference: Booking UDF.
			get { return DeclarationInternal != null ? DeclarationInternal.MasterBill : ZString.Empty; }
		}

		public ZString InvoiceAmountAndCurrencyCode
		{
			get { return InvoiceCurr == null ? "" : InvoiceAmount.ToString(2) + " " + InvoiceCurr.Code; }
		}

		public ZString JobNumber
		{
			//This is for Commercial Invoice Reference: Forwarding Agent UDF.
			get { return DeclarationInternal != null ? DeclarationInternal.JobNumber : ZString.Empty; }
		}
		#endregion

		#region ZDateTime Fields

		public ZDateTime InvoiceDate
		{
			get { return BaseJobComInvoiceHeader.JZ_InvoiceDate; }
		}

		public ZDateTime PaymentDate
		{
			get { return BaseJobComInvoiceHeader.JZ_PaymentDate; }
		}

		#endregion

		#region Implementation

		protected DocBaseJobDeclaration DeclarationInternal
		{
			get { return CreateJobDeclaration(BaseJobComInvoiceHeader.JobDeclaration); }
		}

		protected DocBaseJobComInvoiceLineCollection InvoiceLinesInternal
		{
			get
			{
				if (fInvoiceLinesInternal == null)
				{
					fInvoiceLinesInternal = CreateJobComInvoiceLineCollection(BaseJobComInvoiceHeader.JobComInvoiceLines);
					fInvoiceLinesInternal.Sort("LineNo", System.ComponentModel.ListSortDirection.Ascending);
				}
				return fInvoiceLinesInternal;
			}
		}
		DocBaseJobComInvoiceLineCollection fInvoiceLinesInternal;

		protected DocBaseJobComInvoiceLineCollection UnclassifiedInvoiceLinesInternal
		{
			get
			{
				if (fUnclassifiedInvoiceLinesInternal == null)
				{
					fUnclassifiedInvoiceLinesInternal = CreateJobComInvoiceLineCollection(BaseJobComInvoiceHeader.JobComInvoiceLines);
					fUnclassifiedInvoiceLinesInternal.RemoveClassifiedLines();
					fUnclassifiedInvoiceLinesInternal.Sort("LineNo", System.ComponentModel.ListSortDirection.Ascending);
				}
				return fUnclassifiedInvoiceLinesInternal;
			}
		}
		DocBaseJobComInvoiceLineCollection fUnclassifiedInvoiceLinesInternal;

		BaseJobComInvoiceHeader BaseJobComInvoiceHeader
		{
			get { return (BaseJobComInvoiceHeader)WrappedObject; }
		}

		protected ZDecimal ReturnMoneyAmountButZeroIfNull(Money money)
		{
			return (money != null) ? money.Amount : new ZDecimal(0);
		}
		#endregion
	}
}
