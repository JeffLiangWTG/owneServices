using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class CommonDeclarationControlBag : ControlBag
	{
		public static CommonDeclarationControlBag Instance => instance ?? (instance = new CommonDeclarationControlBag());

		[ThreadStatic]
		static CommonDeclarationControlBag instance;

		public CommonDeclarationControlBag()
		{
			ExportCodeTextBox = RegisterControl(nameof(ExportCodeTextBox));
			ExportNameTextBox = RegisterControl(nameof(ExportNameTextBox));
			DeclarantCodeTextBox = RegisterControl(nameof(DeclarantCodeTextBox));
			CarrierCodeCodeFindBox = RegisterControl(nameof(CarrierCodeCodeFindBox));
			VesselCodeFindBox = RegisterControl(nameof(VesselCodeFindBox));
			VesselNameCodeFindBox = RegisterControl(nameof(VesselNameCodeFindBox));
			DeclarationReferenceTextBox = RegisterControl(nameof(DeclarationReferenceTextBox));
			FinalDestinationCodeFindBox = RegisterControl(nameof(FinalDestinationCodeFindBox));
			ExportControlNumberUserControl = RegisterControl(nameof(ExportControlNumberUserControl));
			VoyageFlightNoBoundTextBox = RegisterControl(nameof(VoyageFlightNoBoundTextBox));
			DateOfArrivalBoundDateEdit = RegisterControl(nameof(DateOfArrivalBoundDateEdit));
			PortOfLoadingCodeFindBox = RegisterControl(nameof(PortOfLoadingCodeFindBox));
			ExportDateBoundDateEdit = RegisterControl(nameof(ExportDateBoundDateEdit));
			PortOfDischargeCodeFindBox = RegisterControl(nameof(PortOfDischargeCodeFindBox));
			ReceiptModeDropEdit = RegisterControl(nameof(ReceiptModeDropEdit));
			DeliveryModeDropEdit = RegisterControl(nameof(DeliveryModeDropEdit));
			BookingNumberTextBox = RegisterControl(nameof(BookingNumberTextBox));
			AllEntryInsSeparatorUserControl = RegisterControl(nameof(AllEntryInsSeparatorUserControl));
		}

		protected override Control CreateTemplate() => new CommonDeclarationLayoutTemplate();

		public ControlReference ExportCodeTextBox { get; }
		public ControlReference ExportNameTextBox { get; }
		public ControlReference DeclarantCodeTextBox { get; }
		public ControlReference CarrierCodeCodeFindBox { get; }
		public ControlReference VesselCodeFindBox { get; }
		public ControlReference VesselNameCodeFindBox { get; }
		public ControlReference DeclarationReferenceTextBox { get; }
		public ControlReference FinalDestinationCodeFindBox { get; }
		public ControlReference ExportControlNumberUserControl { get; }
		public ControlReference VoyageFlightNoBoundTextBox { get; }
		public ControlReference DateOfArrivalBoundDateEdit { get; }
		public ControlReference PortOfLoadingCodeFindBox { get; }
		public ControlReference ExportDateBoundDateEdit { get; }
		public ControlReference PortOfDischargeCodeFindBox { get; }
		public ControlReference ReceiptModeDropEdit { get; }
		public ControlReference DeliveryModeDropEdit { get; }
		public ControlReference BookingNumberTextBox { get; }
		public ControlReference AllEntryInsSeparatorUserControl { get; }
	}
}
