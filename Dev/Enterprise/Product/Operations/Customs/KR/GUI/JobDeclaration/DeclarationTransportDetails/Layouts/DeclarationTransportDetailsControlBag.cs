using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class DeclarationTransportDetailsControlBag : ControlBag
	{
		DeclarationTransportDetailsControlBag()
		{
			TransshipmentPortCodeFindBox = RegisterControl(nameof(DeclarationTransportDetailsUserControl.TransshipmentPortCodeFindBox));
			TransshipmentDateEdit = RegisterControl(nameof(DeclarationTransportDetailsUserControl.TransshipmentDateEdit));
			VesselCountryCodeFindBox = RegisterControl(nameof(DeclarationTransportDetailsUserControl.VesselCountryCodeFindBox));
			CarrierKRCCodeFindBox = RegisterControl(nameof(DeclarationTransportDetailsUserControl.CarrierKRCCodeFindBox));
			VoyageFlightNumberTextBox = RegisterControl(nameof(DeclarationTransportDetailsUserControl.VoyageFlightNumberTextBox));
			FolioNumberTextBox = RegisterControl(nameof(DeclarationTransportDetailsUserControl.FolioNumberTextBox));
			PortOfLoadingCodeFindBox = RegisterControl(nameof(DeclarationTransportDetailsUserControl.PortOfLoadingCodeFindBox));
			ExportDateEdit = RegisterControl(nameof(DeclarationTransportDetailsUserControl.ExportDateEdit));
			IATALoadPortDropEdit = RegisterControl(nameof(DeclarationTransportDetailsUserControl.IATALoadPortDropEdit));
			VoyageDurationCalcEdit = RegisterControl(nameof(DeclarationTransportDetailsUserControl.VoyageDurationCalcEdit));
			RadioCallSignTextBox = RegisterControl(nameof(DeclarationTransportDetailsUserControl.RadioCallSignTextBox));
			MRNTypeDropEdit = RegisterControl(nameof(DeclarationTransportDetailsUserControl.MRNTypeDropEdit));
			MRNNumberTextBox = RegisterControl(nameof(DeclarationTransportDetailsUserControl.MRNNumberTextBox));
		}

		public static DeclarationTransportDetailsControlBag Instance => instance ?? (instance = new DeclarationTransportDetailsControlBag());

		[ThreadStatic]
		static DeclarationTransportDetailsControlBag instance;

		public ControlReference TransshipmentPortCodeFindBox;
		public ControlReference TransshipmentDateEdit;
		public ControlReference VesselCountryCodeFindBox;
		public ControlReference CarrierKRCCodeFindBox;
		public ControlReference PortOfLoadingCodeFindBox;
		public ControlReference ExportDateEdit;
		public ControlReference IATALoadPortDropEdit;
		public ControlReference VoyageFlightNumberTextBox;
		public ControlReference FolioNumberTextBox;
		public ControlReference VoyageDurationCalcEdit;
		public ControlReference RadioCallSignTextBox;
		public ControlReference MRNNumberTextBox;
		public ControlReference MRNTypeDropEdit;

		protected override Control CreateTemplate() => new DeclarationTransportDetailsUserControl();

		public static ResourceStringData PortOfDischargeCaption => Res.GetData("2D61E29C-7AB0-477C-B31A-C0DF7BD1FA7E", "Discharge", "Port Of Discharge", "");
		public static ResourceStringData AirCarrierCaption => Res.GetData("55C8E5CB-3784-47ED-8554-A697881A7648", "KRC", "Carrier KRC");
		public static ResourceStringData SeaCarrierCaption => Res.GetData("6D7587DB-8351-47E5-85E9-A053952048CD", "KRC", "Carrier KRC");
	}
}
