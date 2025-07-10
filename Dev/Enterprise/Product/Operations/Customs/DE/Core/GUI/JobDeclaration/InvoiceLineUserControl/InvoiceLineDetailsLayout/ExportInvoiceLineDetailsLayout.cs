using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class ExportInvoiceLineDetailsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var builder = new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
			var commonBag = builder.CommonBag;
			var euBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
			builder.AddControlBag(euBag);
			var deBag = InvoiceLineDetailsControlBag.Instance;
			builder.AddControlBag(deBag);

			builder.AddColumn();
			builder.Add(deBag.FixedMaxLengthEntryInstructionGuidDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.FixedMaxLengthInvoiceNumberDropEdit, ControlWidthClass.Long);
			builder.SetCaption(deBag.FixedMaxLengthInvoiceNumberDropEdit, _ => Res.GetData("aaeb4913-3f6a-4cf0-a825-e1745996b010", "Invoice Number"));
			builder.Add(commonBag.PartNoCodeFindBox, ControlWidthClass.Auto);
			builder.Add(euBag.UnformattedProcedureCodeFindBox, ControlWidthClass.Long);
			builder.Add(deBag.FixedMaxLengthWithDescriptionTariffFindBox, ControlWidthClass.Long);
			builder.Add(euBag.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
			builder.SetCaption(euBag.SupplementaryCode1DropEdit, _ => Res.GetData("5742c608-ec4a-4e96-8f5a-fc483dfcb657", "Sup. Code 1"));
			builder.Add(euBag.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
			builder.SetCaption(euBag.SupplementaryCode2DropEdit, _ => Res.GetData("ba156daa-7e27-4dbb-a3a1-5abde4f90bcc", "Sup. Code 2"));
			builder.Add(euBag.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
			builder.SetCaption(euBag.AdditionalSupplementaryCodesAndGDMUserControl, _ => Res.GetData("78d45206-9148-4e22-b798-9f36716bae88", "Add. Sup. Codes"));
			builder.Add(deBag.DescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.OriginFederalStateDropEdit, ControlWidthClass.Medium);
			builder.Add(deBag.ExportCountryCodeFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(deBag.IsMainPackCheckBox, ControlWidthClass.Auto);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(euBag.CusNumberCodeFindBox, ControlWidthClass.Auto);
			builder.Add(deBag.DgSubstanceUserControl, ControlWidthClass.Auto);
			builder.Add(deBag.UsualReplacementCheckBox, ControlWidthClass.Auto);
			builder.Add(deBag.ReimportDateEdit, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.AddControlBehaviour<ZCalcDropEdit>(commonBag.BondedWhsQuantityCalcDropEdit, UpdateBondedWhsQuantityCalcDropEdit);

			builder.Add(commonBag.SerialNumberTextBox, ControlWidthClass.Auto);

			builder.Add(commonBag.PreviousEntryNumberTextBox, ControlWidthClass.Auto);
			builder.SetCaption(commonBag.PreviousEntryNumberTextBox, line => line?.JI_PreviousEntryNumberBondedWarehouseCaption);

			builder.Add(commonBag.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Auto);
			builder.SetCaption(commonBag.PreviousEntryLineNumberCalcEdit, line => line?.JI_PreviousEntryLineNumberBondedWarehouseCaption);

			builder.Add(deBag.BondedWHSOrderNumberTextBox, ControlWidthClass.Auto);
			builder.SetVisibility(deBag.BondedWHSOrderNumberTextBox, line => line.IsExportWarehouseVisible, line => line.JI_ProcedureInfo);

			builder.Add(deBag.BondedWHSOrderLineNumberCalcEdit, ControlWidthClass.Auto);
			builder.SetVisibility(deBag.BondedWHSOrderLineNumberCalcEdit, line => line.IsExportWarehouseVisible, line => line.JI_ProcedureInfo);

			return builder.Build();
		}

		static void UpdateBondedWhsQuantityCalcDropEdit(ZCalcDropEdit control, JobComInvoiceLine invoiceLine)
		{
			control.UnitPreBoundMaxLength = 3;
		}
	}
}
