using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.GUI.ARAP.Journal;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class ARBalancingJournalController : MiscellaneousTransactionController
	{
		public override ModuleIdentifier ModuleID => null;

		public override Type TypeOfTopLevelBusinessObject => typeof(ARJournal);

		protected override ControllerID IDCore => ControllerIDs.ARBalancingJournal;

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return new BalancingJournalForm((Journal)businessEntity);
		}
	}
}
