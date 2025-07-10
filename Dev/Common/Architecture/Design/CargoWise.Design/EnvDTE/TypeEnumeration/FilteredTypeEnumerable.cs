using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.Design.DTE
{
	/// <summary>
	/// A wrapper over a DTypeEnumerator.
	/// </summary>
	public abstract class FilteredTypeEnumerable : IEnumerable, IEnumerable<Type>
	{
		protected FilteredTypeEnumerable()
		{ Inner = new ArrayList(); }

		protected FilteredTypeEnumerable(IEnumerable inner)
		{ Inner = inner; }

		/// <summary>
		/// Get the inner type enumerator.
		/// </summary>
		protected IEnumerable Inner { get; set; }

		/// <summary>
		/// Get all the types in this enumerable.
		/// </summary>
		public Type[] GetTypes()
		{
			ArrayList result = new ArrayList();
			foreach (Type type in this)
			{
				result.Add(type);
			}
			return (Type[])result.ToArray(typeof(Type));
		}

		/// <summary>
		/// Does the given type match this filter?
		/// </summary>
		protected abstract bool MatchesFilter(Type type);

		#region IEnumerable Members

		public IEnumerator<Type> GetEnumerator()
		{
			foreach (Type type in Inner)
			{
				EnumeratorInterruptedException.ThrowIfInterruptingOnCurrentThread();
				if (MatchesFilter(type))
				{
					yield return type;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			foreach (Type type in this)
			{
				yield return type;
			}
		}

		#endregion
	}
}
