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
	public class CusMAWBDataObjectWriter : CusMAWBDataObjectWriter<CusMAWB, CusHAWB, AirManifestDataObjectWriterHelper>
	{
		public CusMAWBDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override AirManifestDataObjectWriterHelper CreateNewHVLVAirDataObjectWriterHelper(CusMAWB mawbBO)
		{
			return new AirManifestDataObjectWriterHelper(mawbBO);
		}

		protected override CusHAWBDataObjectWriter<CusHAWB, AirManifestDataObjectWriterHelper> GetNewCusHAWBDataObjectWriter(AirManifestDataObjectWriterHelper mawbHelper)
		{
			return new CusHAWBDataObjectWriter(writeManager, mawbHelper);
		}

		protected override void PopulateCountrySpecificData(CusMAWB mawbBO, Shipment mawbData, AirManifestDataObjectWriterHelper mawbHelper, bool keepExistingData)
		{
			base.PopulateCountrySpecificData(mawbBO, mawbData, mawbHelper, keepExistingData);

			var lookups = mawbBO.Lookups;
			var refUNLOCOList = mawbBO.Factory.GetRefUNLOCOList();

			mawbData.TotalNoOfPiecesLanded = PopulateValue(mawbData.TotalNoOfPiecesLanded, keepExistingData, () => mawbBO.NumberOfPiecesReceived);

			if (!mawbBO.Profile.IsEmpty)
			{
				mawbData.Folio = PopulateValue(mawbData.Folio, keepExistingData, () => mawbBO.Profile);
				mawbData.CustomsProfileIdentifier = PopulateValue(mawbData.CustomsProfileIdentifier, keepExistingData, () => new ValueTypePair { Type = "PIMA", Value = mawbBO.Profile });
			}

			if (!mawbBO.ShipmentDescriptionCode.IsEmpty)
			{
				mawbData.ShipmentSubType = PopulateValue(mawbData.ShipmentSubType, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(mawbBO.ShipmentDescriptionCode, mawbBO.MasterLevelHouseHelper.Lookups.ShipmentDescriptionCodeList));
			}

			if (!mawbBO.ConsignmentOrEntryType.IsEmpty)
			{
				mawbData.ShipmentType = PopulateValue(mawbData.ShipmentType, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(mawbBO.ConsignmentOrEntryType, mawbBO.MasterLevelHouseHelper.Lookups.ConsignmentOrEntryTypesList));
			}

			if (!mawbBO.AirportOfOrigin.IsEmpty)
			{
				mawbData.PortOfLoading = PopulateValue(mawbData.PortOfLoading, keepExistingData, () => ListHelper.GetWithName(mawbBO.AirportOfOrigin, refUNLOCOList));
			}

			if (!mawbBO.AirportOfArrival.IsEmpty)
			{
				mawbData.PortOfFirstArrival = PopulateValue(mawbData.PortOfFirstArrival, keepExistingData, () =>
				{
					var codeDesc = ListHelper.GetWithDescription<CodeDescriptionPair>(mawbBO.AirportOfArrival, mawbBO.MasterLevelHouseHelper.Lookups.UkInventoryControlledAirportsList);
					return new UNLOCO
					{
						Code = codeDesc.Code,
						Name = codeDesc.Description
					};
				});
			}

			if (!mawbBO.CargoTerminalOperatorAirportAndShed.IsEmpty)
			{
				mawbData.WarehouseLocation = PopulateValue(mawbData.WarehouseLocation, keepExistingData, () => mawbBO.CargoTerminalOperatorAirportAndShed);
			}

			if (!mawbBO.CustomsActionCode.IsEmpty)
			{
				mawbData.EntryStatus = PopulateValue(mawbData.EntryStatus, keepExistingData, () => ListHelper.GetWithDescription<EntryStatus>(mawbBO.CustomsActionCode, lookups.CustomsActionsCodes));
			}

			if (!mawbBO.PresenceOnNetworkStatus.IsEmpty)
			{
				mawbData.MessageStatus = PopulateValue(mawbData.MessageStatus, keepExistingData, () => ListHelper.GetWithDescription<CodeDescriptionPair>(mawbBO.PresenceOnNetworkStatus, mawbBO.MasterLevelHouseHelper.Lookups.PresenceOnNetworkList));
			}

			if (!mawbBO.LatestCustomsActionText.IsEmpty)
			{
				mawbData.AddAddInfo("CustomsActionText", mawbBO.LatestCustomsActionText);
			}

			if (!mawbBO.CustomsActionDate.IsEmpty)
			{
				mawbData.AddAddInfo("CustomsActionDate", mawbBO.CustomsActionDate);
			}

			if (!mawbBO.TemporaryStorageEndDate.IsEmpty)
			{
				mawbData.AddAddInfo("StorageDate", mawbBO.TemporaryStorageEndDate);
			}

			if (!mawbBO.Status1Date.IsEmpty)
			{
				mawbData.AddAddInfo("Status1Date", mawbBO.Status1Date);
			}

			if (!mawbBO.Status2Granted.IsEmpty)
			{
				mawbData.AddAddInfo("Status2", mawbBO.Status2Granted);
			}

			if (mawbBO.HasSplits)
			{
				mawbData.SetAdditionalBillCollection(() => mawbData.AdditionalBillCollection ?? new List<AdditionalBill>());
				foreach (SplitBasic split in mawbBO.Splits)
				{
					var additionalBill = new AdditionalBill(writeManager.WriterStrategy)
					{
						BillNumber = split.SplitReference,
						MessageStatus = ListHelper.GetWithDescription<CodeDescriptionPair>(split.PresenceOnNetworkStatus, mawbBO.MasterLevelHouseHelper.Lookups.PresenceOnNetworkList),
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
					additionalBill.AddAddInfo("CustomsActionCodeDescription", (ZString)ListHelper.GetDescription(split.CustomsActionCode, mawbBO.MasterLevelHouseHelper.Lookups.CustomsActionsCodes));
					additionalBill.AddAddInfo("HandlingInformation", split.HandlingInformationForBinding);
					additionalBill.AddAddInfo("StorageDate", split.TemporaryStorageEndDate);
					mawbData.AdditionalBillCollection.Add(additionalBill);
				}
			}
		}
	}
}
