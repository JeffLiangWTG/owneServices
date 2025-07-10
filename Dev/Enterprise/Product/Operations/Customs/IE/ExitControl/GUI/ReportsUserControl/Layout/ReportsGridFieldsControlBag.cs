using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public class ReportsGridFieldsControlBag : ControlBag
	{
		protected override Control CreateTemplate() => new ReportsGridFieldsControl();

		[ThreadStatic]
		static ReportsGridFieldsControlBag instance;
		public static ReportsGridFieldsControlBag Instance => instance ?? (instance = new ReportsGridFieldsControlBag());

		public ReportsGridFieldsControlBag()
		{
			FormattedDateTimeDateEdit = RegisterControl(nameof(ReportsGridFieldsControl.FormattedDateTimeDateEdit));
			LongFormattedDateTimeDateEdit = RegisterControl(nameof(ReportsGridFieldsControl.LongFormattedDateTimeDateEdit));
			TypeOfLocationDropEdit = RegisterControl(nameof(ReportsGridFieldsControl.TypeOfLocationDropEdit));
			UNLOCOCodeFindBox = RegisterControl(nameof(ReportsGridFieldsControl.UNLOCOCodeFindBox));
			DiscrepanciesCheckBox = RegisterControl(nameof(ReportsGridFieldsControl.DiscrepanciesCheckBox));
			DeclarantTypeDropEdit = RegisterControl(nameof(ReportsGridFieldsControl.DeclarantTypeDropEdit));
			TransportModeDropEdit = RegisterControl(nameof(ReportsGridFieldsControl.TransportModeDropEdit));
			DeclarantAddressDropEdit = RegisterControl(nameof(ReportsGridFieldsControl.DeclarantAddressDropEdit));
			RepresentativeAddressDropEdit = RegisterControl(nameof(ReportsGridFieldsControl.RepresentativeAddressDropEdit));
			OfficeOfExportCodeFindBox = RegisterControl(nameof(ReportsGridFieldsControl.OfficeOfExportCodeFindBox));
			EnquiryInformationCodeDropEdit = RegisterControl(nameof(ReportsGridFieldsControl.EnquiryInformationCodeDropEdit));
			AdditionalDeclarationTypeDropEdit = RegisterControl(nameof(ReportsGridFieldsControl.AdditionalDeclarationTypeDropEdit));
			LocationTextBox = RegisterControl(nameof(ReportsGridFieldsControl.LocationTextBox));
		}

		public ControlReference FormattedDateTimeDateEdit { get; }
		public ControlReference LongFormattedDateTimeDateEdit { get; }
		public ControlReference TypeOfLocationDropEdit { get; }
		public ControlReference UNLOCOCodeFindBox { get; }
		public ControlReference DiscrepanciesCheckBox { get; }
		public ControlReference DeclarantTypeDropEdit { get; }
		public ControlReference TransportModeDropEdit { get; }
		public ControlReference DeclarantAddressDropEdit { get; }
		public ControlReference RepresentativeAddressDropEdit { get; }
		public ControlReference OfficeOfExportCodeFindBox { get; }
		public ControlReference EnquiryInformationCodeDropEdit { get; }
		public ControlReference AdditionalDeclarationTypeDropEdit { get; }
		public ControlReference LocationTextBox { get; }
	}
}
