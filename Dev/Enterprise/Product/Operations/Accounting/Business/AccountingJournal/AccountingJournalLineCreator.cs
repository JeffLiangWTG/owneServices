using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public abstract class AccountingJournalLineCreator
	{
		protected AccountingJournalLineCreator(ReadOnlyBusinessObjectFactory factory)
		{
			this.Factory = factory;
		}
		protected BusinessObjectFactory Factory;

		public IEnumerable<AccountingJournalLine> CreateAccountJournalLines()
		{
			var result = new List<AccountingJournalLine>();
			if (CanAccountingJournalLineBeCreated())
			{
				result.AddRange(CreateAccountJournalLinesCore());
			}
			return result;
		}

		protected abstract IEnumerable<AccountingJournalLine> CreateAccountJournalLinesCore();

		protected abstract void PopulateLineProperties(AccTransactionLines line, BusinessObject sourceObject);

		protected abstract bool CanAccountingJournalLineBeCreated();
	}
}
