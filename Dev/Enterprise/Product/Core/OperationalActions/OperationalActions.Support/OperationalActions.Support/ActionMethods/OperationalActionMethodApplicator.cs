using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Support
{
	public abstract class OperationalActionMethodApplicator : NonPersistentBusinessObject
	{
		protected OperationalActionMethodApplicator(string name)
		{
			this.name = name;
		}

		protected OperationalActionMethodApplicator(string name, BusinessObjectFactory factory)
			: base(factory)
		{
			this.name = name;
		}

		public string Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return name; }
		}

		public void InitialiseBeforeIndividiualBatchRun() => InitialiseBeforeIndividiualBatchRunCore();

		protected virtual void InitialiseBeforeIndividiualBatchRunCore()
		{
		}

		public void InitialiseBeforeAllBatchesRun() => InitialiseBeforeAllBatchesRunCore();

		protected virtual void InitialiseBeforeAllBatchesRunCore()
		{
		}

		public void Build(ZGuid[] selectItemPKs) => BuildCore(selectItemPKs);

		protected virtual void BuildCore(ZGuid[] selectItemPKs)
		{
		}

		public virtual void Apply(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			ApplyCore(log, targets);
		}

		public bool SupportsSummary => SupportsSummaryCore;

		public void SummaryLog(IOperationalActionSectionLog log)
		{
			if (log == null)
			{
				throw new ArgumentNullException(nameof(log));
			}

			if (!SupportsSummary)
			{
				throw new InvalidOperationException("This applicator does not support summaries.");
			}

			SummaryLogCore(log);
		}

		protected abstract void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets);

		protected virtual bool SupportsSummaryCore => false;

		protected virtual void SummaryLogCore(IOperationalActionSectionLog log)
		{
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly string name;
	}
}
