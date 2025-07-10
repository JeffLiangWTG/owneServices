using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.GUI.Testing;

class ImportInvoiceLineUserControlTest : BaseInvoiceLineUserControlTest<ImportInvoiceLineUserControl>
{
	protected override ImportInvoiceLineUserControl GetInvoiceLineUserControlForTest() => new ImportInvoiceLineUserControlForTest();

	protected override IEnumerable<ZTabPage> GetExpectedTabPagesInOrder(ImportInvoiceLineUserControl control)
	{
		yield return control.LineDetailTabControl.FindSingle<ZTabPage>("NewLineDetailsTabPage");
		yield return control.LineChargesTabPage;
		yield return control.SupportingDocumentsTabPage;
		yield return control.PackagesPivotTabPage;
		yield return control.TaxesAndFeesTabPage;
		yield return control.PermitsTabPage;
		yield return control.NonCustomsLawTabPage;
		yield return control.InAndOutwardProcessingTabPage;
		yield return control.AdditionalInformationTabPage;
		yield return control.VehiclesTabPage;
		yield return control.TobaccosTabPage;
		yield return control.SpecialMentionsTabPage;
		yield return control.LineDetailTabControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
	}

	protected override List<KeyValuePair<string, Type>> GetDefaultColumnsForGrid()
	{
		return new List<KeyValuePair<string, Type>>
			{
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_LineNo, typeof(ZCalcEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLine.Schema.EntryLineNumber, typeof(ZCalcEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_CEI, typeof(ZGuidDropEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_Procedure, typeof(ZCodeFindBoxColumnStyleInfo)),
				new KeyValuePair<string, Type>(Customs.Business.BaseJobComInvoiceLine.Schema.JI_FormattedTariff, typeof(Universal.GUI.TariffColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_Description, typeof(ZTextBoxColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin, typeof(ZCodeFindBoxColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_PrimaryPreference, typeof(ZDropEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity, typeof(ZCalcEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ, typeof(ZDropEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_Weight, typeof(ZCalcEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_WeightUQ, typeof(ZDropEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_NetWeight, typeof(ZCalcEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_NetWeightUQ, typeof(ZDropEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_LinePrice, typeof(ZCalcEditColumnStyleInfo)),
			};
	}

	protected override JobDeclaration GetJobDeclarationForTest()
	{
		var declaration = base.GetJobDeclarationForTest();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		return declaration;
	}

	public void TestColumnLayoutContextForInvoiceLinesGrid()
	{
		using (var control = new ImportInvoiceLineUserControl())
		{
			AssertEquals("Column layout context should be import", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestDynamicLayoutApplied()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			Assert(control.DynamicLayoutApplied);
		}
	}

	public void TestHasDifferentPanelLayout()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(true, control.HasDifferentPanelLayout);
		}
	}

	public void TestUniversalTariffType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(UniversalReferenceConstants.TariffTypes.ImportTariff, control.UniversalTariffType);
		}
	}

	public void TestLineSummaryControls()
	{
		Declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		using (var form = new ZForm(Declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.SetDataBinding(Declaration, ZString.Empty);
			control.JobDeclaration = Declaration;
			form.Controls.Add(control);
			form.Show();

			CombineAssertions(() =>
			{
				AssertCalcFieldInvisible(control, "JI_Calc_FreightConvertToLocalCurrencyControl");
				AssertCalcFieldInvisible(control, "JI_Calc_InsuranceConvertToLocalCurrencyControl");
				AssertCalcFieldInvisible(control, "JI_Calc_CIFConvertToLocalCurrencyControl");
				AssertCalcFieldInvisible(control, "JI_Calc_FOBConvertToLocalCurrencyControl");
				AssertCalcField(control, "CalcStatisticalValue", nameof(JobComInvoiceLine.JI_Calc_StatisticalValue));
				AssertCalcField(control, "CalcCustomsValue", nameof(JobComInvoiceLine.JI_CustomsValue));
			});
		}
	}

	public void TestDutyRateUserControlVisibility()
	{
		var tariffHelper = new RefCusTariffTestHelper(Factory);
		var singleRateTariff = tariffHelper.CreateImportTariffWithSingleRate(RefCusTariffTestHelper.ImportTariffBeverages);
		var multipleRateTariff = tariffHelper.CreateImportTariffWithMultipleRates(RefCusTariffTestHelper.ImportTariffBycycle);

		var invoiceLine = (JobComInvoiceLine)Declaration.InvoiceLines.First();
		invoiceLine.JI_CountryOfOrigin = RefCusTariffTestHelper.Country;
		invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.PreferentialTariff;

		using (var form = new ZForm(Declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.SetDataBinding(Declaration, ZString.Empty);
			control.JobDeclaration = Declaration;
			form.Controls.Add(control);
			form.Show();

			var dutyRateUserControl = control.FindSingle<DutyRateUserControl>("DutyRateUserControl");

			CombineAssertions(() =>
			{
				invoiceLine.JI_Tariff = singleRateTariff.ZZ1_TariffCode;
				AssertEquals("Single rate", false, dutyRateUserControl.Visible);
				invoiceLine.JI_Tariff = multipleRateTariff.ZZ1_TariffCode;
				AssertEquals("Multiple rates", true, dutyRateUserControl.Visible);
				invoiceLine.JI_PrimaryPreference = UniversalReferenceConstants.PrimaryPreferenceCodes.NormalTariff;
				AssertEquals("Single rate due to preference", false, dutyRateUserControl.Visible);
			});
		}
	}

	public void TestTaxesAndFeesUserControl()
	{
		using (var control = new ImportInvoiceLineUserControl())
		{
			CombineAssertions(() =>
			{
				AssertEquals("AdditionalTaxesUserControl binding", "FilteredInvoiceLines.AdditionalTaxes", control.BindingSource.GetBindingMember(control.AdditionalTaxesUserControl));
				AssertEquals("AdditionalFeesUserControl binding", "FilteredInvoiceLines.AdditionalFees", control.BindingSource.GetBindingMember(control.AdditionalFeesUserControl));
				AssertEquals("Tab contains AdditionalTaxesUserControl", true, control.TaxesAndFeesTabPage.Contains(control.AdditionalTaxesUserControl));
				AssertEquals("Tab contains AdditionalFeesUserControl", true, control.TaxesAndFeesTabPage.Contains(control.AdditionalFeesUserControl));
			});
		}
	}

	class ImportInvoiceLineUserControlForTest : ImportInvoiceLineUserControl
	{
		public new ZBool DynamicLayoutApplied => base.DynamicLayoutApplied;
		public new bool HasDifferentPanelLayout => base.HasDifferentPanelLayout;
		public new string UniversalTariffType => base.GetUniversalTariffType();
	}
}
