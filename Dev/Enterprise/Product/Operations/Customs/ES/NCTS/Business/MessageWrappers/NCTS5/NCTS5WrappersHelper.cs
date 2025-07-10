using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public static class NCTS5WrappersHelper
	{
		public static bool IsUnloadingStateNEWorMISorDIF(this ZString unloadedState)
		{
			switch (unloadedState)
			{
				case NctsUnloadedStateList.Codes.NEW:
				case NctsUnloadedStateList.Codes.MIS:
				case NctsUnloadedStateList.Codes.DIF:
					return true;
			}
			return false;
		}

		public static bool IsUnloadingStateNEWorDIF(this ZString unloadedState)
		{
			switch (unloadedState)
			{
				case NctsUnloadedStateList.Codes.NEW:
				case NctsUnloadedStateList.Codes.DIF:
					return true;
			}
			return false;
		}

		public static bool IsUnloadingStateMISorDIF(this ZString unloadedState)
		{
			switch (unloadedState)
			{
				case NctsUnloadedStateList.Codes.MIS:
				case NctsUnloadedStateList.Codes.DIF:
					return true;
			}
			return false;
		}

		public static bool IsUnloadingStateMISorNEW(this ZString unloadedState)
		{
			switch (unloadedState)
			{
				case NctsUnloadedStateList.Codes.MIS:
				case NctsUnloadedStateList.Codes.NEW:
					return true;
			}
			return false;
		}

		public static bool IsUnloadingStateNEW(this ZString unloadedState) => unloadedState == NctsUnloadedStateList.Codes.NEW;

		public static bool IsUnloadingStateDIF(this ZString unloadedState) => unloadedState == NctsUnloadedStateList.Codes.DIF;

		public static bool IsUnloadingStateMIS(this ZString unloadedState) => unloadedState == NctsUnloadedStateList.Codes.MIS;

		public static bool HasNctsBillDifferences(this NctsBill bill) => bill.MovementDetail.B9_UnloadedState.IsUnloadingStateDIF() &&
			((bill.HasDifference && bill.B0_Weight != bill.MovementDetail.DifferenceMoveDetail.DifferenceWeight)
			|| bill.SupportingDocuments.Any(x => x.CSI_Status != SupportingDocumentStatusList.Codes.DEC)
			|| bill.AdditionalDocuments.Any(x => x.CSI_Status != NctsBillAdditionalDocumentStatusList.Codes.DEC)
			|| bill.PreviousDocuments.Any(x => x.CSI_Status != NctsUnloadedStateList.Codes.DEC)
			|| bill.ArrivalGoodsItems.Any(x => x.BY_UnloadedState != NctsUnloadedStateList.Codes.DEC)
			|| bill.ArrivalTransportInfos.Any(x => x.TPM_TransportState != NctsUnloadedStateList.Codes.DEC));

		public static bool HasNctsArrivalGoodItemDifferences(this NctsArrivalCargoDesc goodItem) => goodItem.BY_UnloadedState.IsUnloadingStateDIF() &&
			(goodItem.HasNctsArrivalCommodityDifferences()
			|| goodItem.SupportingDocuments.Any(x => x.CSI_Status != SupportingDocumentStatusList.Codes.DEC)
			|| goodItem.AdditionalInfos.Any(x => x.CSI_Status != NctsBillAdditionalDocumentStatusList.Codes.DEC)
			|| goodItem.Packages.Where(x => x.B5_TypeOfDifference != NctsUnloadedStateList.Codes.DEC).Any());

		public static bool HasNctsArrivalCommodityDifferences(this NctsArrivalCargoDesc goodItem) => goodItem.BY_UnloadedState.IsUnloadingStateDIF() &&
			(goodItem.BY_Description != goodItem.UnloadedGoodsItem.BY_Description
			|| goodItem.BY_CusC4Number != goodItem.UnloadedGoodsItem.BY_CusC4Number
			|| goodItem.BY_HarmonisedTariff.SubstringSafe(0, 8) != goodItem.UnloadedGoodsItem.BY_HarmonisedTariff.SubstringSafe(0, 8)
			|| goodItem.HasNctsArrivalGoodsMeasureDifferences());

		public static bool HasNctsArrivalGoodsMeasureDifferences(this NctsArrivalCargoDesc goodItem) => goodItem.BY_UnloadedState.IsUnloadingStateDIF() &&
			(goodItem.BY_GrossWeight != goodItem.UnloadedGoodsItem.BY_GrossWeight
			|| goodItem.BY_NetWeight != goodItem.UnloadedGoodsItem.BY_NetWeight);

		public static IReadOnlyCollection<CommonDepartureTransportMeansWrapper> GetDepartureTransportMeansForDepartureHeader(ZString inlandTransportModeAtDeparture, ZString transportTypeAtDeparture, ZString transportAtDeparture, ZString transportCountryAtDeparture,
			ZString vesselNameAtDeparture, ZString vesselCountryAtDeparture, ZString trailer1IDAtDeparture, ZString trailer1NationalityAtDeparture, ZString trailer2IDAtDeparture, ZString trailer2NationalityAtDeparture)
		{
			var departureTransportMeans = new List<CommonDepartureTransportMeansWrapper>();

			ZShort seqNum = 1;
			if (inlandTransportModeAtDeparture == ModeOfTransportList.Codes._3_RoadTransport)
			{
				AddDepartureTransportMeansFor3(departureTransportMeans, seqNum, transportAtDeparture, transportCountryAtDeparture, trailer1IDAtDeparture, trailer1NationalityAtDeparture, trailer2IDAtDeparture, trailer2NationalityAtDeparture);
			}
			else if (inlandTransportModeAtDeparture != ModeOfTransportList.Codes._5_PostalConsignment && inlandTransportModeAtDeparture != ModeOfTransportList.Codes._7_FixedTransportInstallations)
			{
				AddDepartureTransportMeansCommon(departureTransportMeans, inlandTransportModeAtDeparture, seqNum, transportTypeAtDeparture, transportAtDeparture, transportCountryAtDeparture, vesselNameAtDeparture, vesselCountryAtDeparture);
			}

			return departureTransportMeans.AsReadOnly();
		}

		static void AddDepartureTransportMeansCommon(List<CommonDepartureTransportMeansWrapper> departureTransportMeans, ZString mot, ZShort seqNum, ZString transportTypeAtDeparture, ZString transportAtDeparture, ZString transportCountryAtDeparture,
			ZString vesselNameAtDeparture, ZString vesselCountryAtDeparture)
		{
			var id = mot == ModeOfTransportList.Codes._1_SeaTransport ? vesselNameAtDeparture : transportAtDeparture;
			var nationality = mot == ModeOfTransportList.Codes._1_SeaTransport ? vesselCountryAtDeparture : transportCountryAtDeparture;
			if (!transportTypeAtDeparture.IsEmpty || !id.IsEmpty || !nationality.IsEmpty)
			{
				departureTransportMeans.Add(new CommonDepartureTransportMeansWrapper(transportTypeAtDeparture, id, nationality, seqNum));
			}
		}

		static void AddDepartureTransportMeansFor3(List<CommonDepartureTransportMeansWrapper> departureTransportMeans, ZShort seqNum, ZString transportAtDeparture, ZString transportCountryAtDeparture,
			ZString trailer1IDAtDeparture, ZString trailer1NationalityAtDeparture, ZString trailer2IDAtDeparture, ZString trailer2NationalityAtDeparture)
		{
			var transportMode = !transportAtDeparture.IsEmpty ? (ZString)NctsTransportTypeOfIdList.Codes._30 : ZString.Empty;
			var trailer1Mode = !trailer1IDAtDeparture.IsEmpty ? (ZString)NctsTransportTypeOfIdList.Codes._31 : ZString.Empty;
			var trailer2Mode = !trailer2IDAtDeparture.IsEmpty ? (ZString)NctsTransportTypeOfIdList.Codes._31 : ZString.Empty;

			var transportList = new List<Tuple<ZString, ZString, ZString>>();

			if (!transportMode.IsEmpty || !transportAtDeparture.IsEmpty || !transportCountryAtDeparture.IsEmpty)
			{
				transportList.Add(transportMode, transportAtDeparture, transportCountryAtDeparture);
			}
			if (!trailer1Mode.IsEmpty || !trailer1IDAtDeparture.IsEmpty || !trailer1NationalityAtDeparture.IsEmpty)
			{
				transportList.Add(trailer1Mode, trailer1IDAtDeparture, trailer1NationalityAtDeparture);
			}
			if (!trailer2Mode.IsEmpty || !trailer2IDAtDeparture.IsEmpty || !trailer2NationalityAtDeparture.IsEmpty)
			{
				transportList.Add(trailer2Mode, trailer2IDAtDeparture, trailer2NationalityAtDeparture);
			}

			foreach (var transportData in transportList)
			{
				departureTransportMeans.Add(new CommonDepartureTransportMeansWrapper(transportData.Item1, transportData.Item2, transportData.Item3, seqNum));
				seqNum++;
			}
		}
	}
}
