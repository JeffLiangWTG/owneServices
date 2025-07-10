using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

class ExportInvoiceLineUserControlTest : BaseInvoiceLineUserControlTest<ExportInvoiceLineUserControl>
{
	protected override IEnumerable<ZTabPage> GetExpectedTabPagesInOrder(ExportInvoiceLineUserControl control)
	{
		yield return control.LineDetailTabControl.FindSingle<ZTabPage>("NewLineDetailsTabPage");
		yield return control.LineChargesTabPage;
		yield return control.SupportingDocumentsTabPage;
		yield return control.PreviousDocumentsTabPage;
		yield return control.PackagesPivotTabPage;
		yield return control.RestrictionsTabPage;
		yield return control.InAndOutwardProcessingTabPage;
		yield return control.AdditionalInformationTabPage;
		yield return control.VehiclesTabPage;
		yield return control.TobaccosTabPage;
		yield return control.LineDetailTabControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
	}

	protected override List<KeyValuePair<string, Type>> GetDefaultColumnsForGrid()
	{
		return new List<KeyValuePair<string, Type>>()
			{
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_LineNo, typeof(ZCalcEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLine.Schema.EntryLineNumber, typeof(ZCalcEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_CEI, typeof(ZGuidDropEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_Procedure, typeof(ZCodeFindBoxColumnStyleInfo)),
				new KeyValuePair<string, Type>(Customs.Business.BaseJobComInvoiceLine.Schema.JI_FormattedTariff, typeof(Universal.GUI.TariffColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_Description, typeof(ZTextBoxColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_CountryOfOrigin, typeof(ZCodeFindBoxColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_InvoiceQuantity, typeof(ZCalcEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_InvoiceUQ, typeof(ZDropEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_Weight, typeof(ZCalcEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_WeightUQ, typeof(ZDropEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_NetWeight, typeof(ZCalcEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_NetWeightUQ, typeof(ZDropEditColumnStyleInfo)),
				new KeyValuePair<string, Type>(JobComInvoiceLineSchema.Constants.JI_LinePrice, typeof(ZCalcEditColumnStyleInfo)),
			};
	}

	protected override ExportInvoiceLineUserControl GetInvoiceLineUserControlForTest() => new ExportInvoiceLineUserControlForTest();

	protected override JobDeclaration GetJobDeclarationForTest()
	{
		var declaration = base.GetJobDeclarationForTest();
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew();
		return declaration;
	}

	public void TestColumnLayoutContextForInvoiceLinesGrid()
	{
		using (var control = new ExportInvoiceLineUserControl())
		{
			AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestDynamicLayoutApplied()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			Assert(control.DynamicLayoutApplied);
		}
	}

	public void TestHasDifferentPanelLayout()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals(true, control.HasDifferentPanelLayout);
		}
	}

	[RequiresSTA]
	public void TestUniversalTariffType()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals(UniversalReferenceConstants.TariffTypes.ExportTariff, control.UniversalTariffType);
		}
	}

	public void TestLineSummaryControls()
	{
		var declaration = GetJobDeclarationForTest();
		declaration.Invoices.AddNew();
		using (var form = new ZForm(declaration))
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			control.JobDeclaration = declaration;
			form.Controls.Add(control);
			form.Show();

			CombineAssertions(() =>
			{
				AssertCalcField(control, "CalcStatisticalValue", nameof(JobComInvoiceLine.JI_Calc_StatisticalValue));

				AssertCalcFieldInvisible(control, "JI_Calc_FreightConvertToLocalCurrencyControl");
				AssertCalcFieldInvisible(control, "JI_Calc_InsuranceConvertToLocalCurrencyControl");
				AssertCalcFieldInvisible(control, "JI_Calc_DutyConvertToLocalCurrencyControl");
				AssertCalcFieldInvisible(control, "JI_Calc_GSTConvertToLocalCurrencyControl");
				AssertCalcFieldInvisible(control, "JI_Calc_CIFConvertToLocalCurrencyControl");
				AssertCalcFieldInvisible(control, "JI_Calc_FOBConvertToLocalCurrencyControl");
				AssertCalcField(control, "CalcStatisticalValue", nameof(JobComInvoiceLine.JI_Calc_StatisticalValue));
			});
		}
	}
}

class ExportInvoiceLineUserControlForTest : ExportInvoiceLineUserControl
{
	public new ZBool DynamicLayoutApplied => base.DynamicLayoutApplied;
	public new IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => base.GetNewInvoiceLineDetailsPanelLayout();
	public new bool HasDifferentPanelLayout => base.HasDifferentPanelLayout;
	public new string UniversalTariffType => base.GetUniversalTariffType();
}
