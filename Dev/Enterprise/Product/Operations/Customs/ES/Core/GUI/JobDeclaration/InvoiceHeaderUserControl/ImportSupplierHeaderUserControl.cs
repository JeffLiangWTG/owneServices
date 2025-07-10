using System;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.GUI;

public partial class ImportSupplierHeaderUserControl : EU.GUI.EUNonLayoutImportSupplierHeaderUserControl
{
	public ImportSupplierHeaderUserControl()
	{
		InitializeComponent();
		AdditionalDocumentsTabPage.RunWhenBindingOrFirstShown((s, args) => InitAdditionalDocumentsUserControl());
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		SetupHeaderColumns();
	}

	void SetupHeaderColumns()
	{
		var supplierColumnStyle = JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<Core.Forms.ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == "SupplierOrgPK");
		if (supplierColumnStyle != null)
		{
			JobComInvoiceHeadersBoundGrid.ColumnStyles.Remove(supplierColumnStyle);
		}
	}

	void InitAdditionalDocumentsUserControl()
	{
		additionalDocumentsUserControl.UserControlType = GetAdditionalDocumentsUserControlType();
		additionalDocumentsUserControl.HostedControlCreated += (sender, args) =>
		{
			if (additionalDocumentsUserControl.HostedControl is EU.GUI.PlugIn.ISupportingInfoUserControls)
			{
				EU.GUI.PlugIn.SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(additionalDocumentsUserControl.HostedControl, (ZArchitecture.Core.NoResString)"Invoices", SupportingInfoColumnLayoutContext);
			}
		};
	}

	void SetupTabsOrder()
	{
		if (!IsUCC6AndIsImport)
		{
			var index = InvoiceTabControl.TabPages.IndexOf(SupportingDocumentsTabPage);
			InvoiceTabControl.TabPages.Insert(AdditionalDocumentsTabPage, index + 1);
		}
	}

	Type GetAdditionalDocumentsUserControlType() => typeof(AdditionalInfosUserControlWithGrid);

	protected override Type GetAdditionalInfosUserControlType() => IsUCC6AndIsImport
			? typeof(AdditionalInfosUserControlWithGrid)
			: base.GetAdditionalInfosUserControlType();

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		SetupTabsOrder();
	}

	protected override Type GetSupportingDocumentsUserControlType() => typeof(ImportSupportingDocumentsUserControl);

	protected override Type GetPreviousDocumentsUserControlType() => typeof(PreviousDocumentsUserControl);

	bool IsUCC6AndIsImport => JobDeclaration is JobDeclaration jobDeclaration && jobDeclaration.IsUCC6AndIsImport;
}
