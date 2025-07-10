using System.Collections.Generic;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(ImportLicenseInvoiceLineDetailsLayout))]
	sealed class ImportLicenseInvoiceLineDetailsLayoutTest : BaseInvoiceLineDetailsLayoutTest<ImportLicenseInvoiceLineDetailsLayout>
	{
		public void TestGoodsConditionFieldsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var layout = ((IPanelLayoutProvider)new ImportLicenseInvoiceLineDetailsLayout()).Layout;

			invoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.TemporaryAdmission;
			AssertEquals("JI_UsedMaterialRegime = 1", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.GoodsConditionOperationTypeDropEdit, invoiceLine));
			AssertEquals("JI_UsedMaterialSerialNumber", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.SerialNumberGoodsConditionTextBox, invoiceLine));
			AssertEquals("JI_UsedMaterialManufactureYear", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.YearGoodsConditionTextBox, invoiceLine));
			AssertEquals("JI_BrandName", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.BrandGoodsConditionTextBox, invoiceLine));
			AssertEquals("JI_Model", false, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ModelGoodsConditionTextBox, invoiceLine));

			invoiceLine.JI_UsedMaterialRegime = UsedMaterialRegimeList.Codes.Nationalization;
			AssertEquals("JI_UsedMaterialRegime = 2", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.GoodsConditionOperationTypeDropEdit, invoiceLine));
			AssertEquals("JI_UsedMaterialSerialNumber", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.SerialNumberGoodsConditionTextBox, invoiceLine));
			AssertEquals("JI_UsedMaterialManufactureYear", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.YearGoodsConditionTextBox, invoiceLine));
			AssertEquals("JI_BrandName", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.BrandGoodsConditionTextBox, invoiceLine));
			AssertEquals("JI_Model", true, layout.IsVisible(InvoiceLineDetailsControlBag.Instance.ModelGoodsConditionTextBox, invoiceLine));
		}

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

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (InvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WithDescriptionTariffFindBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.FullGoodsDescriptionTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.NaladiHsCodeFindBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.GoodsConditionSeparatorUserControl, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.UsedMaterialRegimeDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.GoodsConditionOperationTypeDropEdit, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.BrandGoodsConditionTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.ModelGoodsConditionTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.SerialNumberGoodsConditionTextBox, ControlWidthClass.Long);
				yield return (InvoiceLineDetailsControlBag.Instance.YearGoodsConditionTextBox, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.DutyTaxRegimeSeparatorUserControl, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.DutyTaxRegimeDropEdit, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.DutyLegalBaseDropEdit, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.TariffAgreementSeparatorUserControl, ControlWidthClass.Medium);
				yield return (InvoiceLineDetailsControlBag.Instance.TariffAgreementDropEdit, ControlWidthClass.Medium);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Auto);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();
	}
}
