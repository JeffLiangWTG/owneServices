using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsConfiguration
	{
		public static NctsConfiguration GetConfiguration(BusinessObjectFactory factory, string countryOrGrouping)
		{
			return factory.GetCachedValue(FormattableString.Invariant($"NctsConfiguration_{countryOrGrouping}"), () =>
			{
				object supporter = null;
				var builders = ObjectFactory.Get<Hashtable>("NCTS.NctsConfiguration");
				if (!string.IsNullOrEmpty(countryOrGrouping))
				{
					var objectHandle = (ObjectHandle)builders[countryOrGrouping];
					supporter = objectHandle?.GetObject();
				}
				if (supporter == null)
				{
					var objectHandle = (ObjectHandle)builders[Core.Constants.CountryCodes.EuropeanUnion];
					supporter = objectHandle.GetObject();
				}
				return (NctsConfiguration)supporter;
			});
		}

		public INctsHeaderValidationDecider GetValidationDecider(NctsHeader header) => GetValidationDeciderCore(header);

		protected virtual INctsHeaderValidationDecider GetValidationDeciderCore(NctsHeader header) => header switch
		{
			{ IsPhase5Departure: true } => GetHeaderDeparturePhase5ValidationDecider(),
			{ IsPhase5Arrival: true } => GetHeaderArrivalPhase5ValidationDecider(),
			_ => null,
		};

		public ZBool AllowMixedCaseAuthorisationNumbers => AllowMixedCaseAuthorisationNumbersCore;

		protected virtual ZBool AllowMixedCaseAuthorisationNumbersCore => false;

		protected virtual INctsHeaderDeparturePhase5ValidationDecider GetHeaderDeparturePhase5ValidationDecider() => new NctsHeaderDeparturePhase5ValidationDecider();

		protected virtual INctsHeaderArrivalPhase5ValidationDecider GetHeaderArrivalPhase5ValidationDecider() => new NctsHeaderArrivalPhase5ValidationDecider();

		public MovementHeaderConfiguration MovementHeaderConfiguration => movementHeaderConfiguration ?? (movementHeaderConfiguration = GetNewMovementHeaderConfiguration());
		MovementHeaderConfiguration movementHeaderConfiguration;

		protected virtual MovementHeaderConfiguration GetNewMovementHeaderConfiguration() => new MovementHeaderConfiguration();

		public ArrivalCusTransportMeansConfiguration ArrivalCusTransportMeansConfiguration() => GetNewArrivalCusTransportMeansConfiguration();

		protected virtual ArrivalCusTransportMeansConfiguration GetNewArrivalCusTransportMeansConfiguration() => new ArrivalCusTransportMeansConfiguration();

		public NctsOrderInventorySelectionHeader GetNewOrderInventorySelectionHeader(NctsBill parent) => GetNewOrderInventorySelectionHeaderCore(parent);

		protected virtual NctsOrderInventorySelectionHeader GetNewOrderInventorySelectionHeaderCore(NctsBill parent) => new(parent);

		public NctsInventorySelectionHeader GetNewInventorySelectionHeader(NctsBill parent) => GetNewInventorySelectionHeaderCore(parent);

		protected virtual NctsInventorySelectionHeader GetNewInventorySelectionHeaderCore(NctsBill parent) => new(parent);

		public bool IsImportBondedWarehouseOrderAvailable => CachedValueHelper.GetValue(ref isImportBondedWarehouseOrderAvailable, GetImportBondedWarehouseOrderAvailable);
		protected virtual bool GetImportBondedWarehouseOrderAvailable() => true;

		CachedValue<bool> isImportBondedWarehouseOrderAvailable;

		public virtual ZBool MiscAdditionalInfosSupport(BusinessObject businessObject) => true;
		public virtual ZBool MiscSupportingDocumentsSupport(BusinessObject businessObject) => true;
		public virtual ZBool MiscPreviousDocumentsSupport(BusinessObject businessObject) => true;
		public virtual ZBool MiscGuaranteesSupport(BusinessObject businessObject) => false;
		public virtual ZBool UseUniversalFeeCalculation => false;
		public virtual ZBool ReceiveIE043UnloadingPermissionDetailsMessage => true;

		public ZBool UseAdditionalDeclarationType => UseAdditionalDeclarationTypeCore;
		protected virtual ZBool UseAdditionalDeclarationTypeCore => false;

		public ZBool UsePresentationDateTime => UsePresentationDateTimeCore;
		protected virtual ZBool UsePresentationDateTimeCore => false;

		public ZBool UseCompanyOrgProxyFallBack => UseCompanyOrgProxyFallBackCore;
		protected virtual ZBool UseCompanyOrgProxyFallBackCore => false;

		public ZBool UseDeclarantFallBack => UseDeclarantFallBackCore;
		protected virtual ZBool UseDeclarantFallBackCore => false;

		public ZBool UseBranchOrgProxyFallBack => UseBranchOrgProxyFallBackCore;
		protected virtual ZBool UseBranchOrgProxyFallBackCore => false;

		public ZBool UseGuaranteeGridValidation => UseGuaranteeGridValidationCore;
		protected virtual ZBool UseGuaranteeGridValidationCore => false;

		public ZBool ClearExistingGuaranteesConfiguration => ClearExistingGuaranteesConfigurationCore;
		protected virtual ZBool ClearExistingGuaranteesConfigurationCore => false;

		public ZBool DocDataPlugInSupport(NctsHeader nctsHeader)
		{
			return nctsHeader != null
				&& DocDataPlugInSupportCore(nctsHeader);
		}

		ZBool DocDataPlugInSupportCore(NctsHeader nctsHeader) => DocDataPlugInSupportForDepartureMovement && nctsHeader.IsDepartureMovement;

		protected virtual ZBool DocDataPlugInSupportForDepartureMovement => false;

		public ZBool FullLoadPortSupport => FullLoadPortSupportCore;

		protected virtual ZBool FullLoadPortSupportCore => false;

		public bool IsMultipleMovementsEnabled => CachedValueHelper.GetValue(ref enableMultipleMovements, GetIsMultipleMovementsEnabled);

		protected virtual bool GetIsMultipleMovementsEnabled() => NctsCustomsDataRegistry.Instance.EnableMultipleMovements.Value;

		CachedValue<bool> enableMultipleMovements;

		public ZBool MiscTabPageSupport(NctsHeader nctsHeader)
		{
			return nctsHeader != null
				&& MiscTabPageSupportCore(nctsHeader);
		}

		protected virtual ZBool MiscTabPageSupportCore(NctsHeader nctsHeader) => false;

		public ZString GetDefaultMessageStatusForArrival(NctsHeader nctsHeader) => GetDefaultMessageStatusForArrivalCore(nctsHeader);

		protected virtual ZString GetDefaultMessageStatusForArrivalCore(NctsHeader nctsHeader) => nctsHeader.IsPhase5 ? NctsMessageStatusList.Codes.Unknown : NctsMessageStatusList.Codes.ArrivalNotificationNotSent;

		protected virtual GoodsItemsConfiguration GetNewGoodsItemsConfiguration() => new GoodsItemsConfiguration();

		public GoodsItemsConfiguration GoodsItemsConfiguration => goodsItemsConfiguration ?? (goodsItemsConfiguration = GetNewGoodsItemsConfiguration());
		GoodsItemsConfiguration goodsItemsConfiguration;

		public GuaranteeConfiguration GuaranteeConfiguration => guaranteeConfiguration ?? (guaranteeConfiguration = GetNewGuaranteeConfiguration());
		GuaranteeConfiguration guaranteeConfiguration;

		protected virtual GuaranteeConfiguration GetNewGuaranteeConfiguration() => new GuaranteeConfiguration();

		public MessageSendingConfiguration MessageSendingConfiguration => messageSendingConfiguration ?? (messageSendingConfiguration = GetNewMessageSendingConfiguration());
		MessageSendingConfiguration messageSendingConfiguration;
		protected virtual MessageSendingConfiguration GetNewMessageSendingConfiguration() => new MessageSendingConfiguration();

		public ValidationRuleConfiguration ValidationRuleConfiguration => validationRuleConfiguration ?? (validationRuleConfiguration = GetNewValidationRuleConfiguration());
		ValidationRuleConfiguration validationRuleConfiguration;
		protected virtual ValidationRuleConfiguration GetNewValidationRuleConfiguration() => new ValidationRuleConfiguration();

		protected virtual NctsEuOfficeCodeConfiguration GetNewNctsEuOfficeCodeConfiguration() => new NctsEuOfficeCodeConfiguration();

		public NctsEuOfficeCodeConfiguration NctsEuOfficeCodeConfiguration => nctsEuOfficeCodeConfiguration ?? (nctsEuOfficeCodeConfiguration = GetNewNctsEuOfficeCodeConfiguration());
		NctsEuOfficeCodeConfiguration nctsEuOfficeCodeConfiguration;

		protected virtual NctsPackageConfiguration GetNewNctsPackageConfiguration() => new NctsPackageConfiguration();

		public NctsPackageConfiguration NctsPackageConfiguration => nctsPackageConfiguration ?? (nctsPackageConfiguration = GetNewNctsPackageConfiguration());
		NctsPackageConfiguration nctsPackageConfiguration;

		public CountryOfRoutingConfiguration CountryOfRoutingConfiguration => countryOfRoutingConfiguration ?? (countryOfRoutingConfiguration = GetNewCountryOfRoutingConfiguration());
		CountryOfRoutingConfiguration countryOfRoutingConfiguration;

		protected virtual CountryOfRoutingConfiguration GetNewCountryOfRoutingConfiguration() => new CountryOfRoutingConfiguration();

		public BillConfiguration BillConfiguration => billConfiguration ?? (billConfiguration = GetNewBillConfiguration());
		BillConfiguration billConfiguration;

		protected virtual BillConfiguration GetNewBillConfiguration() => new BillConfiguration();

		public CommonPreviousDocumentConfiguration CommonPreviousDocumentConfiguration => commonPreviousDocumentConfiguration ??= GetNewCommonPreviousDocumentConfiguration();
		CommonPreviousDocumentConfiguration commonPreviousDocumentConfiguration;

		protected virtual CommonPreviousDocumentConfiguration GetNewCommonPreviousDocumentConfiguration() => new();

		public CusSealConfiguration CusSealConfiguration => cusSealConfiguration ?? (cusSealConfiguration = GetNewCusSealConfiguration());
		CusSealConfiguration cusSealConfiguration;

		protected virtual CusSealConfiguration GetNewCusSealConfiguration() => new CusSealConfiguration();

		public CusSupplyChainActorReferenceConfiguration CusSupplyChainActorReferenceConfiguration => cusSupplyChainActorReferenceConfiguration ??= GetNewCusSupplyChainActorReferenceConfiguration();
		CusSupplyChainActorReferenceConfiguration cusSupplyChainActorReferenceConfiguration;

		protected virtual CusSupplyChainActorReferenceConfiguration GetNewCusSupplyChainActorReferenceConfiguration() => new CusSupplyChainActorReferenceConfiguration();

		public NctsContainerConfiguration NctsContainerConfiguration => nctsContainerConfiguration ?? (nctsContainerConfiguration = GetNewNctsContainerConfiguration());
		NctsContainerConfiguration nctsContainerConfiguration;

		protected virtual NctsContainerConfiguration GetNewNctsContainerConfiguration() => new NctsContainerConfiguration();

		public CusAuthorizationUsageConfiguration CusAuthorizationUsageConfiguration => cusAuthorizationUsageConfiguration ??= GetNewCusAuthorizationUsageConfiguration();
		CusAuthorizationUsageConfiguration cusAuthorizationUsageConfiguration;

		protected virtual CusAuthorizationUsageConfiguration GetNewCusAuthorizationUsageConfiguration() => new CusAuthorizationUsageConfiguration();

		public ZBool UseLocalReferenceNumberIgnoreInDatabaseCheck => UseLocalReferenceNumberIgnoreInDatabaseCheckCore;
		protected virtual ZBool UseLocalReferenceNumberIgnoreInDatabaseCheckCore => false;

		public ZBool IsBondedWarehouseSupported => IsBondedWarehouseSupportedCore;
		protected virtual ZBool IsBondedWarehouseSupportedCore => false;

		public ZBool IsDepartureRetransmissionSupported => IsDepartureRetransmissionSupportedCore;
		protected virtual ZBool IsDepartureRetransmissionSupportedCore => false;

		public EnRouteIncidentConfiguration EnRouteIncidentConfiguration => enRouteIncidentConfiguration ?? (enRouteIncidentConfiguration = GetNewEnRouteIncidentConfiguration());
		EnRouteIncidentConfiguration enRouteIncidentConfiguration;

		protected virtual EnRouteIncidentConfiguration GetNewEnRouteIncidentConfiguration() => new EnRouteIncidentConfiguration();

		public LocationOfGoodsFromAuthorisationDefaulterConfiguration LocationOfGoodsFromAuthorisationDefaulterConfiguration => locationOfGoodsFromAuthorisationDefaulterConfiguration ??= GetNewLocationOfGoodsFromAuthorisationDefaulterConfigurationCore();
		LocationOfGoodsFromAuthorisationDefaulterConfiguration locationOfGoodsFromAuthorisationDefaulterConfiguration;
		protected virtual LocationOfGoodsFromAuthorisationDefaulterConfiguration GetNewLocationOfGoodsFromAuthorisationDefaulterConfigurationCore() => new LocationOfGoodsFromAuthorisationDefaulterConfiguration();

		public CusTransportMeansConfiguration CusTransportMeansConfiguration => cusTransportMeansConfiguration ??= GetNewCusTransportMeansConfiguration();
		CusTransportMeansConfiguration cusTransportMeansConfiguration;
		protected virtual CusTransportMeansConfiguration GetNewCusTransportMeansConfiguration() => new CusTransportMeansConfiguration();
	}
}
