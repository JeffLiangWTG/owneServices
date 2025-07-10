using System.Collections.Generic;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineDetailsLayout))]
	sealed class ImportInvoiceLineDetailsLayoutTest : BaseInvoiceLineDetailsLayoutTest<ImportInvoiceLineDetailsLayout>
	{
		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
				yield return ThirdColumnControls;
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (InvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WithDescriptionTariffFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.FullGoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.ComplementaryDescriptionTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.GoodsApplicationDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.GoodsConditionDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.ManufacturerIndicatorDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.ManufacturerAddressControl, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.NaladiNccaCodeFindBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.NaladiHsCodeFindBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.ImportLicenseNumberTextBox, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.RequiresImportLicenseCheckBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.ImportLicenseFineSeparatorUserControl, ControlWidthClass.LongNoCaption);
				yield return (InvoiceLineDetailsControlBag.Instance.ImportLicenseTypeDropEdit, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.ImportLicenseAuthorizationDateTextBox, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.ImportLicenseFeeTypeDropEdit, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Auto);
			}
		}

		public void TestVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			var layout = ((IPanelLayoutProvider)new ImportInvoiceLineDetailsLayout()).Layout;
			CombineAssertions(() =>
			{
				AssertEquals("ImportLicenseNumberTextBox Should not be visible", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ImportLicenseNumberTextBox, invoiceLine));
				AssertEquals("RequiresImportLicenseCheckBox Should not be visible", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.RequiresImportLicenseCheckBox, invoiceLine));
				AssertEquals("ImportLicenseFineSeparatorUserControl Should not be visible", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ImportLicenseFineSeparatorUserControl, invoiceLine));
				AssertEquals("ImportLicenseTypeDropEdit Should not be visible", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ImportLicenseTypeDropEdit, invoiceLine));
				AssertEquals("ImportLicenseAuthorizationDateTextBox Should not be visible", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ImportLicenseAuthorizationDateTextBox, invoiceLine));
				AssertEquals("ImportLicenseFeeTypeDropEdit Should not be visible", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ImportLicenseFeeTypeDropEdit, invoiceLine));
				AssertEquals("GoodsApplicationDropEdit Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.GoodsApplicationDropEdit, invoiceLine));
				AssertEquals("GoodsConditionDropEdit Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.GoodsConditionDropEdit, invoiceLine));
				AssertEquals("NaladiHsCodeFindBox Should be visible", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.NaladiHsCodeFindBox, invoiceLine));
				AssertEquals("NaladiNccaCodeFindBox Should be visible", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.NaladiNccaCodeFindBox, invoiceLine));
				AssertEquals("ComplementaryDescriptionTextBox Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ComplementaryDescriptionTextBox, invoiceLine));
			});

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			CombineAssertions(() =>
			{
				AssertEquals("ImportLicenseNumberTextBox Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ImportLicenseNumberTextBox, invoiceLine));
				AssertEquals("RequiresImportLicenseCheckBox Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.RequiresImportLicenseCheckBox, invoiceLine));
				AssertEquals("ImportLicenseFineSeparatorUserControl Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ImportLicenseFineSeparatorUserControl, invoiceLine));
				AssertEquals("ImportLicenseTypeDropEdit Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ImportLicenseTypeDropEdit, invoiceLine));
				AssertEquals("ImportLicenseAuthorizationDateTextBox Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ImportLicenseAuthorizationDateTextBox, invoiceLine));
				AssertEquals("ImportLicenseFeeTypeDropEdit Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ImportLicenseFeeTypeDropEdit, invoiceLine));
				AssertEquals("GoodsApplicationDropEdit Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.GoodsApplicationDropEdit, invoiceLine));
				AssertEquals("GoodsConditionDropEdit Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.GoodsConditionDropEdit, invoiceLine));
				AssertEquals("ComplementaryDescriptionTextBox Should not be visible", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ComplementaryDescriptionTextBox, invoiceLine));
			});

			declaration.MakeNonPersistent();
			invoice.JZ_MessageType = BRJobMessageTypeList.Codes.Import;
			AssertEquals("RequiresImportLicenseCheckBox Should be visible", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.RequiresImportLicenseCheckBox, invoiceLine));
		}
	}
}
