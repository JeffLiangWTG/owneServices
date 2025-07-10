using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.Common
{
	/// <summary>
	/// A wrapper for an IEnumerator. All methods are proxied to the inner, including IDisposable.
	/// </summary>
	public abstract class WrappedEnumerator<T> : IEnumerator<T>, IDisposable
	{
		protected WrappedEnumerator(IEnumerator<T> inner)
		{
			Argument.NotNull(inner, nameof(inner));
			this.inner = inner;
		}

		public IEnumerator<T> Inner
		{ get { return inner; } }
		IEnumerator<T> inner;

		#region IEnumerator Members

		public virtual void Reset()
		{ this.Inner.Reset(); }

		public virtual T Current
		{ get { return this.Inner.Current; } }

		object IEnumerator.Current
		{ get { return Current; } }

		public virtual bool MoveNext()
		{ return this.Inner.MoveNext(); }

		#endregion

		#region IDisposable Members

		[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly"), SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
		public virtual void Dispose()
		{
			IDisposable innerDisposable = Inner;
			if (innerDisposable != null)
			{
				innerDisposable.Dispose();
			}
			this.inner = null;
		}

		#endregion
	}
}