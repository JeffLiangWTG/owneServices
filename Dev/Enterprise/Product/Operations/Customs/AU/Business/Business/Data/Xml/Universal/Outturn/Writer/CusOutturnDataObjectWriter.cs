using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;
using UAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UEntryType = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryType;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnDataObjectWriter : TopLevelDataObjectWriter<CusOutturn, UShipment>
	{
		public CusOutturnDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.Outturn;

		protected override void PopulateDataObject(CusOutturn sourceBO, UShipment shipment)
		{
			shipment.ShipmentType = new CodeDescriptionPair
			{
				Code = sourceBO.C5_OutturnResultType,
				Description = sourceBO.Lookups.OutturnResultTypeList.GetDescriptionFromCode(sourceBO.C5_OutturnResultType)
			};
			shipment.WayBillType = new WayBillType
			{
				Code = WayBillTypeList.Codes.House,
				Description = WayBillTypeList.Descriptions.House
			};
			shipment.WayBillNumber = sourceBO.C5_HouseBill;
			shipment.OuterPacks = sourceBO.C5_OuterPacks;
			shipment.OuterPacksPackageType = new PackageType
			{
				Code = sourceBO.C5_OuterPackUnits,
				Description = sourceBO.Lookups.PackageTypes.GetDescriptionFromCode(sourceBO.C5_OuterPackUnits)
			};
			shipment.TotalNoOfPacks = sourceBO.C5_PackagesOutturned;
			shipment.EntryStatus = new EntryStatus
			{
				Code = sourceBO.CustomsStatus.Code,
				Description = sourceBO.CustomsStatus.Description
			};

			PopulateAddInfos(sourceBO, shipment);
			PopulateNotes(sourceBO, shipment);
			PopulateAdditionalReferences(sourceBO, shipment);
		}

		static void PopulateAddInfos(CusOutturn sourceBO, UShipment shipment)
		{
			shipment.SetAddInfoCollection(() => new List<UAddInfo>
			{
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.IsDamage,
					Value = sourceBO.C5_DamageIndicator ? YesNoList.Codes.Yes : YesNoList.Codes.No
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.IsPillage,
					Value = sourceBO.C5_PillageIndicator ? YesNoList.Codes.Yes : YesNoList.Codes.No
				}
			});
		}

		static void PopulateNotes(CusOutturn sourceBO, UShipment shipment)
		{
			shipment.SetNoteCollection(() => new DataObjectList<Note>(new[]
			{
				new Note
				{
					Description = Outturn.Constants.Note.Descriptions.GoodsDescription,
					IsCustomDescription = ZBool.True,
					NoteText = sourceBO.C5_GoodsDescription
				}
			}));
		}

		static void PopulateAdditionalReferences(CusOutturn sourceBO, UShipment shipment)
		{
			var collection = new DataObjectList<AdditionalReference>
			{
				new AdditionalReference
				{
					Type = new UEntryType
					{
						Code = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
						Description = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID
					},
					ContextInformation = Core.Constants.CountryCodes.Australia,
					ReferenceNumber = sourceBO.UnderbondResponsiblePartyID
				},
			};
			shipment.SetAdditionalReferenceCollection(() => collection);
		}
	}
}
