using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.TaxFramework
{
	public partial class WithholdingJournalParentPivotForm : ZForm
	{
		public WithholdingJournalParentPivotForm(
			IReadOnlyCollection<WithholdingJournalsPerInvoice> journalsForMultipleInvoices,
			IWithholdingJournalRealizerForMultipleInvoices withholdingJournalRealizerForMultipleInvoices)
			: base(CombineWithholdingJournals(journalsForMultipleInvoices))
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);

			DisplayMode = ODisplayMode.Edit;
			if(journalsForMultipleInvoices == null )
			{
				Dispose();
				throw new ArgumentNullException(nameof(journalsForMultipleInvoices));
			}
			JournalCollectionPerInvoice = journalsForMultipleInvoices;

			if (withholdingJournalRealizerForMultipleInvoices == null)
			{
				Dispose();
				throw new ArgumentNullException(nameof(withholdingJournalRealizerForMultipleInvoices));
			}
			WithholdingJournalRealizerForMultipleInvoices = withholdingJournalRealizerForMultipleInvoices;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var errorMessage = WithholdingJournalRealizerForMultipleInvoices.Realize(JournalCollectionPerInvoice);

			if (!string.IsNullOrEmpty(errorMessage))
			{
				Globals.Message.Show(errorMessage);
				return ContinueWithSave.No;
			}

			return ContinueWithSave.Yes;
		}

		static WithholdingJournalForDisplayCollection CombineWithholdingJournals(IReadOnlyCollection<WithholdingJournalsPerInvoice> journalCollectionPerInvoice)
		{
			if (journalCollectionPerInvoice != null && journalCollectionPerInvoice.Count != 0)
			{
				var factroy = journalCollectionPerInvoice.First().ParentInvoice.Factory;
				var realizeJournalCollectionPerInvoice = journalCollectionPerInvoice.SelectMany(journalsPerInvoice => journalsPerInvoice.Journals);

				var combinedCollection = new WithholdingJournalForDisplayCollection(factroy);
				combinedCollection.AddRange(realizeJournalCollectionPerInvoice);

				return combinedCollection;
			}

			return new WithholdingJournalForDisplayCollection(new BusinessObjectFactory());
		}

		public override string FormVerb => string.Empty;

		protected override bool AllowNew => false;

		public IWithholdingJournalRealizerForMultipleInvoices WithholdingJournalRealizerForMultipleInvoices { get; }

		public IReadOnlyCollection<WithholdingJournalsPerInvoice> JournalCollectionPerInvoice { get; }
	}
}
