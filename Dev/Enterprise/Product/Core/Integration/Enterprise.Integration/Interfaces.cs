/// <summary>
/// !!! If you add members to these types, create a new file for them. !!!
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Macros;
using CargoWise.Types;
using static Enterprise.Integration.Customs.Shared;
using static Enterprise.Integration.DocumentWrappers;

[assembly: SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]

namespace Enterprise.Integration
{
	#region Customs

	public static partial class Customs
	{
		#region Base (Enterprise.Customs.Business)
		public interface ICusPollingTransaction { }
		public interface ICusEntryLine { }
		public interface ICusEntryLineFee { }
		public interface ICargoReportWorkflow { }
		public interface IConsolChargesLocator { }
		public interface IBaseCusClassPartPivot : ICusClassPartPivot
		{
			bool IsHTB { get; }
			bool IsImportClassification { get; }
			bool IsExportClassification { get; }
		}
		public interface IBaseJobDeclarationCollection { }
		public interface ICommercialInvoiceCollection { }
		public interface ICusEntryHeaderCharges { }
		public interface ICusOutturn { }
		public interface ICusInBondContainer : IBaseCusInBondContainer { }
		public interface IBaseJobComInvHeaderCharge
		{
			ZBool NeedCheckChargeType { get; }
		}
		public interface IBaseJobDeclarationCollectionProvider { }
		public interface IYesNoListProvider { }
		public interface IInvoiceRelatedDeclarationGenPivot { }
		public interface IGroupRelatedDeclarationGenPivot { }
		public interface IGlobalOrgSupplierPartDataLoad { }
		public interface IOrgSupplierBuyerLinkAddInfo { }
		public interface IOrgSupBuyLinkTrnModeAddInfo { }
		public interface ICommonCusPermitHeader
		{
			ZGuid PK { get; }
			ZGuid CPH_OH_PermitHolder { get; set; }
			ZString CPH_RN_NKCountryCode { get; set; }
			ZString CPH_Number { get; set; }
			ZDate CPH_StartDate { get; set; }
			ZString CPH_Type { get; set; }
			ZString CPH_SubType { get; set; }
			ZString CPH_QtyValIndicator { get; set; }
			ZString ShortName { get; }
		}
		public interface IBaseCusGuaranteeHeader : ICommonCusPermitHeader
		{
		}
		public interface ICusGuaranteeHeaderTypeDecider
		{
		}
		public interface ICusGuaranteeRule { }
		public interface IBaseCusPermitHeader : ICommonCusPermitHeader
		{
		}
		public interface ICusPermitHeaderTypeDecider
		{
		}
		public interface ICusAuthorisationHeader : ICommonCusPermitHeader
		{
		}

		public interface ICustomsRule : ICommonCusPermitHeader
		{
		}

		public interface ICommonCusPermitLineTransaction
		{
			ZGuid CPL_CPH_PermitHeader { get; set; }
			ZDecimal CPL_TranQty { get; set; }
			ZString CPL_TransactionCategory { get; set; }
			ZString CPL_TransactionStatus { get; set; }
			ZString CPL_TransactionType { get; set; }
			ZDecimal CPL_TranValue { get; set; }
		}

		public interface IBaseCusReconDeclaration
		{
		}

		public interface ICusReconDeclaration : IBaseCusReconDeclaration
		{
		}

		public interface IConsolidatedDeclaration : IBaseCusReconDeclaration
		{
		}

		public interface ICusReconEntry
		{
		}

		public interface ICusPackageJob { }

		public interface ICusPackage { }

		public interface ICusReconEntryLine
		{
		}

		public interface ICusRefTariffVersion
		{
			ZString CRT_Version { get; set; }
			ZString CRT_Description { get; set; }
			ZDate CRT_EffectiveDate { get; set; }
		}

		public interface ICusGoodsLocation { }

		public interface ICusReference { }

		public interface ICusGoodsCatalog { }

		public interface ICusGoodsCatalogProductionInfo { }

		public interface ICusReconCustomsCharge { }
		public interface ICusReconSnapshot { }

		public static partial class Shared
		{
			public interface IInvoiceHeaderPackagePivot { } // This requirement is BS, and I add this only to make BusinessObjectFactoryTest.TestGetBusinessObjectBaseTypeFromTablePrefix() shut up
			public interface IInvoiceLinePackagePivot { } // This requirement is BS, and I add this only to make BusinessObjectFactoryTest.TestGetBusinessObjectBaseTypeFromTablePrefix() shut up
			public interface IGlobalOrgSupplierPart { }
			public interface IBaseCusSCAPivot { }
			public interface IBaseCusStatementHeader { }
			public interface IBaseCusStatementLine { }
			public interface IBaseCusStatementLineCharge { }
			public interface IBaseCusStatementProcessTask { }
			public interface IDeclarationMessageTypeCodeDescriptionPairProvider { }
			public interface IDeclarationMessageSubTypeCodeDescriptionPairProvider { }
			public interface IDeclarationPackModeCodeDescriptionPairProvider { }
			public interface IDeclarationPaymentMethodCodeDescriptionPairProvider { }
			public interface IDeclarationTransportModeCodeDescriptionPairProvider { }
			public interface IDeclarationEntryStatusCodeDescriptionPairProvider { }
			public interface IDeclarationMessageStatusCodeDescriptionPairProvider { }
			public interface IDeclarationServiceLevelCodeDescriptionPairProvider { }
			public interface IDeclarationEntryTypeCodeDescriptionPairProvider { }
			public interface IDeferredMessageSchedulingProcessor { }
			public interface ICustomsProductValueObjectDataAdapter { }
			public interface IAdditionalReferenceNumberTypesCodeDescriptionPairProvider { }
			public interface ICCRProcessor { }
			public interface ICreateBrokerageOnShipmentProcessor { }
			public interface IProcedureCodesCodeDescriptionPairProvider { }
			public interface IPreviousProcedureCodesCodeDescriptionPairProvider { }

			public enum DISReferenceNumberFountainStrategyStateSeverity
			{
				Ok,
				Warning,
				Error
			}

			public interface IDISReferenceNumberFountainStrategyState
			{
				DISReferenceNumberFountainStrategyStateSeverity Severity { get; }
				string Message { get; }
			}

			public interface IDISReferenceNumberFountainStrategy
			{
				IDISReferenceNumberFountainStrategyState CheckState();

				IDISReferenceNumberFountainStrategyState CheckStateForAddingNewRecord();

				ZString GetDISReferenceNumber();

				IUniqueIndexFailureHandler GetUniqueIndexFailureHandler(BusinessObject businessObject);
			}

			public interface ICusEntryPayInfo { }

			public interface ICusPackageJobTypeDecider { }

			public interface ICusPackageTypeDecider { }
		}

		#endregion

		#region ManifestBase

		public static partial class ManifestBase
		{
			public interface IAsycudaArrivalHeader { }
			public interface IAsycudaArrivalLine { }
			public interface IAsycudaTransferHeader { }
			public interface IAsycudaTransferBill { }
			[WTG.StaticAnalysis.Annotation.CodeAlive("To be implemented for customs or removed if not used")]
			public interface IAsycudaTax { }
			public interface IAsycudaBillScreening { }
			public interface IEUMemberStateCommunicationTypeDecider { }
		}

		#endregion

		#region Asycuda

		public static partial class ASYCUDA
		{
			public interface IAsycudaManifestHeaderTypeDecider { }
			public interface IAsycudaManifestHeaderProcessTask { }
			public interface IAsycudaArrivalHeader : ManifestBase.IAsycudaArrivalHeader { }
			public interface IAsycudaArrivalLine : ManifestBase.IAsycudaArrivalLine { }
			public interface IAsycudaContainer : ManifestBase.IAsycudaContainer { }
			public interface IAsycudaContainerBillOrPackageLink : ManifestBase.IAsycudaContainerBillOrPackageLink { }
			public interface IAsycudaManifestModuleCollection : IBusinessObjectCollection { }

			public static class ApplicationCodeTypes
			{
				public const string Consolidator = "NVC";
			}

			public static partial class ACEManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader, IOriginatorCodeProvider
				{
					ZDateTime EstDateAtFirstArrival { get; set; }
				}
				public partial interface IAsycudaBill : ASYCUDA.IAsycudaBill, IOriginatorCodeProvider { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
				public interface IAsycudaContainerBillOrPackageLink : ASYCUDA.IAsycudaContainerBillOrPackageLink { }
				public interface ICusPerson : ASYCUDA.ICusPerson { }
				public interface ICusPersonCountry : ASYCUDA.ICusPersonCountry { }
				[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
				public interface IUSAMSImportAirManifestCreator : US.USAMS.IUSAMSCreator { }
			}

			public static partial class ASYCUDAManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
				public interface IAsycudaTax : ASYCUDA.IAsycudaTax { }
				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
				public interface IAsycudaContainerBillOrPackageLink : ASYCUDA.IAsycudaContainerBillOrPackageLink { }
				public interface ICusPerson : ASYCUDA.ICusPerson { }
				public interface ICusPersonCountry : ASYCUDA.ICusPersonCountry { }
				public interface IAsycudaArrivalHeader : ASYCUDA.IAsycudaArrivalHeader { }
			}

			public static partial class EUManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
			}

			public static partial class NZManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
				public interface IAsycudaContainerBillOrPackageLink : ASYCUDA.IAsycudaContainerBillOrPackageLink { }
				public interface ICusPerson : ASYCUDA.ICusPerson { }
				public interface ICusPersonCountry : ASYCUDA.ICusPersonCountry { }
			}

			public static partial class SGAccess
			{
				public partial interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public partial interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : IAsycudaPackWithOnePackedItemRelationship { }
				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
				public interface IAsycudaContainerBillOrPackageLink : ASYCUDA.IAsycudaContainerBillOrPackageLink { }
				public interface ICusPerson : ASYCUDA.ICusPerson { }
				public interface ICusPersonCountry : ASYCUDA.ICusPersonCountry { }
			}

			public static partial class UYManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
			}

			public static partial class FJManifest
			{
				public partial interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
			}

			public static partial class ZAManifest
			{
				public partial interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
				public interface IAsycudaContainerBillOrPackageLink : ASYCUDA.IAsycudaContainerBillOrPackageLink { }
				public interface ICusPerson : ASYCUDA.ICusPerson { }
				public interface ICusPersonCountry : ASYCUDA.ICusPersonCountry { }
			}

			public static partial class TRManifest
			{
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
				public interface IAsycudaContainerBillOrPackageLink : ASYCUDA.IAsycudaContainerBillOrPackageLink { }
				public interface ICusPerson : ASYCUDA.ICusPerson { }
				public interface ICusPersonCountry : ASYCUDA.ICusPersonCountry { }
			}

			public static partial class NOManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
			}

			public static partial class TRETrade
			{
				public interface IAsycudaBill : ManifestBase.IAsycudaBill
				{
					ZString ABL_ShipperRegNo { get; set; }
					ZString ABL_ConsigneeRegNo { get; set; }
					ZString ArrivalCountry { get; set; }
					ZDecimal ABL_NetWeight { get; set; }
					ZString ABL_NetWeightUQ { get; set; }
				}

				public interface IAsycudaPackedItem : ASYCUDA.IAsycudaPackedItem
				{
					ZString API_CustomsUQ2 { get; set; }
					ZDecimal API_GoodsValue { get; set; }
					ZDecimal API_CustomsQty2 { get; set; }
					ZDecimal API_GrossWeight { get; set; }
					ZString API_GrossWeightUQ { get; set; }
					ZDecimal API_NetWeight { get; set; }
					ZString API_NetWeightUQ { get; set; }
					ZString API_RX_NKGoodsValueCurrency { get; set; }
				}

				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
			}

			public static partial class MXManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
			}

			public static partial class CLManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
			}

			public static partial class BRManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }

				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
			}

			public static partial class COManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
			}

			public static partial class ARManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
			}

			public static partial class INManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
			}

			public static partial class JPManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader
				{
					ZString AMA_InputReference { get; set; }
				}

				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
			}

			public static partial class TWManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
			}

			public static partial class TWBriefCustomsDeclaration
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
			}

			public static partial class CKManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
			}

			public static partial class USExportManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
			}

			public static partial class EUH7
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }

				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }

				public interface IAsycudaManifestHeaderProcessTask { }

				public interface IAsycudaManifestHeaderTypeDecider
				{
					Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory);
				}
			}

			public static partial class EUICS2
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
			}

			public static partial class ILManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }

				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }

				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }

				public interface IAsycudaContainer : ASYCUDA.IAsycudaContainer { }

				public interface IAsycudaPackedItem : ASYCUDA.IAsycudaPackedItem { }
			}

			public static partial class PEManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
				public interface IAsycudaPack : ASYCUDA.IAsycudaPack { }
			}

			public static partial class AEManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
			}

			#region VN
			public static partial class VNManifest
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }

				public interface IAsycudaBill : ASYCUDA.IAsycudaBill { }
			}
			#endregion
		}

		#endregion

		#region AU

		public static partial class AU
		{
			public interface IHouseBillsCargoMessageProcessor { }
			public interface IAirCargoMessageProcessorJob { }
			public interface ISeaCargoMessageProcessor { }
			public interface ISeaCargoMessageProcessorJob { }
			public interface ICusSeaManOBLHeaderMessageStatus { }
			public interface ICMRCARSTMessage { }
			public interface ICargoReportWorkflow { }
			public interface IClassification { }
			public interface ICusClassPartPivot : Shared.ICusClassPartPivot { }
			public interface ICusContainerInvoiceLinePivot : Shared.ICusContainerInvoiceLinePivot { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface IBill : Customs.IBill { }
			public interface IPackingGroup { }
			public interface IPackage { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionedCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IGroupInvoiceCharge { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionedCharge { }
			public interface IJobDeclarationFilterBusinessObject { }
			public interface IOrgSupplierPart { }
			public interface IAirCargoMessage { }
			public interface ICusMAWBBase : Shared.ICusMAWB { }
			public interface ICusMAWB : ICusMAWBBase
			{
				ZString GetOutturnsReconciliationMessage();
			}
			public interface ICTOCusMAWB { }
			public interface ICusHAWB : Shared.ICusHAWB
			{
				ZString CS_CustomsStatus { get; set; }
				ICodeDescriptionPairList ConsolidatedCargoStatusesList { get; }
			}
			public interface ICusHAWBProcessTask { }
			public interface ICusMAWBProcessTask { }
			public interface ICusOutturnHeaderProcessTask { }
			public interface ICusPoster { }
			public interface ICTOCusHAWB { }
			public interface ICusHAWBBase { }
			public interface IExportCustomsManifestLines { }
			public interface IExportCustomsManifestHeader { }
			public interface ICusSCAHouse : IBaseCusSCAHouse
			{
				ZGuid CA_JS { get; set; }
				ZString CA_HouseBill { get; }
				ZString CA_ShipmentStatus { get; set; }
				ICodeDescriptionPairList ShipmentStatusesList { get; }
			}
			public interface ICusSCAPivot
			{
				ZString CV_AssociatedHouse { get; }
			}
			public interface ICusSCAOceanBill : IBaseCusSCAOceanBill { }
			public interface ICusSCAContainer { }
			public interface ICMRTypeDecider { }
			public interface ICMRInterchange { }
			public interface ICOLSInterchange { }
			public interface IEXDOCInterchange { }
			public interface IAUDeclarationValueObjectDataAdapter { }
			public interface IAUStandAloneInvoiceValueObjectDataAdapter { }
			public interface IAUShipmentDeclarationValueObjectDataAdapter { }
			public interface IAUShipmentDeclarationGenerator { }
			public interface IAUCusOrgSupplierPartDataLoad { }
			public interface ICusUnderbond : Customs.ICusUnderbond { }
			public interface ICusOutturn { }
			public interface ICusSeaManTranHead { }
			public interface ICusSeaManOBLHeader { }
			public interface ICusSeaManOBLDetail { }
			public interface ICusSeaManArrivalPort { }
			public interface ICusHAWBConsigneeMatchApproval { }
			public interface ICusHAWBConsignorMatchApproval { }
			public interface ICusHAWBImporterMatchApproval { }
			public interface IImportAndUpdateDataReferenceFileData { }
			public interface ICusOutturnHeader : Customs.ICusOutturnHeader { }
			public interface ICusHAWBForwardingShipmentCustomsStatusProvider { }
			public interface ICusSCAHouseForwardingShipmentCustomsStatusProvider { }
			public interface ICFSShipmentStatusProvider { }
			public interface IQuarantineColsDirection { }
			public interface IQuarantineColsHeader { }
			public interface ICusStorageDocPivot { }
			public interface IQuarantineExdocHeader { }
			public interface IQuarantineExdocLine { }
			public interface IEDIMessageTypeDecider { }
			public interface IEXDOCMessage { }
			public interface INEXDOCMessage { }
			public interface IShipnetToBillOfLadingImporter { }
			public interface IPackLineStatus { }
			public interface ICusPartShip { }
			public interface ICusSCAOceanBillProcessTask { }
			public interface ICusSCAHouseProcessTask { }
			public interface IOrgCusCodeValidation { }
			public interface ISendWithdrawalMessageProcessor { }
			public interface ICLREGInfoProvider : ICusAddInfo { }
			public interface ICustomsManifestLineSequence : ICusCodeData { }
			public interface IOrgImpAddInfo
			{
				ZBool IsDutyDeferred { get; set; }
			}
			public interface IConsolidatedDeclaration : Customs.IConsolidatedDeclaration { }
			public interface ICusCalculationRule { }
			public interface IAUCompanyCredentialsPlugIn { }
			public interface ICreateDeclarationHelper : Shared.ICreateDeclarationHelper { }
		}

		#endregion

		#region ZA

		public static partial class ZA
		{
			public interface ICusOutturn { }
			public interface IAsycudaManifestHeader : ManifestBase.IAsycudaManifestHeader { }
			public interface IAsycudaBill : ManifestBase.IAsycudaBill { }
			public interface IAsycudaContainer : ManifestBase.IAsycudaContainer { }
			public interface IAsycudaPack : ManifestBase.IAsycudaPack { }
			public interface IAsycudaContainerBillOrPackageLink : ManifestBase.IAsycudaContainerBillOrPackageLink { }
			public interface IAsycudaManifestHeaderProcessTask { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface ICusLineTariffDetail : Customs.ICusLineTariffDetail { }
			public interface IBill : Customs.IBill { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IInvoiceCharge { }
			public interface IInvoiceLineCharge { }
			public interface IOrgSupplierPart { }
			public interface IApportionedCharge { }
			public interface IZAMessage { }
			public interface ICusClassPartPivot : Shared.ICusClassPartPivot { }
			public interface IJobDeclarationFilterBusinessObject { }
			public interface IFRNCustomsOfficesProvider { }
			public interface ICustomsOfficesProvider { }
			public interface IOrgCusCodeValidation { }
			public interface IEDIMessageTypeDecider { }
			public interface ICusPermitHeader { }
			public interface IOrgSupplierPartDataLoad { }
			public interface IReleasePrintIndicatorProvider { }
			public interface IProvisionalPaymentTypeProvider { }
			public interface ICusPermitRule { }
			public interface ICusEntryPayInfoTypeDecider { }
			public interface ICusEntryPayInfo { }
			public interface IShipmentTypeProvider { }
			public interface ICusVehicle : Customs.ICusVehicle { }

			#region DataAdapters
			public interface IZADeclarationValueObjectDataAdapter { }

			#endregion
		}

		#endregion

		#region AE

		public static class AE
		{
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusClassPartPivot : Shared.ICusClassPartPivot { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface IEDIMessageTypeDecider : ITypeDecider { }
			public interface IBill : Customs.IBill { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IInvoiceCharge { }
			public interface IOrgCusCodeValidation { }
			public interface IOrgSupplierPart { }
			public interface IApportionedCharge { }
			public interface IJobDeclarationFilterBusinessObject { }
			public interface ICusVehicle : Customs.ICusVehicle { }
		}

		#endregion

		#region CN

		public static partial class CN
		{
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusClassPartPivot { }
			public interface IOrgSupplierPart { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface IGroupInvoiceCharge { }
			public interface IBill : Customs.IBill { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceHeaderContract { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IJobDeclarationFilterBusinessObject { }
			public interface IOrgCusCodeValidation { }
			public interface IOrgImpAddInfo { }
			public interface ICustomsOffice : ICusCodeData { }
			public interface IEDIMessage { }
			public interface IEDIInterchange { }
			public interface IAttachmentInvoiceLineGenPivot { }
			public interface ICreateDeclarationHelper : Shared.ICreateDeclarationHelper { }
		}

		#endregion

		#region MY

		public static partial class MY
		{
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface IBill : Customs.IBill { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart { }
			public interface IJobDeclarationFilterBusinessObject { }
		}

		#endregion

		#region NZ

		public static partial class NZ
		{
			public interface ICusSCAOceanBillProcessTask { }
			public interface IConsolCustomsCharges { }
			public interface IForwardingShipmentCustomsStatusProvider { }
			public interface ICusMAWBProcessTask { }
			public interface ICusUnderbond : Customs.ICusUnderbond { }
			public interface IConsolidatedDeclaration : Customs.IConsolidatedDeclaration { }

			#region Express
			public interface ICusHAWB : Shared.ICusHAWB { }
			public interface ICusHAWBProcessTask { }

			public interface ICusSCAPackingLine : IBaseCusSCAPivot { }

			#endregion

			#region MasterFiles			
			public interface IOrgCusCodeValidation { }
			#endregion

			#region Base Declaration
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IInvoiceCharge { }
			public interface IBill { }
			public interface IPackage { }
			public interface IPackingGroup { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusContainerInvoiceLinePivot : Shared.ICusContainerInvoiceLinePivot { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface INZCMessage { }
			public interface INZCustomsInterchange { }
			public interface INZEBACCAInterchange { }
			public interface IPackLineStatus { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceLineCharge { }
			public interface ICreateDeclarationHelper : Shared.ICreateDeclarationHelper { }

			#endregion

			#region CusEntry Bits for Different Entry Types
			public interface IFormalEntryCusEntryHeader { }
			public interface ICompletionCusEntryHeader { }
			public interface IPrimaryIndustriesCusEntryHeader { }
			public interface IOriginalCusEntryHeader { }
			public interface IECIWriteOffCusEntryHeader { }
			public interface IECIWriteOffManifestingCusEntryHeader { }
			#endregion

			#region Custom Lookups for Declaration

			public interface ILowValueConsignmentStatusList { }
			public interface IFormalEntryStatusList { }

			#endregion

			#region JobDeclarationFilterBusinessObject
			public interface IJobDeclarationFilterBusinessObject { }
			#endregion

			#region DataAdapters
			public interface INZDeclarationValueObjectDataAdapter { }
			public interface INZStandAloneInvoiceValueObjectDataAdapter { }
			public interface INZShipmentDeclarationValueObjectDataAdapter { }
			public interface INZShipmentDeclarationGenerator { }
			#endregion

			#region DataLoad

			public interface INZCusOrgSupplierPartDataLoad { }

			#endregion
		}

		#endregion

		#region SG

		public static class SG
		{
			public interface ICusClassification : IBaseCusClassification { }
			public interface IOrgSupplierPart { }
			public interface ICusLineTariffDetail { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface ISGEDIMessageTypeDecider { }
			public interface IGroupInvoiceCharge { }
			public interface IBill : Customs.IBill { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IJobDeclarationFilterBusinessObject { }
			public interface ICusClassPartPivot : Shared.ICusClassPartPivot { }
			public interface ICMDPermitNumber : ICusCodeData { }

			public interface ISGCustomsRegistry
			{
				IRegistryItem ACCESSEnable { get; }
				IRegistryItem CycleNumbers { get; }
			}

			#region DataLoad

			public interface ISGOrgSupplierPartDataLoad { }

			#endregion
		}

		#endregion

		#region US

		public static partial class US
		{
			public interface IATF6AFormPermitNumbersSelector
			{
				Either<string, ZString[]> SelectPermitNumbers(object obj);
			}
			public interface IATF6ADataBuilder
			{
				object Build(object declaration, object parameters);
			}
			public interface ICensusWarningOverride : ICusCodeData { }
			public interface IFDARelatedBillsGenPivot { }
			public interface IFDARelatedContainersGenPivot { }
			public interface IPGARelatedContainersGenPivot { }
			public interface IShippingOrPackingingUnitList { }
			[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICAMProcessor { }
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusLineTariffDetail { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusContainerInvoiceLinePivot : Shared.ICusContainerInvoiceLinePivot { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusClassPartPivot : Shared.ICusClassPartPivot { }
			public interface ICusAttributeFilter { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface ICusStatementHeader { }
			public interface ICusStatementHeaderCollection : IBusinessObjectCollection { }
			public interface ICusStatementLine { }
			public interface ICusStatementLineCharge { }
			public interface ICBPEDIInterchangeTypeDecider { }
			public interface IMQEDIMessage { }
			public interface ICBPMessageForTesting { }
			public interface IBill : Customs.IBill { }
			public interface IPackingGroup { }
			public interface IPackage { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IGroupInvoiceCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader
			{
				ZString US_LicenseNo { get; set; }
				ZString US_DDTCRegistrationNo { get; set; }
				ZGuid BuyerOrgPK { get; }
			}
			public interface IVehicle
			{
				IVehicleAddInfo AddInfo { get; }
			}

			public interface IVehicleAddInfo { }

			public interface IVehicleDetails
			{
				IVehicleDetailsAddInfo AddInfo { get; }
			}
			public interface IVehicleDetailsAddInfo { }

			public interface IPesticide
			{
				IPesticideAddInfo AddInfo { get; }
			}
			public interface IPesticideAddInfo { }

			public interface IPesticideLine
			{
				IPesticideLineAddInfo AddInfo { get; }
			}
			public interface IPesticideLineAddInfo { }

			public interface IUSACEFDA
			{
				IUSACEFDAAddInfo AddInfo { get; }
			}
			public interface IUSACEFDAAddInfo { }

			public interface IUSLot
			{
				IUSLotAddInfo AddInfo { get; }
			}
			public interface IUSLotAddInfo { }

			public interface INHTSAHeader
			{
				INHTSAHeaderAddInfo AddInfo { get; }
			}
			public interface INHTSAHeaderAddInfo { }

			public interface INHTSADetails
			{
				INHTSADetailsAddInfo AddInfo { get; }
			}
			public interface INHTSADetailsAddInfo { }

			public interface IOrgSupplierPart { }
			public interface IEDIMessageTypeDecider { }
			public interface IOrgSupplierPartDataLoad { }
			public interface IOrgImpAddInfo { }
			public interface IOrgCusCodeValidation { }
			public interface IForwardingShipmentCustomsStatusProvider { }

			public interface IForwardingShipmentCustomsQueryProvider
			{
				ZQuery GetSimplifiedEntryBillStatusQuery(SQLComparisonOperator filterOperator, ZString value);
				ZQuery GetHoldExamBillStatusQuery(ZString value);
			}

			public interface ICreateDeclarationHelper : Shared.ICreateDeclarationHelper { }
			public interface IRestrictedCode : ICusCodeData { }
			public interface IImportersControlledGroupName : ICusCodeData { }
			public interface IAllocationQuantityPerFPI : ICusCodeData { }
			public interface IUSCForeignPortCollection { }
			public interface IUSCForeignPortForExportCollection { }
			public interface IUSRegionDistrictPortCollection { }
			public interface IUSCarrierCombinedCollectionProvider { }
			public interface IUSCCountryCollectionProvider { }
			public interface IUSRegionDistrictPortCollectionProvider { }
			public interface IUSForeignPortCollectionProvider { }
			public interface IBrokerPaymentTypeCodeDescriptionPairProvider { }
			public interface ILiquidationTypeCodeDescriptionPairProvider { }
			public interface IMonthListDescriptionPairProvider { }
			public interface IReleaseStatusCodeDescriptionPairProvider { }
			public interface IStatementLineFilterByOptionProvider { }
			public interface IImporterBondTypeCodeDescriptionPairProvider { }
			public interface IEIStatusCodeDescriptionPairProvider { }
			public interface IENSStatusCodeDescriptionPairProvider { }
			public interface IFDAStatusCodeDescriptionPairProvider { }
			public interface IPaymentStatusCodeDescriptionPairProvider { }
			public interface IEntryModeCodeDescriptionPairProvider { }
			public interface IFTZAdmissionTypeCodeDescriptionPairProvider { }
			public interface ITaxDeferredCodeDescriptionPairProvider { }
			public interface IReconIssueCodeDescriptionPairProvider { }
			public interface ISPICodeDescriptionPairProvider { }
			public interface IProductClaimCodeDescriptionPairProvider { }
			public interface IJobApplicationCodeProvider { }
			public interface IUpdateCertificateRequestDate
			{
				ZGuid[] Update(ILogger logger, ZGuid[] jobDeclarationPKs);
			}
			public interface IUpdateBillDispositionSource
			{
				ZGuid[] Update(ILogger logger, ZGuid[] pKs);
			}

			public interface IMIDOrganisation
			{
				BusinessObject CreateMIDOrganizationIfNecessary(ZString midNo, BusinessObjectFactory factory);
			}

			#region DIS

			public static partial class DIS
			{
				public interface IEDIInterchange { }
				public interface IEDIMessage { }
			}

			#endregion

			#region Data Transfer

			public interface IUSDeclarationValueObjectDataAdapter { }
			public interface IUSProductValueObjectDataAdapter { }
			public interface IUSStandAloneInvoiceDataAdapter { }
			public interface IUSShipmentDeclarationGenerator { }
			public interface IUSOrganisationCountrySpecificDataTransferTool { }
			public interface IUSUniversalCustomsDataObjectProvider
			{
				void PublishDeclarationUniversalEvent(IJobDeclaration declaration);
			}

			#endregion

			#region Managers
			public interface IPrintManager
			{
				bool IsOkToPrint();
			}

			public interface IDeliveryOrderPrintManager : IPrintManager { }
			#endregion

			#region InBond
			public static partial class InBond
			{
				public interface ICusInBondHeaderProcessTask { }
				public interface ICusInBondBill : Customs.ICusInBondBill { }
				public interface ICusInBondMoveDetail : Customs.ICusInBondMoveDetail { }
				public interface ICusInBondContainer : Customs.ICusInBondContainer { }
				public interface ICusInBondCargoDesc : Customs.ICusInBondCargoDesc { }
				public interface ICusInbondBillAddRef : Customs.ICusInbondBillAddRef { }
				public interface IInBondQPMessageStatusCodeDescriptionPairProvider { }
				public interface IInBondWPMessageStatusCodeDescriptionPairProvider { }
			}
			#endregion

			#region USAMA
			public interface IAIMEDIMessageTypeDecider { }
			public interface IAIMEDIInterchange { }
			#endregion

			#region US Export Manifest

			public interface IUEMEDIMessageTypeDecider { }
			public interface IUEMEDIInterchange { }

			#endregion

			#region USAMS
			public static partial class USAMS
			{
				public interface IEDIMessageTypeDecider { }
				public interface ICusInBondHeader : Customs.ICusInBondHeader
				{
					ZString BH_LatestDispositionCode { get; }
					ZString BH_LatestDispositionCodeDescription { get; }
				}
				public interface ICusInBondHeaderProcessTask { }
				public interface ICusInBondBill : Customs.ICusInBondBill { }
				public interface ICusInBondMoveDetail : Customs.ICusInBondMoveDetail { }
				public interface ICusInbondBillAddRef : Customs.ICusInbondBillAddRef { }
				[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
				public interface ICusUSAMSCreator : IUSAMSCreator { }
			}
			#endregion

			#region eManifest

			public static partial class eManifest
			{
				public interface IEDIMessage { }
				public interface IEDIMessageTypeDecider { }
				public interface ICusInBondHeader : Customs.ICusInBondHeader { }
				public interface ICusInBondHeaderProcessTask { }
				public interface ICusInBondBill : Customs.ICusInBondBill { }
				public interface ICusInBondMoveHeader : Customs.ICusInBondMoveHeader { }
				public interface ICusInBondCargoDesc : Customs.ICusInBondCargoDesc { }
			}

			#endregion

			#region ISF

			public static partial class ISF
			{
				public interface ICusISFHeaderProcessTask { }
				public interface ITrackingCusISFHeader : ICusISFHeader { }
				public interface IJobInvoicingImporterSecurityFilingConsumerType { }
				public interface IActionReasonCodeCodeDescriptionPairProvider { }
				public interface IBillTypeCodeDescriptionPairProvider { }
				public interface IBondActivityCodeCodeDescriptionPairProvider { }
				public interface IDispositionCodeCodeDescriptionPairProvider { }
				public interface IEntryTypeCodeDescriptionPairProvider { }
				public interface IMessageStatusCodeDescriptionPairProvider { }
				public interface IShipmentTypeCodeDescriptionPairProvider { }
				public interface ITransportModeCodeDescriptionPairProvider { }
				public interface IISFDocAddressControl { }
			}

			#endregion

			public interface IImporterNumberRequester
			{
				ZString CurrentCompanyRegistryItemValidationForRequestImporterBond();
				IMQEDIMessage RequestImporterBond(BusinessObject parent, string importerNumber);
				bool IsValidImporterBondNumber(ZString importerBondNumber);
				bool HasPermissionToSendImporterBondNumber(ZString importerBondNumber);
			}
			public interface ICusPermitRule { }
			public interface ICusPermitHeader { }
			public interface IUSModuleEntryHeaderCollectionProvider { }
			public interface IUSFDAProductNumberCollectionProvider { }
			public interface IUSFDAAgencyProgramCodeDescriptionPairProvider { }
			public interface IUSFDAProcessingCodeDescriptionPairProvider { }
			public interface IUSITARExemptionNumberDescriptionPairProvider { }
			public interface IUSLicenseTypeCollectionProvider { }
			public interface IUSDOTAgencyProgramCodeDescriptionPairProvider { }
			public interface IUSDOTBoxNumberDescriptionPairProvider { }
			public interface IUSCarrierCombined
			{
				ZString UI_Code { get; set; }
				ZString UI_ModeOfTransportation { get; set; }
			}
			public interface IUSAPHISProgramTypeDescriptionPairProvider { }
			public interface IUSAPHISProcessingCodeDescriptionPairProvider { }
			public interface IUSFTZPTTMessageStatusListProvider { }
			public interface IUSFTZAdmissionMessageStatusListProvider { }
			public interface IUSFTZConcurrenceMessageStatusListProvider { }
			public interface IUSFTZGoodsArrivalMessageStatusListProvider { }
			public interface IUSFTZDeliveryOfGoodsMessageStatusListProvider { }

			public interface ICustomsRuleRule { }
		}

		#endregion

		#region EU

		public static partial class EU
		{
			public interface IEUOrgSupplierPartDataLoad { }
			public interface IPackage : IBasePackage { }
			public interface IJobComInvoiceLineTax { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICustomsOfficesProvider { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEquipment : Shared.ICusEquipment { }
			public interface ICusEUEntryHeader { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface IOrgCusCodeValidation { }
			public interface IOrgImpAddInfo { }
			public interface IGroupInvoiceCharge { }
			public interface IBill : Customs.IBill { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IEDIMessageTypeDecider { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration
			{
				IBusinessObjectCollection<ICusCodeData> CustomsOfficeCollection { get; }
			}
			public interface IJobEUDeclaration { }
			public interface IJobDeclarationFilterBusinessObject { }
			public interface IForwardingShipmentCustomsStatusProvider { }
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusClassPartPivot : Shared.ICusClassPartPivot { }
			public interface IPreviousDocument : ICusSupportingInfo { }
			public interface ISupportingDocument : ICusSupportingInfo { }
			public interface IPackingGroup { }
			public interface ICusPermitHeader
			{
				ZGuid PK { get; }
			}
			public interface ICusGuaranteeHeader : IBaseCusGuaranteeHeader { }
			public interface ICusGuaranteeRule { }
			public interface IEntrySubStyleDescriptionPairProvider { }

			public interface ITemporaryStorageHeaderTypeDecider { }
			public interface ITemporaryStorageHeader : ManifestBase.IAsycudaManifestHeader { }
			public interface ITemporaryStorageBill : ManifestBase.IAsycudaBill { }
			public interface ICusGoodsLocation : Customs.ICusGoodsLocation { }
			public interface ICusVehicle : Customs.ICusVehicle { }
			public interface ITemporaryStorageContainer : ManifestBase.IAsycudaContainer { }
			public interface ITemporaryStorageCusGoodsLocation : ICusGoodsLocation { }
			public interface ICustomsReferenceCollectionDataObjectReader;
			public interface IRepresentationTypeList
			{
				ZString _2DirectCode { get; }
				ZString _2DirectDescription { get; }
			}

			public static partial class NCTS
			{
				public interface ICusInBondHeaderTypeDecider { }
				public interface INctsPackageTypeDecider { }
				public interface INctsContainer { }
				public interface INctsHeaderProcessTask { }
				public interface IArrivalMovementHeaderProcessTask { }
				public interface IDepartureMovementHeaderProcessTask { }
				public interface IDepartureMovementHeader : ICusInBondMoveHeader { }
				public interface IArrivalMovementHeader : ICusInBondMoveHeader { }
				public interface IUnloadingMovementHeader : ICusInBondMoveHeader { }
				public interface IArrivalAndUnloadingCargoDesc { }
				public interface ICommonCargoDesc { }
				public interface IDepartureCargoDesc { }
				public interface IArrivalCargoDesc { }
				public interface INctsCargoDescFee { }

				public interface INctsPackage : ICusInvPack { }
				public interface ICusInBondBill : Customs.ICusInBondBill { }
				public interface IEnRouteSeal : ICusInBondEvent { }
				public interface IEnRouteTransshipment : ICusInBondEvent { }
				public interface IEnRouteIncident : ICusInBondEvent { }
				public interface INCTSDepartureOfficesCollectionProvider { }
				public interface INCTSDestinationOfficesCollectionProvider { }
				public interface INctsEuOfficeCode { }
				public interface IGuaranteeNumbersCodeDescriptionPairProvider { }
				public interface IDeclarationTypesCodeDescriptionPairProvider { }
				public interface ICusAuthorizationUsage : EU.ICusAuthorizationUsage { }
				public interface ICusGoodsLocation : EU.ICusGoodsLocation { }
				public interface ICusNctsContainerGenPivotTypeDecider { }
				public interface INctsCusInBondContainerPackageGenPivot { }
				public interface INctsDepartureHeaderContainer : INctsCusInBondContainer { }
			}

			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface ICusTempStorageLineItemCollection<out TCusTempStorageLineItem> : IActiveBusinessObjectCollection<TCusTempStorageLineItem>
			{
				new TCusTempStorageLineItem this[int index] { get; }
			}
			public interface ICusTempStorageRegLineCollection<out TCusTempStorageRegLine> : TemporaryStorage.ICusTempStorageRegLineCollection<TCusTempStorageRegLine> { }

			public interface ICusTempStorageRegHeader : TemporaryStorage.ICusTempStorageRegHeader
			{
				ICommonGuarantee Guarantee { get; }
			}

			public interface ICommonGuarantee { }
			public interface ICusTempStorageRegHeaderStatusListProvider { }
			public interface ICusTempStorageRegHeaderAppCodesListProvider { }
			public interface ICusTempStorageRegLine : TemporaryStorage.ICusTempStorageRegLine { }
			public interface ICusTempStorageRegLineTransactionCollection<out TCusTempStorageRegLineTransaction> : TemporaryStorage.ICusTempStorageRegLineTransactionCollection<TCusTempStorageRegLineTransaction> { }
			public interface ICusTempStorageRegLineItem : TemporaryStorage.ICusTempStorageRegLineItem { }
			public interface ICusTempStorageRegLineTransaction : TemporaryStorage.ICusTempStorageRegLineTransaction { }
			public interface ICusTempStorageRegPremises : TemporaryStorage.ICusTempStorageRegPremises { }
			public interface ICusTempStorageRegLineItemPivot : TemporaryStorage.ICusTempStorageRegLineItemPivot { }
			public interface ICusTempStorageRegPremisesProvider { }
			public interface IOrgSupplierPart { }
			public interface ICusExitItem { }
			public interface ICusExitItemPackage { }
			public interface IISTCustomsProfileCodeDescriptionPairProvider { }
			public interface IISTLocationOfGoodsCodeDescriptionPairProvider { }
			public interface ICreateDeclarationHelper : Shared.ICreateDeclarationHelper { }

			public interface ITemporaryStorageHeaderProcessTask { }

			public interface IUCC6TemporaryStorageAdditionalInfosUserControlWithGrid { }

			public interface ICusFiscalReference : ICusReference { }

			public interface ICusSupplyChainActorReference : ICusReference { }
		}

		public static partial class EUExitControl
		{
			public interface ICusExitConsignmentPivot { }
			public interface ICusExitContainer { }
			public interface ICusExitSeal { }
			public interface ICusExitHeaderProcessTask { }
			public interface ICusGoodsLocation : EU.ICusGoodsLocation { }
		}

		public static partial class EUICS2
		{
			public interface IEUICS2ManifestTypes
			{
				ICodeDescription ENSCodeDescription { get; }
			}
		}

		public static partial class EUH7
		{
			public interface ICusGoodsLocation : EU.ICusGoodsLocation { }

			public interface IEUH7ManifestTypes
			{
				ICodeDescription EH7CodeDescription { get; }
			}
		}

		#endregion

		#region GB

		public static partial class GB
		{
			public interface IJobComInvoiceLineTax : EU.IJobComInvoiceLineTax { }
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }

			public interface IGroupInvoiceCharge : EU.IGroupInvoiceCharge { }
			public interface IInvoiceApportionCharge : EU.IInvoiceApportionCharge { }
			public interface IInvoiceCharge : EU.IInvoiceCharge { }
			public interface IInvoiceLineApportionCharge : EU.IInvoiceLineApportionCharge { }
			public interface IInvoiceLineCharge : EU.IInvoiceLineCharge { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : EU.ICusEntryLineFee { }
			public interface ICusEntryInstruction : EU.ICusEntryInstruction { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface ICusInBondHeader : EU.NCTS.ICusInBondHeader { }
			public interface IDepartureMovementHeader : EU.NCTS.IDepartureMovementHeader { }
			public interface IDepartureCargoDesc : EU.NCTS.IDepartureCargoDesc { }
			public interface IGbEdiMessage { }

			public interface IJobDeclaration : IBaseJobDeclaration
			{
				ZString ReasonWhyCannotPrintNch1Document { get; }
			}
			public interface IOrgImpAddInfo { }
			public interface IGBOrgSupplierPartDataLoad { }
			public interface IDeclarationMessageSenderChooser { }
			public interface IDeclarationMessageSenderChooserOptions { }

			public interface IGBEDIMessageTypeDecider : ITypeDecider { }

			public static partial class CCSUK
			{
				public interface ICusOutturn { }
				public interface ICusUnderbond : Customs.ICusUnderbond { }
				public interface ICusUnderbond_TranshipmentRemoval : ICusUnderbond { }
				public interface ICusUnderbond_InterAirportRemoval : ICusUnderbond { }
				public interface ICusUnderbond_InterShedRemoval : ICusUnderbond { }
				public interface ICusUnderbond_Fallback : ICusUnderbond { }
				public interface ICusMAWB : Shared.ICusMAWB, ICcsukCusAwbBase, IBusiness { }
				public interface ICusMAWBCollection : IBusinessObjectCollection<ICusMAWB>
				{
					new ICusMAWB this[int index] { get; }
				}
				public interface ICusHAWB : Shared.ICusHAWB, ICcsukCusAwbBase, IBusiness { }
				public interface ICusHAWBCollection : IBusinessObjectCollection<ICusHAWB>
				{
					new ICusHAWB this[int index] { get; }
				}
				public interface ICusHAWBProcessTask { }
				public interface ICusMAWBProcessTask { }
				public interface ICcsukCusAwbBase
				{
					ZGuid PK { get; }
					ZDecimal Weight { get; }
					ZString WeightCode { get; }
					ZShort NumberOfPiecesExpected { get; set; }
					ZShort NumberOfPiecesReceived { get; set; }
					ZString AirportOfArrival { get; set; }
					ZString AirportOfDestination { get; set; }
					ZString AirportOfOrigin { get; set; }
					ZPropertyInfo AirportOfArrivalInfo { get; }
					ZPropertyInfo AirportOfDestinationInfo { get; }
					ZPropertyInfo AirportOfOriginInfo { get; }
					ZString ShipmentDescriptionCode { get; set; }
					ZString DescriptionOfGoods { get; }
					ZString Profile { get; }
					ZString CargoTerminalOperatorAirportAndShed { get; }
					ZString CargoTerminalOperatorAirport { get; }
					ZString CargoTerminalOperator { get; }
					ZString AgentBadge { get; set; }
					ZPropertyInfo AgentBadgeInfo { get; }
					ZString LatestCustomsActionText { get; set; }
					ZString CustomsActionCode { get; }
					ZBool HasSplits { get; }
					ZDateTime CustomsActionDate { get; }
					ZDateTime Status1Date { get; }
				}
				public interface ISplitHouse { }
				public interface ISplitBasic { }
				public interface IGbCcsukMUCREntryNumValidation { }
				public interface ICcsukForwardingShipmentCustomsStatusProvider { }
				public interface ICcsukForwardingConsolCustomsStatusProvider { }
			}

			public static partial class GBChief
			{
				public interface IStatusChecker { }
				public interface IChiefExportConsolIntegrationWrapper
				{
					ZString GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol(Forwarding.IForwardingConsol forwardingConsol);
				}
				public interface ICusAddInfoImplementer
				{
					Type GetMawbExportAddInfoType();
				}
				public interface ISimpleGenerator { }
				public interface IGbMessageManagerCreditCheckWithSecurityHelper { bool ShouldCheckCreditForThisDeclarationAndMessage(); }
			}

			public static partial class GBGVMS
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IGBGVMSEDIMessageTypeDecider : ITypeDecider { }
				public interface ICusGoodsLocation : EU.ICusGoodsLocation { }
			}

			public static partial class GBICS
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
				public interface IAsycudaManifestHeaderBaseTypeDecider : ITypeDecider { }
				public interface IGBICSEDIMessageTypeDecider : ITypeDecider { }
			}

			public static partial class GBH7
			{
				public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }

				public interface IAsycudaBill : ASYCUDA.EUH7.IAsycudaBill, ASYCUDA.IAsycudaBill, ManifestBase.IAsycudaBill { }

				public interface ICusGoodsLocation : EUH7.ICusGoodsLocation, EU.ICusGoodsLocation, Customs.ICusGoodsLocation { }

				public interface IH7MessageSender
				{
					int Send();
				}
			}

			public static class GBCDS
			{
				public interface IGbCDSEdiMessage : IGbEdiMessage { }
				public interface IGBCDSEDIMessageTypeDecider : ITypeDecider { }
				public interface ICDSConsolMessageSender { }
			}

			public interface ICcsukCusunderbondDocumentProvider
			{
				ZString ReportTypeCode { get; }
			}

			public interface IGBCompanyCredentialsPlugIn { }

			public interface ICusPermitHeader { }

			public interface IGBCustomsDataRegistry
			{
				IRegistryItem EnableIcsManifest { get; }

				IRegistryItem EnableSSGBManifest { get; }
			}

			public interface ICusEntryPayInfo { }
		}

		public static partial class GBEMCS
		{
			public interface IEMCSJobDeclaration : EUEMCS.IJobDeclaration { }
			public interface IEMCSEDIMessageTypeDecider : ITypeDecider { }
		}

		public static partial class GBNCTS
		{
			public interface INCTSEDIMessageTypeDecider : ITypeDecider { }
		}

		#endregion

		#region DE

		public static class DE
		{
			public interface IJobDeclarationContext { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart { }
			public interface ICusClassPartPivot : EU.ICusClassPartPivot { }
			public interface IJobComInvoiceLineTax : EU.IJobComInvoiceLineTax { }
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface ICusEntryHeader : EU.ICusEntryHeader { }
			public interface ICusEntryLine : EU.ICusEntryLine { }
			public interface ICusEntryLineFee : EU.ICusEntryLineFee { }
			public interface ICusTempStorageJobHeader : EU.ICusTempStorageJobHeader { }
			public interface ICusTempStorageDecTypeDecider { }
			public interface ICusTempStorageLineTypeDecider { }
			public interface ICusInBondHeader : EU.NCTS.ICusInBondHeader
			{
				ZString EffectiveMessageStatus { get; set; }
			}
			public interface IDepartureMovementHeader : EU.NCTS.IDepartureMovementHeader
			{
				void DeleteGuaranteeTransactions();
			}
			public interface IArrivalMovementHeader : EU.NCTS.IArrivalMovementHeader { }
			public interface ICusTempStorageRegHeader : EU.ICusTempStorageRegHeader { }
			public interface ICusTempStorageRegLine : EU.ICusTempStorageRegLine { }
			public interface ICusTempStorageRegLineTransaction : EU.ICusTempStorageRegLineTransaction { }
			public interface ICusEntryInstruction : EU.ICusEntryInstruction, Customs.ICusEntryInstruction { }
			public interface IEDIMessageTypeDecider { }
			public interface IOrgImpAddInfo { }
			public interface IDepartureCargoDesc : EU.NCTS.IDepartureCargoDesc { }
			public interface IArrivalCargoDesc : EU.NCTS.IArrivalCargoDesc { }
			public interface INctsPackage : EU.NCTS.INctsPackage { }
			public interface INctsCusGoodsLocation : EU.NCTS.ICusGoodsLocation { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IDECustomsRegistry
			{
				IRegistryItem ShowMonthlyClosing { get; }
			}
			public interface IOrgCusCodeValidation { }
			public interface ICusReconDeclaration : Customs.ICusReconDeclaration { }
			public interface ICusReconEntry : Customs.ICusReconEntry { }
			public interface ICusReconEntryLine : Customs.ICusReconEntryLine { }
			public interface ICusGuaranteeHeader : EU.ICusGuaranteeHeader { }
			public interface ICusGuaranteeRule : EU.ICusGuaranteeRule { }
			public interface ICusExitControlHeader : EU.ICusExitControlHeader { }
			public interface ICusExitDetail : EU.ICusExitDetail { }
			public interface ICusExitItem : EU.ICusExitItem { }
			public interface ICusContainer : EU.ICusContainer { }
			public interface ICusGoodsLocation : EU.ICusGoodsLocation { }
			public interface IMonthlyClosingDeclarationTypeListProvider { }
			public interface IPackage : EU.IPackage { }
			public interface INctsMessageProcessor
			{
				Dictionary<string, Type> NctsMessageProcessors { get; }
			}

			public interface IExitControlMessageProcessor
			{
				Dictionary<string, Type> ExitControlMessageProcessors { get; }
			}
		}

		public static partial class DEEMCS
		{
			public interface IEDIMessageTypeDecider { }
			public interface IEMCSJobDeclaration : EUEMCS.IJobDeclaration { }
			public interface IEMCSCusContainer : EUEMCS.ICusContainer { }
		}

		public static partial class DEExitControl
		{
			public interface ICusExitHeader : EUExitControl.ICusExitHeader { }
			public interface ICusExitConsignment : EUExitControl.ICusExitConsignment { }
			public interface ICusExitConsignmentItem : EUExitControl.ICusExitConsignmentItem { }
			public interface ICusExitReport : EUExitControl.ICusExitReport { }
			public interface ICusExitConsignmentPivot : EUExitControl.ICusExitConsignmentPivot { }
		}

		#endregion

		#region IT

		public static partial class IT
		{
			public interface IJobDeclarationContext { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface ICusEntryHeader : EU.ICusEntryHeader { }
			public interface ICusEntryLine : EU.ICusEntryLine { }
			public interface ICusEntryLineFee : EU.ICusEntryLineFee { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface ICusEntryInstruction : EU.ICusEntryInstruction { }
			public interface ICusClassPartPivot : EU.ICusClassPartPivot { }
			public interface ICusInBondHeader : EU.NCTS.ICusInBondHeader { }
			public interface ICustomsOfficesProvider : EU.ICustomsOfficesProvider { }
			public interface IDefermentAccountNumberProvider { }
			public interface IDepartureMovementHeader : EU.NCTS.IDepartureMovementHeader { }
			public interface IDepartureCargoDesc : EU.NCTS.IDepartureCargoDesc { }
			public interface INctsDepartureHeaderContainer : EU.NCTS.INctsDepartureHeaderContainer { }
			public interface INctsCargoDescFee : EU.NCTS.INctsCargoDescFee { }
			public interface INctsPackage : EU.NCTS.INctsPackage { }
			public interface IEDIMessage { }
			public interface IMethodOfPaymentListProvider { }
			public interface IOrgCusCodeValidation { }
			public interface IPackage : EU.IPackage { }
			public interface IEDIInterchange { }
			public interface IOrgSupplierPart { }
			public interface IEntryInstructionStyleListProvider { }
			public interface ICusGuaranteeHeader : EU.ICusGuaranteeHeader { }
			public interface ICusGoodsLocation : EU.ICusGoodsLocation { }
			public interface INctsAuthorization : ICusReference { }
			public interface INctsCusGoodsLocation : EU.NCTS.ICusGoodsLocation { }
			public interface ITemporaryStorageHeader : EU.ITemporaryStorageHeader { }
			public interface ITemporaryStorageBill : EU.ITemporaryStorageBill { }
			public interface ITemporaryStorageCusGoodsLocation : EU.ITemporaryStorageCusGoodsLocation { }
			public interface ITemporaryStorageContainer : EU.ITemporaryStorageContainer { }
			public interface ICusTempStorageRegHeader : EU.ICusTempStorageRegHeader { }
			public interface ICusTempStorageRegLine : EU.ICusTempStorageRegLine { }
		}

		public static partial class ITH7
		{
			public interface IAsycudaManifestHeader : ASYCUDA.EUH7.IAsycudaManifestHeader { }
			public interface ICusGoodsLocation : EUH7.ICusGoodsLocation, EU.ICusGoodsLocation, Customs.ICusGoodsLocation { }
		}

		#endregion

		#region HK

		public static partial class HK
		{
			public interface IExtendedForwardingConsolValidation { }
			public interface IISCProcessor { }
			public interface ITraxonMessage { }
			public interface ITraxonInterchange { }
		}

		#endregion

		#region CA

		public static partial class CA
		{
			public interface ILookupsHelper
			{
				ICodeDescriptionPairList TreatmentCodes(BusinessObjectFactory factory);
			}
			public interface ICAJobComInvoiceHeaderCCNs { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusClassPartPivot : Shared.ICusClassPartPivot { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface ICusStatementHeader
			{
				ZGuid PK { get; }
				ZGuid B2_GC { get; set; }
				ZString B2_EntryFilerCode { get; set; }
			}
			public interface ICusStatementLine
			{
				ZGuid B3_B2 { get; set; }
				ZString B3_EntryFilerCode { get; set; }
				ZString B3_EntryNum { get; set; }
			}
			public interface ICusStatementLineCharge { }
			public interface IGroupInvoiceCharge { }
			public interface IBill : Customs.IBill { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader
			{
				ZString CA_PortOfClearance { get; set; }
			}
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine
			{
				IDutyAndTaxCollection DutiesAndTaxes { get; }
				string TablePrefix { get; }
			}
			public interface IJobDeclaration : IBaseJobDeclaration
			{
				ICusEntryNumber JE_CAEDProofOfReportCusEntryNumber { get; }
				bool IsCADEnabled { get; }
				ZString JE_CERSProofOfReportNumber { get; set; }
				ZString JE_CustomsOffice { get; set; }
				IReleaseStatusWrapper ReleaseStatusWrapper { get; }
				BusinessObjectCollection Bills { get; }
				IActiveCusEntryHeaderCollection ActiveEntryHeaders { get; }
				void UpdateTransactionNumber(ZString accountSecurityCode, ZString sequentialNumber);
			}

			public interface IOrgSupplierPart { }
			public interface IJobDeclarationFilterBusinessObject { }
			public interface IEDIMessage { }
			public interface IEDIMessageTypeDecider { }
			public interface IXmlEDIMessageTypeDecider
			{
				Type GetXmlEDIMessageType(DataRow row, BusinessObjectFactory factory);
			}
			public interface ICusSCAHouse { }
			public interface ICusSCAOceanBill : IBaseCusSCAOceanBill { }
			public interface ICusSCAContainer { }
			public interface ICusSCAPivot { }
			public interface IPackage { }
			public interface ICAOrgSupplierPartDataLoad { }
			public interface IEDIInterchange { }
			public interface IUDMInterchangeTypeDecider : ITypeDecider { }
			public interface IACROSSServiceOptionsCodeDescriptionPairProvider { }
			public interface ICATariffTreatmentCodeDescriptionPairProvider { }
			public interface ICARemissionTypeCodeDescriptionPairProvider { }
			public interface ICABondTypeCodeDescriptionPairProvider { }
			public interface ICACarrierCombinedCollectionProvider { }
			public interface ICACSubLocationCollectionProvider { }
			public interface ICACCBSAOfficeCodesCollectionProvider { }
			public interface IOrgImpAddInfo
			{
				ZBool IsCSAApprovedImporter { get; }
				ZString CAAccountSecurityNumber { get; }
				ZString ZO_LVSInvoiceDetailCode { get; set; }
			}
			public interface ICFSShipmentStatusProvider { }
			public interface ICFSShipmentRNSStatusProvider { }
			public interface ICAValuationBasisListProvider
			{
				ICodeDescriptionPairList GetCodeDescriptionPairList();
			}

			public interface IManualReleaseNote
			{
				ZDateTime ManualReleaseDate { get; }
				ZString ManualReleaseReason { get; }
				ZDateTime ManualReleaseSystemDate { get; }
				ZString ManualReleaseUser { get; }
				ZString NoteText { get; }
			}

			public interface IManualReleaseSupport
			{
				ZString ManualReleaseNoteText { get; }
				void ManualRelease(IManualReleaseNote note);
				void DeleteManualRelease();
				ZString GetReasonForCannotManualRelease();
			}

			public interface IManualCancelSupport
			{
				ZString ManualCancelNoteText { get; }
				void ManualCancel(IManualReleaseNote note);
				ZString GetReasonForCannotManualCancel();
			}

			public interface IManualSubmissionNote
			{
				ZString MessageType { get; }
				ZDateTime ManualSubmissionDate { get; }
				ZString PortOfClearanceOverride { get; }
				ZString NoteText { get; }
				bool IsDeleted { get; }
			}

			public interface IManualSubmissionSupport
			{
				ZString ManualSubmissionNoteText { get; }
				void ManualSubmission(IManualSubmissionNote[] notes);
				ZString GetReasonForCannotManualSubmission(ZString messageType);
			}

			public interface ICusSCAOceanBillProcessTask
			{
			}
			public interface IFreightPercentage : ICusCodeData { }

			public interface ISafeFoodLicense : ICusCodeData { }

			public interface ICusPermitHeader { }

			public interface ICusPermitRule { }

			public interface ICusRuling
			{
				ZString ZZX_RulingType { get; set; }
				ZString ZZX_Description { get; set; }
				ZString ZZX_RulingNumber { get; set; }
				ICACusRulingConfigCollection Configurations { get; }
			}

			public interface ITradeChainPartner { }

			public interface IOrgCusCodeValidation { }

			public interface IJobCAComInvoiceHeader { }

			public interface ICAStateOfOriginCodeDescriptionPairProvider { }

			public interface ICustomsRuleRule { }

			public interface ICAExportMessageStatusListProvider { }
			public interface ICreateDeclarationHelper : Shared.ICreateDeclarationHelper { }
		}

		#endregion

		#region TW
		public static partial class TW
		{
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusClassPartPivot : Shared.ICusClassPartPivot { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface IBill : Customs.IBill { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart { }
			[WTG.StaticAnalysis.Annotation.CodeAlive("To be implemented for this customs country or removed if not used")]
			public interface IOrgSupplierPartDataLoad { }
			public interface ICusInBondHeader : Customs.ICusInBondHeader { }
			public interface IPackage { }
			public interface ITWInterchange { }
			public interface IEDIMessageTypeDecider { }
			public interface IOrgCusCodeValidation { }
			public interface IInvoiceLineRelatedCAHeadersGenPivot { }
			public interface IOrgImpAddInfo { }

			public interface ITWCustomsRegistry
			{
				IRegistryItem CustomsPackingListEnable { get; }
				IRegistryItem EnableBriefCustomsDeclaration { get; }
				IRegistryItem EnableNX201_01 { get; }
				IRegistryItem EnableNX201_07 { get; }
				IRegistryItem EnableNX301 { get; }
				IRegistryItem EnableNX301_AX { get; }
				IRegistryItem EnableNX301_DN { get; }
				IRegistryItem EnableNX401 { get; }
				IRegistryItem EnableNX601 { get; }
				IRegistryItem EnableNX603 { get; }
			}
			public interface IJobComInvoiceLineTax { }
			public interface ITWDeclarationTypeListCodeDescriptionPairProvider { }
			public interface ITWOfficeOfReceiptListDescriptionPairProvider { }
			public interface ITWGoodsLocationCollectionProvider { }
			public interface IJobTWComInvoiceLine { }
			public interface ICusPackingList : Customs.ICusPackingList { }
			public interface ICusPackageJob : Customs.ICusPackageJob { }
			public interface ICusPackage : Customs.ICusPackage { }
			public interface ICusPackableItem { }

			public interface IDocumentOptionsToPrint
			{
				bool GetDocumentOptionsToPrintCustomsDeclaration(IJobDeclaration declaration, ICollection documents);
			}
			public interface IOrgCustomLabels { }
			public interface ICreateDeclarationHelper : Shared.ICreateDeclarationHelper { }
		}
		#endregion

		#region JP

		public static partial class JP
		{
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusClassPartPivot { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface IBill : Customs.IBill { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart { }
			[WTG.StaticAnalysis.Annotation.CodeAlive("To be implemented for this customs country or removed if not used")]
			public interface IOrgSupplierPartDataLoad { }

			public interface IEDIMessage { }

			public interface IOrgCusCodeValidation { }

			public interface IComprehensiveValuation : ICusReference { }
			public interface IOtherLawReference : ICusReference { }
			public interface IGuaranteeReference : ICusReference { }
		}

		#endregion

		#region BR

		public static class BR
		{
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface ICusGoodsCatalog : Customs.ICusGoodsCatalog { }
			public interface IBill : Customs.IBill { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface IOrgCusCodeValidation { }
			public interface IOrgSupplierPart { }
			[WTG.StaticAnalysis.Annotation.CodeAlive("To be implemented for this customs country or removed if not used")]
			public interface IOrgSupplierPartDataLoad { }
			public interface IOrgImpAddInfo { }
			public interface ICusClassPartPivot : Shared.ICusClassPartPivot { }
			public interface IDeclarationRelatedImportLicenseEntryGenPivot { }
			public interface IRelatedEntryInstructionGenPivot { }

			public interface IEDIInterchange { }
			public interface IEDIMessage { }
			public interface IBRCustomsDataRegistry
			{
				bool EnableLPCO { get; }
				bool EnableImportLicense { get; }
				bool EnableForeignOperator { get; }
				bool EnableImportSiscomex { get; }
				bool EnableCatalogModule { get; }
			}
			public interface IClearanceDocAddressControl { }

			public interface ICusLPCOHeader { }
			public interface ICusLPCOHeaderProcessTask { }

			public interface ILocalPartNumber : ICusGoodsCatalogProductionInfo { }

			public interface IAdditionalIdentification : ICusCodeData { }

			public interface ICusBRForeignOperator { }

			public interface IForeignOperator : ICusGoodsCatalogProductionInfo { }
		}

		#endregion

		#region FR

		public static partial class FR
		{
			public interface ICusClassPartPivot : EU.ICusClassPartPivot { }
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart : EU.IOrgSupplierPart { }
			public interface ICusEntryInstruction : EU.ICusEntryInstruction { }
			public interface IEDIMessage { }
			public interface ICusEntryHeader : EU.ICusEntryHeader { }
			public interface ICusEntryLine : EU.ICusEntryLine { }
			public interface ICusEntryLineFee : EU.ICusEntryLineFee { }
			public interface IGroupInvoiceCharge : EU.IGroupInvoiceCharge { }
			public interface ICusTempStorageJobHeader : EU.ICusTempStorageJobHeader { }
			public interface ICusTempStorageDec { }
			public interface ICusTempStorageLine { }
			public interface IRegionOrgImpAddInfo { }
			public interface IOrgImpAddInfo { }
			public interface ICusInBondHeader : EU.NCTS.ICusInBondHeader { }
			public interface IDepartureMovementHeader : EU.NCTS.IDepartureMovementHeader { }
			public interface IArrivalMovementHeader : EU.NCTS.IArrivalMovementHeader { }
			public interface IFREDIMessageTypeDecider : ITypeDecider { }
			public interface ICusTempStorageDecTypeDecider { }
			public interface ICusTempStorageLineItem { }
			public interface ICusGuaranteeHeader : EU.ICusGuaranteeHeader { }
			public interface ICusExitControlHeader : EU.ICusExitControlHeader { }
			public interface ICusExitDetail : EU.ICusExitDetail { }
			public interface ICustomsOfficesProvider : EU.ICustomsOfficesProvider { }
			public interface IProcedureCodesCodeDescriptionPairProvider : Shared.IProcedureCodesCodeDescriptionPairProvider { }
			public interface IISWrittenoffD48CodeDescriptionPairProvider { }
			public interface ICustomsProfileCodeDescriptionPairProvider { }
			public interface IJobDeclarationFilterBusinessObject : EU.IJobDeclarationFilterBusinessObject { }
			public interface ICusGuaranteeRule : EU.ICusGuaranteeRule { }
			public interface INctsEuOfficeCode : EU.NCTS.INctsEuOfficeCode { }
			public interface INctsPackage : EU.NCTS.INctsPackage { }
			public interface IDepartureCargoDesc : EU.NCTS.IDepartureCargoDesc { }
			public interface IFRNctsDepartureHeaderContainer : EU.NCTS.INctsDepartureHeaderContainer { }
			public interface INctsCargoDescFee : EU.NCTS.INctsCargoDescFee { }
			public interface INctsCusGoodsLocation : EU.NCTS.ICusGoodsLocation { }
			public interface ICusStatementHeader { }
			public interface ICusStatementLine { }
			public interface ICusStatementLineCharge { }
			public interface ICusStatementLineTypeDecider { }
			public interface ICusStatementEntry { }
			public interface ICusStatementChargesDetail { }
			public interface ICusTempStorageRegHeader : EU.ICusTempStorageRegHeader { }
			public interface ICusTempStorageRegLine : EU.ICusTempStorageRegLine { }
			public interface ICusTempStorageRegLineTransaction : EU.ICusTempStorageRegLineTransaction { }
			public interface IInvoiceLineCharge : EU.IInvoiceLineCharge { }

			public interface IFRDocAddressControl { }

			public interface IPackage : EU.IPackage { }

			public interface ITemporaryStorageHeader : EU.ITemporaryStorageHeader, IBusiness { }
			public interface ITemporaryStorageBill : EU.ITemporaryStorageBill { }

			public interface ICusAuthorizationUsage : EU.ICusAuthorizationUsage { }

			public interface ICusGoodsLocation : EU.ICusGoodsLocation { }
		}

		public static partial class FRH7
		{
			public interface IH7ManifestHeader : ASYCUDA.EUH7.IAsycudaManifestHeader { }

			public interface ICusGoodsLocation : EUH7.ICusGoodsLocation { }
		}

		#endregion

		#region ES

		public static partial class ES
		{
			public interface ICusClassPartPivot : EU.ICusClassPartPivot { }
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart : EU.IOrgSupplierPart { }
			public interface ICusEntryLine : EU.ICusEntryLine { }
			public interface ICusEntryLineFee : EU.ICusEntryLineFee { }
			public interface ICusEntryInstruction : EU.ICusEntryInstruction { }
			public interface ICusEntryHeader : EU.ICusEntryHeader { }
			public interface ICusEUEntryHeader : EU.ICusEUEntryHeader { }
			public interface ICusInBondHeader : EU.NCTS.ICusInBondHeader { }
			public interface ICusGuaranteeHeader : EU.ICusGuaranteeHeader { }
			public interface IDepartureMovementHeader : EU.NCTS.IDepartureMovementHeader { }
			public interface IArrivalMovementHeader : EU.NCTS.IArrivalMovementHeader { }
			public interface INctsDepartureHeaderContainer : EU.NCTS.INctsDepartureHeaderContainer { }
			public interface IUnloadingMovementHeader : EU.NCTS.IUnloadingMovementHeader { }
			public interface IEDIMessage { }
			public interface IEDIInterchange { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IDepartureCargoDesc : EU.NCTS.IDepartureCargoDesc { }
			public interface IArrivalCargoDesc : EU.NCTS.IArrivalCargoDesc { }
			public interface INctsPackage : EU.NCTS.INctsPackage { }
			public interface IArrivalAndUnloadingCargoDesc : EU.NCTS.IArrivalAndUnloadingCargoDesc { }
			public interface IOrgCusCodeValidation { }
			public interface IOrgImpAddInfo { }
			public interface ICusTempStorageJobHeader : EU.ICusTempStorageJobHeader { }
			public interface ICusTempStorageDec { }
			public interface ICusTempStorageLine : EU.ICusTempStorageLine { }
			public interface ICusTempStorageLineItem : EU.ICusTempStorageLineItem { }
			public interface ITemporaryStorageHeader : EU.ITemporaryStorageHeader { }
			public interface ITemporaryStorageBill : EU.ITemporaryStorageBill { }
			public interface ICusExitControlHeader : EU.ICusExitControlHeader { }
			public interface ICusExitDetail : EU.ICusExitDetail { }
			public interface IESDocC10Header { }
			public interface ICusGuaranteeRule : EU.ICusGuaranteeRule { }
			public interface ICusGoodsLocation : EU.ICusGoodsLocation { }
			public interface INctsCusGoodsLocation : EU.NCTS.ICusGoodsLocation { }
			public interface IPackage : EU.IPackage { }
			public interface ICusVehicle : EU.ICusVehicle { }
			public interface ICusEquipment : EU.ICusEquipment { }

			public interface INctsEuOfficeCode : EU.NCTS.INctsEuOfficeCode { }

			public interface ICusTempStorageRegHeader : EU.ICusTempStorageRegHeader { }
			public interface ICusTempStorageRegLine : EU.ICusTempStorageRegLine { }
			public interface ICusTempStorageRegLineTransaction : EU.ICusTempStorageRegLineTransaction { }
			public interface ICusTempStorageRegLineItemPivot : EU.ICusTempStorageRegLineItemPivot { }
		}

		public static partial class ESExitControl
		{
			public interface ICusExitHeader : EUExitControl.ICusExitHeader { }
			public interface ICusExitReport : EUExitControl.ICusExitReport { }
			public interface ICusExitConsignment : EUExitControl.ICusExitConsignment { }
			public interface ICusExitConsignmentPackage : EUExitControl.ICusExitConsignmentPackage { }
			public interface ICusExitContainer : EUExitControl.ICusExitContainer { }
			public interface ICusExitConsignmentPivot : EUExitControl.ICusExitConsignmentPivot { }
			public interface ICusExitConsignmentItem : EUExitControl.ICusExitConsignmentItem { }
		}

		public static partial class ESH7
		{
			public interface IAsycudaManifestHeader : ASYCUDA.EUH7.IAsycudaManifestHeader { }
			public interface ICusGoodsLocation : EUH7.ICusGoodsLocation { }
		}

		#endregion

		#region TR
		public static partial class TR
		{
			public interface INctsCargoDescFee : EU.NCTS.INctsCargoDescFee { }
			public interface IDepartureCargoDesc : EU.NCTS.IDepartureCargoDesc { }
			public interface ICusInBondHeader : EU.NCTS.ICusInBondHeader
			{
				IBusinessObjectCollection Messages { get; }
				ZString BM_CustomsStatus { get; set; }
				ZString BH_HeaderType { get; set; }
				ZString LrnRegistrationNumber { get; set; }
				ZDateTime LrnRegistrationDate { get; set; }
				ZString ArrivalMrnFromUser { get; set; }
				ZDateTime MrnIssueDateFromUser { get; set; }
			}
			public interface IDepartureMovementHeader : EU.NCTS.IDepartureMovementHeader { }
			public interface ICusInBondSPTSHeader : EU.NCTS.ICusInBondHeader
			{
				IBusinessObjectCollection Messages { get; }
				ZString MessageStatus { get; set; }
				ZDateTime RegistrationDate { get; set; }
				ZString RegistrationNumber { get; set; }
			}
			public interface IOrgCusCodeValidation { }
			public interface IEDIMessageTypeDecider { }
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IInvoiceLineCharge : EU.IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge : EU.IInvoiceLineApportionCharge { }
			public interface IOrgSupplierPart : EU.IOrgSupplierPart { }
			public interface ICusEntryHeader : EU.ICusEntryHeader { }
			public interface ICusEUEntryHeader : EU.ICusEUEntryHeader { }
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusEntryInstruction : EU.ICusEntryInstruction { }
			public interface ICusEntryPayInfo { }
			public interface ICusEntryLine : EU.ICusEntryLine { }
			public interface ICusEntryLineFee : EU.ICusEntryLineFee { }
			public interface ICusGuaranteeHeader : EU.ICusGuaranteeHeader { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceCharge { }
			public interface IEDIInterchange { }

			public interface ICusStatementHeader { }
			public interface ICusStatementLine { }
			public interface ICusStatementLineCharge { }
			public interface ICusContainer : EU.ICusContainer { }
			public interface ICusVehicle : EU.ICusVehicle { }
		}
		#endregion

		#region IE
		public static partial class IE
		{
			public interface ICusAuthorizationUsage : EU.ICusAuthorizationUsage { }
			public interface ICusClassPartPivot : EU.ICusClassPartPivot { }
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart : EU.IOrgSupplierPart { }
			public interface ICusEntryInstruction : EU.ICusEntryInstruction { }
			public interface IGroupInvoiceCharge : EU.IGroupInvoiceCharge { }
			public interface IInvoiceApportionCharge : EU.IInvoiceApportionCharge { }
			public interface IInvoiceCharge : EU.IInvoiceCharge { }
			public interface IInvoiceLineApportionCharge : EU.IInvoiceLineApportionCharge { }
			public interface IInvoiceLineCharge : EU.IInvoiceLineCharge { }
			public interface ICusEntryHeader : EU.ICusEntryHeader { }
			public interface ICusEntryLine : EU.ICusEntryLine { }
			public interface ICusEntryLineFee : EU.ICusEntryLineFee { }
			public interface ICusContainer : EU.ICusContainer { }
			public interface ICusGoodsLocation : EU.ICusGoodsLocation { }
			public interface IEDIMessageTypeDecider { }
			public interface IEDIInterchangeTypeDecider { }
			public interface IPackage : EU.IPackage { }
			public interface ICusGuaranteeHeader : EU.ICusGuaranteeHeader { }
			public interface ITemporaryStorageHeader : EU.ITemporaryStorageHeader { }
			public interface ITemporaryStorageBill : EU.ITemporaryStorageBill { }
			public interface IUCC6TemporaryStorageAdditionalInfosUserControlWithGrid : EU.IUCC6TemporaryStorageAdditionalInfosUserControlWithGrid { }
			public interface IAsycudaManifestHeader : ASYCUDA.IAsycudaManifestHeader { }
			public interface IRegionOrgImpAddInfo { }
		}

		public static partial class IEEMCS
		{
			public interface IEMCSJobDeclaration : EUEMCS.IJobDeclaration { }
			public interface IEDIMessageTypeDecider { }
		}

		public static partial class IEExitControl
		{
			public interface ICusExitHeader : EUExitControl.ICusExitHeader { }
			public interface ICusExitConsignment : EUExitControl.ICusExitConsignment { }
			public interface ICusExitConsignmentItem : EUExitControl.ICusExitConsignmentItem { }
			public interface ICusAuthorizationUsage : EU.ICusAuthorizationUsage { }
			public interface ICusExitReport : EUExitControl.ICusExitReport { }
			public interface ICusExitConsignmentPackage : EUExitControl.ICusExitConsignmentPackage { }
			public interface ICusExitConsignmentPivot : EUExitControl.ICusExitConsignmentPivot { }
			public interface ICusExitContainer : EUExitControl.ICusExitContainer { }
		}

		public static partial class IENCTS
		{
			public interface IEDIMessageTypeDecider { }
			public interface ICusInBondHeader : EU.NCTS.ICusInBondHeader { }
			public interface INctsEuOfficeCode : EU.NCTS.INctsEuOfficeCode { }
			public interface INctsCusGoodsLocation : EU.NCTS.ICusGoodsLocation { }
			public interface IDepartureMovementHeader : EU.NCTS.IDepartureMovementHeader { }
			public interface IArrivalMovementHeader : EU.NCTS.IArrivalMovementHeader { }
			public interface IDepartureCargoDesc : EU.NCTS.IDepartureCargoDesc { }
		}

		public static partial class IEH7
		{
			public interface IAsycudaManifestHeader : ASYCUDA.EUH7.IAsycudaManifestHeader { }

			public interface IAsycudaBill : ASYCUDA.EUH7.IAsycudaBill { }

			public interface ICusGoodsLocation : EUH7.ICusGoodsLocation { }
		}

		public static partial class IEPBN
		{
			public interface IEDIMessageTypeDecider { }
		}

		#endregion

		#region KR
		public static class KR
		{
			public interface IBill : Customs.IBill { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusClassPartPivot : IBaseCusClassPartPivot { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface ICusEntrySnapshot { }
			public interface ICusMiscRequestHeader { }
			public interface ICusMiscRequestLine { }
			public interface ICusStatementHeader { }
			public interface ICusStatementLine { }
			public interface ICusStatementLineCharge { }
			public interface IEDIMessage { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgImpAddInfo { }
			public interface IOrgSupplierPart { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceLineCharge { }
			public interface ICusPollingTransaction : Customs.ICusPollingTransaction { }
			public interface IEDIInterchange { }
			public interface IKRTransportTypeCodeDescriptionPairProvider { }
			public interface IKRTransactionTypeCodeDescriptionPairProvider { }
			public interface IKRExportTypeCodeDescriptionPairProvider { }
			public interface IKRDeclarationProcedureTypeCodeDescriptionPairProvider { }
			public interface IKRInvoicePaymentTermCodeDescriptionPairProvider { }
			public interface IKRCustomsOfficeCollectionProvider { }
			public interface IOrgCusCodeValidation { }
			public interface IKRLocalExportMessageTypeCodeDescriptionPairProvider { }
			public interface IKRLocalExportTransactionNatureCodeDescriptionPairProvider { }
			public interface IKRLocalExportGoodsTypeCodeDescriptionPairProvider { }
			public interface IKRLocalExportDrawbackApplicantTypeCodeDescriptionPairProvider { }
			public interface ICusReconDeclaration : Customs.ICusReconDeclaration { }
			public interface ICusReconEntry : Customs.ICusReconEntry { }
			public interface ICusReconEntryLine : Customs.ICusReconEntryLine { }
			public interface ICusReconCustomsCharge : Customs.ICusReconCustomsCharge { }
			public interface ICusReconSnapshot : Customs.ICusReconSnapshot { }
			public interface ICusVehicle : Customs.ICusVehicle { }
		}
		#endregion

		#region UY
		public static class UY
		{
			public interface IUYMessage { }
			public interface IUYInterchange { }
		}
		#endregion

		#region PL
		public static partial class PL
		{
			public interface ICusClassPartPivot : EU.ICusClassPartPivot { }
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart : EU.IOrgSupplierPart { }
			public interface ICusEntryInstruction : EU.ICusEntryInstruction { }
			public interface IInvoiceCharge { }
			public interface IGroupInvoiceCharge { }
			public interface IEDIMessage { }
			public interface ICusInBondHeader : EU.NCTS.ICusInBondHeader { }
			public interface IDepartureMovementHeader : EU.NCTS.IDepartureMovementHeader { }
			public interface IArrivalMovementHeader : EU.NCTS.IArrivalMovementHeader { }
			public interface IArrivalCargoDesc : EU.NCTS.IArrivalCargoDesc { }
			public interface INctsCusGoodsLocation : EU.NCTS.ICusGoodsLocation { }
			public interface INctsCusAuthorizationUsage : EU.NCTS.ICusAuthorizationUsage { }
			public interface ICusEntryHeader : EU.ICusEntryHeader { }
			public interface ICusEntryLine : EU.ICusEntryLine { }
			public interface ICusEntryLineFee : EU.ICusEntryLineFee { }
			public interface IOrgCusCodeValidation { }
			public interface ICusGuaranteeHeader : EU.ICusGuaranteeHeader { }
			public interface ICusGuaranteeRule : EU.ICusGuaranteeRule { }
			public interface ICusTempStorageJobHeader : EU.ICusTempStorageJobHeader { }
			public interface ICusTempStorageLine : EU.ICusTempStorageLine { }
			public interface ICusTempStorageDec : EU.ICusTempStorageDec { }
			public interface ICusTempStorageLineItem : EU.ICusTempStorageLineItem { }
			public interface IPackingGroup : EU.IPackingGroup { }
			public interface IPackage : EU.IPackage { }
			[SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
			public interface ICusGoodsLocation : EU.ICusGoodsLocation { }
			public interface ICusVehicle : EU.ICusVehicle { }
			public interface INctsEuOfficeCode : EU.NCTS.INctsEuOfficeCode { }
		}

		public static partial class PLNCTS
		{
			public interface IEDIMessage : PL.IEDIMessage { }
		}

		public static partial class PLExitControl
		{
			public interface ICusExitHeader : EUExitControl.ICusExitHeader { }
			public interface ICusExitConsignment : EUExitControl.ICusExitConsignment { }
			public interface ICusExitConsignmentItem : EUExitControl.ICusExitConsignmentItem { }
			public interface ICusExitReport : EUExitControl.ICusExitReport { }
			public interface ICusExitConsignmentPackage : EUExitControl.ICusExitConsignmentPackage { }
			public interface ICusExitConsignmentPivot : EUExitControl.ICusExitConsignmentPivot { }
			public interface ICusExitContainer : EUExitControl.ICusExitContainer { }
			public interface IEDIMessage : PL.IEDIMessage { }
		}
		#endregion

		#region CL

		public static class CL
		{
			public interface ICLCustomsRegistry
			{
				IRegistryItem EnableGlobalManifest { get; }
			}

			public interface ICLMessage { }
		}

		#endregion

		#region BE

		public static class BE
		{
			public interface ICusClassPartPivot : EU.ICusClassPartPivot { }
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart : EU.IOrgSupplierPart { }
			public interface ICusEntryHeader : EU.ICusEntryHeader { }
			public interface ICusEntryLine : EU.ICusEntryLine { }
			public interface ICusEntryLineFee : EU.ICusEntryLineFee { }
			public interface IInvoiceCharge { }
			public interface ICusEntryInstruction : EU.ICusEntryInstruction { }
			public interface ICusContainer : EU.ICusContainer { }
			public interface ICusGoodsLocation : EU.ICusGoodsLocation { }
			public interface IDepartureMovementHeader : EU.NCTS.IDepartureMovementHeader { }
			public interface IArrivalMovementHeader : EU.NCTS.IArrivalMovementHeader { }
			public interface ICusInBondHeader : EU.NCTS.ICusInBondHeader { }
			public interface IEDIInterchange { }
			public interface IEDIMessage { }
			public interface ICusGuaranteeRule : EU.ICusGuaranteeRule { }
			public interface ICusGuaranteeHeader : EU.ICusGuaranteeHeader { }
			public interface INctsCusGoodsLocation : EU.NCTS.ICusGoodsLocation { }
			public interface INctsMessageProcessorProvider
			{
				Dictionary<string, Type> NctsMessageProcessors { get; }
			}
			public interface IDepartureCargoDesc : EU.NCTS.IDepartureCargoDesc { }

			public interface ITemporaryStorageHeader : EU.ITemporaryStorageHeader, ManifestBase.IAsycudaManifestHeader, ICancellable { }
		}

		#endregion

		#region MX
		public static class MX
		{
			public interface IMXMessage { }

			public interface IMXCustomsDataRegistry
			{
				IRegistryItem MXTestingSystem { get; }

				IRegistryItem EnableMXManifests { get; }
			}

			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface IBill : Customs.IBill { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart { }
			[WTG.StaticAnalysis.Annotation.CodeAlive("To be implemented for this customs or removed if not used")]
			public interface IOrgSupplierPartDataLoad { }
			public interface ICusVehicle : Customs.ICusVehicle { }
		}
		#endregion

		#region IN

		public static partial class IN
		{
			public interface IINCustomsDataRegistry
			{
				IRegistryItem INEnableConsolGeneralManifest { get; }
				IRegistryItem INEnableImportGeneralManifest { get; }
			}

			public interface IINMessage { }

			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface IBill : Customs.IBill { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart { }
			[WTG.StaticAnalysis.Annotation.CodeAlive("To be implemented for this customs or removed if not used")]
			public interface IOrgSupplierPartDataLoad { }
			public interface IOrgImpAddInfo { }
		}

		#endregion

		#region NL

		public static class NL
		{
			public interface ICusInBondHeader : EU.NCTS.ICusInBondHeader { }
			public interface IDepartureMovementHeader : EU.NCTS.IDepartureMovementHeader { }
			public interface IArrivalMovementHeader : EU.NCTS.IArrivalMovementHeader { }
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart : EU.IOrgSupplierPart { }
			public interface IOrgImpAddInfo { }
			public interface IOrgCusCodeValidation { }
			public interface IRegionOrgImpAddInfo { }
			public interface IEDIMessage { }
			public interface IEDIMessageTypeDecider { }
			public interface IFiscalReference : ICusSupportingInfo { }
			public interface ICusEntryHeader : EU.ICusEntryHeader { }
			public interface ICusEntryInstruction : EU.ICusEntryInstruction { }
			public interface ICusAuthorizationUsage : EU.ICusAuthorizationUsage { }
			public interface ICusGuaranteeHeader : EU.ICusGuaranteeHeader { }
			public interface ICusEntryLine : EU.ICusEntryLine { }
			public interface ICusEntryLineFee : EU.ICusEntryLineFee { }
			public interface ICusContainer : EU.ICusContainer { }
			public interface ICusClassPartPivot : EU.ICusClassPartPivot { }
			public interface ICusGoodsLocation : EU.ICusGoodsLocation { }
			public interface INctsCusGoodsLocation : EU.NCTS.ICusGoodsLocation { }

			public interface INctsMessageProcessorProvider
			{
				Dictionary<string, Type> NctsMessageProcessors { get; }
			}
			public interface IDepartureCargoDesc : EU.NCTS.IDepartureCargoDesc { }
		}

		#endregion

		#region SE

		public static class SE
		{
			public interface ICusEntryInstruction : EU.ICusEntryInstruction { }
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart : EU.IOrgSupplierPart { }
		}

		#endregion

		#region CH

		public static partial class CH
		{
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface ICusVehicle : Customs.ICusVehicle { }
			public interface IBill : Customs.IBill { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface IOrgSupplierPart { }
			[WTG.StaticAnalysis.Annotation.CodeAlive("To be implemented for this customs or removed if not used")]
			public interface IOrgSupplierPartDataLoad { }
			public interface ICusPermitHeader { }
			public interface IEDIMessage { }
			public interface IPackage { }
			public interface IPackingGroup { }
			public interface IOrgCusCodeValidation { }
			public interface IEDIInterchange { }
			public interface ICusInBondHeader : EU.NCTS.ICusInBondHeader { }
			public interface IDepartureCargoDesc : EU.NCTS.IDepartureCargoDesc { }
			public interface IArrivalCargoDesc : EU.NCTS.IArrivalCargoDesc { }
			public interface INctsEuOfficeCode : EU.NCTS.INctsEuOfficeCode { }
			public interface INctsCusGoodsLocation : EU.NCTS.ICusGoodsLocation { }
			public interface IDepartureMovementHeader : EU.NCTS.IDepartureMovementHeader, ICusInBondMoveHeader { }
			public interface IArrivalMovementHeader : EU.NCTS.IArrivalMovementHeader, ICusInBondMoveHeader { }
			public interface INctsRelatedArrivalGenPivot { }
			public interface INctsRelatedExportEntryHeaderGenPivot { }
		}

		#endregion

		#region AR

		public static class AR
		{
			public interface IARCustomsDataRegistry
			{
				IRegistryItem EnableARManifests { get; }
			}

			public interface IARMessage { }
		}

		#endregion

		#region CO

		public static class CO
		{
			public interface ICOCustomsDataRegistry
			{
				IRegistryItem EnableCOManifests { get; }
			}
		}

		#endregion

		#region NO

		public static class NO
		{
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusClassPartPivot : Shared.ICusClassPartPivot { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface IBill : Customs.IBill { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart { }
			public interface INOCustomsRegistry
			{
				IRegistryItem EnableNOManifests { get; }
			}
			public interface IOrgCusCodeValidation { }
			public interface INOMessage { }
			public interface IEDIMessageTypeDecider { }
			public interface IEmmaEDIInterchange { }
			public interface INOTemporaryStorageRegistry
			{
				bool IsTemporaryStorageRegisterEnabled { get; }
			}
			public interface ICusInBondHeader : EU.NCTS.ICusInBondHeader { }
			public interface IArrivalMovementHeader : EU.NCTS.IArrivalMovementHeader
			{
				ZString GoodsRegistrationNumber { get; set; }
			}
			public interface IEmmaMessageGenerationProcessorProvider : IBaseAutoSendingMessageSupporter
			{
				IProcessor GetEmmaMessageGenerationActionProcessor(ICusEntryHeader entryHeader);
			}
		}

		#endregion

		#region IL

		public static class IL
		{
			public interface IILCustomsDataRegistry
			{
				IRegistryItem ILEnableILManifest { get; }
				IRegistryItem SendILCustomsMessagesWithoutDigitalSignature { get; }
			}
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryInstruction : Customs.ICusEntryInstruction { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface IBill : Customs.IBill { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart { }
			[WTG.StaticAnalysis.Annotation.CodeAlive("To be implemented for this customs or removed if not used")]
			public interface IOrgSupplierPartDataLoad { }
			public interface IEDIMessage { }
			public interface IEDIMessageTypeDecider : ITypeDecider { }
			public interface ILManifestInboundInterchangeImporter : ASYCUDA.IGlobalManifestActionMenuItemInfo { }
			public interface IILGPMMessageBuilder { }
			public interface IILDLOMessageBuilder { }
			public interface IEDIInterchange { }
		}

		#endregion

		#region DK

		public static class DK
		{
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart : EU.IOrgSupplierPart { }
		}

		#endregion

		#region PE

		public static class PE
		{
			public interface IPECustomsDataRegistry
			{
				IRegistryItem EnablePEManifests { get; }
			}
		}

		#endregion

		#region VN
		public static class VN
		{
			public interface IVNCustomsDataRegistry
			{
				IRegistryItem EnableVNManifest { get; }
			}
		}
		#endregion

		#region FI

		public static class FI
		{
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart : EU.IOrgSupplierPart { }
		}

		#endregion

		#region _CustomsTemplate_

		public static partial class _CustomsTemplate_
		{
			public interface ICusEntryHeaderCharges : Customs.ICusEntryHeaderCharges { }
			public interface ICusClassification : IBaseCusClassification { }
			public interface ICusContainer : IBaseCusContainer { }
			public interface ICusEntryHeader : Customs.ICusEntryHeader { }
			public interface ICusEntryLine : Customs.ICusEntryLine { }
			public interface ICusEntryLineFee : Customs.ICusEntryLineFee { }
			public interface IBill : Customs.IBill { }
			public interface IGroupInvoiceCharge { }
			public interface IInvoiceCharge { }
			public interface IInvoiceApportionCharge { }
			public interface IInvoiceLineCharge { }
			public interface IInvoiceLineApportionCharge { }
			public interface IJobComInvoiceGroupHeader : IBaseJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : IBaseJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : IBaseJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart { }
			[WTG.StaticAnalysis.Annotation.CodeAlive("To be implemented for this customs or removed if not used")]
			public interface IOrgSupplierPartDataLoad { }

			#region DocumentWrappers
			public interface IDocDeclaration : IDocBaseJobDeclaration { }
			public interface IDocJobComInvoiceLine : IDocBaseJobComInvoiceLine { }
			#endregion
		}

		#endregion

		#region _EUCustomsTemplate_

		public static class _EUCustomsTemplate_
		{
			public interface IJobComInvoiceGroupHeader : EU.IJobComInvoiceGroupHeader { }
			public interface IJobComInvoiceHeader : EU.IJobComInvoiceHeader { }
			public interface IJobComInvoiceLine : EU.IJobComInvoiceLine { }
			public interface IJobDeclaration : IBaseJobDeclaration { }
			public interface IOrgSupplierPart : EU.IOrgSupplierPart { }
		}

		#endregion

		#region DataTransfer

		public static partial class DataTransfer
		{
			public interface IDeclarationValueObjectDataAdapter { }
			public interface IDeclarationXmlDataImporter { }
		}

		public static partial class ASYCUDA
		{
			public interface IABLEntryNumRelatedPacksGenPivot { }

			public interface IAsycudaManifestUniversalMessagingProcessor : IProcessor { }
		}
		#endregion

		#region TemporaryStorage

		public static partial class TemporaryStorage
		{
			public interface ICusTempStorageRegLineCollection<out TCusTempStorageRegLine> :
				IActiveBusinessObjectCollection<TCusTempStorageRegLine>
			{
			}

			public interface ICusTempStorageRegHeader : IBusiness
			{
				public ZGuid PK { get; }
				public ICusTempStorageRegPremises Premises { get; }
				public ICusTempStorageRegLineCollection<ICusTempStorageRegLine> CusTempStorageRegLines { get; }
				public ZString SRH_Status { get; set; }
				public ZString SRH_Reference { get; set; }
				public ZString SRH_AppCode { get; set; }
				public ZGuid SRH_SRP_Premises { get; set; }
				public ZString SRH_PreviousReferenceType { get; set; }
				public ZString SRH_PreviousReference { get; set; }
				public ZString SRH_InternalReference { get; set; }
				public ZDate SRH_ArrivalDate { get; }
			}

			public interface ICusTempStorageRegLine
			{
				public ZInt SRL_LineNumber { get; set; }
				public ZGuid PK { get; }
				public ZString SRL_CustomsStatus { get; set; }
				public ZString SRL_PackageMarks { get; set; }
				public ICusTempStorageRegLineTransactionCollection<ICusTempStorageRegLineTransaction> CusTempStorageRegLineTransactions { get; }
				public ICusTempStorageRegLineTransaction OpeningBalanceTransaction { get; }
				public ZDecimal GrossWeightRemainingCalculated { get; }
				public ZInt PackagesRemainingCalculated { get; }
				public bool IsPackageTypeBulk { get; }
				public bool IsPackageTypeFrame { get; }
				public bool IsClosed { get; }
				public bool IsOpen { get; }
				public ZString SRL_PackageType { get; set; }
				public ICusTempStorageRegHeader RegHeader { get; }
				public ZDecimal BondAmountRemainingCalculated { get; }
				public ZInt SRL_PackagesRemaining { get; set; }
				public ZString SRL_OwnerReference { get; set; }
				public ZGuid SRL_SRH { get; set; }
				public ZString SRL_LocationOfGoods { get; set; }
				public ZString SRL_GoodsOwnerIdentifier { get; set; }
				public ZInt OriginalPackagesQuantity { get; }
				public ZString TSDItemNumber { get; }
				public ZString CommodityCode { get; }
				public ZString GoodsDescription { get; }
				public ZBool HasManyPivotItems { get; }

				public IRegLineItemQuantityCollection<IRegLineItemQuantity> RegLineItemQuantities { get; }
			}

			public interface ICusTempStorageRegLineTransactionCollection<out TCusTempStorageRegLineTransaction> :
				IActiveBusinessObjectCollection<TCusTempStorageRegLineTransaction>
			{
			}

			public interface IRegLineItemQuantityCollection<out TRegLineItemQuantity> :
				INonPersistentBusinessObjectCollection
			{
			}

			public interface IRegLineItemQuantity
			{
				public ZDecimal SRV_GrossWeight { get; }
				public ICusTempStorageRegLineItem RegLineItem { get; }
			}

			public interface ICusTempStorageRegLineItem
			{
				public ZGuid PK { get; }
				public ZInt SRI_GoodsItemNumber { get; set; }
				public ZString SRI_Tariff { get; }
				public ZString SRI_GoodsDescription { get; }
				public ZString SRI_CusC4Number { get; }
			}

			public interface ICusTempStorageRegLineTransaction : IBusinessObjectState
			{
				public ZGuid PK { get; }
				public ZString SRT_TransactionType { get; set; }
				public ZString SRT_TransactionStatus { get; set; }
				public ZString SRT_InternalReferenceNumber { get; set; }
				public ZString SRT_InternalReferenceType { get; set; }
				public ZDecimal SRT_GrossWeight { get; set; }
				public ZInt SRT_PackageQty { get; set; }
				public ZString SRT_Comments { get; set; }
				public ZString SRT_Reference { get; set; }
				public ZString SRT_ReferenceType { get; set; }
				public ZDecimal SRT_BondAmount { get; set; }
				public ZGuid SRT_SRL { get; set; }
				public ZDateTimeOffset SRT_TransactionDate { get; set; }
				public ZDateTimeOffset SRT_PhysicalInOutDate { get; set; }
				public ICusTempStorageRegLine RegLine { get; }
				public ZDateTime SRT_SystemCreateTimeUtc { get; set; }
			}
			public interface ICusTempStorageRegPremises : IBusinessObjectState, IBusiness
			{
				public ZGuid PK { get; }
				public ZString SRP_Code { get; set; }
				public ZString SRP_Description { get; set; }
				public ZGuid SRP_OA_PremisesAddress { get; set; }
				public ZString SRP_Type { get; set; }
				public ZBool SRP_IsActive { get; set; }
				public ZString SRP_CustomsLocation { get; set; }
			}
			public interface ICusTempStorageRegPremisesCollection
				: IActiveBusinessObjectCollection
			{
			}
			public interface ICusTempStorageRegLineItemPivot
			{
				public ZGuid SRV_SRI_Item { get; set; }
				public ZGuid SRV_SRL_Line { get; set; }
			}
		}

		#endregion
	}

	#endregion

	#region DocWrapper

	public static class DocumentWrappers
	{
		public interface IDocAWB { }
		public interface IDocAgencyShipment { }
		public interface IDocBaseCusEntryHeader { }
		public interface IDocBaseJobDeclaration { }
		public interface IDocWrappersProvider
		{
			Type DocJobDeclarationType { get; }

			IDocBaseJobDeclaration NewDocDeclarationWrapper(Customs.IBaseJobDeclaration declaration, BusinessObjectFactory factoryToWrap);
		}
		public interface IDocCompanyCampaignItem { }
		public interface IDocBaseJobComInvoiceLine { }
		public interface IDocARInvoice { }
		public interface IDocCommonCartage { }
		public interface IDocLoadListConsol { }
		public interface IDocOrganisation { }
		public interface IDocForwardingConsol { }
		public interface IDocForwardingShipment { }
		public interface IDocShipment { }
		public interface IDocGatePassShipment { }
		public interface IDocOrder { }
		public interface IDocDocPackUnpackContainerRego { }
		public interface IDocContactPasswordEmail { }
		public interface IDocContactPasswordInstructionEmail { }
		public interface IDocContactMasterPasswordInstructionEmail { }
		public interface IDocSalesCall { }
		public interface IDocBounceBackEmailProcessResult { }
		public interface IDocRecruitmentRejectionEmail { }
		public interface IDocPersonMergeEmailSender { }
		public interface IDocOrgPartyScreeningStatus { }
		public interface IDocEConversationMessageNotification { }
		public interface IBusinessObjectForCustomFieldsOverridable
		{
			void OverrideBusinessObjectForCustomFields(BusinessObject businessObject);
		}
		public interface IDocARInvoiceDataProvider
		{
			ZString GetTaxId(BusinessObject parent, BusinessObjectFactory factory);

			ZString GetRecipientTaxIDNumber(BusinessObject parent, BusinessObjectFactory factory);
		}
	}

	public interface IDocFreightWrapperCreator
	{
		IDocumentWrapper CreateFreightWrapper(BusinessObject parent, BusinessObjectFactory factory);
		Type GetFreightWrapperType(Type parentType);
	}

	public interface ITransportWrapperProvider
	{
		string GetTransportReference(BusinessObject parent, BusinessObjectFactory factory);
	}

	#endregion

	#region Environment

	public static class Environment
	{
		public interface INullEnvProvider
		{
			void Enable();
		}
	}

	#endregion

	#region Initialisation

	public static class Initialisation
	{
		public interface IInitialiser { }
	}

	#endregion

	#region MarketingManager

	public static class MarketingManager
	{
		public interface ICRMCampaignProcessTasks { }

		public static class GUI
		{
			public interface IGlbCompanyCampaignItemForm { }
		}
	}

	#endregion

	#region Registry

	public static class Registry
	{
		public interface IRegistryBusinessObjectCollectionTemplate { }
	}

	#endregion

	#region Security

	public static class Security
	{
		public interface IInteractiveSecurityOverrideProvider { }
		public interface INonInteractiveSecurityOverrideProvider { }
	}

	#endregion

	#region Startup

	public interface IModuleInitialisedAtDATStartup
	{
		void Initialise();
	}

	#endregion
}
