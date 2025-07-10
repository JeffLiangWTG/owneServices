using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineDetailsLayout))]
	sealed class ImportInvoiceLineDetailsLayoutTest : LayoutsAbstractTest
	{
		public void TestRegionOfDestinationDropEditVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
				AssertRegionOfDestinationDropEditVisible("RegionOfDestinationDropEdit not visible for import UCC5", false);

				declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.Interfaced;
				AssertRegionOfDestinationDropEditVisible("RegionOfDestinationDropEdit visible for import UCC6", true);

				void AssertRegionOfDestinationDropEditVisible(string testCase, bool expectedVisible)
				{
					using var form = new ZForm(declaration);
					using var control = new ImportInvoiceLineUserControl();
					control.JobDeclaration = declaration;
					control.InitializeGridLayout();

					form.Controls.Add(control);
					form.Show();

					var regionOfDestinationDropEdit = control.FindSingleOrDefault<ZDropEdit>("RegionOfDestinationDropEdit");

					AssertEquals($"{testCase}: RegionOfDestinationDropEdit.visible", expectedVisible, regionOfDestinationDropEdit.Visible);
				}
			});
		}

		protected override int ControlBagCount => 3;

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
				yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PartNoCodeFindBox, ControlWidthClass.Auto);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.FormattedProcedureCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalProcedureCodesUserControl, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.CusNumberCodeFindBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WithDescriptionTariffFindBox, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
				yield return (ImportInvoiceLineDetailsControlBag.Instance.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
				yield return (ImportInvoiceLineDetailsControlBag.Instance.CountryOfSupplyCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.RegionOfDestinationDropEdit, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WithDescriptionPrimaryPreferenceDropEdit, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.QuotaDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.ValuationCodeDropEdit, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.TaxTypeDropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode1DropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.SupplementaryCode2DropEdit, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.AdditionalSupplementaryCodesUserControl, ControlWidthClass.Long);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> ThirdColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.NetWeightCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Medium);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.DestinationCodeFindBox, ControlWidthClass.Long);
				yield return (EU.GUI.InvoiceLineDetailsControlBag.Instance.DispatchCodeFindBox, ControlWidthClass.Long);
			}
		}
	}
}
