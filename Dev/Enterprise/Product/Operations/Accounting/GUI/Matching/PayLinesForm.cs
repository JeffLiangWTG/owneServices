using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Matching
{
	public partial class PayLinesForm : ZChildForm, IButtonPostTextOverride
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public PayLinesForm()
		{
		}

		public PayLinesForm(InvoicingBasePayLineMediator businessObject)
			: base(businessObject)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, okButton, cancelButton);
			if (businessObject.IsNew)
			{
				DisplayMode = ODisplayMode.Edit;
			}
			else
			{
				((IBusinessObjectState)businessObject).ClearHasChangesIncludingChildren();
			}
		}

		InvoicingBasePayLineMediator Mediator
		{
			get { return BusinessEntity as InvoicingBasePayLineMediator; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			SetupColumns();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		public override string FormCaption
		{
			get { return base.FormCaption + string.Format(" - {0}:{1}:{2}", Mediator.MasterInvoice.AH_Ledger, Mediator.MasterInvoice.AH_TransactionType, Mediator.MasterInvoice.AH_TransactionNum); }
		}

		bool dataConveyed;

		protected override void OnFormClosed(FormClosedEventArgs e)
		{
			if (!dataConveyed)
			{
				Mediator.CancelEdits();
			}
			base.OnFormClosed(e);
		}

		protected override void SaveInternal()
		{
			Mediator.ConveyData();
			dataConveyed = true;
		}

		void SetupColumns()
		{
			for (int i = InvoiceLinesGrid.ColumnStyles.Count - 1; i >= 0; i--)
			{
				ZGridColumnInfo columnStyle = (ZGridColumnInfo)InvoiceLinesGrid.ColumnStyles[i];
				switch (columnStyle.ColumnName)
				{
					case BaseCharge.Schema.ChargeType:
						if (Mediator.MasterInvoice.AH_Ledger != LedgerTypes.AccountsReceivable)
						{
							InvoiceLinesGrid.ColumnStyles.Remove(columnStyle);
						}
						break;
					case "ConsolidatedInvoiceRef":
						columnStyle.Caption = Mediator.MasterInvoice.AH_Ledger == LedgerTypes.AccountsReceivable ? Res.GetString("PayLinesForm|e4a54108-c7f0-4c8e-8deb-7f30f4f9c770", "Pay Lines") : Res.GetString("PayLinesForm|5a5ccddf-53ba-4BC2-9ab1-1366bc0e8665", "Pay Lines");
						break;
					case "ChargeExRate":
						((ZCalcEditColumnStyleInfo)columnStyle).Decimals = GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
						break;
				}
			}
		}

		#region Event Handlers

		void PayAllLinesButton_Click(object sender, EventArgs e)
		{
			Mediator.SetAllLinesMatchingFiltersToFullyPaid();
		}

		void FullyPaySelectedLinesButton_Click(object sender, EventArgs e)
		{
			Mediator.SetSelectedLinesToFullyPaid(InvoiceLinesGrid.SelectedElements, true);
		}

		void FullyPayAllLinesButton_Click(object sender, EventArgs e)
		{
			Mediator.SetAllLinesToFullyPaid(true);
		}

		void DeselectFullyPayAllLinesButton_Click(object sender, EventArgs e)
		{
			Mediator.SetAllLinesToFullyPaid(false);
		}

		#endregion

		#region IButtonPostTextOverride Members

		string IButtonPostTextOverride.PostButtonText
		{
			get { return Res.GetString("dc5d9522-370d-4670-a9fc-bfce98e80310", "&OK"); }
		}

		#endregion
	}
}
