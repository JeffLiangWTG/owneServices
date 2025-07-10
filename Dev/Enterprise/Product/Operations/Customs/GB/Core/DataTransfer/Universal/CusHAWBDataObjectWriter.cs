using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AirManifest;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.DataTransfer.Universal
{
	public class CusHAWBDataObjectWriter : CusHAWBDataObjectWriter<CusHAWB, AirManifestDataObjectWriterHelper>
	{
		public CusHAWBDataObjectWriter(IDataWritingManager manager, AirManifestDataObjectWriterHelper helper)
			: base(manager, helper)
		{
		}

		protected override AirManifestDataObjectWriterHelper GetNewAirManifestLineDataObjectWriterHelper(CusHAWB hawbBO)
		{
			return helper == null ? new AirManifestDataObjectWriterHelper(hawbBO) : new AirManifestDataObjectWriterHelper(hawbBO, helper);
		}

		protected override CusHAWBDataObjectWriter<CusHAWB, AirManifestDataObjectWriterHelper> GetNewCusHAWBDataObjectWriter(AirManifestDataObjectWriterHelper hawbHelper)
		{
			return new CusHAWBDataObjectWriter(writeManager, hawbHelper);
		}

		protected override void PopulateCountrySpecificDetails(Shipment hawbData, CusHAWB hawbBO, AirManifestDataObjectWriterHelper hawbHelper, bool keepExistingData)
		{
			base.PopulateCountrySpecificDetails(hawbData, hawbBO, hawbHelper, keepExistingData);

			var lookups = hawbBO.Lookups;
			var refUNLOCOList = hawbBO.Factory.GetRefUNLOCOList();

			hawbData.TotalNoOfPiecesLanded = PopulateValue(hawbData.TotalNoOfPiecesLanded, keepExistingData, () => hawbBO.CS_PiecesLanded);

			if (!hawbBO.CS_FolioReference.IsEmpty)
			{
				hawbData.Folio = PopulateValue(hawbData.Folio, keepExistingData, () => hawbBO.CS_FolioReference);
				hawbData.CustomsProfileIdentifier = PopulateValue(hawbData.CustomsProfileIdentifier, keepExistingData, () => new ValueTypePair { Type = "PIMA", Value = hawbBO.CS_FolioReference });
			}

			if (!hawbBO.CS_ShipmentType.IsEmpty)
			{
				hawbData.ShipmentSubType = PopulateValue(hawbData.ShipmentSubType, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(hawbBO.CS_ShipmentType, lookups.ShipmentDescriptionCodeList));
			}

			if (!hawbBO.ConsignmentOrEntryType.IsEmpty)
			{
				hawbData.ShipmentType = PopulateValue(hawbData.ShipmentType, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(hawbBO.ConsignmentOrEntryType, lookups.ConsignmentOrEntryTypesList));
			}

			if (!hawbBO.AirportOfOrigin.IsEmpty)
			{
				hawbData.PortOfLoading = PopulateValue(hawbData.PortOfLoading, keepExistingData, () => ListHelper.GetWithName(hawbBO.AirportOfOrigin, refUNLOCOList));
			}

			if (!hawbBO.AirportOfArrival.IsEmpty)
			{
				hawbData.PortOfFirstArrival = PopulateValue(hawbData.PortOfFirstArrival, keepExistingData, () =>
				{
					var codeDesc = ListHelper.GetWithDescription<CodeDescriptionPair>(hawbBO.AirportOfArrival, lookups.UkInventoryControlledAirportsList);
					return new UNLOCO
					{
						Code = codeDesc.Code,
						Name = codeDesc.Description
					};
				});
			}

			if (!hawbBO.CargoTerminalOperatorAirportAndShed.IsEmpty)
			{
				hawbData.WarehouseLocation = PopulateValue(hawbData.WarehouseLocation, keepExistingData, () => hawbBO.CargoTerminalOperatorAirportAndShed);
			}

			if (!hawbBO.CS_CustomsStatus.IsEmpty)
			{
				hawbData.EntryStatus = PopulateValue(hawbData.EntryStatus, keepExistingData, () => ListHelper.GetWithDescription<EntryStatus>(hawbBO.CS_CustomsStatus, lookups.CustomsActionsCodes));
			}

			if (!hawbBO.PresenceOnNetworkStatus.IsEmpty)
			{
				hawbData.MessageStatus = PopulateValue(hawbData.MessageStatus, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(hawbBO.PresenceOnNetworkStatus, lookups.PresenceOnNetworkList));
			}

			if (!hawbBO.LatestCustomsActionText.IsEmpty)
			{
				hawbData.AddAddInfo("CustomsActionText", hawbBO.LatestCustomsActionText);
			}

			if (!hawbBO.CustomsActionDate.IsEmpty)
			{
				hawbData.AddAddInfo("CustomsActionDate", hawbBO.CustomsActionDate);
			}

			if (!hawbBO.TemporaryStorageEndDate.IsEmpty)
			{
				hawbData.AddAddInfo("StorageDate", hawbBO.TemporaryStorageEndDate);
			}

			if (!hawbBO.Status1Date.IsEmpty)
			{
				hawbData.AddAddInfo("Status1Date", hawbBO.Status1Date);
			}

			if (!hawbBO.Status2Granted.IsEmpty)
			{
				hawbData.AddAddInfo("Status2", hawbBO.Status2Granted);
			}

			if (hawbBO.HasSplits)
			{
				hawbData.SetAdditionalBillCollection(() => hawbData.AdditionalBillCollection ?? new List<AdditionalBill>());
				foreach (SplitHouse split in hawbBO.Splits)
				{
					var additionalBill = new AdditionalBill(writeManager.WriterStrategy)
					{
						BillNumber = split.SplitReference,
						MessageStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(split.PresenceOnNetworkStatus, lookups.PresenceOnNetworkList),
						NoOfPacks = (ZDecimal)split.NumberOfPiecesExpected,
					};
					additionalBill.AddAddInfo("Agent", split.AgentBadge);
					additionalBill.AddAddInfo("Weight", split.Weight);
					additionalBill.AddAddInfo("WeightUnit", split.WeightCode);
					additionalBill.AddAddInfo("NumberOfPiecesReceived", split.NumberOfPiecesReceived);
					additionalBill.AddAddInfo("Status1Date", split.Status1Date);
					additionalBill.AddAddInfo("CustomsActionText", split.LatestCustomsActionText);
					additionalBill.AddAddInfo("CustomsActionDate", split.CustomsActionDate);
					additionalBill.AddAddInfo("CustomsActionCode", split.CustomsActionCode);
					additionalBill.AddAddInfo("CustomsActionCodeDescription", (ZString)ListHelper.GetDescription(split.CustomsActionCode, lookups.CustomsActionsCodes));
					additionalBill.AddAddInfo("HandlingInformation", split.HandlingInformationForBinding);
					additionalBill.AddAddInfo("StorageDate", split.TemporaryStorageEndDate);
					hawbData.AdditionalBillCollection.Add(additionalBill);
				}
			}
		}
	}
}
