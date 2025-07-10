using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public sealed class ExportPermitMessageDocumentWrapper(IJPInboundMessageParseResult parseResult, BusinessObjectFactory factory)
	: InboundMessageIncludingItemsDocumentWrapper<IExportClearancePermit, ExportClearancePermitItemDocumentWrapper, ExportClearancePermitItemProvider>(parseResult, factory)
{
	#region Header Fields

	public ZString H_2 => messageProvider?.MainHSCode ?? ZString.Empty;

	public ZString H_3 => messageProvider?.ValueType ?? ZString.Empty;

	public ZString H_4 => messageProvider?.DeclarationType ?? ZString.Empty;

	public ZString H_5 => messageProvider?.DeclarationSubType ?? ZString.Empty;

	public ZString H_6 => messageProvider?.CargoType ?? ZString.Empty;

	public ZString H_7 => messageProvider?.MainPartyType ?? ZString.Empty;

	public ZString H_9 => messageProvider?.InspectionType ?? ZString.Empty;

	public ZString H_10 => messageProvider?.CustomsOffice ?? ZString.Empty;

	public ZString H_11 => messageProvider?.CustomsOfficeDepartment ?? ZString.Empty;

	public ZString H_12 => messageProvider?.DeclarationDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_13 => messageProvider?.DeclarationNumber ?? ZString.Empty;

	public ZString H_14 => messageProvider?.DeclarationCondition ?? ZString.Empty;

	public ZString H_17 => messageProvider?.IntoStorageIndication ?? ZString.Empty;

	public ZString H_18 => messageProvider?.ExporterCode ?? ZString.Empty;

	public ZString H_19 => messageProvider?.ExporterName ?? ZString.Empty;

	public ZString H_20 => messageProvider?.Postcode ?? ZString.Empty;

	public ZString H_21 => messageProvider?.Prefecture ?? ZString.Empty;

	public ZString H_22 => messageProvider?.City ?? ZString.Empty;

	public ZString H_23 => messageProvider?.Street ?? ZString.Empty;

	public ZString H_24 => messageProvider?.AdditionalInformation ?? ZString.Empty;

	public ZString H_25 => messageProvider?.ExporterPhone ?? ZString.Empty;

	public ZString H_26 => messageProvider?.AttorneyForCustomsProceduresCode ?? ZString.Empty;

	public ZString H_27 => messageProvider?.AttorneyForCustomsProceduresAcceptanceNumber ?? ZString.Empty;

	public ZString H_28 => messageProvider?.AttorneyForCustomsProceduresName ?? ZString.Empty;

	public ZString H_29 => messageProvider?.ConsigneeCode ?? ZString.Empty;

	public ZString H_30 => messageProvider?.ConsigneeName ?? ZString.Empty;

	public ZString H_31 => messageProvider?.ConsigneePostcode ?? ZString.Empty;

	public ZString H_32 => messageProvider?.ConsigneeStreet1 ?? ZString.Empty;

	public ZString H_33 => messageProvider?.ConsigneeStreet2 ?? ZString.Empty;

	public ZString H_34 => messageProvider?.ConsigneeCity ?? ZString.Empty;

	public ZString H_35 => messageProvider?.ConsigneeState ?? ZString.Empty;

	public ZString H_36 => messageProvider?.ConsigneeCountryCode ?? ZString.Empty;

	public ZString H_37 => messageProvider?.AgentCode ?? ZString.Empty;

	public ZString H_38 => messageProvider?.AgentName ?? ZString.Empty;

	public ZString H_39 => messageProvider?.CustomsBrokerCode ?? ZString.Empty;

	public ZString H_40 => messageProvider?.InspectionWitness ?? ZString.Empty;

	public ZString H_41 => messageProvider?.ExportControlNumber ?? ZString.Empty;

	public ZString H_42 => messageProvider?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_43 => messageProvider?.QuantityUnit ?? ZString.Empty;

	public ZString H_44 => messageProvider?.AWBNumber ?? ZString.Empty;

	public ZString H_45 => messageProvider?.GrossWeight.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_46 => messageProvider?.GrossWeightUnit ?? ZString.Empty;

	public ZString H_47 => messageProvider?.CustomsDepotCode ?? ZString.Empty;

	public ZString H_48 => messageProvider?.CustomsDepotName ?? ZString.Empty;

	public ZString H_49 => messageProvider?.StorageCustoms ?? ZString.Empty;

	public ZString H_50 => messageProvider?.StorageCustomsDepartment ?? ZString.Empty;

	public ZString H_51 => messageProvider?.FinalDestinationCode ?? ZString.Empty;

	public ZString H_52 => messageProvider?.FinalDestinationName ?? ZString.Empty;

	public ZString H_53 => messageProvider?.PreInspectedCargoType ?? ZString.Empty;

	public ZString H_54 => messageProvider?.PortOfLoading ?? ZString.Empty;

	public ZString H_55 => messageProvider?.PortOfLoadingName ?? ZString.Empty;

	public ZString H_56 => messageProvider?.TradeType ?? ZString.Empty;

	public ZString H_57 => messageProvider?.CustomsInspectionCode ?? ZString.Empty;

	public ZString H_58 => messageProvider?.VesselCode ?? ZString.Empty;

	public ZString H_59 => messageProvider?.VesselName ?? ZString.Empty;

	public ZString H_60 => messageProvider?.DateOfDeparture?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_61 => messageProvider?.LoadingConfirmationDuty ?? ZString.Empty;

	public ZString H_62 => messageProvider?.LoadingConfirmationDomesticConsumptionTax ?? ZString.Empty;

	public ZString H_63 => messageProvider?.LoadingConfirmationOther ?? ZString.Empty;

	public ZString H_64 => messageProvider?.MarksAndNumbers ?? ZString.Empty;

	public ZString H_65 => messageProvider?.ExportApprovalCertificateCategory ?? ZString.Empty;

	public ZString H_66_1 => ApprovalCertificates[0].Type;

	public ZString H_66_2 => ApprovalCertificates[1].Type;

	public ZString H_66_3 => ApprovalCertificates[2].Type;

	public ZString H_66_4 => ApprovalCertificates[3].Type;

	public ZString H_66_5 => ApprovalCertificates[4].Type;

	public ZString H_66_6 => ApprovalCertificates[5].Type;

	public ZString H_66_7 => ApprovalCertificates[6].Type;

	public ZString H_66_8 => ApprovalCertificates[7].Type;

	public ZString H_66_9 => ApprovalCertificates[8].Type;

	public ZString H_66_10 => ApprovalCertificates[9].Type;

	public ZString H_66_11 => ApprovalCertificates[10].Type;

	public ZString H_66_12 => ApprovalCertificates[11].Type;

	public ZString H_66_13 => ApprovalCertificates[12].Type;

	public ZString H_66_14 => ApprovalCertificates[13].Type;

	public ZString H_66_15 => ApprovalCertificates[14].Type;

	public ZString H_67_1 => ApprovalCertificates[0].Number;

	public ZString H_67_2 => ApprovalCertificates[1].Number;

	public ZString H_67_3 => ApprovalCertificates[2].Number;

	public ZString H_67_4 => ApprovalCertificates[3].Number;

	public ZString H_67_5 => ApprovalCertificates[4].Number;

	public ZString H_67_6 => ApprovalCertificates[5].Number;

	public ZString H_67_7 => ApprovalCertificates[6].Number;

	public ZString H_67_8 => ApprovalCertificates[7].Number;

	public ZString H_67_9 => ApprovalCertificates[8].Number;

	public ZString H_67_10 => ApprovalCertificates[9].Number;

	public ZString H_67_11 => ApprovalCertificates[10].Number;

	public ZString H_67_12 => ApprovalCertificates[11].Number;

	public ZString H_67_13 => ApprovalCertificates[12].Number;

	public ZString H_67_14 => ApprovalCertificates[13].Number;

	public ZString H_67_15 => ApprovalCertificates[14].Number;

	public ZString H_68 => messageProvider?.InvoiceType ?? ZString.Empty;

	public ZString H_69 => messageProvider?.InvoiceNumber ?? ZString.Empty;

	public ZString H_70 => messageProvider?.ElectronicInvoiceReceiptNumber ?? ZString.Empty;

	public ZString H_71 => messageProvider?.InvoiceIncoterm ?? ZString.Empty;

	public ZString H_72 => messageProvider?.InvoiceCurrency ?? ZString.Empty;

	public ZString H_73 => messageProvider?.InvoicePrice.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_74 => messageProvider?.InvoiceValuationType ?? ZString.Empty;

	public ZString H_75 => messageProvider?.FOBCurrency ?? ZString.Empty;

	public ZString H_76 => messageProvider?.FOBPrice.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_77_1 => ExchangeRates[0].CurrencyCode;

	public ZString H_77_2 => ExchangeRates[1].CurrencyCode;

	public ZString H_78_1 => ExchangeRates[0].Amount.ToString();

	public ZString H_78_2 => ExchangeRates[1].Amount.ToString();

	public ZString H_79 => messageProvider?.TotalBasicPrice.FormatNumberInDocument() ?? ZString.Empty;

	public ZString H_80 => messageProvider?.HasTotalBasicPrice ?? ZString.Empty;

	public ZString H_81 => messageProvider?.MessageCount.ToString() ?? ZString.Empty;

	public ZString H_82 => messageProvider?.ColumnCount.ToString() ?? ZString.Empty;

	public ZString H_83_1 => VanningLocationCodes[0];

	public ZString H_83_2 => VanningLocationCodes[1];

	public ZString H_83_3 => VanningLocationCodes[2];

	public ZString H_83_4 => VanningLocationCodes[3];

	public ZString H_83_5 => VanningLocationCodes[4];

	public ZString H_84 => messageProvider?.VanningLocationName ?? ZString.Empty;

	public ZString H_85 => messageProvider?.VanningLocationPrefecture ?? ZString.Empty;

	public ZString H_86 => messageProvider?.VanningLocationCity ?? ZString.Empty;

	public ZString H_87 => messageProvider?.VanningLocationStreet ?? ZString.Empty;

	public ZString H_88 => messageProvider?.VanningLocationAdditionalInformation ?? ZString.Empty;

	public ZString H_90 => messageProvider?.ContainerCount.ToString() ?? ZString.Empty;

	public ZString H_91 => messageProvider?.NotesCustoms ?? ZString.Empty;

	public ZString H_92 => messageProvider?.NotesCustomsBroker ?? ZString.Empty;

	public ZString H_93 => messageProvider?.NotesOwner ?? ZString.Empty;

	public ZString H_94 => messageProvider?.OwnerSectionCode ?? ZString.Empty;

	public ZString H_95 => messageProvider?.OwnerReferenceNumber ?? ZString.Empty;

	public ZString H_96 => messageProvider?.InternalReferenceNumber ?? ZString.Empty;

	public ZString H_97 => messageProvider?.UserReferenceNumber ?? ZString.Empty;

	public ZString H_98 => messageProvider?.ExporterImporterInput ?? ZString.Empty;

	public ZString H_100 => messageProvider?.CustomsNotificationColumn ?? ZString.Empty;

	public ZString H_101 => messageProvider?.ApprovalDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_102 => messageProvider?.CustomsOfficeDirectorName ?? ZString.Empty;

	public ZString H_103 => messageProvider?.BondedTransportationApprovalStartDate?.ToNACCSDate() ?? ZString.Empty;

	public ZString H_104 => messageProvider?.BondedTransportationApprovalEndDate?.ToNACCSDate() ?? ZString.Empty;

	#endregion

	#region Collections

	InboundApprovalCertificateProvider[] ApprovalCertificates
	{
		get
		{
			if (approvalCertificates == null)
			{
				approvalCertificates = Enumerable.Range(0, 15).Select(_ => new InboundApprovalCertificateProvider()).ToArray();
				var resultFromProvider = messageProvider?.ApprovalCertificates?.ToArray();
				if (resultFromProvider != null)
				{
					var length = resultFromProvider.Length;
					for (var i = 0; i < length; i++)
					{
						approvalCertificates[i] = resultFromProvider[i];
					}
				}
			}
			return approvalCertificates;
		}
	}
	InboundApprovalCertificateProvider[] approvalCertificates;

	InboundMoneyProvider[] ExchangeRates
	{
		get
		{
			if (exchangeRates == null)
			{
				exchangeRates = Enumerable.Range(0, 2).Select(_ => new InboundMoneyProvider()).ToArray();
				var resultFromProvider = messageProvider?.ExchangeRates?.ToArray();
				if (resultFromProvider != null)
				{
					var length = resultFromProvider.Length;
					for (var i = 0; i < length; i++)
					{
						exchangeRates[i] = resultFromProvider[i];
					}
				}
			}
			return exchangeRates;
		}
	}
	InboundMoneyProvider[] exchangeRates;

	string[] VanningLocationCodes
	{
		get
		{
			if (vanningLocationCodes == null)
			{
				vanningLocationCodes = new string[5];
				var resultFromProvider = messageProvider?.VanningLocationCodes?.ToArray();
				if (resultFromProvider != null)
				{
					for (var i = 0; i < resultFromProvider.Length; i++)
					{
						vanningLocationCodes[i] = resultFromProvider[i];
					}
				}
			}
			return vanningLocationCodes;
		}
	}
	string[] vanningLocationCodes;

	protected override DocumentWrapperCollection<ExportClearancePermitItemDocumentWrapper, ExportClearancePermitItemProvider> GetItemsCore() => new(messageProvider.ExportClearancePermitItems, Factory);

	#endregion

	protected override ZString GetShipmentTypeCore() => Constants.DocumentMessageCodes.ShipmentTypes.Export;
}




