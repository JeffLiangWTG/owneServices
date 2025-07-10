using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag: Gen
	///</summary>
	public interface ICusProcedure
	{
		#region Procedure

		///<summary>
		/// Xml Tag: HorsValeur
		///</summary>
		IAlternateCalcValue AlternateCalcValue { get; }

		///<summary>
		/// Xml Tag: typeproc
		///</summary>
		ZString ProcedureType { get; }

		///<summary>
		/// Xml Tag: procedure1
		///</summary>
		ZString EntryStyle { get; }

		///<summary>
		/// Xml Tag: procedure2
		///</summary>
		ZString EntryStyleCode { get; }

		///<summary>
		/// Xml Tag: nbrart
		///</summary>
		ZShort ArticleCount { get; }

		#endregion

		#region Prevalidation

		///<summary>
		/// Xml Tag: datpreval
		///</summary>
		ZString EstimatedAssessmentDate { get; }

		///<summary>
		/// Xml Tag: heurpreval
		///</summary>
		ZString EstimatedAssessmentHour { get; }

		#endregion

		///<summary>
		/// Xml Tag: datdepot
		///</summary>
		ZString DeclEmergencyProcDate { get; }

		///<summary>
		/// Xml Tag: nbrcol
		///</summary>
		ZInt PackageCount { get; }

		///<summary>
		/// Xml Tag: locagr
		///</summary>
		ZString AgreedGoodsLocation { get; }

		///<summary>
		/// Xml Tag: magasin
		///</summary>
		ZString ClearanceLocation { get; }

		///<summary>
		/// Xml Tag: nattrans
		///</summary>
		ZString TransactionNature { get; }

		///<summary>
		/// Xml Tag: etatMembredestinationFinale
		///</summary>
		ZString ArrivalState { get; }

		///<summary>
		/// Xml Tag: etatMembredestinationFinale
		///</summary>
		ZString DepartureState { get; }

		///<summary>
		/// Xml Tag: Bureau
		///</summary>
		ICusOffice Office { get; }

		///<summary>
		/// Xml Tag: Operateur
		///</summary>
		#region ImportSupplier

		///<summary>
		/// Xml Tag: Expediteurs
		///</summary>
		IEnumerable<IOrganisation> Suppliers { get; }

		///<summary>
		/// Xml Tag: Destinataires
		///</summary>
		IEnumerable<IOrganisation> Importers { get; }

		///<summary>
		/// Xml Tag: Destinatairefinal
		///</summary>
		IOrganisation ImporterDeliveryAddress { get; }

		#region RepTax

		IOrganisation RepTaxOrganisation { get; }

		///<summary>
		/// Xml Tag: opedest
		///</summary>
		ZString ImporterEORINumber { get; }

		///<summary>
		/// Xml Tag: opeben
		///</summary>
		ZString AgreementOwnerEORI { get; }

		///<summary>
		/// Xml Tag: numagr
		///</summary>
		ZString DeltaGAuthorisationNumber { get; }

		///<summary>
		/// Xml Tag: operep
		///</summary>
		ZString BranchCusBrokerageCode { get; }

		///<summary>
		/// Xml Tag: modrep
		///</summary>
		ZString RepresentationModeCode { get; }

		///<summary>
		/// Xml Tag: numcre
		///</summary>
		ZString DeferalApprovalCreditNumber { get; }

		///<summary>
		/// Xml Tag: numcod
		///</summary>
		ZString VariousOperationCreditNumber { get; }

		///<summary>
		/// Xml Tag: prifac
		///</summary>
		ZDecimal EntryGoodsPriceSum { get; }

		///<summary>
		/// Xml Tag: devfac
		///</summary>
		ZString EntryGoodsPriceCurrency { get; }

		///<summary>
		/// Xml Tag: coursdevise
		///</summary>
		ZDecimal EntryGoodsPriceCurrencyRate { get; }

		///<summary>
		/// Xml Tag: modpaiement
		///</summary>
		ZString PaymentMode { get; }

		///<summary>
		/// Xml Tag: modgarantie
		///</summary>
		ZString GuaranteeMode { get; }

		#endregion

		#endregion

		///<summary>
		/// Xml Tag: ConditionsLivraison
		///</summary>
		IDeliveryTerms DeliveryTerms { get; }
		///<summary>
		/// Xml Tag: Transport
		///</summary>
		ITransport Transport { get; }

		#region ElementsOfValueGeneration

		///<summary>
		/// Xml Tag: CumulTiers
		///</summary>
		ICostsAndInsurance ThirdCountryTransportCosts { get; }

		///<summary>
		/// Xml Tag: CumulCEHorsFRInclus
		///</summary>
		ICostsAndInsurance EUTransportCostsInInvoice { get; }

		///<summary>
		/// Xml Tag: CumulCEHorsFRExclus
		///</summary>
		ICostsAndInsurance EUTransportCostsNotInInvoice { get; }

		///<summary>
		/// Xml Tag: CumulFRInclus
		///</summary>
		ICostsAndInsurance FRTransportCostsInInvoice { get; }

		///<summary>
		/// Xml Tag: CumulFRExclus
		///</summary>
		ICostsAndInsurance FRTransportCostsNotInInvoice { get; }

		///<summary>
		/// Xml Tag: CumulAerienTiers
		///</summary>
		ICostsAndInsurance ThirdCountryAirCosts { get; }

		///<summary>
		/// Xml Tag: CumulAerienFR
		///</summary>
		ICostsAndInsurance FRAirCosts { get; }

		///<summary>
		/// Xml Tag: AutresFraisAjout
		///</summary>
		IAmountAndCurrency OthAddedCosts { get; }

		#region OthDeducCosts

		///<summary>
		/// Xml Tag:Interets
		///</summary>
		IAmountAndCurrency Interest { get; }

		///<summary>
		/// Xml Tag:Commission
		///</summary>
		IAmountAndCurrency Commission { get; }

		///<summary>
		/// Xml Tag:FraisBaseTVA
		///</summary>
		IAmountAndCurrency VATBaseCosts { get; }

		#endregion

		///<summary>
		/// Xml Tag: FraisDom
		///</summary>
		ICostsAndInsurance DOMCostsAndInsurance { get; }

		#region Export

		IEnumerable<ZString> Itinerary { get; }

		ZString CommercialReference { get; }

		ZString SpecificCircumstanceIndicator { get; }

		ZString OrigineState { get; }

		ZString DestinationState { get; }

		#endregion

		#endregion

		/// <summary>
		/// Xml Tag: horsvaleur
		/// </summary>
		ZString ValuationBypassCode { get; }

		/// <summary>
		/// Xml Tag: motiv
		/// </summary>
		ZString ValuationBypassReason { get; }

		/// <summary>
		/// opedestfinalintracomm
		/// </summary>
		ZString VATOrganization { get; }
	}
}
