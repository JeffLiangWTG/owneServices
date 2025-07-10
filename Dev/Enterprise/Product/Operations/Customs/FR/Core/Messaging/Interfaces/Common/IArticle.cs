using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag: ArticleImport
	///</summary>
	public interface IArticle
	{
		#region Identification

		///<summary>
		/// Xml Tag: numart
		///</summary>	
		ZShort EntryNumber { get; }
		///<summary>
		/// Xml Tag: refLogistique
		///</summary>	
		ZString ShippingIdReference { get; }

		#endregion

		#region AlternateValue

		///<summary>
		/// Xml Tag: HorsTarif
		///</summary>	
		IAlternateCalcValue AlternateCalcValue { get; }

		///<summary>
		/// Xml Tag: nomenc
		///</summary>	
		ZString TariffCode { get; }
		///<summary>
		/// Xml Tag: observations
		///</summary>	
		ZString Observation { get; }

		#endregion

		#region TariffAdditional and SupplementaryUnit

		///<summary>
		/// Xml Tag: Cacos
		///</summary>	
		IEnumerable<ITariffAdditionalCode> CETariffAdditionalCodes { get; }
		///<summary>
		/// Xml Tag: Canas
		///</summary>	
		IEnumerable<ITariffAdditionalCode> FRTariffAdditionalCodes { get; }

		///<summary>
		/// Xml Tag:dispopart
		///</summary>
		IEnumerable<ITariffAdditionalCode> PartDispos { get; }

		///<summary>
		/// Xml Tag: Cacos for D2M message
		///</summary>	
		IEnumerable<ITariffAdditionalCode> SecondMessageCETariffAdditionalCodes { get; }
		///<summary>
		/// Xml Tag: Canas for D2M message
		///</summary>	
		IEnumerable<ITariffAdditionalCode> SecondMessageFRTariffAdditionalCodes { get; }

		///<summary>
		/// Xml Tag:dispopart for D2M message
		///</summary>
		IEnumerable<ITariffAdditionalCode> SecondMessagePartDispos { get; }

		#region EntryLine

		///<summary>
		/// Xml Tag:descom
		///</summary>
		ZString EntryLineDescription { get; }

		///<summary>
		/// Xml Tag:msb
		///</summary>
		ZDecimal GrossWeight { get; }

		///<summary>
		/// Xml Tag:msn
		///</summary>
		ZDecimal CustomsQuantity { get; }

		///<summary>
		/// Xml Tag:ori
		///</summary>
		ZString CountryGoodsOrigineCode { get; }

		///<summary>
		/// Xml Tag:pro
		///</summary>
		ZString CountryGoodsSupplyCode { get; }

		///<summary>
		/// Xml Tag:Cnts
		///</summary>
		IEnumerable<ZString> QuotaRefNumber { get; }

		///<summary>
		/// Xml Tag:valevaluation
		///</summary>
		ZString ValuationMethod { get; }

		#endregion

		///<summary>
		/// Xml Tag: UniSpe
		///</summary>	
		ISupplementaryUnit SuppUnit { get; }

		#endregion

		#region CusProcedure

		///<summary>
		/// Xml Tag: regdou
		///</summary>	
		ZString ProcedureCode { get; }
		///<summary>
		/// Xml Tag: regdoupre
		///</summary>	
		ZString PreviousCode { get; }
		///<summary>
		/// Xml Tag: compcom
		///</summary>	
		ZString Concession { get; }

		#endregion

		#region Warehouse

		///<summary>
		/// Xml Tag: enttyp
		///</summary>	
		ZString WarehouseType { get; }
		///<summary>
		/// Xml Tag: entref
		///</summary>	
		ZString WarehouseReference { get; }
		///<summary>
		/// Xml Tag: entpays
		///</summary>	
		ZString WarehouseCountryCode { get; }

		ZBool IsPlacingGoodsUnderBW { get; }

		#endregion

		#region EcoRegime and Preference

		ZBool HasSpecificRegimeAuthorisation { get; }

		///<summary>
		/// Xml Tag: AutorisationEco
		///</summary>	
		IEcoRegimeAuthorization EcoRegimeAuthorization { get; }

		///<summary>
		/// Xml Tag: RegimeEco
		///</summary>	
		IEcoRegimeDatas EcoRegimeDatas { get; }
		///<summary>
		/// Xml Tag: Preference
		///</summary>	
		IPreference Preference { get; }

		#endregion

		#region Container and packing

		///<summary>
		/// Xml Tag: conteneur
		///</summary>	
		IEnumerable<ZString> Containers { get; }
		///<summary>
		/// Xml Tag: Colisage
		///</summary>	
		IPacking Packing { get; }

		#endregion

		#region PreviousDocument,SpecMens,SupportingDocuments

		///<summary>
		/// Xml Tag: PriseEnCharge
		///</summary>	
		ISupportingDocument PreviousDocument { get; }
		///<summary>
		/// Xml Tag: Menspecs
		///</summary>
		///
		IEnumerable<ITariffAdditionalCode> SpecMens { get; }

		///<summary>
		/// Xml Tag: Documents
		///</summary>	
		IEnumerable<ISupportingDocumentOnly> SupportingDocuments { get; }

		///<summary>
		/// Xml Tag: Documents for D2M message
		///</summary>	
		IEnumerable<ISupportingDocumentOnly> SecondMessageSupportingDocuments { get; }

		#endregion

		#region FinancialDatasImport

		///<summary>
		/// Xml Tag:prifac
		///</summary>
		ZDecimal InvoiceLinePrice { get; }
		///<summary>
		/// Xml Tag:devfac
		///</summary>
		ZString CurrencyCode { get; }
		///<summary>
		/// Xml Tag:coursdevise
		///</summary>
		ZDecimal ExchangeRate { get; }
		///<summary>
		/// Xml Tag:prixcif
		///</summary>
		ZDecimal CIFPrice { get; }
		///<summary>
		/// Xml Tag:applicif
		///</summary>
		ZBool CIFApplicationFlag { get; }
		///<summary>
		/// Xml Tag:valstat
		///</summary>
		ZDecimal StatisticalAmount { get; }
		///<summary>
		/// Xml Tag:valdou
		///</summary>
		ZDecimal CustomsValue { get; }
		///<summary>
		/// Xml Tag:asstva
		///</summary>
		ZDecimal TVAAssessedAmount { get; }
		///<summary>
		/// Xml Tag: depliv
		///</summary>	
		ZString DeliveryDepartment { get; }
		///<summary>
		/// Xml Tag: depexp
		///</summary>	
		ZString ExpeditionDepartment { get; }
		///<summary>
		/// Xml Tag: ajuval
		///</summary>	
		ZDecimal ValuationAdjustPercent { get; }

		ZBool ShouldSendCustomsStatisticAndVatValues { get; }
		#endregion

		#region ArticleAddCosts

		///<summary>
		/// Xml Tag: FraisEmballage
		///</summary>	
		IAmountAndCurrency PackingCosts { get; }

		///<summary>
		/// Xml Tag: Commission
		///</summary>	
		IAmountAndCurrency Commission { get; }
		///<summary>
		/// Xml Tag: Redevance
		///</summary>	
		IAmountAndCurrency Fee { get; }
		///<summary>
		/// Xml Tag: Revente
		///</summary>	
		IAmountAndCurrency Resale { get; }
		///<summary>
		/// Xml Tag: FraisAccessoire
		///</summary>	
		IAmountAndCurrency OthCosts { get; }

		#endregion

		#region ArticleDeductCosts

		///<summary>
		/// Xml Tag: FraisMontage
		///</summary>	
		IAmountAndCurrency AssemblyCosts { get; }
		///<summary>
		/// Xml Tag: FraisDouane
		///</summary>	
		IAmountAndCurrency CustomsCosts { get; }

		#endregion

		#region PreCalcEntryLine

		///<summary>
		/// Xml Tag: LignesPrecalcs
		///</summary>	
		IEnumerable<IPreCalcEntryLine> PreCalcEntryLines { get; }
		///<summary>
		/// Xml Tag: TaxSpes
		///</summary>	
		ISupplementaryUnit ThirdUnit { get; }

		#endregion

		#region Entry header charges

		///<summary>
		/// Xml Tag: goes into LignesPrecalcs on article line 1 only
		///</summary>
		IEnumerable<ITax> EntryHeaderCharges { get; }

		#endregion

		#region PAC

		///<summary>
		/// Xml Tag: certifcontingent
		///</summary>	
		ZString CusImportCertification { get; }
		///<summary>
		/// Xml Tag: certifdroitcommun
		///</summary>	
		ZString CEQuotaCertification { get; }
		///<summary>
		/// Xml Tag: sucretaux1
		///</summary>	
		ZDecimal SugarRate1 { get; }
		///<summary>
		/// Xml Tag: sucretaux2
		///</summary>	
		ZDecimal SugarRate2 { get; }
		///<summary>
		/// Xml Tag: sucretaux3
		///</summary>	
		ZDecimal SugarRate3 { get; }
		///<summary>
		/// Xml Tag: sucrePolarisation
		///</summary>	
		ZDecimal SugarPolarisation { get; }
		///<summary>
		/// Xml Tag: infospac
		///</summary>	
		ZString PACInformations { get; }

		#endregion

		#region ApplicantSpecificsInfo and SpecificInfos

		///<summary>
		/// Xml Tag: demandeur
		///</summary>	
		IOrganisation Applicant { get; }
		///<summary>
		/// Xml Tag: natperf
		///</summary>	
		ZString ApplicantInwardNature { get; }
		///<summary>
		/// Xml Tag: description
		///</summary>	
		ZString ApplicantDescription { get; }
		///<summary>
		/// Xml Tag: conditions
		///</summary>	
		ZString ApplicantConditions { get; }
		///<summary>
		/// Xml Tag: burapur
		///</summary>	
		ZString ApplicantPurOffice { get; }
		///<summary>
		/// Xml Tag: lieuperf
		///</summary>	
		ZString ApplicantInwardLocation { get; }
		///<summary>
		/// Xml Tag: formalitetransf
		///</summary>	
		ZString ApplicantTransFormality { get; }

		///<summary>
		/// Xml Tag: Infosspec
		///</summary>	
		ZString SpecificInfos { get; }

		#endregion

		#region Export

		sbyte Seals { get; }

		IEnumerable<ZString> SealIds { get; }

		ZString TransportMethodPayment { get; }

		IEnumerable<ZString> DangerousGoodsDeltas { get; }

		ZString PACCode { get; }

		ZDecimal Restitution { get; }

		ZString ExportCertification1 { get; }

		ZString ExportCertification2 { get; }

		ZString RestitutionMention { get; }

		ZString Recipient { get; }

		ZString InvariantMention { get; }

		ZString VariantMention { get; }

		ZDecimal VariantRate { get; }

		ZDecimal LoadingStartDate { get; }

		ZDecimal LoadingStartHour { get; }

		ZDecimal LoadingEndDate { get; }

		ZDecimal LoadingEndHour { get; }
		#endregion
	}
}
