using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public sealed class ExportInvoiceLineDetailsLayout : IPanelLayoutProvider
{
	#region IPanelLayoutProvider

	PanelLayout IPanelLayoutProvider.Layout => layout ?? (layout = CreateLayout());
	PanelLayout layout;

	#endregion

	PanelLayout CreateLayout()
	{
		var builder = new ExportInvoiceLineDetailsLayoutBuilder();
		var commonBag = builder.CommonBag;
		var euBag = EU.GUI.InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(euBag);
		var itBag = InvoiceLineDetailsControlBag.Instance;
		builder.AddControlBag(itBag);

		builder.AddColumn();
		builder.Add(commonBag.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
		builder.Add(itBag.InvoiceNumberDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
		builder.Add(euBag.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
		builder.Add(itBag.OriginCountryStateUserControl, ControlWidthClass.Auto);
		builder.Add(itBag.CountryOfExportDropEdit, ControlWidthClass.Auto);
		builder.Add(itBag.CountryOfDestinationDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.PartNoCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.TariffFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.DescriptionLongTextControl, ControlWidthClass.Long);
		builder.Add(euBag.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
		builder.Add(euBag.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
		builder.Add(euBag.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
		builder.Add(itBag.PortTaxRateDropEdit, ControlWidthClass.Auto);
		builder.Add(euBag.CusNumberCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.CommodityCodeFindBox, ControlWidthClass.Auto);

		builder.AddColumn();
		builder.Add(commonBag.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.WeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.NetWeightCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.VolumeCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.PreviousEntryNumberTextBox, ControlWidthClass.Medium);
		builder.Add(commonBag.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Medium);

		builder.SetVisibility(euBag.CusNumberCodeFindBox, x => x.Declaration?.IsUCC6 ?? ZBool.False);
		builder.SetVisibility(itBag.CountryOfDestinationDropEdit, isVisible: IsUCC6, dependencies: MessageVersionInfoDependency);
		builder.SetVisibility(itBag.CountryOfExportDropEdit, isVisible: IsUCC6, dependencies: MessageVersionInfoDependency);
		builder.SetVisibility(euBag.AdditionalProcedureCodesUserControl, isVisible: IsUCC6, dependencies: MessageVersionInfoDependency);
		builder.SetVisibility(itBag.InvoiceNumberDropEdit, isVisible: IsUCC6, dependencies: MessageVersionInfoDependency);

		builder.SetCaption(itBag.CountryOfDestinationDropEdit, i => countryOfDestinationCaption);

		return builder.Build();
	}

	Func<JobComInvoiceLine, bool> IsUCC6 => line => line.Declaration?.IsUCC6 ?? false;

	Func<JobComInvoiceLine, ZPropertyInfo>[] MessageVersionInfoDependency => new Func<JobComInvoiceLine, ZPropertyInfo>[] { line => line.Declaration?.MessageVersionInfo };

	readonly ResourceStringData countryOfDestinationCaption = Res.GetData("C70905FB-8D6F-4A5D-96C0-7165AD3E70E7",
		englishCaption: "Country of Destination",
		englishShortCaption: "Dest. Country",
		englishMediumCaption: "Country of Destination",
		englishFullDescription: "Country of Destination of the goods being moved.");
}
