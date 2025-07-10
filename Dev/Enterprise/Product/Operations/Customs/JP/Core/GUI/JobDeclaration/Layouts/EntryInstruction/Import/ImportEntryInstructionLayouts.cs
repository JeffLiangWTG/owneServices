using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public sealed class ImportEntryInstructionLayouts : IPanelLayoutProvider
	{
		public PanelLayout Layout
		{
			get
			{
				var builder = new ImportEntryInstructionLayoutBuilder();
				var importBag = builder.CommonBag;
				var commonBag = EntryInstructionControlBag.Instance;
				builder.AddControlBag(commonBag);

				builder.AddColumn();
				builder.Add(commonBag.ValueTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.DeclarationTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.AdditionalDeclarationTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(importBag.BondedLocationCodeFindBox, ControlWidthClass.Auto);
				builder.Add(importBag.BondedLocationNameTextBox, ControlWidthClass.Long);
				builder.Add(importBag.BeforePermitApplicationReasonDropEdit, ControlWidthClass.Auto);
				builder.Add(importBag.DeclarationCargoTypeDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.TradeTypePanel, ControlWidthClass.Auto);
				builder.Add(commonBag.CargoQuantityCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.WeightzCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.CustomsWeightCalcDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.ContainerCountCalcEdit, ControlWidthClass.Auto);
				builder.Add(importBag.ContentInspectionResultDropEdit, ControlWidthClass.Auto);
				builder.Add(importBag.DutyDrawbackDropEdit, ControlWidthClass.Auto);
				builder.Add(commonBag.CustomsInspectionCodeTextBox, ControlWidthClass.Auto);

				builder.AddColumn();
				builder.Add(importBag.SpecialDeclarationOfficeGroupBox, ControlWidthClass.LongControl);

				builder.SetVisibility(importBag.SpecialDeclarationOfficeGroupBox, x =>  x.IsSpecialDeclaration);
				builder.SetVisibility(importBag.BondedLocationCodeFindBox, x => x.IsInbondDeclarationType);
				builder.SetVisibility(importBag.BondedLocationNameTextBox, x => x.IsInbondDeclarationType);

				var result = builder.Build();
				result.CollapseEmptyRows = true;
				return result;
			}
		}
	}
}
