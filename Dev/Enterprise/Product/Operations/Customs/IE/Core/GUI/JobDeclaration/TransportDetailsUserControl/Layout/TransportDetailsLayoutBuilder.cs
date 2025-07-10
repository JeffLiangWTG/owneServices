using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using EuFlightAndNationalityUserControlNames = Enterprise.Customs.EU.GUI.FlightAndNationalityUserControl.ControlNames;
using EuInlandRailNames = Enterprise.Customs.EU.GUI.TransportIDAndNationalityRailUserControl.ControlNames;

namespace Enterprise.Customs.IE.GUI
{
	public class TransportDetailsLayoutBuilder : TransportDetailsLayoutBuilder<JobDeclaration>
	{
		protected override int MaxColumns => 1;

		protected override void SetDefaultCaptions()
		{
			base.SetDefaultCaptions();
			var euBag = EU.GUI.TransportDetailsControlBag.Instance;

			#region Borders

			SetCaptions(
				CommonBag.TransportInlandModeAndTypeOfIdUserControl,
				j => new Dictionary<string, ResourceStringData> {
					{ TransportInlandModeAndTypeOfIdUserControl.ControlNames.InlandModeOfTransportDropEdit, JobDeclarationInlandTransportResDataHelper.GetInlandTransportModeCaption(j) },
					{ TransportInlandModeAndTypeOfIdUserControl.ControlNames.TypeOfIDDropEdit, TransportMeansResString(j) },
				},
				j => j.JE_ApplicationCodeInfo
			);

			SetCaptions(
				euBag.FlightAndNationalityUserControl,
				j => new Dictionary<string, ResourceStringData> {
					{ EuFlightAndNationalityUserControlNames.FlightNumberTextBox, FlightNumberResString },
					{ EuFlightAndNationalityUserControlNames.TransportNationalityFindBox, TransportNationalityResString(j) },
				},
				j => j.JE_ApplicationCodeInfo
			);

			SetCaptions(
				euBag.TransportIDAndNationalityRailUserControl,
				j => new Dictionary<string, ResourceStringData> {
					{ EuInlandRailNames.TransportIDTextBox, WagonNumberResString },
					{ EuInlandRailNames.TransportNationalityFindBox, TransportNationalityResString(j) },
				},
				j => j.JE_ApplicationCodeInfo
			);

			SetCaptions(
				euBag.TransportIDAndNationalityUserControl,
				j => new Dictionary<string, ResourceStringData> {
					{ EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox, GetTransportIDResStringForTransportIDAndNationalityUserControl(j) },
					{ EU.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportNationalityFindBox, TransportNationalityResString(j) },
				},
				j => j.JE_TransportModeInfo
			);

			SetCaptions(
				euBag.TransportIDAndNationalityInlandWaterwayENIUserControl,
				j => new Dictionary<string, ResourceStringData> {
					{ TransportIDAndNationalityInlandWaterwayENIUserControl.ControlNames.TransportIDTextBox, ENIResString },
					{ TransportIDAndNationalityInlandWaterwayENIUserControl.ControlNames.TransportNationalityFindBox, TransportNationalityResString(j) },
				},
				j => j.JE_ApplicationCodeInfo
			);

			#endregion

			#region InLands
			SetCaptions(
				CommonBag.TransportInlandRoadUserControl,
				j => GetInlandCaptionDictionary(j, TransportInlandRoadUserControl.ControlNames.TransportIDTextBox, TransportInlandRoadUserControl.ControlNames.TransportNationalityCodeFindBox),
				dependsOnTransportInland
			);
			SetCaptions(
				CommonBag.TransportInlandSeaUserControl,
				j => GetInlandCaptionDictionary(j, TransportInlandSeaUserControl.ControlNames.VesselIDCodeFindBox, TransportInlandSeaUserControl.ControlNames.TransportNationalityCodeFindBox),
				dependsOnTransportInland
			);
			SetCaptions(
				CommonBag.TransportInlandIDAndNationalityUserControl,
				j => GetInlandCaptionDictionary(j, TransportInlandIDAndNationalityUserControl.ControlNames.TransportIDTextBox, TransportInlandIDAndNationalityUserControl.ControlNames.TransportNationalityFindBox),
				dependsOnTransportInland
			);
			#endregion
		}

		static ResourceStringData GetTransportIDResStringForTransportIDAndNationalityUserControl(JobDeclaration declaration)
		{
			ResourceStringData result;
			if (declaration.IsFixedInstallation || declaration.IsOwnPropulsion || declaration.IsMail)
			{
				result = Res.GetData("IE|TransportDetailsLayoutBuilder|TransportIDBorder", "Transport ID", "[19 08 017 000] Transport ID at Border");
			}
			else
			{
				result = Res.GetData("IE|TransportDetailsLayoutBuilder|TransportID", "Transport ID", "[19 08 017 000] Vehicle and/or trailer registration number");
			}
			return result;
		}

		static ResourceStringData TransportMeansResString(JobDeclaration declaration)
		{
			if (declaration.IsUCC5AndIsImport)
			{
				return Res.GetData("IE.JobDeclaration.JE_TransportMeans|UCC5", "[7/9] ID", "[7/9] Type of ID", "[7/9] Type of Identification", "[7/9] Identity of means of transport on arrival > Type of Identification");
			}
			return Res.GetData("BD86F14E-1F5C-4DE0-B2AC-8B6507DD3562", "Type of ID");
		}

		static ResourceStringData ENIResString => Res.GetData("IE|TransportDetailsLayoutBuilder|ENI", "ENI Code", "[19 08 017 000] European Vessel Identification Number (ENI)");

		static ResourceStringData WagonNumberResString => Res.GetData("IE|TransportDetailsLayoutBuilder|WagonNumber", "Wagon Number", "[19 08 017 000] Wagon Number");

		static ResourceStringData FlightNumberResString => Res.GetData("IE|TransportDetailsLayoutBuilder|FlightNumber", "Flight No.", "[19 08 017 000] Flight Number");

		static ResourceStringData TransportNationalityResString(JobDeclaration declaration) => declaration.IsUCC5AndIsImport ? Res.GetData("IE|TransportDetailsLayoutBuilder|InlandNationality|UCC5", "[7/15] Nat.", "[7/15] Nationality", "[7/15] Nationality at the border", "[7/15] Nationality of active means of transport crossing the border") : Res.GetData("IE|TransportDetailsLayoutBuilder|Nationality", "Nationality", "[19 08 062 000] Nationality");

		Dictionary<string, ResourceStringData> GetInlandCaptionDictionary(
			JobDeclaration declaration,
			string idControlName,
			string nationalityControlName
		)
		{
			var result = new Dictionary<string, ResourceStringData>();
			if (!string.IsNullOrEmpty(idControlName))
			{
				result.Add(idControlName, JobDeclarationInlandTransportResDataHelper.GetInlandTransactionIDCaption(declaration));
			}

			if (!string.IsNullOrEmpty(nationalityControlName))
			{
				result.Add(nationalityControlName, InlandNationalityResString);
			}

			return result;
		}

		static ResourceStringData InlandNationalityResString => Res.GetData("IE|TransportDetailsLayoutBuilder|InlandNationality", "Nationality", "[19 08 062 000] Nationality");

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			var euBag = EU.GUI.TransportDetailsControlBag.Instance;
			SetVisibility(euBag.VesselUserControl, j => j.IsSea, j => j.JE_TransportModeInfo);
			SetVisibility(euBag.FlightAndNationalityUserControl, j => j.IsAir || j.IsOwnPropulsion_Air, j => j.JE_MessageTypeInfo, j => j.JE_TransportModeInfo, j => j.ZG_BorderTransportMeansInfo);
			SetVisibility(CommonBag.PortOfFirstArrivalUserControl, x => !x.IsExport, j => j.JE_MessageTypeInfo);
			SetVisibility(euBag.TransportIDAndNationalityUserControl, j => j.TransportIDRequired, j => j.JE_MessageTypeInfo, j => j.JE_TransportModeInfo, j => j.ZG_BorderTransportMeansInfo);
			SetVisibility(euBag.TransportIDAndNationalityRailUserControl, j => j.IsRail && j.IsExport, j => j.JE_MessageTypeInfo, j => j.JE_TransportModeInfo);
			SetVisibility(euBag.TransportIDAndNationalityInlandWaterwayUserControl, j => j.TransportIDInlandWaterwayRequired, j => j.JE_TransportModeInfo, j => j.JE_ApplicationCodeInfo, j => j.ZG_BorderTransportMeansInfo);
			SetVisibility(euBag.TransportIDAndNationalityInlandWaterwayENIUserControl, j => j.TransportIDInlandWaterwayENIRequired, j => j.JE_TransportModeInfo, j => j.JE_ApplicationCodeInfo, j => j.ZG_BorderTransportMeansInfo);
			SetVisibility(CommonBag.VoyageAndNationalityUserControl, j => j.IsSea || j.IsOwnPropulsion_Sea, j => j.JE_MessageTypeInfo, j => j.JE_TransportModeInfo, j => j.ZG_BorderTransportMeansInfo);
			SetVisibility(euBag.AircraftRegistrationNumberTextBox, j => j.AircraftRegistrationNumberVisible, j => j.JE_MessageTypeInfo, j => j.JE_TransportModeInfo, j => j.JE_ApplicationCodeInfo, j => j.ZG_BorderTransportMeansInfo);
			SetVisibility(CommonBag.TransportDetailsPortOfLoadingWithIATAUserControl, x => x.IsImport && x.IsAir, j => j.JE_MessageTypeInfo, j => j.JE_TransportModeInfo);
			SetVisibility(CommonBag.TransportInlandIDAndNationalityUserControl, x => x.JE_TransportModeInland.IsEmpty || (!x.IsRoadInland && !x.IsSeaInland), j => j.JE_ApplicationCodeInfo, j => j.JE_TransportModeInlandInfo);
			SetVisibility(CommonBag.TransportInlandRoadUserControl, x => x.IsRoadInland, j => j.JE_TransportModeInlandInfo);
			SetVisibility(CommonBag.TransportInlandSeaUserControl, x => x.IsSeaInland, j => j.JE_TransportModeInlandInfo);
		}

		readonly Func<JobDeclaration, ZPropertyInfo>[] dependsOnTransportInland = new Func<JobDeclaration, ZPropertyInfo>[] { x => x.JE_TransportModeInlandInfo, x => x.JE_MessageTypeInfo, x => x.JE_TransportMeansInfo };
	}
}
