using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APJournalController))]
	class APJournalControllerTest : AccountingTransactionControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APJournal;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return Journal; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.ReversePayablesJournal; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.NewPayablesJournal; }
		}

		protected override SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.PayablesTransactions; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			Journal = Factory.New<APJournal>();
			Factory.Save();
		}

		protected APJournal Journal;
	}
}
