using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BE.GUI;

public class TransportDetailsLayoutBuilder : TransportDetailsLayoutBuilder<JobDeclaration>
{
	TransportDetailsUserControlBag beBag { get; } = TransportDetailsUserControlBag.Instance;

	protected override int MaxColumns => 1;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();

		var transportModeDependency = new Func<JobDeclaration, ZPropertyInfo>[] { x => x.JE_TransportModeInfo };
		var transportModeAndMessageTypeDependency = new Func<JobDeclaration, ZPropertyInfo>[] { x => x.JE_TransportModeInfo, x => x.JE_MessageTypeInfo };
		var messageTypeVersionAndTransportModeInlandDependency = new Func<JobDeclaration, ZPropertyInfo>[] { x => x.JE_MessageTypeInfo, x => x.JE_TransportModeInlandInfo };

		SetVisibility(CommonBag.MasterBillTextBox, x => x.IsAir, transportModeDependency);
		SetVisibility(CommonBag.OceanBillTextBox, x => x.IsSea, transportModeDependency);
		SetVisibility(beBag.FlightAndNationalityUserControl, x => x.IsAir, transportModeDependency);
		SetVisibility(CommonBag.VoyageAndNationalityUserControl, x => x.IsSea, transportModeDependency);
		SetVisibility(CommonBag.TransportIDAndNationalityUserControl, x => !x.IsAir && !x.IsSea, transportModeDependency);
		SetVisibility(CommonBag.PortOfLoadingUserControl, x => !(x.IsImport && x.IsAir), transportModeAndMessageTypeDependency);
		SetVisibility(CommonBag.TransportDetailsPortOfLoadingWithIATAUserControl, x => x.IsImport && x.IsAir, transportModeAndMessageTypeDependency);
		SetVisibility(CommonBag.VesselCodeFindBox, x => x.IsSea, transportModeDependency);
		SetVisibility(CommonBag.TransportInlandIDAndNationalityUserControl, x => (x.JE_TransportModeInland.IsEmpty || (!(x.IsExport && x.IsRoadInland) && !x.IsSeaInland)), messageTypeVersionAndTransportModeInlandDependency);
		SetVisibility(CommonBag.TransportInlandRoadUserControl, x => x.IsExport && x.IsRoadInland, messageTypeVersionAndTransportModeInlandDependency);
		SetVisibility(CommonBag.TransportInlandSeaUserControl, x => x.IsSeaInland, messageTypeVersionAndTransportModeInlandDependency);
	}

	protected override void SetDefaultCaptions()
	{
		base.SetDefaultCaptions();

		SetVoyageAndNationalityUserControlCaptions();
		SetTransportIDAndNationalityUserControlCaptions();
		SetPortOfLoadingUserControlCaptions();
		SetPortOfDischargeUserControlCaptions();
		SetTransportDetailsPortOfLoadingWithIATAUserControlCaptions();
		SetTransportInlandModeAndTypeOfIdUserControlCaptions();

		#region InLand Transport Details
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
		SetCaptions(
			CommonBag.TransportInlandIDAndNationalityUserControl,
			j => GetInlandCaptionDictionary(j, TransportInlandIDAndNationalityUserControl.ControlNames.TransportIDTextBox, TransportInlandIDAndNationalityUserControl.ControlNames.TransportNationalityFindBox),
			j => j.JE_TransportMeansInfo
		);
		SetCaption(CommonBag.MasterBillTextBox, x => MasterBillResString, x => x.JE_TransportMeansInfo);
		SetCaption(CommonBag.OceanBillTextBox, x => OceanBillResString, x => x.JE_TransportMeansInfo);
		#endregion
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
			result.Add(idControlName, JobDeclarationInlandTransportResDataHelper.GetInlandTransactionIDCaption(declaration));
		}

		if (!string.IsNullOrEmpty(nationalityControlName))
		{
			result.Add(nationalityControlName, InlandTransportNationalityCodeFindBoxResString);
		}

		return result;
	}

	void SetTransportInlandModeAndTypeOfIdUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.TransportInlandModeAndTypeOfIdUserControl.ControlNames.InlandModeOfTransportDropEdit, InlandModeOfTransportResString },
			{ Customs.GUI.TransportInlandModeAndTypeOfIdUserControl.ControlNames.TypeOfIDDropEdit, InlandTransportCodeResString }
		};

		SetCaptions(CommonBag.TransportInlandModeAndTypeOfIdUserControl, _ => captions, x => x.JE_TransportModeInfo);
	}

	void SetVoyageAndNationalityUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.VoyageAndNationalityUserControl.ControlNames.VoyageNumberTextBox, VoyageNumberTextBoxResString },
		};

		SetCaptions(CommonBag.VoyageAndNationalityUserControl, _ => captions, x => x.JE_TransportModeInfo);
	}

	void SetTransportIDAndNationalityUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox, TransportIDTextBoxResString },
		};
		SetCaptions(CommonBag.TransportIDAndNationalityUserControl, _ => captions, x => x.JE_TransportModeInfo);
	}

	void SetPortOfLoadingUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.TransportDetailsPortOfLoadingUserControl.ControlNames.PortOfLoadingFindBox, PortOfLoadingFindBoxResString },
			{ Customs.GUI.TransportDetailsPortOfLoadingUserControl.ControlNames.ExportDateEdit, ExportDateEditResString },
		};
		SetCaptions(CommonBag.PortOfLoadingUserControl, _ => captions, x => x.JE_TransportModeInfo, x => x.JE_MessageTypeInfo);
	}

	void SetPortOfDischargeUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.TransportDetailsPortOfDischargeUserControl.ControlNames.PortOfDischargeFindBox, PortOfDischargeFindBoxResString },
		};
		SetCaptions(CommonBag.PortOfDischargeUserControl, _ => captions);
	}

	void SetTransportDetailsPortOfLoadingWithIATAUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.TransportDetailsPortOfLoadingWithIATAUserControl.ControlNames.PortOfLoadingFindBox, PortOfLoadingFindBoxResString },
			{ Customs.GUI.TransportDetailsPortOfLoadingWithIATAUserControl.ControlNames.ExportDateEdit, ExportDateEditResString },
			{ Customs.GUI.TransportDetailsPortOfLoadingWithIATAUserControl.ControlNames.IATALoadPortCodeFindBox, IATALoadPortCodeFindBoxResString }
		};
		SetCaptions(CommonBag.TransportDetailsPortOfLoadingWithIATAUserControl, _ => captions, x => x.JE_TransportModeInfo, x => x.JE_MessageTypeInfo);
	}

	static ResourceStringData IATALoadPortCodeFindBoxResString => Res.GetData("99E41BC3-958A-4A13-94DC-A2FCC5795897", "IATA");
	static ResourceStringData PortOfDischargeFindBoxResString => Res.GetData("4F6A1B50-26DD-4499-B0B2-AD96C6FC2F28", "Discharge", "Discharge Port", "");
	static ResourceStringData VoyageNumberTextBoxResString => Res.GetData("732D0B99-25BA-4953-B38F-EE7FC82F299E", "Voy.", "[UCC 7/7] Voyage", "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.");
	static ResourceStringData TransportIDTextBoxResString => Res.GetData("00D12446-D134-48D8-9DC8-EB2E9C223703", "Transport ID", "[UCC 7/7] Transport ID");
	static ResourceStringData PortOfLoadingFindBoxResString => Res.GetData("6706347F-DA5A-4939-AC4C-2B977EB4FB74", "Load Port", "Load Port", "");
	static ResourceStringData ExportDateEditResString => Res.GetData("B49B8DD1-F820-4F7F-A75F-ED82FCBA8B7A", "Dep.", "Departure", "");
	static ResourceStringData InlandModeOfTransportResString => Res.GetData("0FDA92D7-7075-47C2-B1E0-9F537EE993E5", "Inland M.O.T.", "[UCC 7/5] Inland M.O.T.", "[UCC 7/5] Inland Mode of Transport");
	static ResourceStringData InlandTransportCodeResString => Res.GetData("B16F571A-1B0E-4218-A3BA-35428D2B2801", "Code", "Code", "[19 06 061 000] type of identification of the transport");
	static ResourceStringData InlandTransportNationalityCodeFindBoxResString => Res.GetData("429B5817-2B9B-4E54-B297-A8A29DB3F62E", "Nationality", "Nationality", "[19 06 062 000] Nationality inland transport");
	static ResourceStringData MasterBillResString => Res.GetData("55F5F01D-B13F-4384-9B6D-0EF8FD1C8B05", "Master Bill", "[UCC 7/7] Master Bill");
	static ResourceStringData OceanBillResString => Res.GetData("611BE0BD-D097-4DA9-8EB0-23DB6745CF7C", "Ocean Bill", "[UCC 7/7] Ocean Bill");
}
