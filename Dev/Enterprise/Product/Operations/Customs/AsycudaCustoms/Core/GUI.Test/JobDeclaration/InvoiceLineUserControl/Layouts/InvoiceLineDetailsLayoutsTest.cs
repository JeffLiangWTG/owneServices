using System.Collections.Generic;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GUI;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	[TestedType(typeof(InvoiceLineDetailsLayouts))]
	sealed class InvoiceLineDetailsLayoutsTest : LayoutsAbstractTest
	{
		public void TestPreviousEntryLineNumberCalcEdit_Visible()
		{
			AssertInWhsAndOutWhsInvoiceLineControlVisibility(nameof(InvoiceLineDetailsUserControl.PreviousEntryLineNumberCalcEdit));
		}

		public void TestPreviousEntryNumberTextBox_Visible()
		{
			AssertInWhsAndOutWhsInvoiceLineControlVisibility(nameof(InvoiceLineDetailsUserControl.PreviousEntryNumberTextBox));
		}

		protected override int ControlBagCount => 2;

		protected override IEnumerable<IEnumerable<(ControlReference, ControlWidthClass)>> IncludedControlsPerColumn
		{
			get
			{
				yield return FirstColumnControls;
				yield return SecondColumnControls;
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> FirstColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.EntryInstructionGuidDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PartNoCodeFindBox, ControlWidthClass.Medium);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.ProcedureCodeFindBox, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.TariffFindBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.DescriptionLongTextControl, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CountryOfOriginCodeFindBox, ControlWidthClass.Long);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.PrimaryPreferenceDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CommodityCodeFindBox, ControlWidthClass.Auto);
			}
		}

		IEnumerable<(ControlReference, ControlWidthClass)> SecondColumnControls
		{
			get
			{
				yield return (CommonInvoiceLineDetailsControlBag.Instance.InvoiceQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsSecondQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.CustomsThirdQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.BondedWhsQuantityCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.LinePriceCurrencyCalcFindBox, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.TaxTypeDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.WeightCalcDropEdit, ControlWidthClass.Auto);
				yield return (CommonInvoiceLineDetailsControlBag.Instance.VolumeCalcDropEdit, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.PreviousEntryNumberTextBox, ControlWidthClass.Auto);
				yield return (InvoiceLineDetailsControlBag.Instance.PreviousEntryLineNumberCalcEdit, ControlWidthClass.Auto);
			}
		}

		protected override ICommonLayoutBuilder CommonLayoutBuilder => new CommonInvoiceLineDetailsLayoutBuilder<JobComInvoiceLine>();

		void AssertInWhsAndOutWhsInvoiceLineControlVisibility(string controlName)
		{
			SetUpTestDataForVisibility();
			using (var form = new JobDeclarationForm(dec))
			{
				form.Show();
				var invoiceLineTab = ((CustomsBrokerageUserControl)form.CustomsBrokerageUserControl).InvoiceLinesTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = invoiceLineTab;
				CombineAssertions(() =>
				{
					var baseInvoiceLineUserControl = invoiceLineTab.FindSingle<BaseInvoiceLineUserControl>("BaseInvoiceLineUserControl");
					var invoiceLineDetailsUserControl = baseInvoiceLineUserControl.LineDetailTabControl;
					var dynamicLineDetailsPanel = invoiceLineDetailsUserControl.FindSingle<DynamicLayoutPanel>("DynamicLineDetailsPanel");
					var control = dynamicLineDetailsPanel.FindSingle<ZTextBox>(controlName);
					AssertEquals("InWhs invoice line should be invisible", false, control.Visible);

					baseInvoiceLineUserControl.CustomsInvoiceLinesBoundGrid.ListManager.Position = 1;
					invoiceLineDetailsUserControl = baseInvoiceLineUserControl.LineDetailTabControl;
					dynamicLineDetailsPanel = invoiceLineDetailsUserControl.FindSingle<DynamicLayoutPanel>("DynamicLineDetailsPanel");
					control = dynamicLineDetailsPanel.FindSingle<ZTextBox>(controlName);
					AssertEquals("OutWhs invoice line should be visible", true, control.Visible);
				});
			}
		}

		void SetUpTestDataForVisibility()
		{
			var inProcedure = Factory.New<RefCusProcedure>();
			inProcedure.ZZ6_ProcedureCode = "AB";
			inProcedure.ZZ6_PreviousProcedureCode = "10";
			inProcedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Botswana;
			inProcedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			inProcedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
			inProcedure.ZZ6_Description = "in procedure";

			var outProcedure = Factory.New<RefCusProcedure>();
			outProcedure.ZZ6_ProcedureCode = "CD";
			outProcedure.ZZ6_PreviousProcedureCode = "11";
			outProcedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Botswana;
			outProcedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
			outProcedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			outProcedure.ZZ6_Description = "out procedure";

			var helper = new WhsDataTestHelper(Factory);
			helper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			dec.JE_OH_Importer = helper.Importer.PK;

			var invoiceHeader = dec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "1";

			inWhsInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			inWhsInvoiceLine.JI_LineNo = 1;
			inWhsInvoiceLine.JI_Procedure = "AB10";

			outWhsInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			outWhsInvoiceLine.JI_LineNo = 2;
			outWhsInvoiceLine.JI_Procedure = "CD11";
		}

		JobDeclaration dec;
		JobComInvoiceLine inWhsInvoiceLine, outWhsInvoiceLine;
	}
}
