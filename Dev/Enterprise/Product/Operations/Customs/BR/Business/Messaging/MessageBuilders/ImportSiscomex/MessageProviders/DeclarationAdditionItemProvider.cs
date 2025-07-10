using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts;
using CargoWise.Customs.BR.MessageContracts.ImportSiscomex.Outgoing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex
{
	public class DeclarationAdditionItemProvider : IDeclarationAdditionItem
	{
		public DeclarationAdditionItemProvider(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		}
		readonly CusEntryLine entryLine;

		public static DeclarationAdditionItemProvider New(CusEntryLine entryLine) => entryLine == null ? null : new DeclarationAdditionItemProvider(entryLine);

		JobComInvoiceLine invoiceLine => entryLine.RandomLine;

		JobComInvoiceHeader invoiceHeader => entryLine.RandomLine.InvoiceHeader;

		IEnumerable<JobComInvoiceLine> invoiceLines => invoiceLinesCached ?? (invoiceLinesCached = entryLine.InvoiceLines.Cast<JobComInvoiceLine>().ToArray());
		JobComInvoiceLine[] invoiceLinesCached;

		public IEnumerable<IDeclarationCustomsValuation> DeclarationCustomsValuationList => customsValuationList ??
			(customsValuationList = invoiceLine.NVECusCodeDataCollection.Cast<NveCusCodeData>()
			.Select(nve => DeclarationCustomsValuationProvider.New(nve)).ToArray());
		IDeclarationCustomsValuation[] customsValuationList;

		public IEnumerable<IDeclarationProductDetails> DeclarationProductDetailsList => productDetails ??
			(productDetails = invoiceLines.Select(x => DeclarationProductDetailsProvider.New(x)).ToArray());
		IDeclarationProductDetails[] productDetails;

		public IEnumerable<IDeclarationLinkedDocument> DeclarationLinkedDocumentList => linkedDocumentList ??
			(linkedDocumentList = invoiceLines.SelectMany(x => x.PreviousDocuments.Cast<PreviousDocument>())
			.Select(previousDoc => DeclarationLinkedDocumentProvider.New(previousDoc)).Distinct().ToArray());
		IDeclarationLinkedDocument[] linkedDocumentList;

		public IEnumerable<IDeclarationDetach> DeclarationDetachList => detachList ??
			(detachList = invoiceLine.TariffDetachs.Cast<TariffDetach>()
			.Select(tariffDetach => DeclarationDetachProvider.New(tariffDetach)).ToArray());
		IDeclarationDetach[] detachList;

		#region Charges

		public IEnumerable<IDeclarationExtraCharge> DeclarationExtraChargeList => charges ??
			(charges = allExtraCharges?.GroupBy(charge => new { charge.J7_ChargeType, charge.J7_RX_NKCurrency })
			.Select(charges => DeclarationExtraChargeProvider.New(charges)).ToArray());
		IDeclarationExtraCharge[] charges;

		JobComInvCharge[] allExtraCharges => invoiceLines.SelectMany(line => line.Charges.Cast<JobComInvCharge>().Union(line.ApportionedCharges))
			.Where(charge => ImportChargesProvider.IsAdditions(charge.J7_ChargeType) && charge.Money.Amount > 0).ToArray();

		#endregion

		#region Discount Charges

		public IEnumerable<IDeclarationDiscountCharge> DeclarationDiscountChargeList => discountCharges ??
			(discountCharges = allDiscountCharges?.GroupBy(discount => new { discount.J7_ChargeType, discount.J7_RX_NKCurrency })
			.Select(charges => DeclarationDiscountChargeProvider.New(charges)).ToArray());
		IDeclarationDiscountCharge[] discountCharges;

		JobComInvCharge[] allDiscountCharges => invoiceLines.SelectMany(line => line.Charges.Cast<JobComInvCharge>().Union(line.ApportionedCharges))
			.Where(charge => ImportChargesProvider.IsDeductions(charge.J7_ChargeType) && charge.Money.Amount > 0).ToArray();

		#endregion

		public IEnumerable<IDeclarationMercosulDocument> DeclarationMercosulDocumentList => mercosulDocument ??
			(mercosulDocument = invoiceLines.SelectMany(x => x.MercosulForeignDeclarations.Cast<MercosulForeignDeclaration>())
			.Select(mercosulDoc => DeclarationMercosulDocumentProvider.New(mercosulDoc)).Distinct().ToArray());
		IDeclarationMercosulDocument[] mercosulDocument;

		public IDeclarationAddress SupplierAddress => fSupplierAddress ?? (fSupplierAddress = DeclarationAddressProvider.New(invoiceHeader?.SupplierAddress));
		IDeclarationAddress fSupplierAddress;

		public IDeclarationAddress ManufacturerAddress => fManufacturerAddress ?? (fManufacturerAddress = CreateManufacturerAddress());
		IDeclarationAddress fManufacturerAddress;

		IDeclarationAddress CreateManufacturerAddress()
		{
			return invoiceLine?.IsManufacturerAddressApplicable ?? false ? DeclarationAddressProvider.New(invoiceLine.ManufacturerAddress) : null;
		}

		public IEnumerable<IDeclarationAdditionTax> DeclarationAdditionTaxes => additionTaxes ??
			(additionTaxes = DeclarationAdditionTaxProvider.New(entryLine, ChargeTypesList.Codes.DTY, Constants.RateTypes.IPI, Constants.RateTypes.Antidumping, Constants.RateTypes.PIS, Constants.RateTypes.Cofins).ToArray());
		IEnumerable<IDeclarationAdditionTax> additionTaxes;

		public IEnumerable<IDeclarationAdditionLegalAct> DeclarationAdditionLegalActList => legalActList ??
			(legalActList = invoiceLine.AdditionalTariffs.Cast<AdditionalTariff>()
			.Select(addTariff => DeclarationAdditionLegalActProvider.New(addTariff))
			.Union(invoiceLine.LegalActInfos.Cast<LegalActInfo>().Where(legalAct => legalAct.CSI_SubType == AdditionalTaxTypeList.Codes.IPITaxBenefit || legalAct.CSI_SubType == AdditionalTaxTypeList.Codes.Antidumping)
			.Select(legalAct => DeclarationAdditionLegalActProvider.New(legalAct))).ToArray());
		IDeclarationAdditionLegalAct[] legalActList;

		public string ExchangeCover => invoiceHeader.ExchangeHedgeType;

		public string IncoTerm => invoiceHeader.JZ_IncoTerm;

		public string TariffCode => invoiceLine.JI_Tariff;

		public string NaladiNcca => invoiceLine.NaladiNcca;

		public string NaladiHs => invoiceLine.NaladiHs;

		public string ValuationCode => invoiceHeader.JZ_ValuationCode;

		public string GoodsOrigin => BRRefCusMapper.MapCW1CountryCodeToCustomsCode(entryLine.Factory, invoiceLine.JI_CountryOfOrigin);

		public string IncoTermPlace => invoiceHeader.JZ_IncoTermPlace;

		public string AdditionNumber => entryLine.CL_LineNumber.ToString();

		public decimal NetWeight => invoiceLines.Sum(netWeight => netWeight.JI_NetWeight);

		public decimal CustomsQty => invoiceLines.Sum(customsQty => customsQty.JI_CustomsQuantity);

		public decimal InvoiceQty => invoiceLines.Sum(invoiceQty => invoiceQty.JI_InvoiceQuantity);

		public decimal FOBValue => invoiceLines.Sum(fobValue => fobValue.JI_Calc_FOB);

		public decimal FOBValueInLocalCurrency => invoiceHeader.ConvertToLocalAmountRounded(FOBValue, invoiceHeader.CalcFOBCurrency).Amount;

		public decimal InvoiceAmount => invoiceLines.Sum(invoiceAmount => invoiceAmount.JI_Calc_InvAmount);

		public decimal InvoiceAmountInLocalCurrency => invoiceHeader.ConvertToLocalAmountRounded(InvoiceAmount, invoiceHeader.Invoice_Currency).Amount;

		public string ExchangeHedgeFinancialInstitution => invoiceHeader.ExchangeHedgeFinancialInstitution;

		public string ExchangeHedgeReason => invoiceHeader.ExchangeHedgeReason;

		public decimal? ExchangeHedgeValue => invoiceHeader.ExchangeHedgeValue.ReturnNullIfEmpty();

		public string ExchangeHedgeROFBACENNumber => invoiceHeader.ExchangeHedgeROFBACENNumber;

		public string DutyLegalBase => invoiceLine.DutyLegalBase;

		public string DutyTaxRegime => invoiceLine.DutyTaxRegime;

		public string AladiCode => IsPrimaryPreferenceFTA && IsFullReductionOrSuspension ? TariffAgreementCode?.GetAttribute(Constants.RefCusCodeList.Attributes.AgreementCodeInImportEntry).ToString() ?? string.Empty : null;

		public string ReasonForTemporaryAdmission => invoiceLine.JI_TemporaryAdmissionReason;

		public string TypeOfAgreement
		{
			get
			{
				if (IsPrimaryPreferenceFTA && IsFullReductionOrSuspension)
				{
					switch (TariffAgreementCode?.GetAttribute(Constants.RefCusCodeList.Attributes.Type))
					{
						case Constants.TariffAgreementTypes.Aladi:
							return "2";
						case Constants.TariffAgreementTypes.OMC:
							return "3";
						case Constants.TariffAgreementTypes.SGPC:
							return "4";
						default:
							return string.Empty;
					}
				}

				return null;
			}
		}

		ZZRefCusCodeListCombined TariffAgreementCode => fTariffAgreementCode ?? (fTariffAgreementCode = invoiceLine.AdditionalTariffs.FindBySubject(AdditionalTaxTypeList.Codes.TariffAgreement)?.TariffAgreementCode);
		ZZRefCusCodeListCombined fTariffAgreementCode;

		bool IsPrimaryPreferenceFTA => invoiceLine.JI_PrimaryPreference == Constants.RatePreferenceType.FreeTradeAgreement;

		bool IsFullReductionOrSuspension => new[] { TaxRegimeList.Codes.FullCollection, TaxRegimeList.Codes.Reduction, TaxRegimeList.Codes.Suspension }.Contains(DutyTaxRegime);

		public string GoodsApplication => invoiceLine.JI_GoodsApplication;

		public string ManufacturerIndicator => invoiceLine.JI_ManufacturerIndicator;

		public string GoodsUsedMaterial => MessageBuilderHelper.MapBooleanValue(invoiceLine.JI_GoodsCondition == GoodsConditionTypeList.Codes.UsedMaterial);

		public string GoodsMadeToOrder => MessageBuilderHelper.MapBooleanValue(invoiceLine.JI_GoodsCondition == GoodsConditionTypeList.Codes.GoodsMadeToOrder);

		public string InvoiceCurrencyCode => BRRefCusMapper.MapCW1CurrencyCodeToCustomsCode(invoiceHeader.Factory, invoiceHeader.JZ_RX_NKInvoice_Currency);

		public string BuySellerRelationship => RelatedIndicatorList.MapToCustomsCodeISW(invoiceHeader.JZ_RelatedIndicator);

		public string MercosulForeignDeclarationType => CertificateTypeList.MapToCustomsCode(invoiceLine.MercosulForeignDeclarationType);

		public string ImportLicenseNumber => invoiceLine.ImportLicenseNumber;

		public string PisCofinsTaxRegime => invoiceLine.PisCofinsTaxRegime;

		public string PisCofinsLegalBase => invoiceLine.PisCofinsLegalBase;

		public string CargoProvenance => MessageSubTypeList.CargoProvenanceIsAvailable(entryLine.Declaration.JE_MessageSubType) ? BRRefCusMapper.MapCW1CountryCodeToCustomsCode(entryLine.Factory, entryLine.Declaration.JE_GoodsOrigin).ToString() : null;
	}
}
