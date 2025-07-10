using System;
using System.Threading;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Common
{
	[ThreadSafe]
	public sealed class LazyOverridable<T> : OverridableBase<T>
	{
		public LazyOverridable(Func<T> defaultValueFactory)
		{
			Argument.NotNull(defaultValueFactory, nameof(defaultValueFactory));
			lazyDefaultValue = new Lazy<T>(defaultValueFactory);
		}

		public LazyOverridable(Func<T> defaultValueFactory, LazyThreadSafetyMode mode)
		{
			Argument.NotNull(defaultValueFactory, nameof(defaultValueFactory));
			lazyDefaultValue = new Lazy<T>(defaultValueFactory, mode);
		}

		public T Value
		{
			get => IsOverriden ? ReadCurrentValue() : lazyDefaultValue.Value;
			set
			{
				RegisterOverridden(this);
				WriteCurrentValue(value);
				IsOverriden = true;
			}
		}

		public override void ResetValue()
		{
			IsOverriden = false;
			WriteCurrentValue(default);
		}

		public bool IsOverriden { get; private set; }

		readonly Lazy<T> lazyDefaultValue;
	}
}
