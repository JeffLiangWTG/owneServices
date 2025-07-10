using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.GUI.Testing;

abstract class BaseInvoiceLineUserControlTest<T> : TestCaseWithFactory where T : BaseInvoiceLineUserControl
{
	public void TestGetInvoiceLineChargesUserControl()
	{
		using (var form = new ZForm(Declaration))
		using (var control = GetInvoiceLineUserControlForTest())
		{
			control.JobDeclaration = Declaration;
			form.Controls.Add(control);
			form.Show();

			AssertType<InvoiceLineChargesUserControl>(control.InvoiceLineCharges);
		}
	}

	public void TestReorderTabPages()
	{
		using (var form = new ZForm(Declaration))
		using (var control = GetInvoiceLineUserControlForTest())
		{
			control.JobDeclaration = declaration;
			form.Controls.Add(control);
			form.Show();

			CombineAssertions(() =>
			{
				var expectedTabPagesInOrder = GetExpectedTabPagesInOrder(control).ToArray();
				foreach (var expectedTabPage in expectedTabPagesInOrder)
				{
					Assert($"{expectedTabPage.Name} is visible", expectedTabPage.TabVisible);
				}
				AssertContainsExactElementsInExactOrder(tabNames(expectedTabPagesInOrder), tabNames(control.LineDetailTabControl.TabPages));

				IEnumerable<string> tabNames(IEnumerable tabPages)
				{
					foreach (ZTabPage tabPage in tabPages)
					{
						yield return tabPage.Name;
					}
				}
			});
		}
	}

	public void TestInvoiceLineGrid_DefaultColumns()
	{
		var defaultColumns = GetDefaultColumnsForGrid();
		if (defaultColumns.Any())
		{
			using (var form = new ZForm(Declaration))
			using (var control = GetInvoiceLineUserControlForTest())
			{
				form.SetDataBinding(Declaration, ZString.Empty);
				control.JobDeclaration = Declaration;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();

				var grid = control.CustomsInvoiceLinesBoundGrid;
				var visibleColumnCount = grid.Columns.Count(x => x.IsVisible);

				CombineAssertions(() =>
				{
					AssertEquals("Column count", defaultColumns.Count, visibleColumnCount);
					for (var i = 0; i < defaultColumns.Count; i++)
					{
						var defaultColumn = defaultColumns[i];
						var columnStyle = grid.GetColumnStyle(defaultColumn.Key);

						AssertType($"{defaultColumn.Key} type", defaultColumn.Value, columnStyle);
						AssertEquals($"{defaultColumn.Key} index", i, grid.ColumnStyles.IndexOf(columnStyle));
						AssertEquals($"{defaultColumn.Key} visibility", true, columnStyle?.IsVisible);
					}
				});
			}
		}
		else
		{
			Assert(true);
		}
	}

	public void TestInvoiceLineGrid_ColumnJI_CEI()
	{
		using (var form = new ZForm(Declaration))
		using (var control = GetInvoiceLineUserControlForTest())
		{
			form.SetDataBinding(Declaration, ZString.Empty);
			control.JobDeclaration = Declaration;
			control.InitializeGridLayout();
			form.Controls.Add(control);
			form.Show();

			AssertEquals("JI_CEI should be shown on Declaration form", false, control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CEI).IsUnavailable);
		}

		Declaration.MakeNonPersistent();

		using (var form = new ZForm(Declaration))
		using (var control = GetInvoiceLineUserControlForTest())
		{
			form.SetDataBinding(Declaration, ZString.Empty);
			control.JobDeclaration = Declaration;
			control.InitializeGridLayout();
			form.Controls.Add(control);
			form.Show();

			AssertEquals("JI_CEI should be hidden on Standalone Invoice form", true, control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLineSchema.Constants.JI_CEI).IsUnavailable);
		}
	}

	public void TestTariffColumnName()
	{
		using (var control = GetInvoiceLineUserControlForTest())
		{
			AssertEquals(control.TariffColumnName, Customs.Business.BaseJobComInvoiceLine.Schema.JI_FormattedTariff);
		}
	}

	public void AssertCalcFieldInvisible(Control control, string name)
	{
		var currencyControl = control.FindSingle<ConvertToLocalCurrencyControl>(name);
		AssertEquals($"{name} visibility", false, currencyControl.Visible);
	}

	public void AssertCalcField(Control control, string name, string bindToAmountProperty)
	{
		var currencyControl = control.FindSingle<ConvertToLocalCurrencyControl>(name);
		AssertEquals($"{name} visibility", true, currencyControl.Visible);
		AssertEquals($"{name} amount binding", $"{nameof(JobDeclaration.FilteredInvoiceLines)}.{bindToAmountProperty}", currencyControl.BindToAmount);
		AssertEquals($"{name} unit binding", $"{nameof(JobDeclaration.FilteredInvoiceLines)}.{nameof(JobComInvoiceLine.LocalCurrency)}.{nameof(JobComInvoiceLine.LocalCurrency.RX_Code)}", currencyControl.BindToUnit);
		AssertEquals($"{name} list binding", $"{nameof(JobDeclaration.FilteredInvoiceLines)}.{nameof(JobComInvoiceLine.Lookups)}+{nameof(JobComInvoiceLineLookups.CurrencyList)}", currencyControl.BindToList);
	}

	protected abstract T GetInvoiceLineUserControlForTest();

	protected abstract IEnumerable<ZTabPage> GetExpectedTabPagesInOrder(T control);

	protected abstract List<KeyValuePair<string, Type>> GetDefaultColumnsForGrid();

	protected JobDeclaration Declaration => declaration ?? (declaration = GetJobDeclarationForTest());
	JobDeclaration declaration;

	protected virtual JobDeclaration GetJobDeclarationForTest() => Factory.NewWithValidTestData<JobDeclaration>();
}
