using System;

namespace CargoWise.EntityFramework
{
	public class EntryPoint
	{
		public IEntryState GetState()
		{
			return new StateManager(this);
		}

		class StateManager : IEntryState
		{
			internal StateManager(EntryPoint parent)
			{
				IsAllowed = (parent.index == 0);
				parent.index += 1;
				this.parent = parent;
			}
			public bool IsAllowed { get; private set; }

			readonly EntryPoint parent;

			void IDisposable.Dispose()
			{
				parent.index -= 1;
			}
		}

		int index;
	}
}
