using System;
using System.Collections.Generic;

namespace CargoWise.Common
{
	public class DisposableList : List<IDisposable>, IDisposable
	{
		public DisposableList(int capacity) : base(capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(capacity));
			}
		}

		public DisposableList(IEnumerable<IDisposable> disposables) : base(disposables) { }

		public void Add(params IDisposable[] disposables)
		{
			foreach (var disposable in disposables)
			{
				base.Add(disposable);
			}
		}

		public void Dispose()
		{
			foreach (IDisposable item in this)
			{
				if (item != null)
				{
					item.Dispose();
				}
			}
		}
	}
}
