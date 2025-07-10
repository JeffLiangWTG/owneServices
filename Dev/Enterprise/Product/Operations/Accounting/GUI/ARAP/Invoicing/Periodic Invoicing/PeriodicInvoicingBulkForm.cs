using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class PeriodicInvoicingBulkForm : AccountingZForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public PeriodicInvoicingBulkForm()
		{
		}

		public PeriodicInvoicingBulkForm(PeriodicInvoiceBulk periodicInvoice)
			: base(periodicInvoice)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostButton, CloseButton);
		}

		public new PeriodicInvoiceBulk BusinessEntity
		{
			get { return base.BusinessEntity as PeriodicInvoiceBulk; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			this.periodicInvoiceBulkControl.BackColor = BackColor;

			if (!AccountingConfigurationRegistry.Instance.EnableMiscInvoiceInPeriodicInvoice.Value)
			{
				periodicInvoiceBulkControl.TabControl.Controls.Remove(periodicInvoiceBulkControl.MiscInvoicesTabPage);
			}
		}

		protected override void SaveCore(ITransactionParticipant[] factories)
		{
			DialogResult = DialogResult.OK;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			periodicInvoiceBulkControl.PreparePeriodicInvoiceControl();
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = base.ValidateAndSave();

			if (result == ContinueWithSave.Yes)
			{
				SetReadOnlyIncludingChildren();
			}

			return result;
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			return (base.ShowPreSaveDialogs() == ContinueWithSave.No) ? ContinueWithSave.No :
				BusinessEntity.CheckLevelSecurityRightsForPeriodicCreditNotes() ? ContinueWithSave.No : ContinueWithSave.Yes;
		}

		public PeriodicInvoiceBulkPoster GeneratePoster()
		{
			return BusinessEntity.CreatePeriodicInvoiceBulkPoster();
		}

		bool IAllowTabBackwardBetweenSomeOfMyChildren.AllowTabBackward(Control control, Control previousControl)
		{
			return control == DateEdit && previousControl == PostDateEdit;
		}
	}
}

