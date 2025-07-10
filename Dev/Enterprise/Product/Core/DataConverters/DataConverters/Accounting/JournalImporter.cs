using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataConverters.Accounting
{
	public class JournalImporter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public JournalImporter(string ledger, ZDateTime postdate, ZGuid clearingAccountPK) : base(new BusinessObjectFactory())
		{
			this.Ledger = ledger;
			this.PostDate = postdate;
			this.ClearingAccountPK = clearingAccountPK;

			Journal[] result = Array.Empty<Journal>();
			JournalList = new ArrayList(result);
		}

		public Journal AddJournalLine(string journalLine)
		{
			Journal journal = null;

			if (ClearingAccountPK.IsValid && !String.IsNullOrWhiteSpace(journalLine))
			{
				var line = new OCsvLine(journalLine);
				CsvJournalConverter journalConverter = new CsvJournalConverter(line, Factory, Ledger, PostDate);
				if (journalConverter.IsValid)
				{
					journal = journalConverter.CreateJournal();
					((IJournalForImport)journal).SetInImportingContext();
					journal.AH_AG = ClearingAccountPK;
					JournalList.Add(journal);
				}

				fErrors = journalConverter.Errors;
				blankLine = false;
			}
			else
			{
				blankLine = true;
			}

			return journal;
		}

		public bool BlankLine
		{
			get { return blankLine;  }
		}

		public bool IsValid
		{
			get { return Errors.IsEmpty; }
		}

		public ZString Errors
		{
			get { return fErrors; }
		}

		#region implementation

		readonly ArrayList JournalList;
		readonly string Ledger;
		readonly ZDateTime PostDate;
		readonly ZGuid ClearingAccountPK;
		ZString fErrors;
		bool blankLine;

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			foreach (Journal journal in JournalList)
			{
				journal.AH_TransactionNum = (Ledger == LedgerTypes.AccountsReceivable) ?
					Env.NumberFountains.ARJournalNo.GetTodaysPeriodFountain().GetNextFormatted(Factory) :
					Env.NumberFountains.APJournalNo.GetTodaysPeriodFountain().GetNextFormatted(Factory);
			}
		}
	}
}
