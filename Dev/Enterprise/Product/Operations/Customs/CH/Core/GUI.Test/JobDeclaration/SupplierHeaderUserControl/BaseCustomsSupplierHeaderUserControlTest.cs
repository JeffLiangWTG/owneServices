using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

abstract class BaseCustomsSupplierHeaderUserControlTest<T> : TestCaseWithFactory where T : BaseCustomsSupplierHeaderUserControl
{
	public void TestTabPagesVisibilityAndOrder()
	{
		using (var form = new ZForm(Declaration))
		using (var control = GetSupplierHeaderUserControlForTest())
		{
			control.JobDeclaration = Declaration;
			form.Controls.Add(control);
			form.Show();

			CombineAssertions(() =>
			{
				var expectedTabPagesInOrder = GetExpectedTabPagesInOrder(control).ToArray();
				foreach (var expectedTabPage in expectedTabPagesInOrder)
				{
					Assert($"{expectedTabPage.Name} is visible", expectedTabPage.TabVisible);
				}
				AssertContainsExactElementsInExactOrder(expectedTabPagesInOrder, control.InvoiceTabControl.TabPages);
			});
		}
	}

	public void TestCalculateFreightButtonTypeAndVisibility()
	{
		var expectedFormType = typeof(CalculateFreightForm);

		Declaration.Invoices.AddNew();

		using (var form = new ZForm())
		using (var userControl = GetSupplierHeaderUserControlForTest())
		{
			userControl.SetDataBinding(Declaration, "");
			userControl.JobDeclaration = Declaration;
			form.Controls.Add(userControl);
			form.Show();

			CombineAssertions(() =>
			{
				AssertClickingButtonShowsForm(expectedFormType, userControl.InvoiceChargesCalculateFreightButton);
				AssertClickingButtonShowsForm(expectedFormType, userControl.GroupChargesCalculateFreightButton);
				AssertEquals("InvoiceChargesCalculateFreightButton.Visible", true, userControl.InvoiceChargesCalculateFreightButton.Visible);
				AssertEquals("GroupChargesCalculateFreightButton.Visible", true, userControl.GroupChargesCalculateFreightButton.Visible);
			});
		}
	}

	public void TestInvoiceChargesGridColumn()
	{
		using (var form = new ZForm(Declaration))
		using (var userControl = GetSupplierHeaderUserControlForTest())
		{
			form.Controls.Add(userControl);
			form.Show();
			userControl.InitializeGridLayout();

			var index = 0;
			var invoiceChargesGridColumnStyle = userControl.InvoiceChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				foreach (var columnName in ExpectedInvoiceChargesGridColumns)
				{
					UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, columnName, index++);
				}
			});
		}
	}

	public void TestBaseGroupChargesGridColumn()
	{
		using (var form = new ZForm(Declaration))
		using (var userControl = GetSupplierHeaderUserControlForTest())
		{
			form.Controls.Add(userControl);
			form.Show();
			userControl.InitializeGridLayout();

			var index = 0;
			var baseGroupChargesGridColumnStyle = userControl.BaseGroupChargesGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				foreach (var columnName in ExpectedBaseGroupChargesGridColumns)
				{
					UserControlTestHelper.AssertColumnStyles(baseGroupChargesGridColumnStyle, columnName, index++);
				}
			});
		}
	}

	public void TestCalculateFreightEntries()
	{
		var invoice = Declaration.Invoices.AddNew();
		var topGroupInvoice = Declaration.TopGroupInvoice;

		using (var form = new ZForm())
		using (var userControl = GetSupplierHeaderUserControlForTest())
		{
			userControl.SetDataBinding(Declaration, "");
			userControl.JobDeclaration = Declaration;
			form.Controls.Add(userControl);
			form.Show();

			CombineAssertions(() =>
			{
				userControl.InvoiceChargesCalculateFreightButton.PerformClick();
				AssertCalculateFreightEntriesNotAdded((CalculateFreightForm)ZFormModaliser.ActiveForm, invoice.Charges);
				userControl.InvoiceChargesCalculateFreightButton.PerformClick();
				AssertCalculateFreightEntriesAdded((CalculateFreightForm)ZFormModaliser.ActiveForm, invoice.Charges);

				userControl.GroupChargesCalculateFreightButton.PerformClick();
				AssertCalculateFreightEntriesNotAdded((CalculateFreightForm)ZFormModaliser.ActiveForm, topGroupInvoice.Charges);
				userControl.GroupChargesCalculateFreightButton.PerformClick();
				AssertCalculateFreightEntriesAdded((CalculateFreightForm)ZFormModaliser.ActiveForm, topGroupInvoice.Charges);
			});
		}
	}

	public void TestCalcCustomsControlsVisibility() => CombineAssertions(() =>
	{
		using (var form = new ZForm(Declaration))
		using (var userControl = GetSupplierHeaderUserControlForTest())
		{
			form.Controls.Add(userControl);
			form.Show();

			AssertNotVisible("JZ_FOBAmountBoundCurrencyControl");
			AssertNotVisible("JZ_Calc_TNIBoundInvoiceCurrencyControl");
			AssertNotVisible("JZ_CIFAmountBoundCurrencyControl");

			void AssertNotVisible(string name) => AssertEquals($"{name}.Visible", false, userControl.Controls.Find("JZ_FOBAmountBoundCurrencyControl", true).FirstOrDefault().Visible);
		}
	});

	void AssertCalculateFreightEntriesNotAdded(CalculateFreightForm form, IJobComInvChargeCollection<JobComInvCharge> chargeCollection)
	{
		chargeCollection.RemoveAll();

		form.QuitButton.PerformClick();

		var charges = chargeCollection.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasFreight);
		AssertEquals($"Charge entries added for {chargeCollection.HumanReadableName}", 0, charges.Length);
	}

	void AssertCalculateFreightEntriesAdded(CalculateFreightForm form, IJobComInvChargeCollection<JobComInvCharge> chargeCollection)
	{
		chargeCollection.RemoveAll();

		var currency = Core.Constants.CurrencyCodes.Switzerland;
		var bizObj = (CalculateFreightBizObj)form.DataSource;

		bizObj.Currency = currency;
		bizObj.TotalAmount = 100;
		bizObj.PercentageToCHBoarder = 80;
		form.OKButton.PerformClick();

		var charges = chargeCollection.GetCharge(Common.CustomsChargeTypeList.Codes.OverseasFreight);
		AssertEquals($"Missing Charge entries for {chargeCollection.HumanReadableName}", 2, charges?.Length);
		AssertCharge(charges.SingleOrDefault(x => x.J7_Amount == 80), currency, true, true, true);
		AssertCharge(charges.SingleOrDefault(x => x.J7_Amount == 20), currency, true, false, true);
	}

	void AssertCharge(JobComInvCharge charge, string currency, bool isDutiable, bool isStatisticalValueApplicable, bool isGSTApplicable)
	{
		AssertNotNull(charge);
		AssertEquals($"{charge.HumanReadableName}.{charge.Currency.HumanReadableName}", currency, charge.Currency.RX_Code);
		AssertEquals($"{charge.HumanReadableName}.IsDutiable", isDutiable, charge.J7_IsDutiable);
		AssertEquals($"{charge.HumanReadableName}.IsStatisticalValueApplicable", isStatisticalValueApplicable, charge.J7_IsStatisticalValueApplicable);
		AssertEquals($"{charge.HumanReadableName}.IsGSTApplicable", isGSTApplicable, charge.J7_IsGSTApplicable);
	}

	void AssertClickingButtonShowsForm(Type formType, ZButton button)
	{
		button.PerformClick();
		AssertEquals(formType, ZFormModaliser.ActiveForm.GetType());
	}

	protected abstract string[] ExpectedInvoiceChargesGridColumns { get; }

	protected abstract string[] ExpectedBaseGroupChargesGridColumns { get; }

	protected abstract T GetSupplierHeaderUserControlForTest();

	protected abstract IEnumerable<ZTabPage> GetExpectedTabPagesInOrder(T control);

	protected JobDeclaration Declaration => declaration ?? (declaration = GetJobDeclarationForTest());
	JobDeclaration declaration;

	protected virtual JobDeclaration GetJobDeclarationForTest() => Factory.NewWithValidTestData<JobDeclaration>();
}
