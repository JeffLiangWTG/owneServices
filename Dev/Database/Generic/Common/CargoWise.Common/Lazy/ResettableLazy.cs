using System;

namespace CargoWise.Common
{
	public static class ResettableLazy
	{
		public static ResettableLazy<T> Create<T>(Func<T> creator, bool isThreadSafe = false)
		{
			Argument.NotNull(creator, nameof(creator));

			return new ResettableLazy<T>(creator, isThreadSafe);
		}
	}

	public class ResettableLazy<T>
	{
		internal ResettableLazy(Func<T> creator, bool isThreadSafe)
		{
			Argument.NotNull(creator, nameof(creator));

			this.creator = creator;
			this.isThreadSafe = isThreadSafe;
		}

		readonly Func<T> creator;
		readonly bool isThreadSafe;

		public T Value
		{
			get
			{
				if (lazy == null)
				{
					lazy = Lazy.Create(creator, isThreadSafe);
				}

				return lazy.Value;
			}
		}

		public void Reset()
		{
			lazy = null;
		}

		Lazy<T> lazy;
	}
}