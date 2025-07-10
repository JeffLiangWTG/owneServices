using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UEntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber;
using UEntryType = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryType;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondDataObjectWriter : TopLevelDataObjectWriter<CusUnderbond, UShipment>
	{
		public CusUnderbondDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.UnderBond;

		protected override void PopulateDataObject(CusUnderbond sourceBO, UShipment shipment)
		{
			shipment.WayBillNumber = sourceBO.C4_MAWB;
			shipment.WayBillType = new WayBillType
			{
				Code = WayBillTypeList.Codes.Master,
				Description = WayBillTypeList.Descriptions.Master
			};

			shipment.VoyageFlightNo = sourceBO.C4_FlightNo;
			shipment.TotalNoOfPieces = sourceBO.C4_PiecesManifested;

			shipment.TransportMode = new CodeDescriptionPair
			{
				Code = sourceBO.C4_ModeOfMovement,
				Description = sourceBO.Lookups.ModeOfTransportList.GetDescriptionFromCode(sourceBO.C4_ModeOfMovement)
			};
			shipment.ShipmentType = new CodeDescriptionPair
			{
				Code = sourceBO.C4_MovementReason,
				Description = sourceBO.Lookups.RequestReasonList.GetDescriptionFromCode(sourceBO.C4_MovementReason)
			};

			shipment.EntryStatus = new EntryStatus
			{
				Code = sourceBO.C4_Status,
				Description = sourceBO.ApprovalStatus
			};

			PopulateDates(sourceBO, shipment);
			PopuldateOrganizationAddresses(writeManager, sourceBO, shipment);
			PopulateAdditionalReferences(sourceBO, shipment);
			PopulateAddInfos(sourceBO, shipment);
			PopulateEntryNumbers(sourceBO, shipment);
			PopulateSubShipments(sourceBO, shipment);
		}

		void PopulateSubShipments(CusUnderbond sourceBo, UShipment shipment)
		{
			var data = ProcessCollection(sourceBo.Outturns, new CusOutturnDataObjectWriter(writeManager));
			shipment.SetSubShipmentCollection(() => data != null ? new DataObjectList<UShipment>(data) : null);
		}

		static void PopulateDates(CusUnderbond sourceBO, UShipment shipment)
		{
			shipment.SetDateCollection(() =>
			{
				var list = new List<Date>
				{
					{ DateType.Arrival, ZBool.False, sourceBO.C4_ArrivalDate },
					{ DateType.Unpack, ZBool.False, sourceBO.C4_Outurned }
				};
				return list;
			});
		}

		static void PopuldateOrganizationAddresses(IDataWritingManager writeManager, CusUnderbond sourceBO, UShipment shipment)
		{
			shipment.AddOrgAddress(writeManager, sourceBO.DischargeAddress, Outturn.Constants.AddressType.DischargeAddress);
			shipment.AddOrgAddress(writeManager, sourceBO.OriginAddress, Outturn.Constants.AddressType.OriginAddress);
			shipment.AddOrgAddress(writeManager, sourceBO.DestinationAddress, Outturn.Constants.AddressType.DestinationAddress);
		}

		static void PopulateAdditionalReferences(CusUnderbond sourceBO, UShipment shipment)
		{
			shipment.SetAdditionalReferenceCollection(() =>
			{
				var collection = new DataObjectList<AdditionalReference>
				{
				new AdditionalReference
				{
					Type = new UEntryType
					{
						Code = Outturn.Constants.AdditionalReference.EntryType.Codes.DischargePremiseID,
						Description = Outturn.Constants.AdditionalReference.EntryType.Descriptions.DischargePremiseID
					},
					ContextInformation = Core.Constants.CountryCodes.Australia,
					ReferenceNumber = sourceBO.C4_DischargePremiseID
				},
				new AdditionalReference
				{
					Type = new UEntryType
					{
						Code = Outturn.Constants.AdditionalReference.EntryType.Codes.OriginPremiseID,
						Description = Outturn.Constants.AdditionalReference.EntryType.Descriptions.OriginPremiseID
					},
					ContextInformation = Core.Constants.CountryCodes.Australia,
					ReferenceNumber = sourceBO.C4_OriginPremiseID
				},
				new AdditionalReference
				{
					Type = new UEntryType
					{
						Code = Outturn.Constants.AdditionalReference.EntryType.Codes.DestinationPremiseID,
						Description = Outturn.Constants.AdditionalReference.EntryType.Descriptions.DestinationPremiseID
					},
					ContextInformation = Core.Constants.CountryCodes.Australia,
					ReferenceNumber = sourceBO.C4_DestinationPremiseID
				},
				new AdditionalReference
				{
					Type = new UEntryType
					{
						Code = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
						Description = DataTransfer.Universal.Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID
					},
					ContextInformation = Core.Constants.CountryCodes.Australia,
					ReferenceNumber = sourceBO.C4_ResponsiblePartyID
				}
				};
				return collection;
			});
		}

		static void PopulateAddInfos(CusUnderbond sourceBO, UShipment shipment)
		{
			shipment.SetAddInfoCollection(() => new List<UAddInfo>
			{
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.UnderbondBySeaVoyage,
					Value = sourceBO.C4_UnderbondBySeaVoyage
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.IsMoveFromDischarge,
					Value = sourceBO.C4_IsMoveFromDischarge ? Customs.Business.YesNoList.Codes.Yes : Customs.Business.YesNoList.Codes.No
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.UnderbondBySeaVessel,
					Value = sourceBO.C4_UnderbondBySeaVessel
				}
			});
		}

		static void PopulateEntryNumbers(CusUnderbond sourceBO, UShipment shipment)
		{
			shipment.SetEntryNumberCollection(() =>
			{
				var list = new List<UEntryNumber>
				{
					new UEntryNumber
					{
						Type = new UEntryType
						{
							Code = Outturn.Constants.EntryNumber.EntryType.Codes.UnderbondStatus,
							Description = Outturn.Constants.EntryNumber.EntryType.Descriptions.UnderbondStatus
						},
						CountryOfIssue = new Country { Code = Core.Constants.CountryCodes.Australia },
						EntryStatus = new EntryStatus
						{
							Code = sourceBO.UnderbondStatus.Code,
							Description = sourceBO.UnderbondStatus.Description
						}
					}
				};

				return list;
			});
		}
	}
}
