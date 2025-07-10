using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	public sealed class DummyOperationalActionMethodApplicator : AutoDummyOperationalActionMethodApplicator, IObsoleteValidation
	{
		public DummyOperationalActionMethodApplicator(string name, BusinessObjectFactory factory, DummyOperationalActionMethodSettings settings) : base(name, factory)
		{
			if (settings != null)
			{
				lockExcuse = settings.LockExcuse;
				Excuse = settings.DefaultExcuse;
			}
		}

		protected override bool Excuse_ReadOnly
		{
			get
			{
				return lockExcuse;
			}
		}

		protected override void InitialiseBeforeIndividiualBatchRunCore()
		{
			base.InitialiseBeforeIndividiualBatchRunCore();
			count = 0;
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			log.SetSectionProgressMax(targets.Length);
			foreach (BusinessObject target in targets)
			{
				ICodeDescription pair = target;
				log.NotifyFormat(OperationalActionLogErrorLevel.Debug, "Updating {0}", pair.Description);
				target.GetLogs().AddNew(Events.StaffVerbalWarningIssued, Excuse);
				log.BumpSectionProgress();
				count++;
			}
		}

		protected override bool SupportsSummaryCore
		{
			get
			{
				return true;
			}
		}

		protected override void SummaryLogCore(IOperationalActionSectionLog log)
		{
			base.SummaryLogCore(log);
			log.NotifyFormat(OperationalActionLogErrorLevel.Informational, "{0} event(s) added.", count);
		}

		protected override void InitialiseBeforeAllBatchesRunCore()
		{
			InitialiseBeforeAllBatchesRunCalled++;
			base.InitialiseBeforeAllBatchesRunCore();
		}

		int count;

		public int InitialiseBeforeAllBatchesRunCalled;
		readonly bool lockExcuse;
	}
}
