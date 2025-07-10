using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public class TransportDetailsLayoutBuilder : EU.GUI.Declaration.TransportDetailsLayoutBuilder<JobDeclaration>
	{
		protected override int MaxColumns => 1;

		protected override void SetDefaultCaptions()
		{
			base.SetDefaultCaptions();

			SetCaptions(
				CommonBag.TransportInlandModeAndTypeOfIdUserControl,
				j => new Dictionary<string, ResourceStringData> { { TransportInlandModeAndTypeOfIdUserControl.ControlNames.InlandModeOfTransportDropEdit, GetInlandTransportModeCaption(j) } }
				, dependentOnMessageType
			);

			SetCaptions(
				CommonBag.TransportInlandIDAndNationalityUserControl,
				j => GetInlandCaptionDictionary(j, TransportInlandIDAndNationalityUserControl.ControlNames.TransportIDTextBox, TransportInlandIDAndNationalityUserControl.ControlNames.TransportNationalityFindBox),
				j => j.JE_TransportMeansInfo
			);

			SetCaptions(
				CommonBag.TransportInlandRoadUserControl,
				j => GetInlandCaptionDictionary(j, TransportInlandRoadUserControl.ControlNames.TransportIDTextBox, TransportInlandRoadUserControl.ControlNames.TransportNationalityCodeFindBox),
				j => j.JE_TransportMeansInfo
			);

			SetCaptions(
				CommonBag.TransportInlandSeaUserControl,
				j => GetInlandCaptionDictionary(j, TransportInlandSeaUserControl.ControlNames.VesselIDCodeFindBox, TransportInlandSeaUserControl.ControlNames.TransportNationalityCodeFindBox),
				j => j.JE_TransportMeansInfo
			);
		}

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetVisibility(EUBag.InlandTransportDetailsUserControl, inlandTransportDetailsUserControlVisible, dependsOnTransportModeInlandAndMessageType);
			SetVisibility(CommonBag.TransportInlandModeAndTypeOfIdUserControl, x => x.IsUCC6, dependsOnTransportModeInlandAndMessageType);
			SetVisibility(CommonBag.TransportInlandIDAndNationalityUserControl, transportInlandIdAndNationalityDetailsUserControlVisible, dependsOnTransportModeInlandAndMessageType);
			SetVisibility(CommonBag.TransportInlandRoadUserControl, x => x.IsUCC6 && x.IsRoadInland, dependsOnTransportModeInlandAndMessageType);
			SetVisibility(CommonBag.TransportInlandSeaUserControl, x => x.IsUCC6 && x.IsSeaInland, dependsOnTransportModeInlandAndMessageType);
		}

		static ResourceStringData GetInlandTransportModeCaption(JobDeclaration declaration) => declaration.IsUCC6 ? Res_InlandTransportMode : null;

		static ResourceStringData GetInlandTransactionIDCaption(BaseJobDeclaration declaration)
		{
			ResourceStringData result = null;

			switch (declaration.JE_TransportModeInland)
			{
				case TransportTypeList.Codes.Road:
					result = Res_ROA;
					break;
				case TransportTypeList.Codes.Air:
					switch (declaration.JE_TransportMeans)
					{
						case TransportMeansList.Codes.IataFlightNumber:
							result = Res_AIR_IATAFlightNumber;
							break;
						case TransportMeansList.Codes.RegistrationNumberOfTheAircraft:
							result = Res_AIR_AircraftNumber;
							break;
					}
					break;
				case TransportTypeList.Codes.InlandWaterwayTransport:
					switch (declaration.JE_TransportMeans)
					{
						case TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel:
							result = Res_IWT_InlandWaterwaysVesselName;
							break;
						case TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode:
							result = Res_IWT_EUVesselCode;
							break;
					}
					break;
				case TransportTypeList.Codes.Rail:
					switch (declaration.JE_TransportMeans)
					{
						case TransportMeansList.Codes.WagonNumber:
							result = Res_RAI_WagonNumber;
							break;
						case TransportMeansList.Codes.TrainNumber:
							result = Res_RAI_TrainNumber;
							break;
					}
					break;
				case TransportTypeList.Codes.Sea:
					switch (declaration.JE_TransportMeans)
					{
						case TransportMeansList.Codes.ImoShipIdentificationNumber:
							result = Res_SEA_IMOShipNumber;
							break;
						case TransportMeansList.Codes.NameOfTheSeaGoingVessel:
							result = Res_SEA_SeaGoingShipName;
							break;
					}
					break;
			}

			return result ?? Res_TransportId;
		}

		Dictionary<string, ResourceStringData> GetInlandCaptionDictionary(
			JobDeclaration declaration,
			string idControlName,
			string nationalityControlName
		)
		{
			var result = new Dictionary<string, ResourceStringData>();
			if (!string.IsNullOrEmpty(idControlName))
			{
				result.Add(idControlName, GetInlandTransactionIDCaption(declaration));
			}

			if (!string.IsNullOrEmpty(nationalityControlName))
			{
				result.Add(nationalityControlName, InlandNationalityResString);
			}

			return result;
		}

		new readonly Func<JobDeclaration, ZPropertyInfo>[] dependsOnTransportModeInlandAndMessageType = { x => x.JE_TransportModeInlandInfo, x => x.JE_MessageTypeInfo };
		readonly Func<JobDeclaration, ZPropertyInfo>[] dependentOnMessageType = { x => x.JE_MessageTypeInfo };
		readonly Func<JobDeclaration, bool> inlandTransportDetailsUserControlVisible = x => !x.IsUCC6;
		readonly Func<JobDeclaration, bool> transportInlandIdAndNationalityDetailsUserControlVisible = x => x.IsUCC6 && (x.JE_TransportModeInland.IsEmpty || (!x.IsRoadInland && !x.IsSeaInland));

		static ResourceStringData Res_VesselName => Res.GetData("C60752AC-B357-4F96-9F7E-A44BB09D259C", englishCaption: "Vessel Name", englishFullDescription: "[19 05 017 000] Vessel Name");
		static ResourceStringData Res_ROA => Res.GetData("858DE619-21F5-4DF1-89D0-DB8E15284269", englishCaption: "Registration No.", englishFullDescription: "[19 05 017 000] Vehicle Registration Number");
		static ResourceStringData Res_AIR_IATAFlightNumber => Res.GetData("DDC9B5C0-039C-43DC-A1D7-1EDF4F97F05B", englishCaption: "Flight No.", englishFullDescription: "[19 05 017 000] Flight No.");
		static ResourceStringData Res_AIR_AircraftNumber => Res.GetData("02544FB8-E2B9-46FB-A3F0-D4E020BB7CC3", englishCaption: "Registration No.", englishFullDescription: "[19 05 017 000] Aircraft Registration Number");
		static ResourceStringData Res_IWT_EUVesselCode => Res.GetData("2C0E4711-A805-4427-A7CD-9F7A379B392A", englishCaption: "ENI Code", englishFullDescription: "[19 05 017 000] European Vessel Identification Number");
		static ResourceStringData Res_IWT_InlandWaterwaysVesselName => Res_VesselName;
		static ResourceStringData Res_RAI_WagonNumber => Res.GetData("6758E3A2-BC6C-452F-9849-E17767E79C38", englishCaption: "Wagon No.", englishFullDescription: "[19 05 017 000] Wagon Number");
		static ResourceStringData Res_RAI_TrainNumber => Res.GetData("BD802BFC-8FC9-43D9-AF66-09305BC8F18A", englishCaption: "Train No.", englishFullDescription: "[19 05 017 000] Train Number");
		static ResourceStringData Res_SEA_IMOShipNumber => Res.GetData("3D5870DA-78C6-4ACD-AFAE-24905425DCD6", englishCaption: "Lloyds No.", englishFullDescription: "[19 05 017 000] Lloyds Number");
		static ResourceStringData Res_SEA_SeaGoingShipName => Res_VesselName;
		static ResourceStringData Res_TransportId => Res.GetData("46152856-5031-472A-9EC5-2FE976F83C16", englishCaptionOrFullDescription: "Transport ID");
		static ResourceStringData Res_InlandTransportMode => Res.GetData("D1F3EE73-4546-49CE-A4F9-D7079F42BDAC", englishCaption: "Trans. Mode", englishFullDescription: "[19 04 001 000] Inland Mode of Transport");
		static ResourceStringData InlandNationalityResString => Res.GetData("DE|TransportDetailsLayoutBuilder|InlandNationality", "Nationality", "[19 08 062 000] Nationality");
	}
}
