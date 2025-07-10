using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARJournalController))]
	class ARJournalControllerTest : AccountingTransactionControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARJournal;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return Journal; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesJournal; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewReceivablesJournal; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.ReceivablesTransactions; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			Journal = Factory.New<ARJournal>();
			Factory.Save();
		}

		protected ARJournal Journal;
	}
}
