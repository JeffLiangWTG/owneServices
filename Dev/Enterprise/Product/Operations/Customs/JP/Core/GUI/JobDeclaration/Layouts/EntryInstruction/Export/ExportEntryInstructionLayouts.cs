using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class ExportEntryInstructionLayouts : IPanelLayoutProvider
	{
		public PanelLayout Layout
		{
			get
			{
				var builder = new ExportEntryInstructionLayoutBuilder();
				var exportBag = builder.CommonBag;
				var commonBag = EntryInstructionControlBag.Instance;
				builder.AddControlBag(commonBag);

				builder.AddColumn();
				builder.Add(exportBag.ExportControlNumberTextBox, ControlWidthClass.Auto);
				builder.Add(exportBag.AwbOrBillNumberTextBox, ControlWidthClass.Auto);
				builder.Add(exportBag.GoodsDescriptionTextBox, ControlWidthClass.Auto);
				builder.Add(commonBag.ValueTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.DeclarationTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.AdditionalDeclarationTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(exportBag.DeclarationCargoTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.TradeTypePanel, ControlWidthClass.Auto);
				builder.Add(commonBag.CargoQuantityCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.WeightzCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.CustomsWeightCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.ContainerCountCalcEdit, ControlWidthClass.Auto);
				builder.Add(exportBag.LoadingConfirmationIsRequiredCheckBox, ControlWidthClass.Auto);
				builder.Add(exportBag.PreInspectedCargoDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.CustomsInspectionCodeTextBox, ControlWidthClass.Auto);

				builder.AddColumn();
				builder.Add(exportBag.VanningLocationsGroupBox, ControlWidthClass.LongControl);

				builder.SetVisibility(exportBag.AwbOrBillNumberTextBox, x => x.JobDeclaration.JE_TransportMode == TransportTypeList.Codes.Air);
				builder.SetVisibility(exportBag.ExportControlNumberTextBox, x => x.JobDeclaration.JE_TransportMode == TransportTypeList.Codes.Sea);
				builder.SetVisibility(exportBag.GoodsDescriptionTextBox, x => x.JobDeclaration.JE_TransportMode == TransportTypeList.Codes.Sea);

				var result = builder.Build();
				return result;
			}
		}
	}
}
