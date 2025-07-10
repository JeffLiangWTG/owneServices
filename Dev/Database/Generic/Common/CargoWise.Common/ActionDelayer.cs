using System;
using System.Collections.Generic;

namespace CargoWise.Common
{
	public class ActionDelayer : IDisposable
	{
		int depth;
		IActionStrategy strategy = new InvokeStrategy();
		IDisposable disposeStrategy = DisposableAction.NoAction;

		readonly Queue<Action> actions = new Queue<Action>();

		public void SetDelayStrategy()
		{
			if (depth == 0)
			{
				strategy = new DelayStrategy(this);
			}
			depth++;
			disposeStrategy = new DelayReleaser(this);
		}

		public void MarkDelayActionsAsUnsafe()
		{
			disposeStrategy = new DelayReleaserWithoutRunning(this);
		}

		public IDisposable TemporaryChangeToDelayStrategy()
		{
			if (depth == 0)
			{
				strategy = new DelayStrategy(this);
			}
			depth++;
			disposeStrategy = new DelayReleaser(this);
			return this;
		}

		DisposableAction TemporaryChangeToInvokeStrategy()
		{
			var orginalStrategy = strategy;
			if (orginalStrategy is DelayStrategy)
			{
				strategy = new InvokeStrategy();
				return new DisposableAction(() => strategy = orginalStrategy);
			}

			return DisposableAction.NoAction;
		}

		public void Do(Action action)
		{
			strategy.Invoke(action);
		}

		public bool IsDelaying => depth > 0;

		public bool RunAllDelayed()
		{
			using (TemporaryChangeToInvokeStrategy())
			{
				if (actions.Count > 0)
				{
					do
					{
						actions.Dequeue()();
					}
					while (actions.Count > 0);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public void Dispose()
		{
			disposeStrategy.Dispose();
		}

		#region Invoke Strategies

		interface IActionStrategy
		{
			void Invoke(Action action);
		}

		class InvokeStrategy : IActionStrategy
		{
			public void Invoke(Action action) => action();
		}

		class DelayStrategy : IActionStrategy
		{
			internal DelayStrategy(ActionDelayer parent)
			{
				this.parent = parent;
			}

			readonly ActionDelayer parent;

			public void Invoke(Action action) => parent.actions.Enqueue(action);
		}

		#endregion

		#region Disposable

		class DelayReleaser : IDisposable
		{
			internal DelayReleaser(ActionDelayer parent)
			{
				this.parent = parent;
			}

			readonly ActionDelayer parent;

			public void Dispose()
			{
				parent.depth--;
				if (parent.depth == 0)
				{
					parent.strategy = new InvokeStrategy();
					parent.RunAllDelayed();
				}
			}
		}

		class DelayReleaserWithoutRunning : IDisposable
		{
			internal DelayReleaserWithoutRunning(ActionDelayer parent)
			{
				this.parent = parent;
			}

			readonly ActionDelayer parent;

			public void Dispose()
			{
				parent.depth--;
				if (parent.depth == 0)
				{
					parent.strategy = new InvokeStrategy();
					parent.actions.Clear();
				}
			}
		}

		#endregion
	}
}
