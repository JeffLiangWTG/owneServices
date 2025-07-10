using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class JobRevenueJournalController : AccountingTransactionController
	{
		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.JobRevenueJournal; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobRevenueJournal); }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ViewJobRevenueJournal; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.NewJobRevenueJournal; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ReverseJobRevenueJournal; }
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			return JobRevenueJournalFormFactory.GetJobRevenueJournalForm((JobRevenueJournal)businessEntity);
		}

		protected override ControllerID IDCore
		{
			get { return ControllerIDs.JobRevenueJournal; }
		}
	}
}
