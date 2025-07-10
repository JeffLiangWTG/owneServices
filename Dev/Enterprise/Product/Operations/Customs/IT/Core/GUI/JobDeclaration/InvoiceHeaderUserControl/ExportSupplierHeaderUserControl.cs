using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class ExportSupplierHeaderUserControl : EU.GUI.EUExportSupplierHeaderUserControl
{
	public ExportSupplierHeaderUserControl()
	{
		InitializeComponent();
	}

	protected override Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLayoutSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(PreviousDocumentsUserControl);

	protected override Type GetAdditionalInfosUserControlType() => typeof(InvoiceHeaderAdditionalInfoUserControlWithGrid);

	protected override void HookInvoiceHeaderEvents(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader)
	{
		base.HookInvoiceHeaderEvents(invoiceHeader);
		HookInvoiceTabPageEnterEventAndSetOrganisationsTabPageVisibility();
	}

	protected override void UnHookInvoiceHeaderEvents(EU.Business.Declaration.JobComInvoiceHeader invoiceHeader)
	{
		base.UnHookInvoiceHeaderEvents(invoiceHeader);
		UnhookInvoiceTabPageEnterEvent();
	}

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		SetOrganisationsTabPageVisibility();
		OrderTabs();
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		new InvoiceHeaderGridSupplierColumnManager(this).SetUpSupplierColumns();
	}

	#region Implementation

	ZTabPage InvoicesTabPage => (ParentForm as JobDeclarationForm)?.CustomsBrokerageUserControl?.InvoicesTabPage;

	void InvoicesTabPage_Enter(object sender, EventArgs e)
	{
		SetOrganisationsTabPageVisibility();
	}

	void OrderTabs()
	{
		var i = 0;
		var tabPagesInExpectedOrder = GetInvoiceTabPagesInExpectedOrder().ToImmutableDictionary(key => (ZInt)i++, value => value);
		InvoiceTabControl.OrderTabPages(tabPagesInExpectedOrder);
	}

	protected virtual List<ZTabPage> GetInvoiceTabPagesInExpectedOrder() => new List<ZTabPage> { ComInvoiceDetailsTabPage, OrganisationsTabPage };

	void SetOrganisationsTabPageVisibility()
	{
		if (JobDeclaration is JobDeclaration itJobDeclaration && itJobDeclaration.IsUCC6 && !itJobDeclaration.JE_JS.IsEmpty)
		{
			OrganisationsTabPage.TabVisible = false;
			return;
		}

		if (CurrentInvoiceHeader is JobComInvoiceHeader invoiceHeader)
		{
			OrganisationsTabPage.TabVisible = invoiceHeader.IsBuyersConsol;
		}
	}

	void UnhookInvoiceTabPageEnterEvent()
	{
		var invoiceTabPage = InvoicesTabPage;
		if (invoiceTabPage != null)
		{
			invoiceTabPage.Enter -= InvoicesTabPage_Enter;
		}
	}

	void HookInvoiceTabPageEnterEventAndSetOrganisationsTabPageVisibility()
	{
		var invoiceTabPage = InvoicesTabPage;
		if (invoiceTabPage != null)
		{
			invoiceTabPage.Enter += InvoicesTabPage_Enter;
			SetOrganisationsTabPageVisibility();
		}
	}

	#endregion
}
