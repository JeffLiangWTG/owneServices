using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed class ImportInvoiceLineDetailsLayout : IPanelLayoutProvider
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
			builder.Add(commonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.InvoiceNumberDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
			builder.Add(euBag.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.WithDescriptionTariffFindBox, ControlWidthClass.Long);
			builder.Add(euBag.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
			builder.Add(euBag.AdditionalSupplementaryCodesAndGDMUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
			builder.Add(commonBag.CountryOfOriginDropEdit, ControlWidthClass.Long);
			builder.Add(euBag.CountryOfSupplyCodeFindBox, ControlWidthClass.Long);

			builder.AddColumn();
			builder.Add(euBag.PreferenceCodeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			builder.Add(deBag.NetPriceCurrencyCalcFindBox, ControlWidthClass.Auto);
			builder.Add(deBag.CessionManagementFlagDropEdit, ControlWidthClass.Medium);
			builder.Add(commonBag.PartNoCodeFindBox, ControlWidthClass.Auto);
			builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);
			builder.Add(euBag.QuotaDropEdit, ControlWidthClass.Auto);
			builder.Add(deBag.QuotaQtyCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(deBag.SupplementaryInformationTextBox, ControlWidthClass.Long);
			builder.Add(deBag.ExportCountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(deBag.DecisiveDateEdit, ControlWidthClass.Auto);

			builder.AddColumn();
			builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(commonBag.CustomsFourthQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.Add(deBag.TobaccoStampTextBox, ControlWidthClass.Auto);

			builder.Add(commonBag.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
			builder.SetVisibility(commonBag.BondedWhsQuantityCalcDropEdit, line => line.IsBondedWhsQuantityVisible, line => line.JI_CEIInfo, line => line.JI_ProcedureInfo);
			builder.AddControlBehaviour<ZCalcDropEdit>(commonBag.BondedWhsQuantityCalcDropEdit, UpdateBondedWhsQuantityCalcDropEdit);

			builder.Add(commonBag.SerialNumberTextBox, ControlWidthClass.Auto);

			builder.Add(commonBag.PreviousEntryNumberTextBox, ControlWidthClass.Auto);
			builder.SetVisibility(commonBag.PreviousEntryNumberTextBox, line => line.IsPreviousEntryNumberVisible, line => line.JI_CEIInfo, line => line.JI_ProcedureInfo);
			builder.SetCaption(commonBag.PreviousEntryNumberTextBox, line => line?.JI_PreviousEntryNumberBondedWarehouseCaption);

			builder.Add(commonBag.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Auto);
			builder.SetVisibility(commonBag.PreviousEntryLineNumberCalcEdit, line => line.IsPreviousEntryNumberVisible, line => line.JI_CEIInfo, line => line.JI_ProcedureInfo);
			builder.SetCaption(commonBag.PreviousEntryLineNumberCalcEdit, line => line?.JI_PreviousEntryLineNumberBondedWarehouseCaption);

			builder.SetVisibility(deBag.ExportCountryCodeFindBox, i => i.EntryInstruction != null && i.EntryInstruction.CEI_Style == ImportDeclarationTypeList.Codes.LUZ, i => i.JI_CEIInfo);
			builder.SetVisibility(deBag.DecisiveDateEdit, i => i.EntryInstruction != null && i.EntryInstruction.CEI_Style == ImportDeclarationTypeList.Codes.LUZ, i => i.JI_CEIInfo);

			builder.SetCaption(deBag.ExportCountryCodeFindBox, i => Res.GetData("94BEE9D8-44E2-49F8-84EF-6EDE87DC6294", "Origin"));

			builder.Add(deBag.BondedWHSOrderNumberTextBox, ControlWidthClass.Auto);
			builder.SetVisibility(deBag.BondedWHSOrderNumberTextBox, line => line.IsOutOfWarehouseWarehousing, line => line.JI_CEIInfo, line => line.JI_ProcedureInfo);
			builder.Add(deBag.BondedWHSOrderLineNumberCalcEdit, ControlWidthClass.Auto);
			builder.SetVisibility(deBag.BondedWHSOrderLineNumberCalcEdit, line => line.IsOutOfWarehouseWarehousing, line => line.JI_CEIInfo, line => line.JI_ProcedureInfo);

			return builder.Build();
		}

		static void UpdateBondedWhsQuantityCalcDropEdit(ZCalcDropEdit control, JobComInvoiceLine invoiceLine)
		{
			control.UnitPreBoundMaxLength = 3;
		}
	}
}
