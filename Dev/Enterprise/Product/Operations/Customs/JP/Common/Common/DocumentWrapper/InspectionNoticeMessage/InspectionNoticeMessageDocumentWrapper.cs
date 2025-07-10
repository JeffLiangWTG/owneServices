using System;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions.Inbound;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.JP.Common.JPOutputInformationCodeList;

namespace Enterprise.Customs.JP.Common
{
	public sealed class InspectionNoticeMessageDocumentWrapper(IJPInboundMessageParseResult parseResult, BusinessObjectFactory factory) : InboundMessageDocumentWrapper<IInspectionInformation>(parseResult, factory)
	{
		#region Header Fields
		public ZString H_2 => messageProvider?.InspectionTypeContent ?? ZString.Empty;

		public ZString H_3 => messageProvider?.DeclarationNumber ?? ZString.Empty;

		public ZString H_4 => messageProvider?.DeclarationCategory ?? ZString.Empty;

		public ZString H_5 => messageProvider?.DeclarationType ?? ZString.Empty;

		public ZString H_6 => messageProvider?.DeclarationCondition ?? ZString.Empty;

		public ZString H_7 => messageProvider?.DeclarantCode ?? ZString.Empty;

		public ZString H_8 => messageProvider?.CustomsOffice ?? ZString.Empty;

		public ZString H_9 => messageProvider?.CustomsOfficeDepartment ?? ZString.Empty;

		public ZString H_10 => messageProvider?.CargoNumber ?? ZString.Empty;

		public ZString H_11 => messageProvider?.MAWBNumber ?? ZString.Empty;

		public ZString H_12 => messageProvider?.ClearanceWarehouse ?? ZString.Empty;

		public ZString H_13 => messageProvider?.CustomsDepotName ?? ZString.Empty;

		public ZString H_14 => messageProvider?.StorageCustoms ?? ZString.Empty;

		public ZString H_15 => messageProvider?.StorageCustomsDepartment ?? ZString.Empty;

		public ZString H_16 => messageProvider?.InspectionWitness ?? ZString.Empty;

		public ZString H_17 => messageProvider?.Quantity.FormatNumberInDocument() ?? ZString.Empty;

		public ZString H_18 => messageProvider?.QuantityUnit ?? ZString.Empty;

		public ZString H_19 => messageProvider?.GrossWeight.FormatNumberInDocument() ?? ZString.Empty;

		public ZString H_20 => messageProvider?.GrossWeightUnit ?? ZString.Empty;

		public ZString H_21 => messageProvider?.Volume.FormatNumberInDocument() ?? ZString.Empty;

		public ZString H_22 => messageProvider?.VolumeUnit ?? ZString.Empty;

		public ZString H_23 => messageProvider?.IncidentCode ?? ZString.Empty;

		public ZString H_24 => messageProvider?.VesselCode ?? ZString.Empty;

		public ZString H_25 => messageProvider?.VesselName1 ?? ZString.Empty;

		public ZString H_26 => messageProvider?.VesselName2 ?? ZString.Empty;

		public ZString H_27 => messageProvider?.HasThreeOrMoreSplitFights ?? ZString.Empty;

		public ZString H_28 => messageProvider?.GoodsDescription ?? ZString.Empty;

		public ZString H_29 => messageProvider?.ImporterExporterCode ?? ZString.Empty;

		public ZString H_30 => messageProvider?.ImporterExporterName ?? ZString.Empty;

		public ZString H_31 => messageProvider?.AttorneyForCustomsProceduresCode ?? ZString.Empty;

		public ZString H_32 => messageProvider?.AttorneyForCustomsProceduresNumber ?? ZString.Empty;

		public ZString H_33 => messageProvider?.AttorneyForCustomsProceduresName ?? ZString.Empty;

		public ZString H_34 => messageProvider?.NotesCustoms ?? ZString.Empty;

		public ZString H_35 => messageProvider?.NotesCustomsBroker ?? ZString.Empty;

		public ZString H_36 => messageProvider?.MarksNumbers ?? ZString.Empty;

		public ZString H_37 => messageProvider?.ContainerCount.FormatNumberInDocument() ?? ZString.Empty;

		public ZString H_38_1 => InspectionContainerNumbers.ElementAtOrDefault(0);

		public ZString H_38_2 => InspectionContainerNumbers.ElementAtOrDefault(1);

		public ZString H_38_3 => InspectionContainerNumbers.ElementAtOrDefault(2);

		public ZString H_38_4 => InspectionContainerNumbers.ElementAtOrDefault(3);

		public ZString H_38_5 => InspectionContainerNumbers.ElementAtOrDefault(4);

		public ZString H_39 => messageProvider?.HasMoreThan5Containers ?? ZString.Empty;

		public ZString H_40 => messageProvider?.Location ?? ZString.Empty;

		public ZString H_41 => messageProvider?.Reserved ?? ZString.Empty;

		public ZString H_42 => messageProvider?.AgentCode ?? ZString.Empty;

		public ZString H_43 => messageProvider?.IntoStorageDate?.ToNACCSDate() ?? ZString.Empty;

		public ZString H_44 => messageProvider?.SpecialCargoCode ?? ZString.Empty;

		public ZString H_45 => messageProvider?.DestinationCode ?? ZString.Empty;

		public ZString H_46 => messageProvider?.InternalReferenceNumber ?? ZString.Empty;

		public ZString H_47 => messageProvider?.ExaminationType ?? ZString.Empty;

		public ZString H_48 => messageProvider?.ExaminationTypeOutput ?? ZString.Empty;

		public ZString H_49 => messageProvider?.OriginalExaminationType ?? ZString.Empty;

		public ZString H_50 => messageProvider?.InspectionType ?? ZString.Empty;

		public ZString H_51 => messageProvider?.InspectionTypeContent2 ?? ZString.Empty;

		public ZString H_52 => messageProvider?.InspectionPurpose ?? ZString.Empty;

		public ZString H_53 => messageProvider?.InspectionPurposeContent ?? ZString.Empty;

		public ZString H_54 => messageProvider?.AssignmentId ?? ZString.Empty;

		public ZString H_55 => messageProvider?.InspectionTypeChanger ?? ZString.Empty;

		public ZString H_56 => messageProvider?.InspectionLocation ?? ZString.Empty;

		public ZString H_57 => messageProvider?.InspectionLocationName ?? ZString.Empty;

		public ZString H_58 => messageProvider?.PostInspectionLocation ?? ZString.Empty;

		public ZString H_59 => messageProvider?.PostInspectionLocationName ?? ZString.Empty;

		public ZString H_60 => messageProvider?.InspectionContent ?? ZString.Empty;

		public ZString H_61 => messageProvider?.InspectionMethod ?? ZString.Empty;

		public ZString H_62 => messageProvider?.InspectionMethodName ?? ZString.Empty;

		public ZString H_63 => messageProvider?.FullInspectionIndicator ?? ZString.Empty;

		public ZString H_64 => messageProvider?.SameInspectionCategoryIndicator ?? ZString.Empty;

		public ZString H_65 => InspectionDateTime.ToNACCSDate();

		public ZString H_66 => InspectionDateTime.ToNACCSTime();
		#endregion

		protected override ZString GetTemplateTypeCore()
		{
			return (string)OutputInformationCode switch
			{
				JPInspectionInformationCodeList.Codes.AAD4991 or JPInspectionInformationCodeList.Codes.AAE5011 => (ZString)"検査指定票（申告書用）",
				JPInspectionInformationCodeList.Codes.SAD4881 or JPInspectionInformationCodeList.Codes.SAE4751 or JPInspectionInformationCodeList.Codes.AAD4881 or JPInspectionInformationCodeList.Codes.AAE4751 => (ZString)"検査指定票（倉主等用）",
				JPInspectionInformationCodeList.Codes.SAD4891 or JPInspectionInformationCodeList.Codes.SAE4761 or JPInspectionInformationCodeList.Codes.AAD4891 or JPInspectionInformationCodeList.Codes.AAE4761 => (ZString)"検査指定票（運搬・倉主等用）",
				JPInspectionInformationCodeList.Codes.SAD4911 or JPInspectionInformationCodeList.Codes.SAE4781 or JPInspectionInformationCodeList.Codes.AAD4911 or JPInspectionInformationCodeList.Codes.AAE4781 => (ZString)"検査取止票",
				JPInspectionInformationCodeList.Codes.SAD4931 => (ZString)"運送指定票",
				JPInspectionInformationCodeList.Codes.SAE4851 or JPInspectionInformationCodeList.Codes.AAE4851 => (ZString)"検査指定票（特定輸出申告）",
				JPInspectionInformationCodeList.Codes.SAD6300 or JPInspectionInformationCodeList.Codes.SAE5550 or JPInspectionInformationCodeList.Codes.AAD6300 or JPInspectionInformationCodeList.Codes.AAE5550 => (ZString)"検査取消票",
				_ => ZString.Empty,
			};
		}

		protected override ZString GetShipmentTypeCore()
		{
			return (string)OutputInformationCode switch
			{
				JPInspectionInformationCodeList.Codes.AAD4991 or JPInspectionInformationCodeList.Codes.SAD4881 or JPInspectionInformationCodeList.Codes.AAD4881 or JPInspectionInformationCodeList.Codes.SAD4891 or JPInspectionInformationCodeList.Codes.AAD4891 or JPInspectionInformationCodeList.Codes.SAD4911 or JPInspectionInformationCodeList.Codes.AAD4911 or JPInspectionInformationCodeList.Codes.SAD4931 or JPInspectionInformationCodeList.Codes.SAD6300 or JPInspectionInformationCodeList.Codes.AAD6300 => Constants.DocumentMessageCodes.ShipmentTypes.Import,
				JPInspectionInformationCodeList.Codes.AAE5011 or JPInspectionInformationCodeList.Codes.SAE4751 or JPInspectionInformationCodeList.Codes.AAE4751 or JPInspectionInformationCodeList.Codes.SAE4761 or JPInspectionInformationCodeList.Codes.AAE4761 or JPInspectionInformationCodeList.Codes.SAE4781 or JPInspectionInformationCodeList.Codes.AAE4781 or JPInspectionInformationCodeList.Codes.SAE4851 or JPInspectionInformationCodeList.Codes.AAE4851 or JPInspectionInformationCodeList.Codes.SAE5550 or JPInspectionInformationCodeList.Codes.AAE5550 => Constants.DocumentMessageCodes.ShipmentTypes.Export,
				_ => ZString.Empty,
			};
		}

		DateTime InspectionDateTime
		{
			get
			{
				if (inspectionDateTime == null)
				{
					var inspectionDate = messageProvider?.InspectionDate;
					var inspectionTime = messageProvider?.InspectionTime;
					if (inspectionDate.HasValue && inspectionTime.HasValue && DateTime.TryParseExact($"{inspectionDate.Value:yyyyMMdd}{inspectionTime.Value.ToString().PadLeft(6, '0')}", "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
					{
						inspectionDateTime = result;
					}
				}
				return inspectionDateTime.GetValueOrDefault();
			}
		}
		DateTime? inspectionDateTime;

		string[] InspectionContainerNumbers => messageProvider?.InspectionContainerNumber?.ToArray() ?? [];
	}
}
