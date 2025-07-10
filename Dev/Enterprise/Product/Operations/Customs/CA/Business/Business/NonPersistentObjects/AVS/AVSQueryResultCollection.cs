using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business
{
	public class AVSQueryResultCollection : NonPersistentBusinessObjectCollection<AVSQueryResult>
	{
		public AVSQueryResultCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine.Factory)
		{
			this.invoiceLine = invoiceLine;
			invoiceLine.Notes.NoteAdded += Notes_NoteAdded;
			invoiceLine.Factory.Saved += Factory_Saved;
		}
		readonly JobComInvoiceLine invoiceLine;
		bool shouldReloadCollection;

		public override void Load()
		{
			RemoveAndDeleteAll();

			foreach (var note in invoiceLine.Notes.FindByDescription(PredefinedNoteTypes.Instance.AIRSValidationResults.Description))
			{
				Add(new AVSQueryResult(note));
			}

			shouldReloadCollection = false;
		}

		void Notes_NoteAdded(NoteAddedEventArgs args)
		{
			shouldReloadCollection = true;
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (shouldReloadCollection)
			{
				Load();
			}
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException();
		}
	}
}
