using System;
using System.Collections.Generic;
using CargoWise.Integration;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment
{
	public partial class IntercompanyCostsApportionmentForm : AccountingZForm
	{
		public IntercompanyCostsApportionmentForm(IntercompanyCostsApportionmentInvoice businessEntity) : base(businessEntity)
		{
			businessEntity.Lines.ShowGLAccountsForImportAction = ShowGLAccountsForImportAction;
		}

		InvoicingBase invoice;
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);

			if (!GlbCompany.CurrentCompany.GC_IsGSTRegistered)
			{
				costsControl.LineSummaryGrid.RemoveFromAvailableColumns("AL_AT", "AL_TaxDate", "AL_A9_VATClass", (NoResString)"Tax", "LocalTax", "GSTInclusiveAmount", "GSTAmount"); // May be an identifier or GUID.
				apportionmentDetails.apportionmentDetailsGrid.RemoveFromAvailableColumns("ForeignGST", "LocalGST"); // May be an identifier or GUID.
			}

			if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				costsControl.LineSummaryGrid.RemoveFromAvailableColumns("AL_GovtChargeCode");
			}
		}

		protected override void SaveCore(ITransactionParticipant[] factories)
		{
			IntercompanyCostsApportionmentInvoice costApportionmentInvoice = (IntercompanyCostsApportionmentInvoice)BusinessEntity;
			try
			{
				invoice = costApportionmentInvoice.CreateBusinessObjectsForPosting();
				if (invoice != null)
				{
					invoice.OnComplianceSequenceFailedToAssign += ComplianceSequenceFailedToAssign;
				}
			}
			catch (EmptyComplianceSubTypeException ex)
			{
				LastSaveSuccessful = false;
				Globals.Message.ShowError(ex.UserFriendlyMessage);
				return;
			}

			List<ITransactionParticipant> aggregationFactories = new List<ITransactionParticipant>();
			aggregationFactories.AddRange(factories);
			foreach (GLJournal journal in costApportionmentInvoice.Journals.Values)
			{
				aggregationFactories.Add(new AggregateWrapper(journal, journal));
			}
			base.SaveCore(aggregationFactories.ToArray());

			apportionmentDetails.Enabled = false;
			costsControl.Enabled = false;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (invoice != null)
				{
					invoice.OnComplianceSequenceFailedToAssign -= new EventHandler(ComplianceSequenceFailedToAssign);
				}
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void ComplianceSequenceFailedToAssign(object sender, EventArgs e)
		{
			var msg = InvoicingBase.GetMessageForComplianceSequenceErrors(e);
			Globals.Message.ShowWarning(msg);
		}

		void ShowGLAccountsForImportAction(AccGLHeaderCollection collection, List<AccGLHeader> gLHeaderList)
		{
			ZFormModaliser.ShowDialogAndDispose(new GLAccountSelectionForm(collection, gLHeaderList));
		}
	}
}
