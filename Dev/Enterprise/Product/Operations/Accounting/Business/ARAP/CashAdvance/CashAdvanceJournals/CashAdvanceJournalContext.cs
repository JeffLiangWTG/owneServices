using System;
using CargoWise.Common;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	internal class CashAdvanceJournalContext
	{
		internal CashAdvanceJournalContext(CashAdvanceJournalCreator journalCreator
								, Action<CashAdvanceRequestHeader, Journal.Journal> newJournalCreatedEventHandler
								, Action<CashAdvanceRequestHeader, string> journalCreationFailedEventHandler)
		{
			this.journalCreator = journalCreator;
			this.newJournalCreatedEventHandler = newJournalCreatedEventHandler;
			this.journalCreationFailedEventHandler = journalCreationFailedEventHandler;
		}

		internal Journal.Journal CreateJournal(CashAdvanceRequestHeader requestHeader, Journal.Journal overpayJournal = null)
		{
			using (new DisposableAction(() => HookEvents(), () => UnhookEvents()))
			{
				Journal.Journal result = null;
				if (journalCreator.IsGenerateRequired(requestHeader))
				{
					result = journalCreator.Generate(requestHeader, overpayJournal);
				}
				return result;
			}
		}

		void HookEvents()
		{
			if (newJournalCreatedEventHandler != null)
			{
				journalCreator.NewJournalCreated += newJournalCreatedEventHandler;
			}
			if (journalCreationFailedEventHandler != null)
			{
				journalCreator.JournalCreationFailed += journalCreationFailedEventHandler;
			}
		}

		void UnhookEvents()
		{
			if (newJournalCreatedEventHandler != null)
			{
				journalCreator.NewJournalCreated -= newJournalCreatedEventHandler;
			}
			if (journalCreationFailedEventHandler != null)
			{
				journalCreator.JournalCreationFailed -= journalCreationFailedEventHandler;
			}
		}

		readonly CashAdvanceJournalCreator journalCreator;
		readonly Action<CashAdvanceRequestHeader, Journal.Journal> newJournalCreatedEventHandler;
		readonly Action<CashAdvanceRequestHeader, string> journalCreationFailedEventHandler;
	}
}
