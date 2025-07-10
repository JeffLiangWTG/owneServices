using System;

namespace Enterprise.DocumentEngine
{
	public class TemporaryValueSetter<T> : IDisposable
	{
		readonly public Action<T> Set;
		readonly T originalValue;

		public TemporaryValueSetter(Action<T> setAction, T originalValue)
		{
			this.Set = setAction;
			this.originalValue = originalValue;
		}

		public TemporaryValueSetter(Action<T> setAction, T originalValue, T temporaryValue)
			: this(setAction, originalValue)
		{
			Set(temporaryValue);
		}

		public void Dispose()
		{
			Set(originalValue);
		}
	}
}
