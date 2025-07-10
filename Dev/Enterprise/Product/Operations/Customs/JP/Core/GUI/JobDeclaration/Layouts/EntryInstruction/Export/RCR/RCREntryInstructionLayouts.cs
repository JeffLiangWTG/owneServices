using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class RCREntryInstructionLayouts : IPanelLayoutProvider
	{
		public PanelLayout Layout
		{
			get
			{
				var builder = new RCREntryInstructionLayoutBuilder();
				var rcrControlBag = builder.CommonBag;

				var commonDeclarationBag = CommonDeclarationControlBag.Instance;
				builder.AddControlBag(commonDeclarationBag);

				builder.AddColumn();
				builder.Add(rcrControlBag.RCRActionDropEdit, ControlWidthClass.Auto);
				builder.Add(rcrControlBag.PreviousBillNumberTextBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.ExportControlNumberUserControl, ControlWidthClass.Auto);
				builder.Add(rcrControlBag.ViaLocationCodeFindBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.ExportCodeTextBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.ExportNameTextBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.DeclarantCodeTextBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.DeclarationReferenceTextBox, ControlWidthClass.Auto);

				builder.AddColumn();
				builder.Add(commonDeclarationBag.AllEntryInsSeparatorUserControl, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.CarrierCodeCodeFindBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.VesselCodeFindBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.VesselNameCodeFindBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.VoyageFlightNoBoundTextBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.DateOfArrivalBoundDateEdit, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.PortOfLoadingCodeFindBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.ExportDateBoundDateEdit, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.PortOfDischargeCodeFindBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.BookingNumberTextBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.FinalDestinationCodeFindBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.ReceiptModeDropEdit, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.DeliveryModeDropEdit, ControlWidthClass.Auto);

				return builder.Build();
			}
		}
	}
}
