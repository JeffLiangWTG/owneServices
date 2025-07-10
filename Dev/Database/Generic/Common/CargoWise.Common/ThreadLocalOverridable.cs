using System.Threading;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Common
{
	[ThreadSafe]
	public class ThreadLocalOverridable<T>
	{
		public ThreadLocalOverridable(bool disposeIfIDisposable = true)
		{
			inner = new Overridable<ThreadLocal<T>>(new ThreadLocal<T>(() => default(T))) { DisposeIfIDisposable = disposeIfIDisposable };
		}

		public T Value
		{
			get => inner.Value.Value;
			set
			{
				if (!inner.IsOverriden)
				{
					lock (inner)
					{
						if (!inner.IsOverriden)
						{
							inner.Value = new ThreadLocal<T>(() => default(T));
						}
					}
				}

				inner.Value.Value = value;
			}
		}

		public void ResetValue()
		{
			inner.ResetValue();
		}

		readonly Overridable<ThreadLocal<T>> inner;
	}
}
