using System;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class APBankFeeJournalController : BankFeeJournalController
	{
		protected override ControllerID IDCore
		{
			get { return ControllerIDs.APBankFeeJournal; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(APJournal); }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}
	}
}
