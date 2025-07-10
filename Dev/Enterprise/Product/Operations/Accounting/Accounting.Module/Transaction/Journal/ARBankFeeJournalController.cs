using System;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARBankFeeJournalController : BankFeeJournalController
	{
		protected override ControllerID IDCore
		{
			get { return ControllerIDs.ARBankFeeJournal; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ARJournal); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
