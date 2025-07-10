using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public sealed class FTAEntryDetailsControlBag : ControlBag
	{
		FTAEntryDetailsControlBag()
		{
			EntryNumberTextBox = RegisterControl(nameof(FTAEntryDetailsUserControl.EntryNumberTextBox));
			LawCodeDropEdit = RegisterControl(nameof(FTAEntryDetailsUserControl.LawCodeDropEdit));
			DepartureDateEdit = RegisterControl(nameof(FTAEntryDetailsUserControl.DepartureDateEdit));
			DeparturePortCodeFindBox = RegisterControl(nameof(FTAEntryDetailsUserControl.DeparturePortCodeFindBox));
			DepartureCountryCodeFindBox = RegisterControl(nameof(FTAEntryDetailsUserControl.DepartureCountryCodeFindBox));
			ManufacturerAddressControl = RegisterControl(nameof(FTAEntryDetailsUserControl.ManufacturerAddressControl));
			ManufacturerAreaPostcodeTextBox = RegisterControl(nameof(FTAEntryDetailsUserControl.ManufacturerAreaPostcodeTextBox));
			CustomsDisbursementBillNoTextBox = RegisterControl(nameof(FTAEntryDetailsUserControl.CustomsDisbursementBillNoTextBox));
			TransshipmentYNDropEdit = RegisterControl(nameof(FTAEntryDetailsUserControl.TransshipmentYNDropEdit));
			TransshipmentDateEdit = RegisterControl(nameof(FTAEntryDetailsUserControl.TransshipmentDateEdit));
			TransshipmentPortCodeFindBox = RegisterControl(nameof(FTAEntryDetailsUserControl.TransshipmentPortCodeFindBox));
			TransshipmentCountryCodeFindBox = RegisterControl(nameof(FTAEntryDetailsUserControl.TransshipmentCountryCodeFindBox));
			ImporterAddressControl = RegisterControl(nameof(FTAEntryDetailsUserControl.ImporterAddressControl));
			ExporterAddressControl = RegisterControl(nameof(FTAEntryDetailsUserControl.ExporterAddressControl));
		}

		public static FTAEntryDetailsControlBag Instance => instance ?? (instance = new FTAEntryDetailsControlBag());

		[ThreadStatic]
		static FTAEntryDetailsControlBag instance;
		public ControlReference EntryNumberTextBox { get; }
		public ControlReference LawCodeDropEdit { get; }
		public ControlReference DepartureDateEdit { get; }
		public ControlReference DeparturePortCodeFindBox { get; }
		public ControlReference DepartureCountryCodeFindBox { get; }
		public ControlReference ManufacturerAddressControl { get; }
		public ControlReference ManufacturerAreaPostcodeTextBox { get; }
		public ControlReference CustomsDisbursementBillNoTextBox { get; }
		public ControlReference TransshipmentYNDropEdit { get; }
		public ControlReference TransshipmentDateEdit { get; }
		public ControlReference TransshipmentPortCodeFindBox { get; }
		public ControlReference TransshipmentCountryCodeFindBox { get; }
		public ControlReference ImporterAddressControl { get; }
		public ControlReference ExporterAddressControl { get; }

		protected override Control CreateTemplate() => new FTAEntryDetailsUserControl();
	}
}
