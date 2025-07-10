using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

class ImportSupplierHeaderUserControlTest : BaseCustomsSupplierHeaderUserControlTest<ImportSupplierHeaderUserControl>
{
	protected override JobDeclaration GetJobDeclarationForTest()
	{
		var declaration = base.GetJobDeclarationForTest();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		return declaration;
	}

	protected override ImportSupplierHeaderUserControl GetSupplierHeaderUserControlForTest() => new ImportSupplierHeaderUserControl();

	protected override IEnumerable<ZTabPage> GetExpectedTabPagesInOrder(ImportSupplierHeaderUserControl control)
	{
		yield return control.InvoiceTabControl.FindSingle<ZTabPage>("ComInvoiceDetailsTabPage");
		yield return control.PreviousDocumentsTabPage;
		yield return control.SupportingDocumentsTabPage;
		yield return control.SpecialMentionsTabPage;
		yield return control.InvoiceTabControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
	}

	protected override string[] ExpectedInvoiceChargesGridColumns => new[]
	{
			InvoiceCharge.Schema.J7_ChargeType,
			InvoiceCharge.Schema.J7_Amount,
			InvoiceCharge.Schema.J7_RX_NKCurrency,
			InvoiceCharge.Schema.J7_IsDutiable,
			InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
			InvoiceCharge.Schema.J7_IsGSTApplicable,
			InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
			InvoiceCharge.Schema.J7_IsIncludedInITOT,
			InvoiceCharge.Schema.J7_DistributeBy,
			InvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable,
			InvoiceCharge.Schema.J7_ExchangeRate,
			InvoiceCharge.Schema.J7_ExchangeRateDate,
		};

	protected override string[] ExpectedBaseGroupChargesGridColumns => new[]
	{
			InvoiceCharge.Schema.J7_ChargeType,
			InvoiceCharge.Schema.J7_Amount,
			InvoiceCharge.Schema.J7_RX_NKCurrency,
			InvoiceCharge.Schema.J7_IsDutiable,
			InvoiceCharge.Schema.J7_IsStatisticalValueApplicable,
			InvoiceCharge.Schema.J7_IsGSTApplicable,
			InvoiceCharge.Schema.J7_Percentage,
			InvoiceCharge.Schema.J7_DistributeBy,
			InvoiceCharge.Schema.J7_FullOrPartialApportionment,
			BaseGroupInvoiceCharge.Schema.J7_Calc_IsIncludedInITOT,
			InvoiceCharge.Schema.IsJ7_ExchangeRateUserEnterable,
			InvoiceCharge.Schema.J7_ExchangeRate,
			InvoiceCharge.Schema.J7_ExchangeRateDate,
		};

	public void TestColumnLayoutContext()
	{
		using (var control = new ImportSupplierHeaderUserControl())
		{
			AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestGridId()
	{
		using (var control = new ImportSupplierHeaderUserControl())
		{
			AssertEquals("GridLayoutT+oYGAheR1633dT3oeP+lw==", control.InvoiceHeadersBoundGrid.GridId);
		}
	}
}
