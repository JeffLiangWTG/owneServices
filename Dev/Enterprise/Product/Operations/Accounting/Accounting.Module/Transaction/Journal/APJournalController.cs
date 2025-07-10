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
	public class APJournalController : AccountingTransactionController
	{
		public APJournalController()
		{
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReversePayablesJournal; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewPayablesJournal; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.PayablesTransactions; }
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.APJournal; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APJournal); }
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new JournalBaseForm((APJournal)businessEntity);
		}
	}
}
