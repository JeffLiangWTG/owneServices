using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.GUI;

public sealed class TransportDetailsLayoutBuilder : Customs.GUI.TransportDetailsLayoutBuilder<JobDeclaration>
{
	protected override int MaxColumns => 2;

	protected override void SetDefaultCaptions()
	{
		base.SetDefaultCaptions();

		SetFlightAndNationalityUserControlCaptions();
		SetTransportDetailsPortOfLoadingWithIATAUserControlCaptions();
		SetPortOfDischargeUserControlCaptions();
		SetOceanBillTextBoxCaption();
		SetVoyageAndNationalityUserControlCaptions();
		SetPortOfLoadingUserControlCaptions();
		SetTransportIDAndNationalityUserControlCaptions();
		SetTransportInlandIDAndNationalityUserControlCaptions();
		SetTransportInlandRailUserControlCaptions();
		SetCaption(EU.GUI.TransportDetailsControlBag.Instance.TransportNationalityCodeFindBox, _ => TransportNationalityResString, x => x.JE_MessageTypeInfo, x => x.JE_TransportModeInfo);
	}

	#region TransportInlandRailUserControl

	void SetTransportInlandRailUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.TransportInlandRailUserControl.ControlNames.TrainNationalityCodeFindBox, Inland18NationalityFindBoxResString },
			{ Customs.GUI.TransportInlandRailUserControl.ControlNames.WagonNationalityCodeFindBox, Inland18NationalityFindBoxResString }
		};
		SetCaptions(CommonBag.TransportInlandRailUserControl, _ => captions);
	}

	#endregion

	#region TransportInlandIDAndNationalityUserControl

	void SetTransportInlandIDAndNationalityUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.TransportInlandIDAndNationalityUserControl.ControlNames.TransportIDTextBox, InlandTransportIDTextBoxResString },
			{ Customs.GUI.TransportInlandIDAndNationalityUserControl.ControlNames.TransportNationalityFindBox, Inland18NationalityFindBoxResString }
		};
		SetCaptions(CommonBag.TransportInlandIDAndNationalityUserControl, _ => captions);
	}

	static ResourceStringData InlandTransportIDTextBoxResString => Res.GetData("D04A6EB5-EB3F-42BD-93A1-4E76CE28FB99", "Trans. ID (Inland)");
	static ResourceStringData Inland18NationalityFindBoxResString => Res.GetData("B8E8F0CF-E113-48D3-9465-1147E4120734", "[18] Nationality");

	#endregion

	#region SetTransportIDAndNationalityUserControlCaptions

	void SetTransportIDAndNationalityUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.TransportIDAndNationalityUserControl.ControlNames.TransportIDTextBox, TransportIDTextBoxResString },
		};
		SetCaptions(CommonBag.TransportIDAndNationalityUserControl, _ => captions, x => x.JE_TransportModeInfo);
	}

	static ResourceStringData TransportIDTextBoxResString => Res.GetData("FD173E4D-57F2-47B0-8CBD-B57F5F5EC7E3", "[21] Transport ID");

	#endregion

	#region PortOfLoadingUserControl

	void SetPortOfLoadingUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.TransportDetailsPortOfLoadingUserControl.ControlNames.PortOfLoadingFindBox, PortOfLoadingFindBoxResString },
			{ Customs.GUI.TransportDetailsPortOfLoadingUserControl.ControlNames.ExportDateEdit, ExportDateEditResString },
		};
		SetCaptions(CommonBag.PortOfLoadingUserControl, _ => captions, x => x.JE_TransportModeInfo, x => x.JE_MessageTypeInfo);
	}

	#endregion

	#region VoyageAndNationalityUserControl

	void SetVoyageAndNationalityUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.VoyageAndNationalityUserControl.ControlNames.VoyageNumberTextBox, VoyageNumberTextBoxResString },
		};

		SetCaptions(CommonBag.VoyageAndNationalityUserControl, _ => captions, x => x.JE_TransportModeInfo);
	}

	static ResourceStringData VoyageNumberTextBoxResString => Res.GetData("BD4283B5-063E-4233-9EF4-B0A71B2C9856", "Voy.", "Voyage", "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.");

	#endregion

	#region OceanBillTextBox

	void SetOceanBillTextBoxCaption()
	{
		SetCaption(CommonBag.OceanBillTextBox, _ => OceanBillTextBoxResString, x => x.JE_TransportModeInfo);
	}

	static ResourceStringData OceanBillTextBoxResString => Res.GetData("31C020AC-EEC9-40E9-B2FD-7DFA337E1242", "Ocean Bill", "Master Bill of the consignment");

	#endregion

	#region PortOfDischargeUserControl

	void SetPortOfDischargeUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ Customs.GUI.TransportDetailsPortOfDischargeUserControl.ControlNames.PortOfDischargeFindBox, PortOfDischargeFindBoxResString },
		};
		SetCaptions(CommonBag.PortOfDischargeUserControl, _ => captions);
	}

	static ResourceStringData PortOfDischargeFindBoxResString => Res.GetData("BD102B48-2864-40B7-9901-C8FA18B501A5", "Discharge", "Discharge Port", "");

	#endregion

	#region TransportDetailsPortOfLoadingWithIATAUserControl

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

	static ResourceStringData PortOfLoadingFindBoxResString => Res.GetData("5BB1BB81-ED79-45CB-8956-11AEA9FB0B2E", "Load Port", "Load Port", "");
	static ResourceStringData ExportDateEditResString => Res.GetData("5E0E9A9E-F08C-45A8-921E-D080AE24244A", "Dep.", "Departure", "");
	static ResourceStringData IATALoadPortCodeFindBoxResString => Res.GetData("4753904E-8BE2-47EA-B301-5A037DB0E533", "IATA");

	#endregion

	#region SetFlightAndNationalityUserControlCaptions

	void SetFlightAndNationalityUserControlCaptions()
	{
		var captions = new Dictionary<string, ResourceStringData>
		{
			{ EU.GUI.FlightAndNationalityUserControl.ControlNames.FlightNumberTextBox, FlightNumberResString },
			{ EU.GUI.FlightAndNationalityUserControl.ControlNames.TransportNationalityFindBox, TransportNationalityResString }
		};

		SetCaptions(EU.GUI.TransportDetailsControlBag.Instance.FlightAndNationalityUserControl, _ => captions, x => x.JE_TransportModeInfo);
	}

	static ResourceStringData FlightNumberResString => Res.GetData("E02960BA-BD51-44EE-AD01-7C3340545402", "Flt.", "Flight", "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.");
	static ResourceStringData TransportNationalityResString => Res.GetData("A2A9B74D-EF06-4FE9-851A-2294033D8C15", "[21] Nationality");

	#endregion
}
