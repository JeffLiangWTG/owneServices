using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Common.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DepotCusOutturnDataObjectWriter : DataTransfer.Universal.Outturn.CusOutturnDataObjectWriter<DepotCusOutturn>
	{
		public DepotCusOutturnDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override void PopulateCountrySpecificDetails(DepotCusOutturn sourceBO, UShipment shipment)
		{
			PopulateAddInfoCollection(sourceBO, shipment);
			shipment.TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(sourceBO.InlandMovementMode,
				new ZArchitecture.Core.CodeDescriptionPairList(Enterprise.ZArchitecture.Core.OLookUpEditType.LocalCartageTransportModes));
			PopulateConsolidatedCargoStatus(sourceBO, shipment);
		}

		void PopulateConsolidatedCargoStatus(DepotCusOutturn sourceBO, UShipment shipment)
		{
			if (HasRecipientRole(RecipientRoleType.ATW))
			{
				shipment.ConsolidatedCargoStatus = PopulateConsolidatedCargoStatusValue(sourceBO, shipment);
			}
		}

		CodeDescriptionPair PopulateConsolidatedCargoStatusValue(CusOutturn sourceBO, UShipment shipment)
		{
			var status = sourceBO.C5_CustomsStatus;
			ZString code;
			if (status.IsEmpty || status == CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed)
			{
				code = ZString.Empty;
			}
			else if (status == CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased)
			{
				code = CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased;
			}
			else
			{
				code = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			}

			return code.IsEmpty ? null : PopulateValue(shipment.ConsolidatedCargoStatus, false, () => ListHelper.GetWithDescription<CodeDescriptionPair>(code, sourceBO.Factory.GetCachedValue<CMRConsolidatedCargoStatuses>()));
		}

		protected void PopulateAddInfoCollection(DepotCusOutturn sourceBO, UShipment shipment)
		{
			var list = new List<UAddInfo>
			{
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.Consignee,
					Value = sourceBO.ConsigneeName
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.FFInd,
					Value = sourceBO.FreightForwarderIndicator
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.GrossWt,
					Value = sourceBO.GrossWeight
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.Mode,
					Value = sourceBO.InlandMovementMode
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.NetWt,
					Value = sourceBO.NetWeight
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.Site,
					Value = sourceBO.RecipientSiteID
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.UBMDest,
					Value = sourceBO.UBMDestinationID
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.UBMOrg,
					Value = sourceBO.UBMOriginID
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.UBMReason,
					Value = sourceBO.UBMRequestReason
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.UBMPartyID,
					Value = sourceBO.UBMResponsibleID
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.UBMPartyName,
					Value = sourceBO.UBMResponsibleIDName
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.ShipmentOrContainerNumber,
					Value = sourceBO.ShipmentOrContainerNumber
				},
				new UAddInfo
				{
					Key = Outturn.Constants.AddInfoType.LoadList,
					Value = sourceBO.LoadList != null ? sourceBO.LoadList.JK_UniqueConsignRef : ZString.Empty
				}
			};

			shipment.AddInfoCollection.AddRange(list);
		}

		protected override void PopulatePackLine(DepotCusOutturn sourceBO, PackingLine packline)
		{
			packline.PackType = ListHelper.GetWithDescription<PackageType>(
				SeaCargoUtilities.ConvertCMRPackageTypeToPkgUnit(sourceBO.C5_OuterPackUnits),
				sourceBO.Lookups.PackageTypes);
			packline.Weight = sourceBO.GrossWeightInDecimal;
			packline.WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(sourceBO.GrossWeightUQ, BindToLists.GetCachedLists(sourceBO.Factory).WeightUnits);
			packline.Volume = sourceBO.VolumeInDecimal;
			packline.VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(
				SeaCargoUtilities.ConvertCMRVolumeUnitToVolumeUnit(sourceBO.VolumeUQ),
				BindToLists.GetCachedLists(sourceBO.Factory).VolumeUnits);
		}
	}
}
