using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.GUI.ARAP.Journal;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARJournalController : AccountingTransactionController
	{
		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseReceivablesJournal; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewReceivablesJournal; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ReceivablesTransactions; }
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new JournalBaseForm((ARJournal)businessEntity);
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARJournal); }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ARJournal; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
