using System.Collections;
using System.Collections.Generic;

namespace CargoWise.GraphEngine.Test
{
	public class Cons<T> : IEnumerable<T>
	{
		public Cons(T first)
			: this(first, null)
		{
		}

		public Cons(T first, Cons<T> rest)
		{
			First = first;
			Rest = rest;
		}

		public T First { get; set; }
		public Cons<T> Rest { get; set; }

		public IEnumerator<T> GetEnumerator()
		{
			return new ConsEnumerator<T>(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public class ConsEnumerator<T> : IEnumerator<T>
	{
		public ConsEnumerator(Cons<T> cons)
		{
			firstCons = cons;
		}

		readonly Cons<T> firstCons;

		Cons<T> CurrentCons { get; set; }

		public T Current => CurrentCons.First;

		object IEnumerator.Current => Current;

		public void Dispose()
		{
			// Nom nom nom.
		}

		public bool MoveNext()
		{
			if (CurrentCons == null)
			{
				CurrentCons = firstCons;
				return true;
			}
			else if (CurrentCons.Rest == null)
			{
				return false;
			}
			else
			{
				CurrentCons = CurrentCons.Rest;
				return true;
			}
		}

		public void Reset()
		{
			CurrentCons = firstCons;
		}
	}
}
