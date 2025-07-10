using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using EUControlBag = Enterprise.Customs.EU.GUI.InvoiceLineDetailsControlBag;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ExportInvoiceLineDetailsLayout))]
	sealed class ExportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
	{
		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn => new[] { Column1, Column2, Column3 };

		CommonInvoiceLineDetailsControlBag CommonInstance => CommonInvoiceLineDetailsControlBag.Instance;
		EUControlBag EUInstance => EUControlBag.Instance;
		ExportInvoiceLineDetailsControlBag IEInstance => ExportInvoiceLineDetailsControlBag.Instance;

		IEnumerable<(ControlReference, ControlWidthClass)> Column1
		{
			get
			{
				yield return (CommonInstance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
				yield return (CommonInstance.PartNoCodeFindBox, ControlWidthClass.Auto);
				yield return (EUInstance.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
				yield return (EUInstance.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
				yield return (EUInstance.CusNumberCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonInstance.WithDescriptionTariffFindBox, ControlWidthClass.Long);
				yield return (CommonInstance.DescriptionLongTextControl, ControlWidthClass.Long);
				yield return (CommonInstance.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonInstance.CountryOfExportCodeFindBox, ControlWidthClass.Long);
				yield return (EUInstance.DestinationCodeFindBox, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> Column2
		{
			get
			{
				yield return (CommonInstance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInstance.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInstance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInstance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
				yield return (EUInstance.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
				yield return (EUInstance.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
				yield return (EUInstance.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
				yield return (EUInstance.NationalAdditionalCode1DropEdit, ControlWidthClass.Medium);
				yield return (EUInstance.NationalAdditionalCode2DropEdit, ControlWidthClass.Medium);
				yield return (EUInstance.NationalAdditionalCodesUserControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> Column3
		{
			get
			{
				yield return (IEInstance.IsMainPackCheckBox, ControlWidthClass.Medium);
				yield return (CommonInstance.WeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInstance.NetWeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInstance.VolumeCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInstance.CommodityCodeFindBox, ControlWidthClass.Medium);
			}
		}

		protected override int ControlBagCount => 3;

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
	}
}
