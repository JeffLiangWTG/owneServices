using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class ECREntryInstructionLayouts : IPanelLayoutProvider
	{
		public PanelLayout Layout
		{
			get
			{
				var builder = new ECREntryInstructionLayoutBuilder();
				var ecrControlBag = builder.CommonBag;

				var exportInstructionBag = ExportEntryInstructionControlBag.Instance;
				builder.AddControlBag(exportInstructionBag);

				var commonInstructionBag = EntryInstructionControlBag.Instance;
				builder.AddControlBag(commonInstructionBag);

				var commonDeclarationBag = CommonDeclarationControlBag.Instance;
				builder.AddControlBag(commonDeclarationBag);

				builder.AddColumn();
				builder.Add(ecrControlBag.CusEntryInstructionNSITextBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.ExportControlNumberUserControl, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.ExportCodeTextBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.ExportNameTextBox, ControlWidthClass.Auto);
				builder.Add(commonDeclarationBag.DeclarantCodeTextBox, ControlWidthClass.Auto);
				builder.Add(exportInstructionBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
				builder.Add(commonInstructionBag.CargoQuantityCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(commonInstructionBag.CustomsWeightCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(ecrControlBag.CustomsVolumeCalcDropEdit, ControlWidthClass.Auto);

				builder.AddColumn();
				builder.Add(commonDeclarationBag.DeclarationReferenceTextBox, ControlWidthClass.Auto);
				builder.Add(ecrControlBag.ECRNotesTextBox, ControlWidthClass.Auto);
				builder.Add(ecrControlBag.SpecialCargoCodeFindBox, ControlWidthClass.Auto);
				builder.Add(ecrControlBag.ECRCargoTypeDropEdit, ControlWidthClass.Auto);

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

				builder.SetCaption(ecrControlBag.SpecialCargoCodeFindBox, c => Res.GetData("2D210689-B04F-44B1-94A4-46F332F09D5B", "Dangerous Goods"));
				return builder.Build();
			}
		}
	}
}
