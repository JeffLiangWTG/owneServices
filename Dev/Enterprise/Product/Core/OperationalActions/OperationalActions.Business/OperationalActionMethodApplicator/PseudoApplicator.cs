using System;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	abstract class PseudoApplicator : OperationalActionMethodApplicator, IObsoleteValidation
	{
		public PseudoApplicator(OperationalActionRunner runner, string name)
			: base(name)
		{
			if (runner == null)
			{
				throw new ArgumentNullException(nameof(runner));
			}

			this.runner = runner;
			this.action = runner.Action;
			this.context = runner.Action.Context;
		}

		public override void Apply(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			Action.Context.Supporter.AroundPseudoApplication(
				targets,
				() => ApplyCore(log, targets)
			);
		}

		public OperationalActionRunner Runner
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return runner; }
		}

		public OperationalAction Action
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return action; }
		}

		public OperationalActionContext Context
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return context; }
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly OperationalActionRunner runner;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly OperationalAction action;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly OperationalActionContext context;
	}
}
