using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;

namespace Enterprise.Accounting.Integration
{
	public class AutoRatingProxy :
		AutoRatingProxyBase,
		IAutoRatingGlbCompany,
		IAutoRatingStandardFreightCostProvider,
		IAutoRatingDescriptionMacroExpander,
		IAutoRatingShipmentConsolidationStatus,
		IJobDataUpdater,
		IAutoRatingSpotChargeInfo
	{
		public AutoRatingProxy(IAutoRating autoRating)
			: base(autoRating,
			autoRating as IAutoRatingAccountingInfo,
			autoRating as IAutoRatingCustomsInfo,
			autoRating as IAutoRatingPackageTypeInfo,
			autoRating as IAutoRatingWeightBreakOverrideProvider,
			autoRating as IAutoRatingFreightConditionsSupportable,
			autoRating as IAutoRatingChargeApplicabilityDecider,
			autoRating as ISpotRate,
			autoRating as IGateway,
			autoRating as IAutoRatingWarehouseInfo,
			autoRating as IManualRateSelectionSupporter)
		{
		}

		#region IAutoRating.JobDatesProvider

		bool IsJobDatesProviderSet;
		IJobDatesProvider fJobDatesProvider;
		public override IJobDatesProvider JobDatesProvider
		{
			get
			{
				if (!IsJobDatesProviderSet && AutoRating != null)
				{
					fJobDatesProvider = AutoRating.JobDatesProvider;
					IsJobDatesProviderSet = true;
				}
				return fJobDatesProvider;
			}
			set
			{
				if (ValuesCanBeSet)
				{
					fJobDatesProvider = value;
					IsJobDatesProviderSet = true;
				}
				else
				{
					throw new NotSupportedException("You cannot set values on this object unless ValuesCanBeSet is true.");
				}
			}
		}

		public void ResetJobDatesProvider()
		{
			IsJobDatesProviderSet = false;
		}

		#endregion

		#region IsApplicableToPaymentTerm

		public override bool IsApplicableToPaymentTermFiltering(string chargeCodeGroup, CostSell costOrSell)
		{
			return base.AutoRating == null || base.IsApplicableToPaymentTermFiltering(chargeCodeGroup, costOrSell);
		}

		#endregion

		#region Host

		public new IAutoRating AutoRating
		{
			get { return base.AutoRating; }
		}

		#endregion

		#region IAutoRatingGlbCompany

		public virtual GlbCompany Company
		{
			get
			{
				if (fCompany == null)
				{
					IAutoRatingGlbCompany autoRatingGlbCompany = AutoRating as IAutoRatingGlbCompany;
					fCompany = autoRatingGlbCompany != null ? autoRatingGlbCompany.Company : GlbCompany.CurrentCompany;
				}

				return fCompany;
			}
#if DEBUG
			set { }
#endif
		}

		GlbCompany fCompany;

		#endregion

		#region IAutoRatingCustomsInfo

		public override EntryInfoCollection Entries
		{
			get
			{
				return base.Entries ?? new EntryInfoCollection();
			}
		}

		public override InvoiceInfoCollection Invoices
		{
			get
			{
				return base.Invoices ?? new InvoiceInfoCollection();
			}
		}

		public override InvoiceInfoCollection TariffsPerInvoice
		{
			get
			{
				return base.TariffsPerInvoice ?? new InvoiceInfoCollection();
			}
		}

		public override InvoiceInfoCollection TariffsPerShipment
		{
			get
			{
				return base.TariffsPerShipment ?? new InvoiceInfoCollection();
			}
		}

		#endregion

		#region JobNumber

		public ZString JobNumber
		{
			get
			{
				var jobNumber = InvoicingSupporter?.JobNumber;

				if (string.IsNullOrWhiteSpace(jobNumber))
				{
					jobNumber = InvoicingSupporter?.Job?.Parent?.JobNumber;
				}

				return jobNumber;
			}
		}

		#endregion

		#region TariffLevel

		public virtual int TariffLevel
		{
			get
			{
				if (AutoRating is AutoRatingProxy)
				{
					return ((AutoRatingProxy)AutoRating).TariffLevel;
				}
				else if (AutoRating is IAutoRatingCompanyTariffLevelProvider)
				{
					return ((IAutoRatingCompanyTariffLevelProvider)AutoRating).TariffLevel;
				}
				else
				{
					return ZInt.Zero;
				}
			}
		}

		#endregion

		#region PaymentTerm

		public override PaymentTermInfos PaymentTerm
		{
			get { return fPaymentTerm ?? (fPaymentTerm = base.PaymentTerm); }
			set
			{
				base.PaymentTerm = value;
				fPaymentTerm = value;
			}
		}

		PaymentTermInfos fPaymentTerm;

		#endregion

		#region IAutoRatingStandardFreightCostProvider

		public IAutoRatingStandardFreightCost StandardFreightCost
		{
			get
			{
				IAutoRatingStandardFreightCost result = AutoRating as IAutoRatingStandardFreightCost;
				if (result == null && AutoRating is IAutoRatingStandardFreightCostProvider)
				{
					result = ((IAutoRatingStandardFreightCostProvider)AutoRating).StandardFreightCost;
				}

				return result;
			}
		}

		#endregion

		#region IAutoRatingDescriptionMacroExpander Members

		public virtual bool CanExpandMacros
		{
			get { return Expander != null && Expander.CanExpandMacros; }
		}

		public virtual string ExpandMacro(string macro)
		{
			return Expander == null ? null : Expander.ExpandMacro(macro);
		}

		IAutoRatingDescriptionMacroExpander Expander
		{
			get
			{
				if (!expanderSet)
				{
					expander = AutoRating as IAutoRatingDescriptionMacroExpander;
					expanderSet = true;
				}

				return expander;
			}
		}

		bool expanderSet;
		IAutoRatingDescriptionMacroExpander expander;

		#endregion

		#region IAutoRatingChargeApplicabilityDecider

		public override bool ShouldRemoveCharge(AccChargeCode chargeCode)
		{
			return AutoRatingChargeApplicabilityDecider != null && base.ShouldRemoveCharge(chargeCode);
		}

		#endregion

		#region IAutoRatingShipmentConsolidationStatus

		public virtual ZString ShipmentConsolidationStatus =>
			AutoRatingShipmentConsolidationStatus?.ShipmentConsolidationStatus ?? ZString.Empty;

		IAutoRatingShipmentConsolidationStatus AutoRatingShipmentConsolidationStatus =>
			AutoRating as IAutoRatingShipmentConsolidationStatus;

		#endregion

		#region IGateway

		public override bool IsIntercompanyTariffApplicable(BillingType billingType, CostSell costSell)
			=> Gateway?.IsIntercompanyTariffApplicable(billingType, costSell) ?? false;

		public override ZBool IsGatewaySellApplicableToGatewayConsol(CostSell costSell)
			=> Gateway?.IsGatewaySellApplicableToGatewayConsol(costSell) ?? true;

		public override ZBool IsContainerNegotiatedCostApplicable(CostSell costSell)
			=> Gateway?.IsContainerNegotiatedCostApplicable(costSell) ?? true;

		public override bool ContinueWithDefaultCosting(BillingType billingType)
			=> Gateway?.ContinueWithDefaultCosting(billingType) ?? true;

		public override ZString GatewayAgentTypeFilteredReason(string agentType, ZGuid gatewayAgentPk, BillingType billingType, CostSell costSell)
			=> Gateway?.GatewayAgentTypeFilteredReason(agentType, gatewayAgentPk, billingType, costSell) ?? ZString.Empty;

		public override ZString GatewayServiceLevelFilteredReason(ZString gatewayServiceLevel, ZGuid gatewayAgentPk)
			=> Gateway?.GatewayServiceLevelFilteredReason(gatewayServiceLevel, gatewayAgentPk) ?? ZString.Empty;

		public override ZBool ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType billingType)
			=> Gateway?.ShouldRemoveNonIntercompanyTariffFRTEntries(billingType) ?? false;

		public override List<ILocation> SortedOverridenPlannedLoad => Gateway?.SortedOverridenPlannedLoad ?? new List<ILocation>();

		public override List<ILocation> SortedOverridenPlannedDischarge => Gateway?.SortedOverridenPlannedDischarge ?? new List<ILocation>();

		public override List<ZGuid> SortedGatewayAgentPKs
		{
			get => base.SortedGatewayAgentPKs ?? new List<ZGuid>();
			set => base.SortedGatewayAgentPKs = value;
		}

		public override List<ZGuid> GatewayAgentPKsForIntercompanyTariff
		{
			get => base.GatewayAgentPKsForIntercompanyTariff ?? new List<ZGuid>();
			set => base.GatewayAgentPKsForIntercompanyTariff = value;
		}

		public override IDictionary<ZString, IList<ZGuid>> LoginGatewayAgentRoles
		{
			get => base.LoginGatewayAgentRoles ?? new Dictionary<ZString, IList<ZGuid>>();
			set => base.LoginGatewayAgentRoles = value;
		}

		public override List<ZGuid> SortedControllingCustomerPKs
		{
			get => base.SortedControllingCustomerPKs ?? new List<ZGuid>();
			set => base.SortedControllingCustomerPKs = value;
		}

		#endregion

		#region IJobDataUpdater

		public virtual bool CanUpdateDate => false;

		public virtual void UpdateServiceLevel(ZString newServiceLevel)
		{
			JobDataUpdater?.UpdateServiceLevel(newServiceLevel);

			// To overcome caching on AutoRatingProxy
			ValuesCanBeSet = true;
			ServiceLevel = AutoRating.ServiceLevel;
		}

		public virtual void UpdateCarrier(OrgHeader newCarrier)
		{
			JobDataUpdater?.UpdateCarrier(newCarrier);

			ValuesCanBeSet = true;

			var oldSourceForCarrier = Creditors.AllOrgsWithSource
				.FirstOrDefault(x => x.Org == Carrier);
			if (oldSourceForCarrier != null)
			{
				var newSourceForCarrier = new[] { OrgWithSource.New(newCarrier, oldSourceForCarrier.Source) };
				foreach (var group in Creditors.ChargeCodeGroups.ToArray())
				{
					var replacementSources = Creditors[group]
						.Where(x => x.Org != Carrier)
						.Union(newSourceForCarrier);

					Creditors[group] = new OrgPrioritizedList(replacementSources);
				}
			}

			Carrier = AutoRating.Carrier;
		}

		public virtual bool UpdateCarrierConfirmationIsNeeded(string newServiceProvider, out string confirmationMessage)
		{
			confirmationMessage = string.Empty;
			return JobDataUpdater?.UpdateCarrierConfirmationIsNeeded(newServiceProvider, out confirmationMessage) ?? false;
		}

		public virtual void UpdateOrigin(ZString newOrigin)
		{
			JobDataUpdater?.UpdateOrigin(newOrigin);

			ValuesCanBeSet = true;
			Origin = AutoRating.Origin;
		}

		public virtual bool UpdateOriginConfirmationIsNeeded(string newOrigin, out string confirmationMessage)
		{
			confirmationMessage = string.Empty;
			return JobDataUpdater?.UpdateOriginConfirmationIsNeeded(newOrigin, out confirmationMessage) ?? false;
		}

		public virtual void UpdateDestination(ZString newDestination)
		{
			JobDataUpdater?.UpdateDestination(newDestination);

			ValuesCanBeSet = true;
			Destination = AutoRating.Destination;
		}

		public virtual bool UpdateDestinationConfirmationIsNeeded(string newDestination, out string confirmationMessage)
		{
			confirmationMessage = string.Empty;
			return JobDataUpdater?.UpdateDestinationConfirmationIsNeeded(newDestination, out confirmationMessage) ?? false;
		}

		public virtual void UpdatePaymentTerms(ZString newPaymentTerms)
		{
			JobDataUpdater?.UpdatePaymentTerms(newPaymentTerms);

			ValuesCanBeSet = true;
			PaymentTerm = AutoRating.PaymentTerm;
		}

		public virtual void UpdateNamedAccount(ZString namedAccount)
		{
			JobDataUpdater?.UpdateNamedAccount(namedAccount);

			ValuesCanBeSet = true;
			NamedAccount = namedAccount;
		}

		public virtual void UpdateCarrierQuoteNumber(ZString carrierQuoteNumber)
		{
			JobDataUpdater?.UpdateCarrierQuoteNumber(carrierQuoteNumber);
		}

		public virtual void UpdateRateCommodityCodeAndFMCTariffID(ZString newRateCommodityCode, ZString newFMCTariffID) => JobDataUpdater?.UpdateRateCommodityCodeAndFMCTariffID(newRateCommodityCode, newFMCTariffID);
		public virtual void UpdateDetailedGoodsDescription(ZString newDetailedGoodDescription, bool append = false) => JobDataUpdater?.UpdateDetailedGoodsDescription(newDetailedGoodDescription, append);

		public virtual void UpdateContainersCarrierQuoteNumber(ZGuid containerRefPK, ZString carrierQuoteNumber)
		{
			JobDataUpdater?.UpdateContainersCarrierQuoteNumber(containerRefPK, carrierQuoteNumber);
		}

		public virtual void SendBookingInformationToCarrier()
		{
			JobDataUpdater?.SendBookingInformationToCarrier();
		}

		public virtual void UpdateSpotBookingTerms(ZString termsAsText)
		{
			JobDataUpdater?.UpdateSpotBookingTerms(termsAsText);
		}

		public virtual bool UpdateContainerPenaltiesConfirmationIsNeeded(IEnumerable<IContainerPenalty> newContainerPenalties, out string confirmationMessage)
		{
			confirmationMessage = string.Empty;
			return JobDataUpdater?.UpdateContainerPenaltiesConfirmationIsNeeded(newContainerPenalties, out confirmationMessage) ?? false;
		}

		public void UpdateContainerPenalties(IEnumerable<IContainerPenalty> containerPenalties, bool deleteExistingDuplicates)
		{
			JobDataUpdater?.UpdateContainerPenalties(containerPenalties, deleteExistingDuplicates);
		}

		public virtual CanUpdateCarrierContractNumberResult CanUpdateCarrierContractNumber(IEnumerable<string> contractNumbers, IDialogService dialogService = null, bool isManualCostSelected = false)
		{
			return JobDataUpdater?.CanUpdateCarrierContractNumber(contractNumbers, dialogService, isManualCostSelected)
					?? new CanUpdateCarrierContractNumberResult { CanUpdate = false };
		}

		public bool IsMultipleCarrierContractNumberSupported => JobDataUpdater?.IsMultipleCarrierContractNumberSupported ?? true;

		public virtual DataUpdateResult UpdateCarrierContractNumber(UpdateCarrierContractNumberToken token)
		{
			var result = JobDataUpdater?.UpdateCarrierContractNumber(token) ?? DataUpdateResult.NoAction;
			if ((int)result > 0 && AutoRating != null)
			{
				ValuesCanBeSet = true;
				CarrierContractNumbers = AutoRating.CarrierContractNumbers;
			}
			return result;
		}

		public bool IsMultipleClientContractNumberSupported => JobDataUpdater?.IsMultipleClientContractNumberSupported ?? true;

		public virtual DataUpdateResult UpdateClientContractNumber(IEnumerable<string> newNumbers)
		{
			var result = JobDataUpdater?.UpdateClientContractNumber(newNumbers) ?? DataUpdateResult.NoAction;

			if ((int)result > 0 && AutoRating != null)
			{
				ValuesCanBeSet = true;
				ClientContractNumbers = AutoRating.ClientContractNumbers;
			}

			return result;
		}

		public override IEnumerable<ZString> CarrierContractNumbers =>
			base.CarrierContractNumbers ?? Enumerable.Empty<ZString>();

		public override IEnumerable<ZString> ClientContractNumbers =>
			base.ClientContractNumbers ?? Enumerable.Empty<ZString>();

		public virtual void UpdateTransports(IEnumerable<ITransport> transports)
		{
			JobDataUpdater?.UpdateTransports(transports);
		}

		public virtual void UpdateChargeable(ZDecimal newChargeable)
		{
			JobDataUpdater?.UpdateChargeable(newChargeable);
		}

		public virtual void UpdateAutoratingDate(ZDate autoratingDate, bool isCosting)
		{
			JobDataUpdater?.UpdateAutoratingDate(autoratingDate, isCosting);
		}

		IJobDataUpdater JobDataUpdater => AutoRating as IJobDataUpdater;

		#endregion

		#region IManualRateSelectionSupporter

		public override bool SupportsManualRateSelection { get => ManualRateSelectionSupporter?.SupportsManualRateSelection ?? false; }

		public override PaymentTermInfos DefaultFilterValueForPaymentTerm { get => ManualRateSelectionSupporter?.DefaultFilterValueForPaymentTerm ?? PaymentTerm; }

		public override ILocation DefaultFilterValueForOrigin { get => ManualRateSelectionSupporter?.DefaultFilterValueForOrigin; }

		public override ILocation DefaultFilterValueForDestination { get => ManualRateSelectionSupporter?.DefaultFilterValueForDestination; }

		public override bool ContinueAutoratingWithoutRateSelector { get => ManualRateSelectionSupporter?.ContinueAutoratingWithoutRateSelector ?? false; }

		public override string OriginMissingMessage
		{
			get => ManualRateSelectionSupporter?.OriginMissingMessage ??
				ResString.GetMultilingualString("135ECB85-4309-4197-8D68-0E6EFE70C487", "Origin is mandatory for running Autorating Costs");
		}

		public override string DestinationMissingMessage
		{
			get => ManualRateSelectionSupporter?.DestinationMissingMessage ??
				ResString.GetMultilingualString("58348414-1A79-4B61-A71E-308E34CF1A98", "Destination is mandatory for running Autorating Costs");
		}

		#endregion

		#region IAutoRatingSpotChargeInfo
		public virtual IDictionary<string, IEnumerable<string>> GetJobSpotCharges()
		{
			return AutoRatingSpotChargeInfo?.GetJobSpotCharges();
		}

		IAutoRatingSpotChargeInfo AutoRatingSpotChargeInfo => AutoRating as IAutoRatingSpotChargeInfo;
		#endregion
	}
}
