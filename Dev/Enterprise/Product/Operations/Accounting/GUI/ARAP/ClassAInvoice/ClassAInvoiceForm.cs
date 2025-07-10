using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	/// <summary>
	/// Summary description for ClassAInvoiceForm.
	/// </summary>
	public partial class ClassAInvoiceForm : ZForm
	{
		ZDropEdit ComplianceSubTypeDropEdit;
		ZArchitecture.ZTextBox TransactionNumberTextBox;
		ZArchitecture.ZTextBox TransactionTypeTextBox;
		ZDateEdit InvoiceDateDateEdit;
		ZDateEdit PostDateDateEdit;
		ZDateEdit ComplianceDocDateEdit;
		ZArchitecture.ZTextBox ClassAInvoiceNumTextBox;
		ZArchitecture.ZTextBox OrganizationTextBox;
		Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		readonly System.ComponentModel.Container components;

		public ClassAInvoiceForm(GovernmentInvoice invoice) : base(invoice)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}

		protected GovernmentInvoice Invoice
		{
			get { return fInvoice ?? (fInvoice = (GovernmentInvoice)DataSource); }
		}
		GovernmentInvoice fInvoice;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			if (!this.IsDesignMode())
			{
				SetVisible(ComplianceSubTypeDropEdit, GlbCompany.CurrentCompany.Country.SupportComplianceSubType);

				var complianceSubTypeAndNumberUpdateRules = ObjectFactory.Get<ICountryComplianceFactory>().GetIComplianceSubTypeAndNumberUpdateRules(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				SetVisible(ComplianceDocDateEdit, complianceSubTypeAndNumberUpdateRules?.IsComplianceDocDateAllowed(Invoice) ?? false);
				SetVisible(ClassAInvoiceNumTextBox, complianceSubTypeAndNumberUpdateRules?.IsComplianceNumberAllowed(Invoice) ?? true);
			}
		}

		void SetVisible(Control control, bool isVisible)
		{
			control.Visible = isVisible;
			control.GetExtension<LabelCaptionRenderer>().Visible = isVisible;
		}

		protected override void HandleSaveException(Exception ex)
		{
			if (BusinessEntity.Factory.HasContext(BusinessContext.CriticalValidation))
			{
				this.DisplayMode = ODisplayMode.ReadOnly;
				SetReadOnlyIncludingChildren();
			}

			base.HandleSaveException(ex);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}

