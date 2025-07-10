using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

class ExportSupplierHeaderUserControlTest : BaseCustomsSupplierHeaderUserControlTest<ExportSupplierHeaderUserControl>
{
	protected override JobDeclaration GetJobDeclarationForTest()
	{
		var declaration = base.GetJobDeclarationForTest();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		return declaration;
	}

	protected override ExportSupplierHeaderUserControl GetSupplierHeaderUserControlForTest() => new ExportSupplierHeaderUserControl();

	protected override IEnumerable<ZTabPage> GetExpectedTabPagesInOrder(ExportSupplierHeaderUserControl control)
	{
		yield return control.InvoiceTabControl.FindSingle<ZTabPage>("ComInvoiceDetailsTabPage");
		yield return control.PreviousDocumentsTabPage;
		yield return control.SupportingDocumentsTabPage;
		if (Declaration.JE_MessageType != CHJobMessageTypeList.Codes.ExportDeclarationActivation)
		{
			yield return control.TransportDocumentsTabPage;
		}

		yield return control.InvoiceTabControl.FindSingle<ZTabPage>("CustomFieldsTabPage");
	}

	public void TestTabPagesVisibilityAndOrderEDA()
	{
		Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		TestTabPagesVisibilityAndOrder();
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
		using (var control = new ExportSupplierHeaderUserControl())
		{
			AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Export), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestReferenceNumberUCR()
	{
		using (var control = new ExportSupplierHeaderUserControlForTesting())
		{
			Assert(control.InvDetailLeftPanelExposed.Contains(control.ReferenceNumberUCRTextBox));
		}
	}

	class ExportSupplierHeaderUserControlForTesting : ExportSupplierHeaderUserControl
	{
		internal Control InvDetailLeftPanelExposed => InvDetailLeftPanel;
	}
}
