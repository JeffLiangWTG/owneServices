using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	public class BaseInvoiceLineUserControlTest : BaseInvoiceLineUserControlForVirtualPropertiesTest<BaseInvoiceLineUserControl>
	{
		public void TestDefaultColumns()
		{
			using (var control = new BaseInvoiceLineUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				var lineGrid = control.CustomsInvoiceLinesBoundGrid;
				var index = 0;
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_LineNo, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_Calc_Invoice, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CEI, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CEI_Description, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_PartNo, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_Procedure, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CC, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_Tariff, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_PrimaryPreference, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_InvoiceQuantity, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_InvoiceUQ, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomsQuantity, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomsUnitQty, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomsSecondQuantity, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomsThirdQuantity, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_LinePrice, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_ZZF_NKTaxType, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_Description, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CountryOfOrigin, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_Weight, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_WeightUQ, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_NetWeight, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_NetWeightUQ, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_Volume, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_VolumeUQ, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_OrderNumber, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_PartAttrib1, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_PartAttrib2, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_PartAttrib3, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_SerialNumber, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.UnitPrice, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomAttrib1, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomAttrib2, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomAttrib3, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomAttrib4, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomAttrib5, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomAttrib6, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_CustomTextBlob1, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_PreviousEntryNumber, index++);
				AssertDefaultColumn(lineGrid, JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber, index++);
				AssertDefaultColumn(lineGrid, nameof(JobComInvoiceLine.VehicleVIN), index++);
			}
		}

		static void AssertDefaultColumn(ZGrid lineGrid, string columnName, int index)
		{
			AssertEquals($"Column {columnName} is visible.", true, lineGrid.GetColumnStyle(columnName).IsVisible);
			AssertEquals($"Column {columnName} should be at index {index}", index, lineGrid.ColumnStyles.IndexOf(lineGrid.GetColumnStyle(columnName)));
		}

		public void TestColumnsWithGroupNames()
		{
			using (var control = new BaseInvoiceLineUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				var lineGrid = control.CustomsInvoiceLinesBoundGrid;
				AssertGroupName(lineGrid, JobComInvoiceLine.Schema.JI_CustomsQuantity, "Customs Quantity");
				AssertGroupName(lineGrid, JobComInvoiceLine.Schema.JI_CustomsUnitQty, "Customs Quantity");
				AssertGroupName(lineGrid, JobComInvoiceLine.Schema.JI_CustomsSecondQuantity, "Additional Quantity 1");
				AssertGroupName(lineGrid, JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty, "Additional Quantity 1");
				AssertGroupName(lineGrid, JobComInvoiceLine.Schema.JI_CustomsThirdQuantity, "Additional Quantity 2");
				AssertGroupName(lineGrid, JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty, "Additional Quantity 2");
			}
		}

		void AssertGroupName(ZGrid lineGrid, string columnName, string groupName)
		{
			AssertEquals($"Column {columnName} should have a GroupName", groupName, lineGrid.GetColumnStyle(columnName).GroupName.Caption);
		}

		public void TestEntryInstructionDescriptionColumnIsReadOnly()
		{
			using (var control = new BaseInvoiceLineUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				var lineGrid = control.CustomsInvoiceLinesBoundGrid;
				var columnStyle = lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI_Description);
				AssertEquals("Is Read Only", true, columnStyle.IsReadOnly);
			}
		}

		public void TestControls()
		{
			using (var control = new BaseInvoiceLineUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("JI_Calc_CIFConvertToLocalCurrencyControl", "VAT/GST Value", control.FindSingle<ConvertToLocalCurrencyControl>("JI_Calc_CIFConvertToLocalCurrencyControl").CaptionResourceString.Caption);
					AssertEquals("JI_Calc_FOBConvertToLocalCurrencyControl", "Customs Value", control.FindSingle<ConvertToLocalCurrencyControl>("JI_Calc_FOBConvertToLocalCurrencyControl").CaptionResourceString.Caption);
					AssertEquals("JI_Calc_FOBInLocalCurrencyControl", "Customs Value", control.FindSingle<ConvertToLocalCurrencyControl>("JI_Calc_FOBInLocalCurrencyControl").CaptionResourceString.Caption);
					AssertEquals("JI_Calc_CIF_InLocalCurrencyControl", "VAT/GST Value", control.FindSingle<ConvertToLocalCurrencyControl>("JI_Calc_CIF_InLocalCurrencyControl").CaptionResourceString.Caption);
					AssertNotNull("PreviousEntryNumberTextBox", control.FindSingleOrDefault<Control>("PreviousEntryNumberTextBox"));
					AssertNotNull("PreviousEntryLineNumberCalcEdit", control.FindSingleOrDefault<Control>("PreviousEntryLineNumberCalcEdit"));
					AssertNotNull("SupportingDocumentsUserControl", control.FindSingleOrDefault<Control>("SupportingDocumentsUserControl"));
				});
			}
		}

		public void TestSetupCustomsInvoiceLinesBoundGridColumns()
		{
			using (var userControl = new BaseInvoiceLineUserControl())
			{
				var declaration = Factory.New<JobDeclaration>();
				userControl.JobDeclaration = declaration;
				userControl.InitializeGridLayout();
				CombineAssertions(() =>
				{
					AssertNotNull("User control should have JI_PreviousEntryNumber column", userControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PreviousEntryNumber));
					AssertNotNull("User control should have JI_PreviousEntryLineNumber column", userControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PreviousEntryLineNumber));
					AssertNotNull("User control should have JI_CEI column", userControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CEI));
				});
			}
		}

		public void TestTariffFindBox()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Bangladesh))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var customsBrokerageUserControl = form.CustomsBrokerageUserControl;
					customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.InvoiceLinesTabPage;
					var invoiceLinesUserControl = customsBrokerageUserControl.InvoiceLinesUserControl;
					var lineDetailTabControl = invoiceLinesUserControl.FindSingle<ZTabControl>("LineDetailTabControl");
					var newLineDetailsTabPage = lineDetailTabControl.FindSingle<ZTabPage>("NewLineDetailsTabPage");
					lineDetailTabControl.SelectedTab = newLineDetailsTabPage;
					var invoiceLineDetailsUserControl = newLineDetailsTabPage.FindSingle<Customs.GUI.InvoiceLineDetailsUserControl>("InvoiceLineDetailsUserControl");
					var dynamicLineDetailsPanel = invoiceLineDetailsUserControl.FindSingle<DynamicLayoutPanel>("DynamicLineDetailsPanel");
					var tariffFindBox = dynamicLineDetailsPanel.FindSingle<Universal.GUI.TariffFindBox>();
					AssertEquals("GetTariffType", "HSN", tariffFindBox.GetTariffType());
					AssertEquals("EffectiveTariffCountry", Core.Constants.CountryCodes.Bangladesh, tariffFindBox.EffectiveTariffCountry);
					AssertEquals("EffectiveDataGrouping", Core.Constants.CountryCodes.Bangladesh, tariffFindBox.EffectiveDataGrouping);
				}
			}

			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					var customsBrokerageUserControl = form.CustomsBrokerageUserControl;
					customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.InvoiceLinesTabPage;
					var invoiceLinesUserControl = customsBrokerageUserControl.InvoiceLinesUserControl;
					var lineDetailTabControl = invoiceLinesUserControl.FindSingle<ZTabControl>("LineDetailTabControl");
					var newLineDetailsTabPage = lineDetailTabControl.FindSingle<ZTabPage>("NewLineDetailsTabPage");
					lineDetailTabControl.SelectedTab = newLineDetailsTabPage;
					var invoiceLineDetailsUserControl = newLineDetailsTabPage.FindSingle<Customs.GUI.InvoiceLineDetailsUserControl>("InvoiceLineDetailsUserControl");
					var dynamicLineDetailsPanel = invoiceLineDetailsUserControl.FindSingle<DynamicLayoutPanel>("DynamicLineDetailsPanel");
					var tariffFindBox = dynamicLineDetailsPanel.FindSingle<Universal.GUI.TariffFindBox>();
					AssertEquals("GetTariffType", "1P1", tariffFindBox.GetTariffType());
					AssertEquals("EffectiveTariffCountry", Core.Constants.CountryCodes.Botswana, tariffFindBox.EffectiveTariffCountry);
					AssertEquals("EffectiveDataGrouping", Core.Constants.CountryCodes.Botswana, tariffFindBox.EffectiveDataGrouping);
				}
			}
		}

		public void TestInvoiceLineChargesUserControl()
		{
			using (var userControl = new BaseInvoiceLineUserControl())
			{
				AssertType(typeof(BaseInvoiceLineChargesUserControl), userControl.InvoiceLineCharges);
			}
		}

		protected override ZString DefaultUniversalTariffType => "1P1"; //Default country is Botswana in Assemblyinfo.cs
		protected override ZBool DefaultDynamicLayoutApplied => ZBool.True;
	}
}
